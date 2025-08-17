using GraphiGrade.Data.Models;

namespace GraphiGrade.Data.Repositories.Abstractions;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Group> Groups { get; }
    IRepository<Exercise> Exercises { get; }
    IRepository<Submission> Submissions { get; }
    IRepository<FileMetadata> FilesMetadata { get; }
    IRepository<UsersGroups> UsersGroups { get; }
    Task SaveAsync();
}
