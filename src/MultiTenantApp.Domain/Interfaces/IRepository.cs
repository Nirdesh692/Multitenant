using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Attendance.Domain.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    Task<TEntity> GetByIdAsync(Guid id);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> filter);
    Task<TEntity> GetSingleAsync(Expression<Func<TEntity, bool>> predicate);
    Task<IEnumerable<TEntity>> GetConditionalAsync(Expression<Func<TEntity, bool>> predicate);
    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);

    Task AddAsync(TEntity entity);
    void Update(TEntity entity);
    void Delete(TEntity entity);
    Task AddRange(List<TEntity> entity);
    void DeleteRange(List<TEntity> entity);
    Task DeleteAllAsync();
    Task<TEntity> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);
    IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] includeProperties);
    Task<TEntity> FirstOrDefault();

    Task<TEntity> GetByGuidAsync(Guid id);
    IQueryable<TEntity> GetAllForQuery();
    IQueryable<TEntity> GetConditionalForQuery(Expression<Func<TEntity, bool>> predicate);
}