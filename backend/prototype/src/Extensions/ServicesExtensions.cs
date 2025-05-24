using GraphiGrade.Services.Externals;
using GraphiGrade.Services.Externals.Abstractions;
using GraphiGrade.Services.Identity;
using GraphiGrade.Services.Identity.Abstractions;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace GraphiGrade.Extensions;

public static class ServicesExtensions
{
    public static void AddGraphiGradeServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient();

        serviceCollection.AddSingleton<IMailgunApiClient, MailgunApiClient>();

        serviceCollection.AddTransient<IEmailSender, EmailSender>();

        serviceCollection.AddTransient<IRecaptchaApiClient, RecaptchaApiClient>();

        serviceCollection.AddTransient<ICaptchaValidator, CaptchaValidator>();
    }
}