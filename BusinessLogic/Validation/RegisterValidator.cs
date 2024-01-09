using BusinessLogic.Extensions;
using BusinessLogic.Models.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Validation;

public class RegisterValidator : AbstractValidator<RegisterModel>
{
    public RegisterValidator(IOptions<IdentityOptions> options)
    {
        RuleFor(x => x.Email).IsValidEmail().MaximumLength(byte.MaxValue);
        RuleFor(x => x.UserName).IsValidUsername();
        RuleFor(x => x.Password).IsValidPassword(options.Value.Password);
    }
}
