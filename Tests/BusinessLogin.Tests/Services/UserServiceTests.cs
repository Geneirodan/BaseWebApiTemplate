using AutoFilterer.Extensions;
using BusinessLogic;
using BusinessLogic.Models.Auth;
using BusinessLogic.Models.Filters;
using BusinessLogic.Models.User;
using BusinessLogic.Services;
using BusinessLogin.Tests.Data;
using BusinessLogin.Tests.Extensions;
using DataAccess.Entities;
using DataAccess.Interfaces;
using FluentAssertions;
using FluentResults;
using JetBrains.Annotations;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Moq;

// ReSharper disable UseCollectionExpression

namespace BusinessLogin.Tests.Services;

[TestSubject(typeof(UserService))]
public class UserServiceTests
{
    private readonly UserService _userService;
    private readonly Mock<IUserRepository> _repository;
    private readonly Mock<UserManager<User>> _userManager;
    private readonly IdentityOptions _options;

    

    internal static readonly RegisterModel validRegisterModel = new("ABC", "email@gmail.com", "1String!");

    public static TheoryData<string> Ids => new() { "0", "1", "2", "3", "4", "5" };

    public static TheoryData<UserFilter> UserFilters =>
        new()
        {
            new UserFilter(),
            new UserFilter { Id = "1" }
        };

