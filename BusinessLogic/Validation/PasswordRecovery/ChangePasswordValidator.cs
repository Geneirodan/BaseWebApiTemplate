using BusinessLogic.Models.PasswordRecovery;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Validation.PasswordRecovery;

public class ChangePasswordValidator(IOptions<IdentityOptions> options) : Abstractions.PasswordValidator<ChangePasswordModel>(options);
