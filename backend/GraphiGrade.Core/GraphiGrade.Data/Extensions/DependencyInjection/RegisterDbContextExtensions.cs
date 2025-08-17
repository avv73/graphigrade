using GraphiGrade.Data.DbContext;
using GraphiGrade.Data.Repositories;
using GraphiGrade.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GraphiGrade.Data.Extensions.DependencyInjection;

public static class RegisterDbContextExtensions
{
    public static void AddGraphiGradeDb(this IServiceCollection services, string dbConnectionString)
    {
        // Setup EF Core DbContext 
        services.AddDbContext<GraphiGradeDbContext>(options => options.UseSqlServer(dbConnectionString));

        // Register unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
