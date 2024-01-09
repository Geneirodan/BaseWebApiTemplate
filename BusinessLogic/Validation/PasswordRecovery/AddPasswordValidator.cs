using BusinessLogic.Extensions;
using BusinessLogic.Models.PasswordRecovery;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Validation.PasswordRecovery;

public class AddPasswordValidator : AbstractValidator<AddPasswordModel>
{
    public AddPasswordValidator(IOptions<IdentityOptions> options) => 
        RuleFor(x => x.Password).IsValidPassword(options.Value.Password);
}