using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MultiTenantApp.Domain.Interfaces;

namespace MultiTenantApp.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    private readonly DbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public Repository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task AddRange(List<TEntity> entities) => await _dbSet.AddRangeAsync(entities);

    public async Task AddAsync(TEntity entity) => await _dbSet.AddAsync(entity);

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter)
    {
        return await _dbSet.AnyAsync(filter);
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }

    public void Delete(TEntity entity) => _dbSet.Remove(entity);

    public async Task DeleteAllAsync()
    {
        var alldelete = await _dbSet.ToListAsync();
        _dbSet.RemoveRange(alldelete);
    }

    public void DeleteRange(List<TEntity> entities) => _dbSet.RemoveRange(entities);

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] includeProperties)
    {
        IQueryable<TEntity> query = _dbSet;
        if (includeProperties != null && includeProperties.Any())
        {
            foreach (var property in includeProperties)
            {
                query = query.Include(property);
            }
        }
        return query;
    }

    public async Task<TEntity> GetByGuidAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<TEntity> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

    public async Task<IEnumerable<TEntity>> GetConditionalAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.Where(predicate).ToListAsync();
    }

    public async Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate) => await _dbSet.SingleAsync(predicate);

    public async void Update(TEntity entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    public IQueryable<TEntity> GetAllForQuery()
    {
        return _dbSet.AsQueryable();
    }

    public IQueryable<TEntity> GetConditionalForQuery(Expression<Func<TEntity, bool>> predicate)
    {
        return _dbSet.Where(predicate).AsQueryable();
    }

    public async Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return (await _dbSet.FirstOrDefaultAsync(predicate))!;
    }

    public async Task<TEntity> FirstOrDefault()
    {
        return (await _dbSet.FirstOrDefaultAsync())!;
    }
    public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate = null)
    {
        IQueryable<TEntity> query = _context.Set<TEntity>();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync();
    }


}