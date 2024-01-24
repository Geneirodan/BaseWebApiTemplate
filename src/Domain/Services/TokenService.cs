using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Domain.Constants;
using Domain.Interfaces;
using Domain.Models.Auth;
using Domain.Options;
using FluentResults;
using Geneirodan.Generics.CrudService.Attributes;
using Geneirodan.Generics.CrudService.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Domain.Services;

[ScopedService]
// ReSharper disable once UnusedType.Global
public class TokenService(IOptions<JwtOptions> options, ITokenRepository tokenRepository, UserManager<User> userManager) : ITokenService
{
    private readonly SymmetricSecurityKey _key = new(Encoding.UTF8.GetBytes(options.Value.Key));
    private readonly int _lifetimeMinutes = options.Value.LifetimeMinutes;
    public async Task<Result<Tokens>> RefreshAsync(Tokens tokens)
    {
        var principal = GetPrincipalFromExpiredToken(tokens.AccessToken);
        var user = await userManager.GetUserAsync(principal);
        
        if(user is null)
            return Result.Fail(Errors.NotFound);
        
        var savedRefreshToken = await tokenRepository.GetSavedRefreshTokensAsync(user.Id, tokens.RefreshToken);
        
        if (savedRefreshToken?.Token != tokens.RefreshToken)
            return Result.Fail(Errors.Forbidden);
        await tokenRepository.DeleteRefreshTokenAsync(user.Id, tokens.RefreshToken);
        var newTokens = await CreateTokensAsync(user);
        return Result.Ok(newTokens);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _key,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        if (securityToken is not JwtSecurityToken jwtSecurityToken
            || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public async Task<Tokens> CreateTokensAsync(User user)
    {
        var token = await GetAccessTokenAsync(user);
        var refreshToken = GetRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id
        };
        await tokenRepository.AddRefreshTokenAsync(refreshTokenEntity);
        await tokenRepository.SaveChangesAsync();
        return new Tokens(token, refreshToken);
    }
    private async Task<string> GetAccessTokenAsync(User user)
    {

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var roles = await userManager.GetRolesAsync(user);

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_lifetimeMinutes),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256)
        };

        var tokenHandler = new JwtSecurityTokenHandler();

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
    private static string GetRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
