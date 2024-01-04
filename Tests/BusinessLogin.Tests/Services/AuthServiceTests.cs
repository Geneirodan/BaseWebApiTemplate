using BusinessLogic;
using BusinessLogic.Interfaces;
using BusinessLogic.Models;
using BusinessLogic.Options;
using BusinessLogic.Services;
using BusinessLogic.Validation.Password;
using BusinessLogin.Tests.Data;
using DataAccess.Entities;
using FluentAssertions;
using FluentResults;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

// ReSharper disable UseCollectionExpression

namespace BusinessLogin.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthService _authService;
    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<SignInManager<User>> _signInManager;
    private readonly Mock<ITokenService> _tokenService;
    private readonly Mock<IMailService> _mailService;
    private readonly IdentityOptions _options;

    public AuthServiceTests()
    {
        _userManager = MockHelpers.TestUserManager<User>();

        _userManager
            .Setup(x => x.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(User);
        _userManager
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(User);
        _userManager
            .Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync(ValidToken);


        _signInManager = MockHelpers.TestSignInManager<User>();

        _signInManager
            .Setup(x => x.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        _tokenService = new Mock<ITokenService>();

        _tokenService.Setup(x => x.CreateTokens(It.IsAny<string>(), It.IsAny<string?>())).Returns(Result.Ok(Users.tokens));

        _mailService = new Mock<IMailService>();

        _mailService
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Ok());

        Mock<IUserService> userService = new();

        userService
            .Setup(x => x.CreateUserAsync(UserServiceTests.validRegisterModel, Roles.User))
            .ReturnsAsync(Result.Ok(User.Adapt<UserViewModel>()));

        var options = MockHelpers.TestIdentityOptions().Object;
        _options = options.Value;

        var googleOptions = new Mock<IOptions<GoogleAuthOptions>>();
        googleOptions.Setup(o => o.Value).Returns(new GoogleAuthOptions());

        _authService = new AuthService(
            _userManager.Object,
            _signInManager.Object,
            _tokenService.Object,
            _mailService.Object,
            options,
            googleOptions.Object,
            userService.Object);
    }
    private static User User => new() { Id = "1", UserName = "name", Email = "email" };

    public static readonly TheoryData<LoginModel> invalidLoginModels =
        new()
        {
            new LoginModel("", "")
        };

    public static readonly TheoryData<LoginModel> validLoginModels =
        new()
        {
            new LoginModel("test", "1String!"),
            new LoginModel("ABC", "1String!"),
            new LoginModel("email1@gmail.com", "1String!")
        };


    [Theory, MemberData(nameof(invalidLoginModels))]
    public async void Login_ModelIsNotValid(LoginModel model)
    {
        var result = await _authService.LoginAsync(model);
        var (userName, password) = model;

        result.IsSuccess.Should().BeFalse();

        var errorsShould = result.Errors.Should();

        errorsShould.ContainEquivalentOf(userName.Length == 0
            ? new Error("\'User Name\' must not be empty.")
            : new Error($"\'User Name\' must be between 3 and 20 characters. You entered {userName.Length} characters."));

        if (password.Length == 0)
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

    [Theory, MemberData(nameof(validLoginModels))]
    public async void Login_Failed(LoginModel model)
    {
        var (userName, password) = model;
        var user = Users.usersTable.FirstOrDefault(x => x.UserName == userName || x.Email == userName);
        _userManager.Setup(x => x.FindByNameAsync(userName))
            .ReturnsAsync(Users.usersTable.FirstOrDefault(x => x.UserName == userName));
        _userManager.Setup(x => x.FindByEmailAsync(userName))
            .ReturnsAsync(Users.usersTable.FirstOrDefault(x => x.Email == userName));
        _signInManager
            .Setup(x => x.PasswordSignInAsync(
                It.Is<User>(y => y.UserName == userName || y.Email == userName),
                password,
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);

        var result = await _authService.LoginAsync(model);

        result.IsSuccess.Should().BeFalse();

        if (user is null)
            result.Errors.Should().ContainEquivalentOf(new Error(Errors.NotFound));
        else if (user.PasswordHash == password)
            result.Errors.Should().ContainEquivalentOf(new Error("Unable to log in user"));
        else
            result.Errors.Should().ContainEquivalentOf(new Error("Wrong password"));
    }

    [Theory, MemberData(nameof(validLoginModels))]
    public async void Login(LoginModel model)
    {
        var result = await _authService.LoginAsync(model);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(Users.tokens);
    }

    [Fact]
    public async void Login_UnableToCreateToken()
    {
        _tokenService.Setup(x => x.CreateTokens(It.IsAny<string>(), It.IsAny<string?>())).Returns(Result.Fail("Token error"));

        var result = await _authService.LoginAsync(new LoginModel("test", "1String!"));

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainEquivalentOf(new Error("Token error"));

    }

    public static readonly TheoryData<RegisterModel> invalidRegisterModels =
        new()
        {
            new RegisterModel("test", "new@mail.com", "!String1"),
            new RegisterModel("ABC", "email1@gmail.com", "1String!"),
            new RegisterModel("DEF", "email1@gmail.com", "1String!"),
            new RegisterModel("DEFf", "email2@gmail.com", "1String!"),
        };

    private const string ValidToken = "valid token";
    private const string InvalidToken = "invalid token";

    public static readonly TheoryData<ConfirmEmailModel> confirmEmailModels =
        new()
        {
            new ConfirmEmailModel("0", ValidToken),
            new ConfirmEmailModel("0", InvalidToken),
            new ConfirmEmailModel("1", ValidToken),
            new ConfirmEmailModel("1", InvalidToken)
        };


    [Theory, MemberData(nameof(confirmEmailModels))]
    public async void ConfirmEmailAsync(ConfirmEmailModel model)
    {
        var (userId, token) = model;
        var user = Users.usersTable.FirstOrDefault(x => x.Id == userId);
        _userManager
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);
        _userManager
            .Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), token))
            .ReturnsAsync(token == ValidToken ? IdentityResult.Success : IdentityResult.Failed());

        var result = await _authService.ConfirmEmailAsync(model);

        if (user is null)
        {
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainEquivalentOf(new Error(Errors.NotFound));
        }
        else
        {
            result.IsSuccess.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

    }

    public static TheoryData<string> Ids => new() { "0", "1", "2", "3", "4", "5" };
    [Theory, MemberData(nameof(Ids))]
    public async void SendEmailConfirmationAsync(string userId)
    {
        var user = Users.usersTable.FirstOrDefault(x => x.Id == userId);
        _userManager
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        var result = await _authService.SendEmailConfirmationAsync(userId, string.Empty, string.Empty);

        if (user is null)
        {
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainEquivalentOf(new Error(Errors.NotFound));
        }
        else if (user.EmailConfirmed)
        {
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainEquivalentOf(new Error("Email of the user is already confirmed"));
        }
        else
        {
            result.IsSuccess.Should().BeTrue();
            _mailService.Verify(x => x.SendEmailAsync(user.Email!, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }


    [Fact]
    public async void SendEmailConfirmationAsync_EmailCantSend()
    {
        _mailService
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Fail("Sending mail errors"));

        var result = await _authService
            .SendEmailConfirmationAsync(Guid.Empty.ToString(), string.Empty, string.Empty);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainEquivalentOf(new Error("Sending mail errors"));
    }
}
