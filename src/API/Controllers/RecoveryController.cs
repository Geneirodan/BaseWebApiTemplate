using API.Requests.Recovery;
using API.Utils;
using Domain.Interfaces;
using Domain.Models.PasswordRecovery;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route(Routes.ApiControllerAction)]
public class RecoveryController(IRecoveryService recoveryService) : BaseController
{

    [HttpPost]
    public async Task<ActionResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var model = request.Adapt<ResetPasswordModel>();
        var result = await recoveryService.ResetPasswordAsync(model);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> AddPasswordAsync(AddPasswordRequest request)
    {
        var model = request.Adapt<AddPasswordModel>() with { Id = GetUserId()! };
        var result = await recoveryService.AddPasswordAsync(model);
        return HandleResult(result);
    }
    [HttpPost]
    [Authorize]
    public async Task<ActionResult> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var model = request.Adapt<ChangePasswordModel>() with { Id = GetUserId()! };
        var result = await recoveryService.ChangePasswordAsync(model);
        return HandleResult(result);
    }
}
