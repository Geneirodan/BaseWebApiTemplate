namespace BusinessLogic.Options;

public class AdminOptions
{
    public const string Section = "Admin";
    
    public string Email { get; init; } = null!;

    public string Password { get; init; } = null!;
}
