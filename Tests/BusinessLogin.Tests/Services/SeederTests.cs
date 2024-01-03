using BusinessLogic;
using BusinessLogic.Options;
using BusinessLogic.Services;
using DataAccess.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BusinessLogin.Tests.Services;

public class SeederTests
{
    private readonly Seeder _seeder;
    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<RoleManager<Role>> _roleManager;
    private readonly AdminOptions _adminOptions;

    public SeederTests()
    {
        var options = MockHelpers.TestAdminOptions().Object;
        
        _adminOptions = options.Value;

        User[] userList =
        [
            new User
            {
                UserName = "Admin",
                NormalizedUserName = "ADMIN",
                Email = _adminOptions.Email,
                EmailConfirmed = true
            }
        ];
        
        _userManager = MockHelpers.TestUserManager<User>();
        
        _userManager
            .Setup(x => x.GetUsersInRoleAsync(Roles.Admin))
            .ReturnsAsync(userList);
        
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _userManager
            .Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(Roles.AllowedRoles);

        _roleManager = MockHelpers.TestRoleManager<Role>();
        
        _roleManager
            .Setup(x => x.CreateAsync(It.IsAny<Role>()))
            .ReturnsAsync(IdentityResult.Success);

        _seeder = new Seeder(_userManager.Object, _roleManager.Object, options);
    }


    [Fact]
    public async void SeedAsync_AdminExists()
    {
        var result = await _seeder.SeedAsync();

        result.IsSuccess.Should().BeTrue();

        _userManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        _userManager.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    public async void SeedAsync_NoAdmin()
    {
        _userManager
            .Setup(x => x.GetUsersInRoleAsync(Roles.Admin))
            .ReturnsAsync(new List<User>());
        
        _userManager
            .Setup(x => x.CreateAsync(It.Is<User>(y => y.UserName == "Admin"), _adminOptions.Password))
            .ReturnsAsync(IdentityResult.Success);
        
        _userManager
            .Setup(x => x.AddToRoleAsync(It.Is<User>(y => y.UserName == "Admin"), Roles.Admin))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _seeder.SeedAsync();

        result.IsSuccess.Should().BeTrue();

        _userManager.Verify(x => x.CreateAsync(It.Is<User>(y => y.UserName == "Admin"), _adminOptions.Password));
        _userManager.Verify(x => x.AddToRoleAsync(It.Is<User>(y => y.UserName == "Admin"), Roles.Admin));
    }

    [Fact]
    public async void SeedAsync_NoRoles()
    {
        _userManager
            .Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(Array.Empty<string>());
        var notSeededRolesCount = Roles.AllowedRoles.Length;

        var result = await _seeder.SeedAsync();

        result.IsSuccess.Should().BeTrue();

        _roleManager.Verify(
            x => x.CreateAsync(It.Is<Role>(y => Roles.AllowedRoles.Contains(y.Name))),
            Times.Exactly(notSeededRolesCount));
    }

    [Fact]
    public async void SeedAsync_OneRoleExists()
    {
        _roleManager
            .Setup(x => x.FindByIdAsync(Roles.Admin))
            .ReturnsAsync(new Role());

        var result = await _seeder.SeedAsync();

        result.IsSuccess.Should().BeTrue();

        _roleManager.Verify(
            x => x.CreateAsync(It.Is<Role>(y => Roles.AllowedRoles.Contains(y.Name))),
            Times.Exactly(Roles.AllowedRoles.Length - 1));
    }
}
