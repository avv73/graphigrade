using GraphiGrade.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphiGrade.Data.DbContext;

public class GraphiGradeDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public GraphiGradeDbContext(DbContextOptions<GraphiGradeDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // UsersGroups many-to-many
        modelBuilder.Entity<UsersGroups>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });

        modelBuilder.Entity<UsersGroups>()
            .HasOne(ug => ug.User)
            .WithMany(u => u.UsersGroups)
            .HasForeignKey(ug => ug.UserId);

        modelBuilder.Entity<UsersGroups>()
            .HasOne(ug => ug.Group)
            .WithMany(g => g.UsersGroups)
            .HasForeignKey(ug => ug.GroupId);

        // ExercisesGroups many-to-many
        modelBuilder.Entity<ExercisesGroups>()
            .HasKey(eg => new { eg.ExerciseId, eg.GroupId });

        modelBuilder.Entity<ExercisesGroups>()
            .HasOne(eg => eg.Exercise)
            .WithMany(e => e.ExercisesGroups)
            .HasForeignKey(eg => eg.ExerciseId);

        modelBuilder.Entity<ExercisesGroups>()
            .HasOne(eg => eg.Group)
            .WithMany(g => g.ExercisesGroups)
            .HasForeignKey(eg => eg.GroupId);

        // FileMetadata
        modelBuilder.Entity<FileMetadata>()
            .HasMany(f => f.ExercisesAsExpectedImage)
            .WithOne(e => e.ExpectedImage)
            .HasForeignKey(e => e.ExpectedImageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FileMetadata>()
            .HasMany(f => f.SubmissionsResultImage)
            .WithOne(s => s.ResultImage)
            .HasForeignKey(s => s.ResultImageId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<FileMetadata>()
            .HasMany(f => f.SubmissionsSourceCode)
            .WithOne(s => s.SourceCode)
            .HasForeignKey(s => s.SourceCodeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Exercise created by user
        modelBuilder.Entity<Exercise>()
            .HasOne(e => e.CreatedBy)
            .WithMany()
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        // Submission
        modelBuilder.Entity<Submission>()
            .HasOne(s => s.User)
            .WithMany(u => u.Submissions)
            .HasForeignKey(s => s.UserId);

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Exercise)
            .WithMany(e => e.Submissions)
            .HasForeignKey(s => s.ExerciseId);

        modelBuilder.Entity<Submission>()
            .Property(s => s.JudgeId)
            .HasColumnType("CHAR(36)");

        modelBuilder.Entity<Submission>()
            .Property(s => s.Score)
            .HasColumnType("DECIMAL(3,2)");

        modelBuilder.Entity<Submission>()
            .HasIndex(s => s.JudgeId)
            .IsUnique();

        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Password)
            .HasColumnType("CHAR(60)");

        base.OnModelCreating(modelBuilder);
    }
}
