namespace Domain.Models.PasswordRecovery;

public record ChangePasswordModel(string Id, string OldPassword, string NewPassword);
