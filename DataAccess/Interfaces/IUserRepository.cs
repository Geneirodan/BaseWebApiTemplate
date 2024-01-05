using DataAccess.Entities;

namespace DataAccess.Interfaces;

public interface IUserRepository : IRepository<User, string>
{
    Task<bool> AddRefreshToken(string id, string token);
    Task<bool> DeleteRefreshToken(string id, string token);
}
