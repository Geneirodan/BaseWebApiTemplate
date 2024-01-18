using Infrastructure.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Entities;

public class User : IdentityUser, IEntity<string>
{
    public virtual List<RefreshToken> RefreshTokens { get; set; } = null!;
}
