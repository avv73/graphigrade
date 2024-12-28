using GraphiGrade.Logger;
using GraphiGrade.Services.Externals.Abstractions;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace GraphiGrade.Services.Identity;

public class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    private readonly IMailgunApiClient _mailgunApiClient;

    private const string SenderEmail = "no-reply@graphigrade.tech";

    private const string ServiceName = nameof(ServiceName);

    public EmailSender(ILogger<EmailSender>? logger, IMailgunApiClient? mailgunApiClient)
    {
        ArgumentNullException.ThrowIfNull(_logger = logger!);
        ArgumentNullException.ThrowIfNull(_mailgunApiClient = mailgunApiClient!);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        _logger.LogGeneralDebug(DateTime.UtcNow, ServiceName, nameof(SendEmailAsync), $"Send Email Call received! - {email} {subject} {htmlMessage}");
        
        bool result = await _mailgunApiClient.SendMailAsync(
            SenderEmail, 
            email, 
            subject,
            textContent:string.Empty,
            htmlContent:htmlMessage);

        _logger.LogGeneralDebug(DateTime.UtcNow, ServiceName, nameof(SendEmailAsync), $"Send Email Call Result: {result}");
    }
}