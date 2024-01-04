using DataAccess.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : IdentityDbContext<User, Role, string>(options)
{
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseLazyLoadingProxies();

}
