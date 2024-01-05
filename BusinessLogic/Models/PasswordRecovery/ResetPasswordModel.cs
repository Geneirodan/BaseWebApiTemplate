using BusinessLogic.Models.Interfaces;

namespace BusinessLogic.Models.PasswordRecovery;

public record ResetPasswordModel(string Email, string Password, string Token) : IPasswordModel;
