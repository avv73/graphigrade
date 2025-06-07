using System.Linq.Expressions;

namespace GraphiGrade.Data.Repositories.Abstractions;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetFirstByFilterAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    void Update(T entity);
    void Delete(T entity);
    Task SaveAsync();
}
