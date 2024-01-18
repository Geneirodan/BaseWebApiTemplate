using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Entities;

public class RefreshToken : Entity<int>
{
    [MaxLength(sbyte.MaxValue)]
    public string Token { get; init; } = null!;
    
    [MaxLength(byte.MaxValue)]
    public string UserId { get; init; } = null!;

    public bool IsActive { get; init; } = true;
}
