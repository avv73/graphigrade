using GraphiGrade.Data;
using GraphiGrade.Models;
using GraphiGrade.Repositories.Abstractions;

namespace GraphiGrade.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GraphiGradeDbContext _dbContext;

    public IRepository<User> Users { get; }
    public IRepository<Group> Groups { get; }
    public IRepository<Exercise> Exercises { get; }
    public IRepository<Submission> Submissions { get; }
    public IRepository<FileMetadata> FilesMetadata { get; }

    public UnitOfWork(GraphiGradeDbContext dbContext)
    {
        _dbContext = dbContext;

        Users = new Repository<User>(_dbContext);
        Groups = new Repository<Group>(_dbContext);
        Exercises = new Repository<Exercise>(_dbContext);
        Submissions = new Repository<Submission>(_dbContext);
        FilesMetadata = new Repository<FileMetadata>(_dbContext);
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

}
