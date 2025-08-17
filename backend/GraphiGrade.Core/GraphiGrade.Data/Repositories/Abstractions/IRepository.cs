using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace GraphiGrade.Data.Repositories.Abstractions;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetFirstByFilterAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdWithIncludesAsync(int id, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes);
    Task<T?> GetFirstByFilterWithIncludesAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> includes);
    Task AddAsync(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    void Update(T entity);
    void Delete(T entity);
    Task SaveAsync();
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetAllWithIncludesAsync(Func<IQueryable<T>, IIncludableQueryable<T, object>> includes);
}
