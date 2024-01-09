using BusinessLogic.Extensions;
using BusinessLogic.Interfaces;
using BusinessLogic.Models.PasswordRecovery;
using BusinessLogic.Validation.PasswordRecovery;
using DataAccess.Entities;
using DataAccess.Interfaces;
using DataAccess.Repositories;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services;

[ScopedService]
public class PasswordRecoveryService(UserManager<User> userManager, IUserRepository userRepository, IOptions<IdentityOptions> identityOptions) : IPasswordRecoveryService
{
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
        var (id, oldPassword, newPassword) = model;
        
        var validationResult = await new ChangePasswordValidator(identityOptions).ValidateAsync(model);
        if (!validationResult.IsValid)
            return validationResult.ToFluentResult();
        
        var user = await userRepository.GetAsync(id);
        if (user is null)
            return Result.Fail(Errors.NotFound);
        
        var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        return result.ToFluentResult();
    }
}
