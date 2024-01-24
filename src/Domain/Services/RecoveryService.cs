using Domain.Extensions;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Domain.Constants;
using Domain.Interfaces;
using Domain.Models.PasswordRecovery;
using Domain.Validation.PasswordRecovery;
using FluentResults;
using Geneirodan.Generics.CrudService.Attributes;
using Geneirodan.Generics.CrudService.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Domain.Services;

[ScopedService]
public class RecoveryService(
    UserManager<User> userManager,
    IUserRepository userRepository,
    IOptions<IdentityOptions> identityOptions,
    IMailService mailService
) : IRecoveryService
{

    public async Task<Result> ForgotPasswordAsync(string email, string confirmUrl, string callbackUrl)
    {
        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return Result.Fail(Errors.NotFound);

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var link = HtmlEncoder.Default.Encode($"{confirmUrl}?userId={user.Id}&token={token}&callbackUrl={callbackUrl}");
        return await mailService.SendEmailAsync(email, "Password recovery", $"Confirm your email by <a href={link}>this link</a>");
    }
    public async Task<Result> ResetPasswordAsync(ResetPasswordModel model)
    {
        var (email, password, token) = model;

        var validationResult = await new ResetPasswordValidator(identityOptions).ValidateAsync(model);
        if (!validationResult.IsValid)
            return validationResult.ToFluentResult();

        var user = await userManager.FindByEmailAsync(email);

        if (user is null)
            return Result.Fail(Errors.NotFound);

        var result = await userManager.ResetPasswordAsync(user, token, password);

        return result.ToFluentResult();
    }

    public async Task<Result> AddPasswordAsync(AddPasswordModel model)
    {
        var (id, password) = model;

        var validationResult = await new AddPasswordValidator(identityOptions).ValidateAsync(model);
        if (!validationResult.IsValid)
            return validationResult.ToFluentResult();

        var user = await userRepository.GetAsync(id);
        if (user is null)
            return Result.Fail(Errors.NotFound);
        var result = await userManager.AddPasswordAsync(user, password);
        return result.ToFluentResult();
    }

    public async Task<Result> ChangePasswordAsync(ChangePasswordModel model)
    {

        var validationResult = await new ChangePasswordValidator(identityOptions).ValidateAsync(model);
        if (!validationResult.IsValid)
            return validationResult.ToFluentResult();

        var user = await userRepository.GetAsync(model.Id);
        if (user is null)
            return Result.Fail(Errors.NotFound);

        var result = await userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        return result.ToFluentResult();
    }
}
