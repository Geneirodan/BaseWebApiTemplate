using BusinessLogic.Validation.Password;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Identity;

namespace BusinessLogin.Tests.Extensions;

public static class ResultErrorsExtensions
{
    internal static void TestUsernameValidation(this IEnumerable<IError> errors, string userName)
    {

        errors.Should().ContainEquivalentOf(userName.Length == 0
            ? new Error("\'User Name\' must not be empty.")
            : new Error($"\'User Name\' must be between 3 and 20 characters. You entered {userName.Length} characters."));
    }
    internal static void TestEmailValidation(this IEnumerable<IError> errors, string email)
    {

        errors.Should().ContainEquivalentOf(email.Length == 0
            ? new Error("\'Email\' must not be empty.")
            : new Error("\'Email\' is not a valid email address."));
    }
    internal static void TestPasswordValidationResult(this List<IError> errors, string password, IdentityOptions identityOptions)
    {

        if (password.Length == 0)
            errors.Should().ContainEquivalentOf(new Error("\'Password\' must not be empty."));
        else
        {
            if (identityOptions.Password.RequireDigit && !password.Any(IsDigit))
                errors.Should().ContainEquivalentOf(new Error(PasswordValidationErrors.RequireDigit));

            if (identityOptions.Password.RequireLowercase && !password.Any(IsLower))
                errors.Should().ContainEquivalentOf(new Error(PasswordValidationErrors.RequireLowercase));

            if (identityOptions.Password.RequireUppercase && !password.Any(IsUpper))
                errors.Should().ContainEquivalentOf(new Error(PasswordValidationErrors.RequireUppercase));

            if (identityOptions.Password.RequireNonAlphanumeric && password.All(IsLetterOrDigit))
                errors.Should().ContainEquivalentOf(new Error(PasswordValidationErrors.RequireNonAlphanumeric));
        }
    }
    
    private static bool IsDigit(char c) => c is >= '0' and <= '9';

    private static bool IsLower(char c) => c is >= 'a' and <= 'z';

    private static bool IsUpper(char c) => c is >= 'A' and <= 'Z';

    private static bool IsLetterOrDigit(char c) => IsUpper(c) || IsLower(c) || IsDigit(c);
}
