using GraphiGrade.Business.Authorization;
using GraphiGrade.Business.Configurations;
using GraphiGrade.Business.Mappers;
using GraphiGrade.Business.Mappers.Abstractions;
using GraphiGrade.Business.Services;
using GraphiGrade.Business.Services.Abstractions;
using GraphiGrade.Business.Services.Utils;
using GraphiGrade.Business.Services.Utils.Abstractions;
using GraphiGrade.Data.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using GraphiGrade.Business.Authorization.Policies.Admin;
using GraphiGrade.Business.Authorization.Policies.SameUser;
using GraphiGrade.Business.Authorization.Policies.UserBelongsToGroup;

namespace GraphiGrade.Business.Extensions.DependencyInjection;

public static class RegisterGraphiGradeServicesExtensions
{
    public static IServiceCollection AddGraphiGradeServices(this IServiceCollection services, GraphiGradeConfig configuration)
    {
        // Register Db
        services.AddGraphiGradeDb(configuration.DbConnectionString);

        // Register utils
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtService, JwtService>();

        // Register mappers
        services.AddScoped<IUserMapper, UserMapper>();
        services.AddScoped<IGroupMapper, GroupMapper>();
        services.AddScoped<IExerciseMapper, ExerciseMapper>();

        // Register services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();

        return services;
    }

    public static IServiceCollection AddGraphiGradeAuthentication(this IServiceCollection services, GraphiGradeConfig configuration)
    {
        services.AddSingleton<IAuthorizationHandler, SameUserHandler>();
        services.AddScoped<IAuthorizationHandler, UserBelongsToGroupHandler>();

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

        services.AddAuthorizationBuilder()
            .AddPolicy(Policy.Admin, policy => policy.Requirements.Add(RequirementsFactory.CreateAdminRequirement()))
            .AddPolicy(Policy.SameUser, policy => policy.Requirements.Add(RequirementsFactory.CreateSameUserRequirement()))
            .AddPolicy(Policy.UserBelongsToGroup, policy => policy.Requirements.Add(RequirementsFactory.CreateUserBelongsToGroupRequirement()))
            .AddPolicy(Policy.UserHasExercise, policy => policy.Requirements.Add(RequirementsFactory.CreateUserHasExerciseRequirement()));

        return services;
    }
}
