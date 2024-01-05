using BusinessLogic.Models.Interfaces;

namespace BusinessLogic.Models.PasswordRecovery;

public record ChangePasswordModel(string Id, string OldPassword, string NewPassword) : IPasswordModel
{
    public string Password => NewPassword;
}
