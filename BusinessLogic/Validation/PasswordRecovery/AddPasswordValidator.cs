using BusinessLogic.Models.PasswordRecovery;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BusinessLogic.Validation.PasswordRecovery;

public class AddPasswordValidator(IOptions<IdentityOptions> options) : Abstractions.PasswordValidator<AddPasswordModel>(options);