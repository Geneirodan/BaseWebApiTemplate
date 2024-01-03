using BusinessLogic.Models;
using BusinessLogic.Models.Filters;
using BusinessLogic.Validation;
using BusinessLogic.Validation.Extensions;
using DataAccess.Entities;
using DataAccess.Repositories;
using FluentResults;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Services;

public class UserService(IUserRepository repository, UserManager<User> userManager, IOptions<IdentityOptions> options)
{

    public async Task<Result<string>> CreateUserAsync(UserCreateModel model, string role)
    {
        var validationResult = await new UserCreateValidator(options).ValidateAsync(model);

        if (!validationResult.IsValid)
            return validationResult.ToFluentResult();

        if (!Roles.AllowedRoles.Contains(role))
            return Result.Fail($"The role {role} is not allowed");

        var user = model.Adapt<User>();

        var identityResult = await userManager.CreateAsync(user, model.Password);
        
        if (!identityResult.Succeeded)
            return identityResult.ToFluentResult();
        
        identityResult = await userManager.AddToRoleAsync(user, role);
        
        return identityResult.Succeeded
            ? Result.Ok(user.Id)
            : identityResult.ToFluentResult();
    }

    public async Task<Result> DeleteUserAsync(string id)
    {
        var user = await repository.GetAsync(id);

        if (user is null)
            return Result.Fail(Errors.NotFound);

        repository.Remove(user);
        return await repository.ConfirmAsync() > 0
            ? Result.Ok()
            : Result.Fail("Unable to save changes while deleting user");
    }

    public async Task<Result<UserViewModel>> GetUserByIdAsync(string id)
    {
        var user = await repository.GetAsync(id);

        if (user is null)
            return Result.Fail(Errors.NotFound);

        var viewModel = user.Adapt<UserViewModel>();

        return Result.Ok(viewModel);
    }

    public async Task<Result<IEnumerable<UserViewModel>>> GetUsersAsync(UserFilter filter)
    {
        var users = await repository.FindAsync(filter);

        var viewModels = users.Adapt<IEnumerable<UserViewModel>>();

        return Result.Ok(viewModels);
    }

    public async Task<Result<UserViewModel>> PatchUserAsync(string id, UserPatchModel model)
    {
        var validationResult = await new PatchValidator().ValidateAsync(model);
        
        if (!validationResult.IsValid)
            return validationResult.ToFluentResult();

        var user = await repository.GetAsync(id);

        if (user is null)
            return Result.Fail(Errors.NotFound);

        model.Adapt(user);

        repository.Update(user);
        
        return await repository.ConfirmAsync() > 0
            ? Result.Ok(user.Adapt<UserViewModel>())
            : Result.Fail("Unable to save changes while patching user");
    }
}