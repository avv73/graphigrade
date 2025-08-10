using GraphiGrade.Data.DbContext;
using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace GraphiGrade.Data.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected GraphiGradeDbContext DbContext { get; }
    protected DbSet<T> DbSet { get; }

    public Repository(GraphiGradeDbContext dbContext)
    {
        DbContext = dbContext;

        DbSet = DbContext.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await DbSet.FindAsync(id);
    }

    public async Task<T?> GetByIdWithIncludesAsync(int id, Func<IQueryable<T>, IIncludableQueryable<T, object>> includes)
    {
        IQueryable<T> query = DbSet.AsQueryable();

        query = includes(query);

        // Find by Id (assuming the entity has an Id property)
        return await query.FirstOrDefaultAsync(e => EF.Property<int>(e, "Id") == id);
    }

    public async Task<T?> GetFirstByFilterAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.FirstOrDefaultAsync(predicate);
    }

    public async Task<T?> GetFirstByFilterWithIncludesAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IIncludableQueryable<T, object>> includes)
    {
        IQueryable<T> query = DbSet.AsQueryable();

        query = includes(query);

        // Find by Id (assuming the entity has an Id property)
        return await query.FirstOrDefaultAsync(predicate);
    }

    public async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.AnyAsync(predicate);
    }

    public void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public void Delete(T entity)
    {
        DbSet.Remove(entity);
    }

    public async Task SaveAsync()
    {
        await DbContext.SaveChangesAsync();
    }
}
