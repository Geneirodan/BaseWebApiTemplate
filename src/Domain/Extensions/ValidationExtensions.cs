using Domain.Constants;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
// ReSharper disable UnusedMethodReturnValue.Global

namespace Domain.Extensions;

public static class ValidationExtensions
{
    public static Result ToFluentResult(this ValidationResult result)
    {
        List<Error> errors = [];
        if (!result.IsValid)
        {
            errors = result.Errors.GroupBy(x => x.PropertyName, (s, failures) =>
            {
                var e = new Error(s);
                e.Reasons.AddRange(failures.Select(x => new Error(x.ErrorMessage)));
                return e;
            }).ToList();
            // foreach (var e in result.Errors)
            // {
            //     var error = new Error(e.PropertyName);
            //     error.Reasons.Add(new Error(e.ErrorMessage));
            //     errors.Add(error);
            // }
        }
        return errors.Count > 0
            ? Result.Fail(errors)
            : Result.Ok();
    }
    public static IRuleBuilderOptions<T, string> IsValidUsername<T>(this IRuleBuilderInitial<T, string> ruleBuilder) =>
        ruleBuilder.Cascade(CascadeMode.Stop).NotEmpty().Length(3, 20);
    public static IRuleBuilderOptions<T, string> IsValidEmail<T>(this IRuleBuilderInitial<T, string> ruleBuilder) =>
        ruleBuilder.Cascade(CascadeMode.Stop).NotEmpty().EmailAddress();

    public static IRuleBuilderOptions<T, string> IsValidPassword<T>(this IRuleBuilderInitial<T, string> ruleBuilder, PasswordOptions passwordOptions)
    {
        var builderOptions = ruleBuilder.NotEmpty();

        if (passwordOptions.RequireDigit)
            builderOptions
                .Must(x => x.Any(c => c is >= '0' and <= '9'))
                .WithMessage(PasswordValidationErrors.RequireDigit);

        if (passwordOptions.RequireLowercase)
            builderOptions
                .Must(x => x.Any(c => c is >= 'a' and <= 'z'))
                .WithMessage(PasswordValidationErrors.RequireLowercase);

        if (passwordOptions.RequireUppercase)
            builderOptions
                .Must(x => x.Any(c => c is >= 'A' and <= 'Z'))
                .WithMessage(PasswordValidationErrors.RequireUppercase);

        if (passwordOptions.RequireNonAlphanumeric)
            builderOptions
                .Must(x => !x.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9'))
                .WithMessage(PasswordValidationErrors.RequireNonAlphanumeric);


        return builderOptions.MinimumLength(passwordOptions.RequiredLength);
    }
}
