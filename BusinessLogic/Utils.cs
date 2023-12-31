using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace BusinessLogic;

public static class Utils
{
    public static Result ToFluentResult(this IdentityResult result) => 
        result.Succeeded 
            ? Result.Ok() 
            : Result.Fail(result.Errors.Select(x => x.Description));
}
