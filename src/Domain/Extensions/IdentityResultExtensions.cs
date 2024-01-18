using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace Domain.Extensions;

public static class IdentityResultExtensions
{
    public static Result ToFluentResult(this IdentityResult result) => 
        result.Succeeded 
            ? Result.Ok() 
            : Result.Fail(result.Errors.Select(x => x.Description));
}