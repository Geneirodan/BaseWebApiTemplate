using BusinessLogic.Models;
using BusinessLogic.Validation.Extensions;
using BusinessLogic.Validation.Password;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Validation;

public class UserCreateValidator : AbstractValidator<UserCreateModel>
{
    public UserCreateValidator(IOptions<IdentityOptions> options)
    {
        RuleFor(x => x.Email).IsValidEmail();
        RuleFor(x => x.UserName).IsValidUsername();
        RuleFor(x => x.Password).IsValidPassword(options.Value.Password);
    }
}
