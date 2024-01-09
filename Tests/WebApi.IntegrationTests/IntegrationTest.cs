using BusinessLogic.Models.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WebApi.IntegrationTests.Extensions;
namespace WebApi.IntegrationTests;

/// <summary>
/// Inherit from this class to create an integration test
/// </summary>
[Collection("Sequential")]
public class IntegrationTest
{
    protected IntegrationTest()
    {
        WebApi = new WebApiApplication();
        TestClient = WebApi.CreateClient();
    }

    private WebApiApplication WebApi { get; }

    protected HttpClient TestClient { get; }

    private async Task Authorize(string username, string password)
    {
        var response = await TestClient.PostAsync("api/auth/login", new { username, password }.ToJson());

        var tokens = await response.AsContent<Tokens>();

        TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, tokens.AccessToken);
    }

    protected Task AuthorizeAsAdmin() => Authorize("Admin", "1String!");

    //protected static JsonContent GetJsonContent(object @object) => JsonContent.Create(@object);
}