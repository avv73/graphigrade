using GraphiGrade.Data;
using GraphiGrade.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GraphiGrade.Repositories;

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

    public async Task AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);
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