    public UserServiceTests()
    {
        var options = MockHelpers.TestIdentityOptions().Object;

        _options = options.Value;


        _userManager = MockHelpers.TestUserManager<User>();

        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), Roles.User))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.GetRolesAsync(It.IsAny<User>()))
            .ReturnsAsync(Roles.AllowedRoles);

        _repository = new Mock<IUserRepository>();
        _userService = new UserService(_repository.Object, _userManager.Object, options);
    }

    [Theory, MemberData(nameof(UserFilters))]
    public async void GetUsersAsync(UserFilter filter)
    {
        var users = UserData.usersTable.ApplyFilter(filter);
        _repository.Setup(x => x.FindAsync(filter)).ReturnsAsync(users);

        var result = await _userService.GetUsersAsync(filter);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(users.Adapt<IEnumerable<UserViewModel>>());
        _repository.Verify(x => x.FindAsync(filter), Times.Once);
    }

    [Theory, MemberData(nameof(Ids))]
    public async void GetUserByIdAsync(string id)
    {
        var user = UserData.usersTable.FirstOrDefault(x => x.Id == id);
        _repository.Setup(x => x.GetAsync(id)).ReturnsAsync(user);

        var result = await _userService.GetUserByIdAsync(id);

        if (UserData.validIds.Contains(id))
        {
            result.IsSuccess.Should().BeTrue();
            result.ValueOrDefault.Should().BeEquivalentTo(user.Adapt<UserViewModel>());
        }
        else
        {
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainEquivalentOf(new Error(Errors.NotFound));
        }
    }
    public static TheoryData<UserPatchModel> UserPatchModels =>
        new()
        {
            new UserPatchModel("", ""),
            new UserPatchModel("1", ""),
            new UserPatchModel("123456789012345678901", "")
        };

    [Theory, MemberData(nameof(UserPatchModels))]
    public async void PatchUserAsync_ModelNotValid(UserPatchModel model)
    {

        var result = await _userService.PatchUserAsync(string.Empty, model);
        var (userName, email) = model;

        result.IsSuccess.Should().BeFalse();
        result.Errors.TestUsernameValidation(userName);
        result.Errors.TestEmailValidation(email);
    }

    [Theory, MemberData(nameof(Ids))]
    public async void PatchUserAsync_Id(string id)
    {
        var validIds = UserData.usersTable.Select(x => x.Id);
        var user = UserData.usersTable.FirstOrDefault(x => x.Id == id);

        _repository.Setup(x => x.GetAsync(id)).ReturnsAsync(user);
        _repository.Setup(x => x.ConfirmAsync()).ReturnsAsync(1);

        var result = await _userService.PatchUserAsync(id, new UserPatchModel("Name", "email@gmail.com"));

        if (validIds.Contains(id))
        {
            result.IsSuccess.Should().BeTrue();
            _repository.Verify(x => x.ConfirmAsync(), Times.Once);
            result.Value.Should().BeEquivalentTo(new UserViewModel(id, "Name", "email@gmail.com"));
        }
        else
        {
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainEquivalentOf(new Error(Errors.NotFound));
        }
    }

    [Fact]
    public async void PatchUserAsync_NotAbleToSaveChanges()
    {
        const string id = "1";
        var user = UserData.usersTable.FirstOrDefault(x => x.Id == id);
        _repository.Setup(x => x.GetAsync(id)).ReturnsAsync(user);
        _repository.Setup(x => x.ConfirmAsync()).ReturnsAsync(0);

        var result = await _userService.PatchUserAsync(id, new UserPatchModel("Name", "email@gmail.com"));

        result.IsSuccess.Should().BeFalse();
        _repository.Verify(x => x.ConfirmAsync(), Times.Once);
        result.Errors.Should().ContainEquivalentOf(new Error("Unable to save changes while patching user"));
    }

    [Theory, MemberData(nameof(Ids))]
    public async void DeleteUserAsync(string id)
    {
        var user = UserData.usersTable.FirstOrDefault(x => x.Id == id);
        _repository.Setup(x => x.GetAsync(id)).ReturnsAsync(user);
        _repository.Setup(x => x.ConfirmAsync()).ReturnsAsync(1);

        var result = await _userService.DeleteUserAsync(id);

        if (UserData.validIds.Contains(id))
        {
            result.IsSuccess.Should().BeTrue();
            _repository.Verify(x => x.ConfirmAsync(), Times.Once);
        }
        else
        {
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainEquivalentOf(new Error(Errors.NotFound));
        }
    }

    [Fact]
    public async void DeleteUserAsync_NotAbleToSaveChanges()
    {
        const string id = "1";
        var user = UserData.usersTable.FirstOrDefault(x => x.Id == id);
        _repository.Setup(x => x.GetAsync(id)).ReturnsAsync(user);
        _repository.Setup(x => x.ConfirmAsync()).ReturnsAsync(0);

        var result = await _userService.DeleteUserAsync(id);

        result.IsSuccess.Should().BeFalse();
        _repository.Verify(x => x.ConfirmAsync(), Times.Once);
        result.Errors.Should().ContainEquivalentOf(new Error("Unable to save changes while deleting user"));
    }

    public static TheoryData<RegisterModel> UserCreateModels =>
        new()
        {
            new RegisterModel("", "", ""),
            new RegisterModel("1", "1", "1"),
            new RegisterModel("123456789012345678901", "string", "string")
        };

    [Theory, MemberData(nameof(UserCreateModels))]
    public async void CreateUserAsync_ModelNotValid(RegisterModel model)
    {
        var result = await _userService.CreateUserAsync(model, Roles.User);
        var (userName, email, password) = model;

        result.IsSuccess.Should().BeFalse();
        result.Errors.TestUsernameValidation(userName);
        result.Errors.TestEmailValidation(email);
        result.Errors.TestPasswordValidationResult(password, _options);
    }

    [Fact]
    public async void CreateUserAsync_RoleNotAllowed()
    {
        const string notAllowedRole = "Some role";
        var result = await _userService.CreateUserAsync(validRegisterModel, notAllowedRole);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainEquivalentOf(new Error($"The role {notAllowedRole} is not allowed"));
    }


    [Fact]
    public async void CreateUserAsync()
    {
        var result = await _userService.CreateUserAsync(validRegisterModel, Roles.User);

        result.IsSuccess.Should().BeTrue();
        result.ValueOrDefault.Should().NotBeNull();

        var (userName, _, password) = validRegisterModel;
        _userManager.Verify(x => x.CreateAsync(It.Is<User>(u => u.UserName == userName), password), Times.Once());
        _userManager.Verify(x => x.AddToRoleAsync(It.Is<User>(u => u.UserName == userName), Roles.User), Times.Once());
    }


    [Fact]
    public async void CreateUserAsync_UnableToCreateUser()
    {
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError()));

        var result = await _userService.CreateUserAsync(validRegisterModel, Roles.User);

        result.IsSuccess.Should().BeFalse();
        var (userName, _, password) = validRegisterModel;
        _userManager.Verify(x => x.CreateAsync(It.Is<User>(u => u.UserName == userName), password), Times.Once());
        _userManager.Verify(x => x.AddToRoleAsync(It.Is<User>(u => u.UserName == userName), Roles.User), Times.Never());
        result.ValueOrDefault.Should().BeEquivalentTo(null as UserViewModel);
    }
    
    [Fact]
    public async void CreateUserAsync_UnableToAddToRole()
    {
        _userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), Roles.User))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError()));

        var result = await _userService.CreateUserAsync(validRegisterModel, Roles.User);
    
        result.IsSuccess.Should().BeFalse();
        var (userName, _, password) = validRegisterModel;
        _userManager.Verify(x => x.CreateAsync(It.Is<User>(u => u.UserName == userName), password), Times.Once());
        _userManager.Verify(x => x.AddToRoleAsync(It.Is<User>(u => u.UserName == userName), Roles.User), Times.Once());
        result.ValueOrDefault.Should().BeEquivalentTo(null as UserViewModel);
    }
}
