namespace BusinessLogic.Validation.Password;

[Flags]
public enum PasswordRules
{
    None = 0,
    RequireDigit = 1,
    RequireLowercase = 2,
    RequireUppercase = 4,
    RequireNonAlphanumeric = 8
}
