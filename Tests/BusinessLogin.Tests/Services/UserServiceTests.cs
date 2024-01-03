using AutoFilterer.Extensions;
using BusinessLogic;
using BusinessLogic.Models;
using BusinessLogic.Models.Filters;
using BusinessLogic.Services;
using BusinessLogic.Validation.Password;
using DataAccess.Entities;
using DataAccess.Repositories;
using FluentAssertions;
using FluentResults;
using JetBrains.Annotations;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Linq.Expressions;

// ReSharper disable UseCollectionExpression

namespace BusinessLogin.Tests.Services;

[TestSubject(typeof(UserService))]
public class UserServiceTests
{
    private readonly UserService _userService;
    private readonly Mock<IUserRepository> _repository;
    private readonly Mock<UserManager<User>> _userManager;
    private readonly IdentityOptions _options;

    private readonly IQueryable<User> _users = new List<User>
    {
        new() { Id = "1", UserName = "ABC", Email = "email1@gmail.com" },
        new() { Id = "2", UserName = "DEF", Email = "email2@gmail.com" },
        new() { Id = "3", UserName = "GHI", Email = "email3@gmail.com" }
    }.AsQueryable();

    private IQueryable<string> ValidIds => _users.Select(x => x.Id);

    public static TheoryData<string> Ids => new() { "0", "1", "2", "3", "4", "5" };

    public static TheoryData<UserFilter> UserFilters =>
        new()
        {
            new UserFilter(),
            new UserFilter { Id = "1" }
        };

    public UserServiceTests()
    {
        var adminOptions = MockHelpers.TestAdminOptions().Object.Value;
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
        var users = _users.ApplyFilter(filter);
        _repository.Setup(x => x.FindAsync(filter)).ReturnsAsync(users);

        var result = await _userService.GetUsersAsync(filter);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(users.Adapt<IEnumerable<UserViewModel>>());
        _repository.Verify(x => x.FindAsync(filter), Times.Once);
    }

