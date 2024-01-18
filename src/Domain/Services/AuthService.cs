using Domain.Extensions;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Domain.Constants;
using Domain.Interfaces;
using Domain.Models.Auth;
using Domain.Models.User;
using Domain.Options;
using FluentResults;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Web;

namespace Domain.Services;

[ScopedService]
public class AuthService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ITokenService tokenService,
    IMailService mailService,
    IOptions<GoogleAuthOptions> googleOptions,
    IUserRepository userRepository,
    IUserService userService) : IAuthService
{
    public async Task<Result<Tokens>> LoginAsync(LoginModel model)
    {
        var (userName, password) = model;

        var user = await userManager.FindByNameAsync(userName)
                   ?? await userManager.FindByEmailAsync(userName);

        if (user is null)
            return Result.Fail(Errors.NotFound);

        var result = await signInManager.PasswordSignInAsync(user, password, lockoutOnFailure: false, isPersistent: true);

        if(result.IsNotAllowed)
            return Result.Fail(Errors.Forbidden).WithError(nameof(result.IsNotAllowed));
        
        if(result.IsLockedOut)
            return Result.Fail(Errors.Forbidden).WithError(nameof(result.IsLockedOut));
        
        if (!result.Succeeded)
            return Result.Fail("Unable to log in user");

        var tokens = await tokenService.CreateTokensAsync(user);
        return Result.Ok(tokens);
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();

    public Task<Result<UserViewModel>> RegisterAsync(RegisterModel model) => userService.RegisterUserAsync(model, Roles.User);

    public async Task<Result> ConfirmEmailAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Fail(Errors.NotFound);

        var result = await userManager.ConfirmEmailAsync(user, token);

        return result.ToFluentResult();
    }

    public async Task<Result> SendEmailConfirmationAsync(string userId, string confirmUrl, string callbackUrl)
    {
        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Fail(Errors.NotFound);

        if (user.EmailConfirmed)
            return Result.Fail("Email of the user is already confirmed");

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        token = HttpUtility.UrlEncode(token);
        callbackUrl = HttpUtility.UrlEncode(callbackUrl);

        var link = $"{confirmUrl}?userId={user.Id}&token={token}&callbackUrl={callbackUrl}";

        var sendEmailResult = await mailService.SendEmailAsync(user.Email!, "Email confirmation", $"Confirm your email by <a href={link}>this link</a>");

        return sendEmailResult.IsFailed ? Result.Fail(sendEmailResult.Errors) : Result.Ok();
    }

    public async Task<Result<Tokens>> GoogleLogin(string token)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new List<string> { googleOptions.Value.ClientId }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);
        var user = await userRepository.GetAsync(x => x.Email == payload.Email);
        if (user is not null)
            return await tokenService.CreateTokensAsync(user);
        user = new User
        {
            Email = payload.Email,
            UserName = payload.Email,
            EmailConfirmed = true
        };
        var identityResult = await userManager.CreateAsync(user);

        if (!identityResult.Succeeded)
            return identityResult.ToFluentResult();

        var roleResult = await userManager.AddToRoleAsync(user, Roles.User);

        if (!roleResult.Succeeded)
            return roleResult.ToFluentResult();
        return await tokenService.CreateTokensAsync(user);
    }
}
