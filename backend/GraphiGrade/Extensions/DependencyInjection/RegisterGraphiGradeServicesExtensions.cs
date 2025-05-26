using System.Text;
using GraphiGrade.Authorization.SameUserOrAdmin;
using GraphiGrade.Configurations;
using GraphiGrade.Constants;
using GraphiGrade.Data;
using GraphiGrade.Repositories;
using GraphiGrade.Repositories.Abstractions;
using GraphiGrade.Services;
using GraphiGrade.Services.Abstractions;
using GraphiGrade.Services.Utils;
using GraphiGrade.Services.Utils.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GraphiGrade.Extensions.DependencyInjection;

public static class RegisterGraphiGradeServicesExtensions
{
    public static void AddGraphiGradeServices(this IServiceCollection services, GraphiGradeConfig configuration)
    {
        // Setup EF Core DbContext 
        services.AddDbContext<GraphiGradeDbContext>(options => options.UseSqlServer(configuration.DbConnectionString));

        // Register unit of work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Register utils
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();

        // Register services
        services.AddScoped<IUserService, UserService>();
    }

    public static void AddGraphiGradeAuthentication(this IServiceCollection services, GraphiGradeConfig configuration)
    {
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration.JwtIssuer,
                    ValidAudience = configuration.JwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.JwtSecretKey))
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policy.SameUserOrAdmin,
                policy => policy.Requirements.Add(new SameUserOrAdminRequirement(Role.AdminRole)));
        });
    }
}
