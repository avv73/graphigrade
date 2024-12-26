namespace GraphiGrade.Services.Externals.Abstractions;

public interface IMailgunApiClient
{
    Task<bool> SendMailAsync(string senderEmail, string recipientEmail, string subject, string textContent);
}