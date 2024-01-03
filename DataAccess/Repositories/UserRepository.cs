using AutoFilterer.Types;
using DataAccess.Entities;
using System.Linq.Expressions;

namespace DataAccess.Repositories;

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
}

public class UserRepository(ApplicationContext context) : Repository<User, string>(context), IUserRepository
{
}
