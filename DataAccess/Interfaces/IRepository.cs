using AutoFilterer.Types;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace DataAccess.Interfaces;

public interface IRepository<TEntity, in TKey> where TEntity : class, IEntity<TKey>
{
    public Task<TEntity?> GetAsync(TKey id);
    
    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> expression);

    public Task<IQueryable<TEntity>> FindAsync(PaginationFilterBase filter);
    
    public Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expression);

    public Task<TEntity> AddAsync(TEntity entity);

    public Task AddRangeAsync(IEnumerable<TEntity> entities);

    public void Update(TEntity entity);

    public void UpdateRange(IEnumerable<TEntity> entities);

    public void Remove(TEntity entity);

    public void RemoveRange(IEnumerable<TEntity> entities);

    public Task<int> ConfirmAsync();
}
