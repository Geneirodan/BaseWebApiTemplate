using AutoFilterer.Extensions;
using AutoFilterer.Types;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess.Repositories;

public abstract class Repository<TEntity, TKey>(ApplicationContext context) : IRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
    // ReSharper disable once MemberCanBePrivate.Global
    protected ApplicationContext Context { get; } = context;

    private DbSet<TEntity> Entities => Context.Set<TEntity>();

    public async Task<TEntity> AddAsync(TEntity entity) => (await Entities.AddAsync(entity)).Entity;

    public Task AddRangeAsync(IEnumerable<TEntity> entities) => Entities.AddRangeAsync(entities);

    public Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expression) => Task.FromResult(Entities.Where(expression));

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> expression) => Entities.FirstOrDefaultAsync(expression);
    
    public Task<IQueryable<TEntity>> FindAsync(PaginationFilterBase filter) => Task.FromResult(Entities.ApplyFilter(filter));

    public async Task<TEntity?> GetAsync(TKey id) => await Entities.FindAsync(id);

    public void Remove(TEntity entity) => Entities.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => Entities.RemoveRange(entities);

    public void Update(TEntity entity) => Entities.Update(entity);
    public void UpdateRange(IEnumerable<TEntity> entities) => Entities.UpdateRange();

    public Task<int> ConfirmAsync() => Context.SaveChangesAsync();
}