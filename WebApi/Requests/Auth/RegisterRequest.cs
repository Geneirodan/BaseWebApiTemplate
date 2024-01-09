using System.ComponentModel.DataAnnotations;

namespace WebApi.Requests.Auth;

public record RegisterRequest
{
    [Required]
    public string UserName { get; init; } = null!;
    
    [Required, EmailAddress]
    public string Email { get; init; } = null!;
    
    [Required, DataType(DataType.Password)]
    public string Password { get; init; } = null!;
}
