using AutoFilterer.Extensions;
using AutoFilterer.Types;
using DataAccess;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BusinessLogic.Repositories;

public abstract class Repository<TEntity>(ApplicationContext context) : IRepository<TEntity, string>
    where TEntity : class, IEntity<string>
{
    // ReSharper disable once MemberCanBePrivate.Global
    protected ApplicationContext Context { get; } = context;

    private DbSet<TEntity> Entities => Context.Set<TEntity>();

    public async Task<TEntity> AddAsync(TEntity entity) => (await Entities.AddAsync(entity)).Entity;

    public Task AddRangeAsync(IEnumerable<TEntity> entities) => Entities.AddRangeAsync(entities);

    public Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> expression) => Task.FromResult(Entities.Where(expression));

    public Task<IQueryable<TEntity>> GetAllAsync(PaginationFilterBase filter) => Task.FromResult(Entities.ApplyFilter(filter));

    public async Task<TEntity?> GetAsync(string id) => await Entities.FindAsync(id);

    public void Remove(TEntity entity) => Entities.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => Entities.RemoveRange(entities);

    public void Update(TEntity entity) => Entities.Update(entity);

    public Task<int> ConfirmAsync() => Context.SaveChangesAsync();
}