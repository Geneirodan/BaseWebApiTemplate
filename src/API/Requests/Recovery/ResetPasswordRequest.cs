using System.ComponentModel.DataAnnotations;

namespace API.Requests.Recovery;

public sealed record ResetPasswordRequest
{
    [Required, DataType(DataType.EmailAddress)]
    public string Email { get; init; } = null!;
    [Required, DataType(DataType.Password)]
    public string Password { get; init; } = null!;
    [Required]
    public string Token { get; init; } = null!;
}