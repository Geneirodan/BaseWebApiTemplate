using Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : IdentityDbContext<User, Role, string>(options)
{
    public virtual DbSet<RefreshToken> RefreshTokens { get; init; } = null!;
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseLazyLoadingProxies();

}
