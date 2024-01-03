using BusinessLogic.Models;
using BusinessLogic.Validation.Extensions;
using FluentValidation;

namespace BusinessLogic.Validation;

public class PatchValidator : AbstractValidator<UserPatchModel>
{
    public PatchValidator()
    {
        RuleFor(x => x.Email).IsValidEmail();
        RuleFor(x => x.UserName).IsValidUsername();
    }
}