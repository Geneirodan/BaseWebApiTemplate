using FluentResults;
using Geneirodan.Generics.CrudService.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Claims;
// ReSharper disable MemberCanBePrivate.Global

namespace API.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected ActionResult<T> HandleResult<T>(Result<T> result) => result.IsSuccess ? result.ValueOrDefault : Error(result);
    protected ActionResult<T> HandleCreatedResult<T>(Result<T> result) => result.IsSuccess ? Created(result.ValueOrDefault) : Error(result);
    protected ActionResult HandleResult(Result result) => result.IsSuccess ? Ok() : Error(result);
    private ActionResult Error(IResultBase result)
    {
        return result.Errors[0].Message switch
        {
            Errors.NotFound => NotFound(result.Errors),
            Errors.Forbidden => Forbid(result.Errors),
            _ => BadRequest(result.Errors)
        };
    }
    protected ActionResult BadRequest(IEnumerable<IError> errors) => Error(errors, StatusCodes.Status400BadRequest);
    protected ActionResult Forbid(IEnumerable<IError> errors) => Error(errors, StatusCodes.Status403Forbidden);
    protected ActionResult NotFound(IEnumerable<IError> errors) => Error(errors, StatusCodes.Status404NotFound);
    protected new ActionResult BadRequest() => Error([], StatusCodes.Status400BadRequest);
    protected new ActionResult Forbid() => Error([], StatusCodes.Status403Forbidden);
    protected new ActionResult NotFound() => Error([], StatusCodes.Status404NotFound);

    private ActionResult<T> Created<T>(T value) => CreatedAtAction(null, value);
    private ActionResult Error(IEnumerable<IError> errors, int statusCode)
    {
        ModelStateDictionary pairs = new();
        foreach (var error in errors)
            if (error.Reasons.Count > 0)
                foreach (var errorReason in error.Reasons)
                    pairs.AddModelError(error.Message, errorReason.Message);
            else
                pairs.AddModelError(string.Empty, error.Message);
        return ValidationProblem(statusCode: statusCode, modelStateDictionary: pairs);
    }
    protected string? GetUserId() => User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
}
