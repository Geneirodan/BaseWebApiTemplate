using BusinessLogic.Validation.Password;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;

namespace BusinessLogic.Validation.Extensions;

public static class ValidationExtensions
{
    public static Result ToFluentResult(this ValidationResult result)
    {
        List<string> errors = [];
        if (!result.IsValid)
            result.Errors.ForEach(e => errors.Add(e.ErrorMessage));
        return errors.Count > 0
            ? Result.Fail(errors)
            : Result.Ok();
    }
    public static IRuleBuilderOptions<T, string> IsValidUsername<T>(this IRuleBuilderInitial<T, string> ruleBuilder) =>
        ruleBuilder.Cascade(CascadeMode.Stop).NotEmpty().Length(3, 20);
    public static IRuleBuilderOptions<T, string> IsValidEmail<T>(this IRuleBuilderInitial<T, string> ruleBuilder) =>
        ruleBuilder.Cascade(CascadeMode.Stop).NotEmpty().EmailAddress();

    public static IRuleBuilderOptions<T, string> IsValidPassword<T>(this IRuleBuilderInitial<T, string> ruleBuilder, PasswordRules rules, int requiredLength)
    {

        var builderOptions = ruleBuilder.NotEmpty();

        if (rules.HasFlag(PasswordRules.RequireDigit))
            builderOptions
                .Must(x => x.Any(IsDigit))
                .WithMessage(PasswordValidationErrors.RequireDigit);

        if (rules.HasFlag(PasswordRules.RequireLowercase))
            builderOptions
                .Must(x => x.Any(IsLower))
                .WithMessage(PasswordValidationErrors.RequireLowercase);

        if (rules.HasFlag(PasswordRules.RequireUppercase))
            builderOptions
                .Must(x => x.Any(IsUpper))
                .WithMessage(PasswordValidationErrors.RequireUppercase);

        if (rules.HasFlag(PasswordRules.RequireNonAlphanumeric))
            builderOptions
                .Must(x => !x.All(IsLetterOrDigit))
                .WithMessage(PasswordValidationErrors.RequireNonAlphanumeric);


        return builderOptions.MinimumLength(requiredLength).WithMessage(PasswordValidationErrors.PasswordLength);
    }

    public static IRuleBuilderOptions<T, string> IsValidPassword<T>(this IRuleBuilderInitial<T, string> ruleBuilder, PasswordOptions passwordOptions) =>
        ruleBuilder.IsValidPassword(GetPasswordRules(passwordOptions, out var length), length);

    public static PasswordRules GetPasswordRules(PasswordOptions passwordOptions, out int passwordLength)
    {
        passwordLength = passwordOptions.RequiredLength;
        var rules = PasswordRules.None;
        if (passwordOptions.RequireDigit)
            rules |= PasswordRules.RequireDigit;
        if (passwordOptions.RequireLowercase)
            rules |= PasswordRules.RequireLowercase;
        if (passwordOptions.RequireUppercase)
            rules |= PasswordRules.RequireUppercase;
        if (passwordOptions.RequireNonAlphanumeric)
            rules |= PasswordRules.RequireNonAlphanumeric;
        return rules;
    }
    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    private static bool IsLower(char c) => c is >= 'a' and <= 'z';

    private static bool IsUpper(char c) => c is >= 'A' and <= 'Z';

    private static bool IsLetterOrDigit(char c) => IsUpper(c) || IsLower(c) || IsDigit(c);
}
