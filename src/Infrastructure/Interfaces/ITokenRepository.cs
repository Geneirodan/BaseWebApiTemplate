using Infrastructure.Entities;

namespace Infrastructure.Interfaces;

public interface ITokenRepository : IRepositoryService
{
    Task<RefreshToken> AddRefreshTokenAsync(RefreshToken token);
    Task DeleteRefreshTokenAsync(string email, string refreshToken);
    Task<RefreshToken?> GetSavedRefreshTokensAsync(string username, string refreshToken);
    Task<int> SaveChangesAsync();
}