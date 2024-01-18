using System.ComponentModel.DataAnnotations;

namespace API.Requests.Auth;

public sealed record LoginRequest
{
    [Required]
    public string UserName { get; init; } = null!;
    [Required, DataType(DataType.Password)]
    public string Password { get; init; } = null!;
}
