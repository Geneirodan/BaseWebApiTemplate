namespace BusinessLogic.Options;

public class GoogleAuthOptions
{
    public const string Section = "Authentication:Google";

    public string ClientId { get; init; } = null!;
}
