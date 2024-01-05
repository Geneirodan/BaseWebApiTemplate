using BusinessLogic.Interfaces;
using BusinessLogic.Models;
using BusinessLogic.Models.Auth;
using BusinessLogic.Options;
using DataAccess.Interfaces;
using FluentResults;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogic.Services;

public class TokenService(IOptions<JwtOptions> options, IUserRepository userRepository) : ITokenService
{
    private readonly SymmetricSecurityKey _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.Key));
    private readonly int _lifetimeMinutes = options.Value.LifetimeMinutes;

    public async Task<Result> AddRefreshTokenToUser(string id, string token)
    {
        await userRepository.AddRefreshToken(id, token);
        return await userRepository.ConfirmAsync() > 0
            ? Result.Ok()
            : Result.Fail("Unable to add refresh token");
    }

    public async Task<Result> DeleteRefreshTokenToUser(string id, string token)
    {
        await userRepository.DeleteRefreshToken(id, token);
        return await userRepository.ConfirmAsync() > 0
            ? Result.Ok()
            : Result.Fail("Unable to add refresh token");
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
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

    public Result<Tokens> CreateTokens(string email, string? userName = null)
    {
        var token = GetAccessToken(email, userName);
        var refreshToken = GetRefreshToken();
        var tokens = new Tokens(token, refreshToken);
        return Result.Ok(tokens);
    }
    private string GetAccessToken(string email, string? userName = null)
    {

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName ?? string.Empty),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

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
