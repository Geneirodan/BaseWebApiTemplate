using BusinessLogic;
using BusinessLogic.Interfaces;
using BusinessLogic.Models;
using BusinessLogic.Models.Auth;
using BusinessLogic.Models.Filters;
using BusinessLogic.Models.User;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Utils;

namespace WebApi.Controllers;

[Route(Routes.ApiController)]
public class UserController(IUserService userService) : BaseController
{
    [HttpGet("current")]
    [Authorize]
    public async Task<ActionResult<UserViewModel>> GetCurrentUser()
    {
        var user = await userService.GetUserByIdAsync(GetUserId() ?? string.Empty);
        return user is not null ? user : NotFound();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserViewModel?>> GetUserById(string id)
    {
        var user = await userService.GetUserByIdAsync(id);
        return user is not null ? user : NotFound();
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<PaginationModel<UserViewModel>>> GetAllUsers([FromQuery] UserFilter filter)
    {
        var result = await userService.GetUsersAsync(filter);

        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserViewModel>> CreateUser([FromBody] RegisterModel model, [FromQuery] string role = Roles.User)
    {
        var result = await userService.RegisterUserAsync(model, role);

        return HandleCreatedResult(result);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<UserViewModel>> PatchUser(string id, [FromBody] UserPatchModel model)
    {
        var result = await userService.PatchUserAsync(id, model);

        return HandleResult(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var result = await userService.DeleteUserAsync(id);

        return HandleResult(result);
    }
}
