namespace BusinessLogic.Validation.Password;

public static class PasswordValidationErrors
{
    public static string RequireDigit => "Password must contain a digit";
    public static string RequireLowercase => "Password must contain a lower case ASCII character";
    public static string RequireUppercase => "Password must contain a upper case ASCII character";
    public static string RequireNonAlphanumeric => "Password must contain a non-alphanumeric character";
}
