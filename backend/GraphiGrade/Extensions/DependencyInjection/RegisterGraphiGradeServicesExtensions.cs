using GraphiGrade.Configurations;
using GraphiGrade.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GraphiGrade.Extensions.DependencyInjection;

public static class RegisterGraphiGradeServicesExtensions
{
    public static void AddGraphiGradeServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Read configuration from Options Pattern to setup services down the line.  
        var optionsConfiguration = new GraphiGradeConfig
        {
            DbConnectionString = configuration.GetSection("GraphiGradeConfig:DbConnectionString").Value!
        };

        // Setup EF Core  
        services.AddDbContext<GraphiGradeDbContext>(options => options.UseSqlServer(optionsConfiguration.DbConnectionString));
    }
}
