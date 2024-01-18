namespace Domain.Models.PasswordRecovery;

public record ResetPasswordModel(string Email, string Password, string Token);
