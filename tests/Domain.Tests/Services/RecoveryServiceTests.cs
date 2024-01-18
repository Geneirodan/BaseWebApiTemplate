using Domain.Constants;
using Domain.Interfaces;
using Domain.Models.PasswordRecovery;
using Domain.Services;
using Domain.Tests.Data;
using Domain.Tests.Extensions;
using FluentAssertions;
using FluentResults;
using Infrastructure.Entities;
using Infrastructure.Interfaces;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Moq;

// ReSharper disable UseCollectionExpression

namespace Domain.Tests.Services;

[TestSubject(typeof(RecoveryService))]
public class RecoveryServiceTests
{
    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<IUserRepository> _repository;
    private readonly RecoveryService _recoveryService;
    private readonly IdentityOptions _options;

    public RecoveryServiceTests()
    {
        _userManager = MockHelpers.TestUserManager<User>();

        _userManager
            .Setup(x => x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(UserData.sampleUser);

        _userManager
            .Setup(x => x.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.AddPasswordAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.ChangePasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _repository = new Mock<IUserRepository>();


        _repository
            .Setup(x => x.GetAsync(It.IsAny<string>()))
            .ReturnsAsync(UserData.sampleUser);

        var options = MockHelpers.TestIdentityOptions().Object;
        _options = options.Value;
        Mock<IMailService> mailService = new();

        mailService
            .Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(Result.Ok());

        _recoveryService = new RecoveryService(_userManager.Object, _repository.Object, options, mailService.Object);
    }

    public static readonly TheoryData<ResetPasswordModel> validResetPasswordModels =
        new()
        {
            new ResetPasswordModel("sample@gmail.com", "1String!", UserData.ValidToken),
            new ResetPasswordModel("email1@gmail.com", "1String!", UserData.ValidToken),
            new ResetPasswordModel("email1@gmail.com", "1String!", UserData.InvalidToken)
        };

    [Theory, MemberData(nameof(validResetPasswordModels))]
    public async Task ResetPasswordAsync(ResetPasswordModel model)
    {
        var (email, password, token) = model;
        var user = UserData.usersTable.FirstOrDefault(x => x.Email == email);
        _userManager
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.ResetPasswordAsync(It.Is<User>(y => y.Email == email), UserData.ValidToken, password))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.ResetPasswordAsync(It.Is<User>(y => y.Email == email), UserData.InvalidToken, password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid token" }));

        var result = await _recoveryService.ResetPasswordAsync(model);

        if (user is null)
        {
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainEquivalentOf(new Error(Errors.NotFound));
        }
        else if (token == UserData.InvalidToken)
        {
           result.IsSuccess.Should().BeFalse();
           result.Errors.Should().ContainEquivalentOf(new Error("Invalid token"));
        }
        else
            result.IsSuccess.Should().BeTrue();
    }

    public static readonly TheoryData<ResetPasswordModel> invalidResetPasswordModels =
        new()
        {
            new ResetPasswordModel("sample@", "String!", UserData.ValidToken),
            new ResetPasswordModel("@gmail", "1", UserData.ValidToken),
            new ResetPasswordModel("email1gmail.com", "1String", UserData.InvalidToken)
        };

    [Theory, MemberData(nameof(invalidResetPasswordModels))]
    public async Task ResetPasswordAsync_ModelNotValid(ResetPasswordModel model)
    {

        var (email, password, _) = model;

        var result = await _recoveryService.ResetPasswordAsync(model);

        result.IsSuccess.Should().BeFalse();
        result.Errors.TestEmailValidation(email);
        result.Errors.TestPasswordValidationResult(password, _options);
    }

    public static readonly TheoryData<AddPasswordModel> validAddPasswordModels =
        new()
        {
            new AddPasswordModel("0", "1String!"),
            new AddPasswordModel("1", "1String!"),
            new AddPasswordModel("2", "1String!")
        };

    [Theory, MemberData(nameof(validAddPasswordModels))]
    public async Task AddPasswordAsync(AddPasswordModel model)
    {

        var (id, password) = model;
        var user = UserData.usersTable.FirstOrDefault(x => x.Id == id);
        _repository
            .Setup(x => x.GetAsync(id))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.AddPasswordAsync(It.Is<User>(y => y.Id == id && y.PasswordHash == null), password))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.AddPasswordAsync(It.Is<User>(y => y.Id == id), password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password already set" }));

        var result = await _recoveryService.AddPasswordAsync(model);

        if (user is null)
        {
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainEquivalentOf(new Error(Errors.NotFound));
        }
        else if (user.PasswordHash is not null)
        {
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainEquivalentOf(new Error("Password already set"));
        }
        else
            result.IsSuccess.Should().BeTrue();
    }

    public static readonly TheoryData<AddPasswordModel> invalidAddPasswordModels =
        new()
        {
            new AddPasswordModel("0", "1String"),
            new AddPasswordModel("1", "1"),
            new AddPasswordModel("2", "String!")
        };

    [Theory, MemberData(nameof(invalidAddPasswordModels))]
    public async Task AddPasswordAsync_ModelNotValid(AddPasswordModel model)
    {

        var (_, password) = model;
        var result = await _recoveryService.AddPasswordAsync(model);

        result.IsSuccess.Should().BeFalse();
        result.Errors.TestPasswordValidationResult(password, _options);
    }

    public static readonly TheoryData<ChangePasswordModel> validChangePasswordModels =
        new()
        {
            new ChangePasswordModel("0", UserData.ValidToken, "1String!"),
            new ChangePasswordModel("1", UserData.ValidToken, "1String!"),
            new ChangePasswordModel("2", UserData.InvalidToken, "1String!")
        };

    [Theory, MemberData(nameof(validChangePasswordModels))]
    public async Task ChangePasswordAsync(ChangePasswordModel model)
    {
        var (id, oldPassword, newPassword) = model;
        var user = UserData.usersTable.FirstOrDefault(x => x.Id == id);
        _repository
            .Setup(x => x.GetAsync(id))
            .ReturnsAsync(user);

        _userManager
            .Setup(x => x.ChangePasswordAsync(It.Is<User>(y => y.Id == id), UserData.ValidToken, newPassword))
            .ReturnsAsync(IdentityResult.Success);

        _userManager
            .Setup(x => x.ChangePasswordAsync(It.Is<User>(y => y.Id == id), UserData.InvalidToken, newPassword))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Invalid old password" }));

        var result = await _recoveryService.ChangePasswordAsync(model);

        if (user is null)
        {
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainEquivalentOf(new Error(Errors.NotFound));
        }
        else if (oldPassword == UserData.InvalidToken)
        {
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainEquivalentOf(new Error("Invalid old password"));
        }
        else
            result.IsSuccess.Should().BeTrue();
    }

    public static readonly TheoryData<ChangePasswordModel> invalidChangePasswordModels =
        new()
        {
            new ChangePasswordModel("0", "Any", "1String"),
            new ChangePasswordModel("1", "Any", "1"),
            new ChangePasswordModel("2", "Any", "String!")
        };

    [Theory, MemberData(nameof(invalidChangePasswordModels))]
    public async Task ChangePasswordAsync_ModelNotValid(ChangePasswordModel model)
    {

        var result = await _recoveryService.ChangePasswordAsync(model);

        result.IsSuccess.Should().BeFalse();
        result.Errors.TestPasswordValidationResult(model.NewPassword, _options, "NewPassword");
    }
}