    [Theory, MemberData(nameof(Ids))]
    public async void GetUserByIdAsync(string id)
    {
        var user = _users.FirstOrDefault(x => x.Id == id);
        _repository.Setup(x => x.GetAsync(id)).ReturnsAsync(user);

        var result = await _userService.GetUserByIdAsync(id);

        if (ValidIds.Contains(id))
        {
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(user.Adapt<UserViewModel>());
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

        result.Errors.Should().ContainEquivalentOf(userName.Length == 0
            ? new Error("\'User Name\' must not be empty.")
            : new Error($"\'User Name\' must be between 3 and 20 characters. You entered {userName.Length} characters."));

        result.Errors.Should().ContainEquivalentOf(email.Length == 0
            ? new Error("\'Email\' must not be empty.")
            : new Error("\'Email\' is not a valid email address."));
    }

    [Theory, MemberData(nameof(Ids))]
    public async void PatchUserAsync_Id(string id)
    {
        var validIds = _users.Select(x => x.Id);
        var user = _users.FirstOrDefault(x => x.Id == id);

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
        var user = _users.FirstOrDefault(x => x.Id == id);
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
        ;
        var validIds = _users.Select(x => x.Id);
        var user = _users.FirstOrDefault(x => x.Id == id);
        _repository.Setup(x => x.GetAsync(id)).ReturnsAsync(user);
        _repository.Setup(x => x.ConfirmAsync()).ReturnsAsync(1);

        var result = await _userService.DeleteUserAsync(id);

        if (validIds.Contains(id))
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
        var user = _users.FirstOrDefault(x => x.Id == id);
        _repository.Setup(x => x.GetAsync(id)).ReturnsAsync(user);
        _repository.Setup(x => x.ConfirmAsync()).ReturnsAsync(0);

        var result = await _userService.DeleteUserAsync(id);

        result.IsSuccess.Should().BeFalse();
        _repository.Verify(x => x.ConfirmAsync(), Times.Once);
        result.Errors.Should().ContainEquivalentOf(new Error("Unable to save changes while deleting user"));
    }

    public static TheoryData<UserCreateModel> UserCreateModels =>
        new()
        {
            new UserCreateModel("", "", ""),
            new UserCreateModel("1", "1", "1"),
            new UserCreateModel("123456789012345678901", "string", "string")
        };

    [Theory, MemberData(nameof(UserCreateModels))]
    public async void CreateUserAsync_ModelNotValid(UserCreateModel model)
    {
        var result = await _userService.CreateUserAsync(model, Roles.User);
        var (userName, email, password) = model;

        result.IsSuccess.Should().BeFalse();

        var errorsShould = result.Errors.Should();

        errorsShould.ContainEquivalentOf(userName.Length == 0
            ? new Error("\'User Name\' must not be empty.")
            : new Error($"\'User Name\' must be between 3 and 20 characters. You entered {userName.Length} characters."));

        errorsShould.ContainEquivalentOf(password.Length == 0
            ? new Error("\'Email\' must not be empty.")
            : new Error("\'Email\' is not a valid email address."));

        if (email.Length == 0)
            errorsShould.ContainEquivalentOf(new Error("\'Password\' must not be empty."));
        else
        {
            if (_options.Password.RequireDigit && !password.Any(x => x is < '0' or > '9'))
                errorsShould.ContainEquivalentOf(new Error(PasswordValidationErrors.RequireDigit));

            if (_options.Password.RequireLowercase && !password.Any(x => x is < 'a' or > 'z'))
                errorsShould.ContainEquivalentOf(new Error(PasswordValidationErrors.RequireLowercase));

            if (_options.Password.RequireUppercase && !password.Any(x => x is < 'A' or > 'Z'))
                errorsShould.ContainEquivalentOf(new Error(PasswordValidationErrors.RequireUppercase));

            if (_options.Password.RequireNonAlphanumeric && password.All(x => x is > '0' and < '9' or > 'a' and < 'z' or > 'A' and < 'Z'))
                errorsShould.ContainEquivalentOf(new Error(PasswordValidationErrors.RequireNonAlphanumeric));
        }

    }

    [Fact]
    public async void CreateUserAsync_RoleNotAllowed()
    {
        const string notAllowedRole = "Some role";
        var result = await _userService.CreateUserAsync(new UserCreateModel("UserName", "email@gmail.com", "1String!"), notAllowedRole);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainEquivalentOf(new Error($"The role {notAllowedRole} is not allowed"));
    }


    [Fact]
    public async void CreateUserAsync()
    {
        var createModel = new UserCreateModel("ABC", "email@gmail.com", "1String!");
        var result = await _userService.CreateUserAsync(createModel, Roles.User);

        result.IsSuccess.Should().BeTrue();
        result.ValueOrDefault.Should().NotBeNullOrEmpty();

        var (userName, _, password) = createModel;
        _userManager.Verify(x => x.CreateAsync(It.Is<User>(u => u.UserName == userName), password), Times.Once());
        _userManager.Verify(x => x.AddToRoleAsync(It.Is<User>(u => u.UserName == userName), Roles.User), Times.Once());
    }


    [Fact]
    public async void CreateUserAsync_UnableToCreateUser()
    {
        _userManager
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError()));

        var createModel = new UserCreateModel("ABC", "email@gmail.com", "1String!");
        var result = await _userService.CreateUserAsync(createModel, Roles.User);

        result.IsSuccess.Should().BeFalse();
        var (userName, _, password) = createModel;
        _userManager.Verify(x => x.CreateAsync(It.Is<User>(u => u.UserName == userName), password), Times.Once());
        _userManager.Verify(x => x.AddToRoleAsync(It.Is<User>(u => u.UserName == userName), Roles.User), Times.Never());
        result.ValueOrDefault.Should().BeEquivalentTo(null);
    }
    
    [Fact]
    public async void CreateUserAsync_UnableToAddToRole()
    {
        _userManager
            .Setup(x => x.AddToRoleAsync(It.IsAny<User>(), Roles.User))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError()));
    
        var createModel = new UserCreateModel("ABC", "email@gmail.com", "1String!");
        var result = await _userService.CreateUserAsync(createModel, Roles.User);
    
        result.IsSuccess.Should().BeFalse();
        var (userName, _, password) = createModel;
        _userManager.Verify(x => x.CreateAsync(It.Is<User>(u => u.UserName == userName), password), Times.Once());
        _userManager.Verify(x => x.AddToRoleAsync(It.Is<User>(u => u.UserName == userName), Roles.User), Times.Once());
        result.ValueOrDefault.Should().BeEquivalentTo(null);
    }
}
