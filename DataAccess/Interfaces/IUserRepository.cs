using AutoFilterer.Types;
using DataAccess.Entities;
using System.Linq.Expressions;

namespace DataAccess.Interfaces;

public interface IUserRepository
{
    Task<User> AddAsync(User entity);
    Task AddRangeAsync(IEnumerable<User> entities);
    Task<IQueryable<User>> FindAsync(Expression<Func<User, bool>> expression);
    Task<IQueryable<User>> FindAsync(PaginationFilterBase filter);
    Task<User?> GetAsync(Expression<Func<User, bool>> expression);
    Task<User?> GetAsync(string id);
    void Remove(User entity);
    void RemoveRange(IEnumerable<User> entities);
    void Update(User entity);
    void UpdateRange(IEnumerable<User> entities);
    Task<int> ConfirmAsync();
    Task<bool> AddRefreshToken(string id, string token);
    Task<bool> DeleteRefreshToken(string id, string token);
}
