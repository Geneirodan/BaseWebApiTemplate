using BusinessLogic.Models.Interfaces;
using BusinessLogic.Validation.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Validation.Abstractions;

public abstract class PasswordValidator<TPasswordModel> : AbstractValidator<TPasswordModel>
    where TPasswordModel : IPasswordModel
{
    protected PasswordValidator(IOptions<IdentityOptions> options) => RuleFor(x => x.Password).IsValidPassword(options.Value.Password);
}
