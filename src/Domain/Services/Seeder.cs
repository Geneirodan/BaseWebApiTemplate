using Domain.Extensions;
using Infrastructure.Entities;
using Domain.Constants;
using Domain.Interfaces;
using Domain.Options;
using FluentResults;
using Geneirodan.Generics.CrudService.Attributes;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Domain.Services;

[ScopedService]
public class Seeder(UserManager<User> userManager, RoleManager<Role> roleManager, IOptions<AdminOptions> options) : ISeeder
{
    private readonly AdminOptions _options = options.Value;

    public async Task<Result> SeedAsync()
    {
        var result = await SeedRoles();
        if (result.IsSuccess)
            result = await SeedAdmin();
        return result;
    }

    private async Task<Result> SeedAdmin()
    {
        const string errorMessage = "Unable to seed admin user";

        var admins = await userManager.GetUsersInRoleAsync(Roles.Admin);
        var admin = admins.FirstOrDefault();

        if (admin is not null)
            return Result.Ok();

        admin = new User { UserName = "Admin", Email = _options.Email, EmailConfirmed = true };

        var result = await userManager.CreateAsync(admin, _options.Password);
        if (!result.Succeeded)
            return Result.Fail(errorMessage);

        result = await userManager.AddToRoleAsync(admin, Roles.Admin);
        return result.Succeeded ? Result.Ok() : Result.Fail(errorMessage);


    }
    private async Task<Result> SeedRoles()
    {
        List<ResultBase> results = [];
        foreach (var expectedRole in Roles.AllowedRoles)
            if (await roleManager.FindByIdAsync(expectedRole) is null)
            {
                var result = await roleManager.CreateAsync(new Role { Name = expectedRole });
                results.Add(result.ToFluentResult());
            }
        return Result.Merge(results.ToArray());

    }
}
