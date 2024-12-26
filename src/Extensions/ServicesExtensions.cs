using GraphiGrade.Services.Externals;
using GraphiGrade.Services.Externals.Abstractions;
using GraphiGrade.Services.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace GraphiGrade.Extensions;

public static class ServicesExtensions
{
    public static void AddGraphiGradeServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient();

        serviceCollection.AddSingleton<IMailgunApiClient, MailgunApiClient>();

        serviceCollection.AddTransient<IEmailSender, EmailSender>();
    }
}
