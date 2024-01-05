using BusinessLogic.Interfaces;
using BusinessLogic.Models;
using BusinessLogic.Models.Auth;
using BusinessLogic.Models.User;
using BusinessLogic.Options;
using BusinessLogic.Validation;
using BusinessLogic.Validation.Extensions;
using DataAccess.Entities;
using FluentResults;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Web;

namespace BusinessLogic.Services;

public class AuthService(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ITokenService tokenService,
    IMailService mailService,
    IOptions<IdentityOptions> identityOptions,
    IOptions<GoogleAuthOptions> googleOptions,
    IUserService userService)
{
    public async Task<Result<Tokens>> LoginAsync(LoginModel model)
    {
        var (userName, password) = model;
        var validationResult = await new LoginValidator(identityOptions).ValidateAsync(model);
        if (!validationResult.IsValid)
            return validationResult.ToFluentResult();

        var user = await userManager.FindByNameAsync(userName)
                   ?? await userManager.FindByEmailAsync(userName);

        if (user is null)
            return Result.Fail(Errors.NotFound);

        var result = await signInManager.PasswordSignInAsync(user, password, lockoutOnFailure: false, isPersistent: true);

        if (!result.Succeeded)
            return Result.Fail("Unable to log in user");

        var (_, isFailed, tokens, errors) = tokenService.CreateTokens(user.Email!);
        return isFailed ? Result.Fail(errors) : Result.Ok(tokens);
    }

    public Task LogoutAsync() => signInManager.SignOutAsync();

    public Task<Result<UserViewModel>> RegisterAsync(RegisterModel model) => userService.CreateUserAsync(model, Roles.User);

    public async Task<Result> ConfirmEmailAsync(ConfirmEmailModel model)
    {
        var user = await userManager.FindByIdAsync(model.UserId);

        if (user is null)
            return Result.Fail(Errors.NotFound);

        var result = await userManager.ConfirmEmailAsync(user, model.Token);

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

    public async Task<Result<Tokens>> GoogleLogin(GoogleAuthModel model)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            // Change this to your google client ID
            Audience = new List<string> { googleOptions.Value.ClientId }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(model.Token, settings);

        return tokenService.CreateTokens(payload.Email);
    }
}
