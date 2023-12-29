using AutoFilterer.Types;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace DataAccess.Interfaces;

public interface IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    Task<TEntity?> GetAsync(string id);

    Task<IQueryable<TEntity>> GetAllAsync(PaginationFilterBase filter);

    Task<TEntity> AddAsync(TEntity entity);

    Task AddRangeAsync(IEnumerable<TEntity> entities);

    Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expression);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);

    Task<int> ConfirmAsync();
}
