using BusinessLogic.Models;
using BusinessLogic.Validation.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Validation;

public class LoginValidator : AbstractValidator<LoginModel>
{
    public LoginValidator(IOptions<IdentityOptions> options)
    {
        RuleFor(x => x.UserName).IsValidUsername();
        RuleFor(x => x.Password).IsValidPassword(options.Value.Password);
    }
}