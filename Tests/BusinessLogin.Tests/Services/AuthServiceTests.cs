using BusinessLogic;
using BusinessLogic.Interfaces;
using BusinessLogic.Models.Auth;
using BusinessLogic.Models.User;
using BusinessLogic.Options;
using BusinessLogic.Services;
using BusinessLogin.Tests.Data;
using BusinessLogin.Tests.Extensions;
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
            .ReturnsAsync(UserData.sampleUser);
        _userManager
            .Setup(x => x.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(UserData.sampleUser);
        _userManager
            .Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
            .ReturnsAsync(UserData.ValidToken);


        _signInManager = MockHelpers.TestSignInManager<User>();

        _signInManager
            .Setup(x => x.PasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        _tokenService = new Mock<ITokenService>();

        _tokenService.Setup(x => x.CreateTokens(It.IsAny<string>(), It.IsAny<string?>())).Returns(Result.Ok(UserData.tokens));

        _mailService = new Mock<IMailService>();

        _mailService
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Ok());

        Mock<IUserService> userService = new();

        userService
            .Setup(x => x.CreateUserAsync(UserServiceTests.validRegisterModel, Roles.User))
            .ReturnsAsync(Result.Ok(UserData.sampleUser.Adapt<UserViewModel>()));

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

    public static readonly TheoryData<LoginModel> invalidLoginModels =
        new()
        {
            new LoginModel("", "")
        };

    [Theory, MemberData(nameof(invalidLoginModels))]
    public async void Login_ModelIsNotValid(LoginModel model)
    {
        var result = await _authService.LoginAsync(model);
        var (userName, password) = model;

        result.IsSuccess.Should().BeFalse();

        result.Errors.Should().ContainEquivalentOf(userName.Length == 0
            ? new Error("\'User Name\' must not be empty.")
            : new Error($"\'User Name\' must be between 3 and 20 characters. You entered {userName.Length} characters."));

        result.Errors.TestPasswordValidationResult(password, _options);
    }
    

    public static readonly TheoryData<LoginModel> validLoginModels =
        new()
        {
            new LoginModel("test", "1String!"),
            new LoginModel("ABC", "1String!"),
            new LoginModel("email1@gmail.com", "1String!")
        };
    [Theory, MemberData(nameof(validLoginModels))]
    public async void Login_Failed(LoginModel model)
    {
        var (userName, password) = model;
        var user = UserData.usersTable.FirstOrDefault(x => x.UserName == userName || x.Email == userName);
        _userManager.Setup(x => x.FindByNameAsync(userName))
            .ReturnsAsync(UserData.usersTable.FirstOrDefault(x => x.UserName == userName));
        _userManager.Setup(x => x.FindByEmailAsync(userName))
            .ReturnsAsync(UserData.usersTable.FirstOrDefault(x => x.Email == userName));
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
        result.Value.Should().BeEquivalentTo(UserData.tokens);
    }

    [Fact]
    public async void Login_UnableToCreateToken()
    {
        _tokenService.Setup(x => x.CreateTokens(It.IsAny<string>(), It.IsAny<string?>())).Returns(Result.Fail("Token error"));

        var result = await _authService.LoginAsync(new LoginModel("test", "1String!"));

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainEquivalentOf(new Error("Token error"));

    }

    public static readonly TheoryData<ConfirmEmailModel> confirmEmailModels =
        new()
        {
            new ConfirmEmailModel("0", UserData.ValidToken),
            new ConfirmEmailModel("0", UserData.InvalidToken),
            new ConfirmEmailModel("1", UserData.ValidToken),
            new ConfirmEmailModel("1", UserData.InvalidToken)
        };

    [Theory, MemberData(nameof(confirmEmailModels))]
    public async void ConfirmEmailAsync(ConfirmEmailModel model)
    {
        var (userId, token) = model;
        var user = UserData.usersTable.FirstOrDefault(x => x.Id == userId);
        _userManager
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);
        _userManager
            .Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), token))
            .ReturnsAsync(token == UserData.ValidToken ? IdentityResult.Success : IdentityResult.Failed());

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
        var user = UserData.usersTable.FirstOrDefault(x => x.Id == userId);
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
