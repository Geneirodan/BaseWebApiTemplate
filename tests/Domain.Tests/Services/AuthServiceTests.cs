using Domain.Constants;
using Domain.Interfaces;
using Domain.Models.Auth;
using Domain.Models.User;
using Domain.Options;
using Domain.Services;
using Domain.Tests.Data;
using FluentAssertions;
using FluentResults;
using Geneirodan.Generics.CrudService.Constants;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

// ReSharper disable UseCollectionExpression

namespace Domain.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthService _authService;
    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<SignInManager<User>> _signInManager;
    private readonly Mock<IMailService> _mailService;

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

        Mock<ITokenService> tokenService = new();

        tokenService.Setup(x => x.CreateTokensAsync(It.IsAny<User>())).ReturnsAsync(UserData.tokens);

        _mailService = new Mock<IMailService>();

        _mailService
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Ok());

        Mock<IUserService> userService = new();

        userService
            .Setup(x => x.RegisterUserAsync(UserServiceTests.validRegisterModel, Roles.User))
            .ReturnsAsync(Result.Ok(UserData.sampleUser.Adapt<UserViewModel>()));

        var googleOptions = new Mock<IOptions<GoogleAuthOptions>>();
        googleOptions.Setup(o => o.Value).Returns(new GoogleAuthOptions());

        var userRepository = new Mock<IUserRepository>();
        _authService = new AuthService(
            _userManager.Object,
            _signInManager.Object,
            tokenService.Object,
            _mailService.Object,
            googleOptions.Object,
            userRepository.Object,
            userService.Object);
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

    public static readonly TheoryData<string, string> confirmEmailModels =
        new()
        {
            { "0", UserData.ValidToken },
            { "0", UserData.InvalidToken },
            { "1", UserData.ValidToken },
            { "1", UserData.InvalidToken }
        };

    [Theory, MemberData(nameof(confirmEmailModels))]
    public async void ConfirmEmailAsync(string userId, string token)
    {
        var user = UserData.usersTable.FirstOrDefault(x => x.Id == userId);
        _userManager
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);
        _userManager
            .Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), token))
            .ReturnsAsync(token == UserData.ValidToken ? IdentityResult.Success : IdentityResult.Failed());

        var result = await _authService.ConfirmEmailAsync(userId, token);

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
