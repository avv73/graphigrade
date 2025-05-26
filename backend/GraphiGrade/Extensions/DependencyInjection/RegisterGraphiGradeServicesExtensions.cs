using GraphiGrade.Data;
using GraphiGrade.Repositories;
using GraphiGrade.Repositories.Abstractions;
using GraphiGrade.Services;
using GraphiGrade.Services.Abstractions;
using GraphiGrade.Services.Utils;
using GraphiGrade.Services.Utils.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GraphiGrade.Extensions.DependencyInjection;

public static class RegisterGraphiGradeServicesExtensions
{
    public static void AddGraphiGradeServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Setup EF Core DbContext 
        services.AddDbContext<GraphiGradeDbContext>(options => options.UseSqlServer(configuration.GetSection("GraphiGradeConfig:DbConnectionString").Value));

        // Register unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register utils
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();

        // Register services
        services.AddScoped<IUserService, UserService>();
    }
}
