using GraphiGrade.Configuration;
using GraphiGrade.Data;
using GraphiGrade.Extensions;
using GraphiGrade.Logger;
using GraphiGrade.Models.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Read configuration from appsettings.
var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: false, reloadOnChange: false)
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>();

var configuration = configurationBuilder.Build();

// Setup configuration through Options Pattern.
builder.Services.AddOptions<GraphiGradeConfig>()
    .Bind(configuration.GetSection("GraphiGradeConfig"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Read configuration from Options Pattern to setup services down the line.
var optionsConfiguration = new GraphiGradeConfig();
configuration.GetSection("GraphiGradeConfig").Bind(optionsConfiguration);

// Setup Db context.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(optionsConfiguration.DbConnectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Setup ASP.NET Core identity.
builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Setup Microsoft SSO.
if (optionsConfiguration.MicrosoftSsoConfig.IsEnabled)
{
    builder.Services.AddAuthentication().AddMicrosoftAccount(microsoftOptions =>
    {
        microsoftOptions.ClientId = optionsConfiguration.MicrosoftSsoConfig.ClientId;
        microsoftOptions.ClientSecret = optionsConfiguration.MicrosoftSsoConfig.ClientSecret;
        microsoftOptions.AuthorizationEndpoint = optionsConfiguration.MicrosoftSsoConfig.AuthorizationEndpoint;
        microsoftOptions.TokenEndpoint = optionsConfiguration.MicrosoftSsoConfig.TokenEndpoint;
    });
}

builder.Services.AddGraphiGradeServices();

builder.Services.AddRazorPages();

LogUtils.PrintStartInfo(builder);

var app = builder.Build();

// Automatically apply migrations for Production before startup.
if (builder.Environment.IsProduction())
{
    Console.WriteLine("[STARTUP] Applying Production migrations to DB...");
    app.ApplyMigrations();
    Console.WriteLine("[STARTUP] Migrations applied!");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
