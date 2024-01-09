using BusinessLogic.Interfaces;
using BusinessLogic.Models.Auth;
using BusinessLogic.Models.User;
using BusinessLogic.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Requests.Auth;
using WebApi.Utils;

namespace WebApi.Controllers;

[Route(Routes.ApiControllerAction)]
public class AuthController(IAuthService authService, ITokenService tokenService) : BaseController
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Tokens>> LoginAsync([FromBody] LoginRequest request)
    {
        var model = request.Adapt<LoginModel>();
        var result = await authService.LoginAsync(model);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserViewModel>> RegisterAsync([FromBody] RegisterRequest request)
    {
        var model = request.Adapt<RegisterModel>();
        var result = await authService.RegisterAsync(model);
        return HandleCreatedResult(result);
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    [Authorize, HttpGet]
    public IActionResult Test() => Ok("Test");
}
