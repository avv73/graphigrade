using GraphiGrade.Data.DbContext;
using GraphiGrade.Data.Models;
using GraphiGrade.Data.Repositories.Abstractions;

namespace GraphiGrade.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly GraphiGradeDbContext _dbContext;

    public IRepository<User> Users { get; }
    public IRepository<Group> Groups { get; }
    public IRepository<Exercise> Exercises { get; }
    public IRepository<Submission> Submissions { get; }
    public IRepository<FileMetadata> FilesMetadata { get; }
    public IRepository<UsersGroups> UsersGroups { get; }
    public IRepository<ExercisesGroups> ExercisesGroups { get; }

    public UnitOfWork(GraphiGradeDbContext dbContext)
    {
        _dbContext = dbContext;

        Users = new Repository<User>(_dbContext);
        Groups = new Repository<Group>(_dbContext);
        Exercises = new Repository<Exercise>(_dbContext);
        Submissions = new Repository<Submission>(_dbContext);
        FilesMetadata = new Repository<FileMetadata>(_dbContext);
        UsersGroups = new Repository<UsersGroups>(_dbContext);
        ExercisesGroups = new Repository<ExercisesGroups>(_dbContext);
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
