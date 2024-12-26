using GraphiGrade.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GraphiGrade.Data;

public class ApplicationDbContext : IdentityDbContext<User, Role, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("identity");

        builder.Entity<User>(entity =>
        {
            entity.HasMany(u => u.UserGroups)
                .WithMany(g => g.Users);

            entity.HasMany(u => u.ManagesGroups)
            .WithOne(g => g.Manager)
            .HasForeignKey(g => g.ManagerId)
            .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
