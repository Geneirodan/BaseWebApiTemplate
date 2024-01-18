using API.Requests.Auth;
using API.Utils;
using Domain.Interfaces;
using Domain.Models.Auth;
using Domain.Models.User;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route(Routes.ApiControllerAction)]
public class AuthController(IAuthService authService, ITokenService tokenService) : BaseController
{
    [HttpPost]
    public async Task<ActionResult<Tokens>> LoginAsync([FromBody] LoginRequest request)
    {
        var model = request.Adapt<LoginModel>();
        var result = await authService.LoginAsync(model);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserViewModel>> RegisterAsync([FromBody] RegisterRequest request)
    {
        var model = request.Adapt<RegisterModel>();
        var result = await authService.RegisterAsync(model);
        return HandleCreatedResult(result);
    }
    
    [HttpGet]
    public async Task<IActionResult> ConfirmEmailAsync(string id, string token)
    {
        var result = await authService.ConfirmEmailAsync(id, token);
        return HandleResult(result);

    }

    [HttpPost]
    public async Task<IActionResult> SendEmailConfirmationAsync(string userId, string confirmUrl, string callbackUrl)
    {
        var result = await authService.SendEmailConfirmationAsync(userId, confirmUrl, callbackUrl);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<Tokens>> GoogleLogin([FromBody] string token)
    {
        var result = await authService.GoogleLogin(token);
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<Tokens>> RefreshTokens([FromBody] Tokens tokens)
    {
        var result = await tokenService.RefreshAsync(tokens);
        return HandleResult(result);
    }
}
