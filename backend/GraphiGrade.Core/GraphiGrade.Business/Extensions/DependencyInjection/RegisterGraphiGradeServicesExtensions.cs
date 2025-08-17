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
using GraphiGrade.Business.Authorization.Policies.UserHasExercise;

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
        services.AddScoped<ISubmissionMapper, SubmissionMapper>();

        // Register services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<ISubmissionService, SubmissionService>();
        services.AddScoped<IGroupService, GroupService>();
        services.AddScoped<IUserResolverService, UserResolverService>();
        services.AddScoped<IBlobStorageService, AzureBlobStorageService>();
        services.AddScoped<IJudgeService, JudgeService>();

        // Register HttpClient for JudgeService
        services.AddHttpClient<IJudgeService, JudgeService>();

        return services;
    }

    public static IServiceCollection AddGraphiGradeAuthentication(this IServiceCollection services, GraphiGradeConfig configuration)
    {
        services.AddScoped<IAuthorizationHandler, SameUserHandler>();
        services.AddScoped<IAuthorizationHandler, AdminHandler>();
        services.AddScoped<IAuthorizationHandler, UserHasExerciseHandler>();
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
