using Domain.Extensions;
using Domain.Models.User;
using FluentValidation;

namespace Domain.Validation;

public class PatchValidator : AbstractValidator<UserPatchModel>
{
    public PatchValidator()
    {
        RuleFor(x => x.Email).IsValidEmail();
        RuleFor(x => x.UserName).IsValidUsername();
    }
}
