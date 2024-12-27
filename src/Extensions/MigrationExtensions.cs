using GraphiGrade.Data;
using Microsoft.EntityFrameworkCore;

namespace GraphiGrade.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();

        using var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Database.Migrate();
    }
}
