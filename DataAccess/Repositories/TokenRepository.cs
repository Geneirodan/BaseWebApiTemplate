using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories;

public class TokenRepository(ApplicationContext context) : ITokenRepository
{
    
    public async Task<RefreshToken> AddRefreshTokenAsync(RefreshToken token)
    {
        var entry = context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();
        return entry.Entity;
    }
    
    public async Task DeleteRefreshTokenAsync(string userId, string refreshToken)
    {
        var token = await context.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.Token == refreshToken);
        if (token != null)
            context.RefreshTokens.Remove(token);
    }
    
    public Task<RefreshToken?> GetSavedRefreshTokensAsync(string userId, string refreshToken) => 
        context.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userId && x.Token == refreshToken && x.IsActive == true);
    
    public Task<int> SaveChangesAsync() => context.SaveChangesAsync();
}
