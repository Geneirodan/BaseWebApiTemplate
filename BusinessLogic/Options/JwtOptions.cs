using System.ComponentModel.DataAnnotations;

namespace BusinessLogic.Options;

public class JwtOptions
{
    public const string Section = "Authentication:Jwt";

    private const int MinimumHmac256KeyLength = 16;

    [Required]
    [StringLength(100, MinimumLength = MinimumHmac256KeyLength,
        ErrorMessage = "Minimum length of the key is 16 characters")]
    public string Key { get; init; } = null!;

    [Required]
    [Range(1, 1000, ErrorMessage = "Lifetime minutes should be between 1 and 1000 inclusive")]
    public int LifetimeMinutes { get; init; }
}
