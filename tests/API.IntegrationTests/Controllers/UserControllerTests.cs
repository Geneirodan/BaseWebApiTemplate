using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using API.Requests.Auth;
using Domain.Constants;
using Domain.Models;
using Domain.Models.Auth;
using Domain.Models.User;
using API.IntegrationTests.Extensions;

namespace API.IntegrationTests.Controllers;

public class UserControllerTests : IntegrationTest
{
    private static readonly RegisterRequest _request = new()
    {
        UserName = "string",
        Email = "test@mail.com",
        Password = "1String!"
    };

    private const string Url = "api/user";
    private const string? ApiUserCurrent = $"{Url}/current";

    [Fact]
    public async void Endpoints_RequireAuthentication()
    {
        var responses = new List<HttpResponseMessage>
        {
            await TestClient.GetAsync(Url),
            await TestClient.GetAsync(ApiUserCurrent),
            await TestClient.GetAsync(RequestUriWithNewId()),
            await TestClient.PostAsync(Url, _request.ToJson()),
            await TestClient.PatchAsync(RequestUriWithNewId(), _request.ToJson()),
            await TestClient.DeleteAsync(RequestUriWithNewId())
        };

        responses.Should().AllSatisfy(x => x.StatusCode.Should().Be(HttpStatusCode.Unauthorized));
    }
    private static string RequestUriWithNewId() => $"{Url}/{Guid.NewGuid()}";

    [Fact]
    public async void GetCurrentUser()
    {
        await AuthorizeAsAdmin();

        var response = await TestClient.GetAsync(ApiUserCurrent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var user = await response.AsContent<UserViewModel>();
        user?.UserName.Should().Be(nameof(Roles.Admin));
    }

    [Fact]
    public async void GetUserById_NotFound()
    {
        await AuthorizeAsAdmin();

        var response = await TestClient.GetAsync(RequestUriWithNewId());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var res = await response.AsContent<ValidationProblemDetails>();
        res?.Errors.Should().BeEmpty();
        res?.Status.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async void GetUserById()
    {
        await AuthorizeAsAdmin();

        var users = await RegisterTestUsers();
        var model = users.FirstOrDefault()!;

        var response = await TestClient.GetAsync($"{Url}/{model.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var (id, userName, email) = (await response.AsContent<UserViewModel>())!;
        id.Should().Be(model.Id);
        userName.Should().Be(model.UserName);
        email.Should().Be(model.Email);
    }

    [Fact]
    public async void GetAllUsers()
    {
        await AuthorizeAsAdmin();

        var testUsers = await RegisterTestUsers();

        var response = await TestClient.GetAsync(Url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.AsContent<PaginationModel<UserViewModel>>();
        var list = users?.List.ToList();
        foreach (var testUser in testUsers)
            list?.Should().ContainEquivalentOf(testUser);
        users?.PageCount.Should().Be(testUsers.Count + 1);
    }

    [Fact]
    public async void GetAllUsers_Filter()
    {
        await AuthorizeAsAdmin();

        var testUsers = await RegisterTestUsers();

        const string username = "Username0";

        var response = await TestClient.GetAsync($"{Url}?userName={username}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.AsContent<PaginationModel<UserViewModel>>();
        var list = users?.List.ToList();
        list.Should().HaveCount(1);
        list.Should().ContainEquivalentOf(testUsers.FirstOrDefault(x => x?.UserName == username));
    }

    [Fact]
    public async Task CreateUser()
    {
        await AuthorizeAsAdmin();

        var response = await TestClient.PostAsync(Url, _request.ToJson());

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var (id, userName, email) = (await response.AsContent<UserViewModel>())!;
        email.Should().Be(_request.Email);
        userName.Should().Be(_request.UserName);
    }

    [Fact]
    public async void CreateUser_UserAlreadyExists()
    {
        await AuthorizeAsAdmin();
        await CreateUser();
        
        var response = await TestClient.PostAsync(Url, _request.ToJson());

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var user = await response.AsContent<ValidationProblemDetails>();
        user?.Errors[string.Empty].Should().ContainEquivalentOf($"Username '{_request.UserName}' is already taken.");
    }

    [Fact]
    public async void PatchUser_ReturnsFail_WhenUserNotFound()
    {
        await AuthorizeAsAdmin();

        var response = await TestClient.PatchAsync(RequestUriWithNewId(), _request.ToJson());

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var details = await response.AsContent<ValidationProblemDetails>();
        details?.Errors[string.Empty].Should().ContainEquivalentOf(Errors.NotFound);
    }

    [Fact]
    public async void PatchUser()
    {
        await AuthorizeAsAdmin();

        var userId = await GetUserId();

        var patchModel = new UserPatchModel("PatchedUsername", "patched@mail.com");

        var patchResponse = await TestClient.PatchAsync($"{Url}/{userId}", patchModel.ToJson());

        patchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var (id, userName, email) = (await patchResponse.AsContent<UserViewModel>())!;

        id.Should().Be(userId);
        userName.Should().Be(patchModel.UserName);
        email.Should().Be(patchModel.Email);

        var userResponse = await TestClient.GetAsync($"{Url}/{userId}");

        userResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        (id, userName, email) = (await userResponse.AsContent<UserViewModel>())!;
        id.Should().Be(userId);
        userName.Should().Be(patchModel.UserName);
        email.Should().Be(patchModel.Email);
    }

    [Fact]
    public async void DeleteUser_ReturnsOk_WhenUserDeleted()
    {
        await AuthorizeAsAdmin();
        var userId = await GetUserId();

        var userResponse = await TestClient.GetAsync($"{Url}/{userId}");
        var deleteResponse = await TestClient.DeleteAsync($"{Url}/{userId}");
        var deletedUserResponse = await TestClient.GetAsync($"{Url}/{userId}");

        userResponse.IsSuccessStatusCode.Should().BeTrue();
        deleteResponse.IsSuccessStatusCode.Should().BeTrue();
        deletedUserResponse.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async void DeleteUser_ReturnsFail_WhenUserNotFound()
    {
        await AuthorizeAsAdmin();

        var response = await TestClient.DeleteAsync(RequestUriWithNewId());
        var details = await response.AsContent<ValidationProblemDetails>();

        response.IsSuccessStatusCode.Should().BeFalse();
        details?.Errors[string.Empty].Should().ContainEquivalentOf(Errors.NotFound);
    }

    private async Task<List<UserViewModel?>> RegisterTestUsers()
    {
        await AuthorizeAsAdmin();
        List<UserViewModel?> ids = [];
        for (var i = 0; i < 3; i++)
        {
            var request = new RegisterModel($"Username{i}", $"some{i}@mail.com", "1String!");
            var response = await TestClient.PostAsync(Url, request.ToJson());
            var item = await response.AsContent<UserViewModel>();
            ids.Add(item);
        }
        return ids;
    }
    
    private async Task<string> GetUserId()
    {
        await AuthorizeAsAdmin();

        var response = await TestClient.PostAsync(Url, _request.ToJson());
        var user = await response.AsContent<UserViewModel>();
        return user!.Id;
    }

}
