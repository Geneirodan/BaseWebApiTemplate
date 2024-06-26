using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using API.Requests.Auth;
using Domain.Constants;
using Domain.Models.Auth;
using Domain.Models.User;
using API.IntegrationTests.Extensions;

// ReSharper disable UseCollectionExpression

namespace API.IntegrationTests.Controllers;

public class AuthControllerTests : IntegrationTest
{
    private static readonly RegisterRequest _registerRequest = new()
    {
        UserName = "string",
        Email = "test@mail.com",
        Password = "1String!"
    };

    private static readonly LoginRequest _validLoginRequest = new() { UserName = _registerRequest.UserName, Password = _registerRequest.Password };
    
    public static readonly TheoryData<LoginRequest> validLoginRequests = new()
    {
        _validLoginRequest,
        new LoginRequest { UserName = _registerRequest.Email, Password = _registerRequest.Password }
    };


    private const string Url = "api/v1";

    [Fact]
    public async Task Register()
    {
        var response = await TestClient.PostAsync($"{Url}/register", _registerRequest.ToJson());

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var (_, userName, email) = (await response.AsContent<UserViewModel>())!;
        email.Should().Be(_registerRequest.Email);
        userName.Should().Be(_registerRequest.UserName);
    }

    [Fact]
    public async void Register_InvalidPassword()
    {
        var request = new RegisterRequest
        {
            UserName = "string",
            Email = "test@mail.com",
            Password = "1"
        };
        var response = await TestClient.PostAsync($"{Url}/register", request.ToJson());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var details = await response.AsContent<ValidationProblemDetails>();
        details?.Errors.ContainsKey(nameof(request.Password)).Should().BeTrue();
        var passwordErrors = details?.Errors[nameof(request.Password)];
        passwordErrors?.Should().ContainEquivalentOf(PasswordValidationErrors.RequireLowercase);
        passwordErrors?.Should().ContainEquivalentOf(PasswordValidationErrors.RequireUppercase);
        passwordErrors?.Should().ContainEquivalentOf(PasswordValidationErrors.RequireNonAlphanumeric);
    }
    [Theory, MemberData(nameof(validLoginRequests))]
    public async Task Login(LoginRequest request)
    {
        await Register();

        var response = await TestClient.PostAsync($"{Url}/login", request.ToJson());

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tokens = await response.AsContent<Tokens>();
        var (accessToken, refreshToken) = tokens;
        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async void Login_Invalid()
    {
        await Register();

        var model = new LoginRequest { UserName = "invalid@email.com", Password = "invalid password" };
        var response = await TestClient.PostAsync($"{Url}/login", model.ToJson());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var details = await response.AsContent<ValidationProblemDetails>();
        details?.Errors.ContainsKey(string.Empty).Should().BeTrue();
        details?.Errors[string.Empty].Should().Contain(Errors.NotFound);
    }
    
    [Fact]
    public async void Refresh()
    {
        await Register();
        
        var loginResponse = await TestClient.PostAsync($"{Url}/login", _validLoginRequest.ToJson());
        var tokens = await loginResponse.AsContent<Tokens>();
        
        var response = await TestClient.PostAsync($"{Url}/RefreshTokens", tokens.ToJson());
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newTokens = await response.AsContent<Tokens>();
        var (accessToken, refreshToken) = newTokens;
        accessToken.Should().NotBeNullOrEmpty();
        refreshToken.Should().NotBeNullOrEmpty();
        accessToken.Should().NotBeEquivalentTo(tokens.AccessToken);
        refreshToken.Should().NotBeEquivalentTo(tokens.RefreshToken);
        
    }

}
