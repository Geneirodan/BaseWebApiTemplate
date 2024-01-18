using Domain.Extensions;
using Domain.Models.PasswordRecovery;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Domain.Validation.PasswordRecovery;

public class ChangePasswordValidator : AbstractValidator<ChangePasswordModel>
{
    public ChangePasswordValidator(IOptions<IdentityOptions> options) => 
        RuleFor(x => x.NewPassword).IsValidPassword(options.Value.Password);
}
