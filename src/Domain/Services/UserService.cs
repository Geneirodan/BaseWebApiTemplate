using AutoFilterer.Extensions;
using Domain.Extensions;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Infrastructure.Repositories;
using Domain.Constants;
using Domain.Interfaces;
using Domain.Models;
using Domain.Models.Auth;
using Domain.Models.Filters;
using Domain.Models.User;
using Domain.Validation;
using FluentResults;
using Geneirodan.Generics.CrudService.Attributes;
using Geneirodan.Generics.CrudService.Constants;
using Geneirodan.Generics.CrudService.Models;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Domain.Services;

[ScopedService]
public class UserService(IUserRepository repository, UserManager<User> userManager, IOptions<IdentityOptions> options) : IUserService
{

    public async Task<Result<UserViewModel>> RegisterUserAsync(RegisterModel model, string role)
    {
        var validationResult = await new RegisterValidator(options).ValidateAsync(model);

        if (!validationResult.IsValid)
            return validationResult.ToFluentResult();

        if (!Roles.AllowedRoles.Contains(role))
            return Result.Fail($"The role {role} is not allowed");

        var user = model.Adapt<User>();

        var identityResult = await userManager.CreateAsync(user, model.Password);

        if (!identityResult.Succeeded)
            return identityResult.ToFluentResult();

        var roleResult = await userManager.AddToRoleAsync(user, Roles.User);

        if (!roleResult.Succeeded)
            return roleResult.ToFluentResult();

        var userViewModel = user.Adapt<UserViewModel>();

        return Result.Ok(userViewModel);
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

    public async Task<UserViewModel?> GetUserByIdAsync(string id)
    {
        var user = await repository.GetAsync(id);
        return user?.Adapt<UserViewModel>();
    }

    public async Task<Result<PaginationModel<UserViewModel>>> GetUsersAsync(UserFilter filter)
    {
        var users = repository.GetAll().ApplyFilterWithoutPagination(filter);
        var paged = await users.ToPaged(filter.Page, filter.PerPage).ToListAsync();
        var viewModels = paged.Adapt<IEnumerable<UserViewModel>>();

        return Result.Ok(new PaginationModel<UserViewModel>(viewModels, users.Count()));
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
