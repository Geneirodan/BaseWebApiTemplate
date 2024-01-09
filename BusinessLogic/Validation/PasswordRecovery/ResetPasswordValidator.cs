using BusinessLogic.Extensions;
using BusinessLogic.Models.PasswordRecovery;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Validation.PasswordRecovery;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordModel>
{
    public ResetPasswordValidator(IOptions<IdentityOptions> options)
    {
        RuleFor(x => x.Email).IsValidEmail();
        RuleFor(x => x.Password).IsValidPassword(options.Value.Password);
    }
}