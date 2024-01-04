using DataAccess.Entities;
using DataAccess.Interfaces;

namespace DataAccess.Repositories;

public class UserRepository(ApplicationContext context) : Repository<User, string>(context), IUserRepository
{
    public async Task<bool> AddRefreshToken(string id, string token)
    {
        var user = await GetAsync(id);
        if (user is null)
            return false;
        
        user.RefreshTokens.Add(new RefreshToken { Token = token });
        return true;
    }
    
    public async Task<bool> DeleteRefreshToken(string id, string token)
    {
        var user = await GetAsync(id);
        var refreshToken = user?.RefreshTokens.FirstOrDefault(x => x.Token == token);
        if (refreshToken == null)
            return false;
        user!.RefreshTokens.Remove(refreshToken);
        return true;
    }
}
