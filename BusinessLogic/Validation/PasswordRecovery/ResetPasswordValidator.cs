using BusinessLogic.Models.PasswordRecovery;
using BusinessLogic.Validation.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Validation.PasswordRecovery;

public class ResetPasswordValidator : Abstractions.PasswordValidator<ResetPasswordModel>
{
    public ResetPasswordValidator(IOptions<IdentityOptions> options) : base(options) => RuleFor(x => x.Email).IsValidEmail();
}