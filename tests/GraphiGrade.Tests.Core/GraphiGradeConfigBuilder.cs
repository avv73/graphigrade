using GraphiGrade.Configuration;

namespace GraphiGrade.Tests.Core;

public static class GraphiGradeConfigBuilder
{
    private const string DbConnectionString = "test-str";

    private const string MailgunApiKey = "mailgun-key";

    private const string MailgunEndpointUrl = "https://test.com/";

    private const string RecaptchaSiteKey = "recaptcha-site-key";

    private const string RecaptchaSecretKey = "recaptcha-secret-key";

    private const string GoogleApiKey = "google-test-key";

    private const string RecaptchaAssessUrl = "https://test.com/?key={0}";

    public static GraphiGradeConfig BuildConfig(
        string dbConnectionString = DbConnectionString, 
        string mailgunApiKey = MailgunApiKey,
        string mailgunEndpointUrl = MailgunEndpointUrl,
        string recaptchaSiteKey = RecaptchaSiteKey,
        string recaptchaSecretKey = RecaptchaSecretKey,
        string googleApiKey = GoogleApiKey,
        string recaptchaAssessUrl = RecaptchaAssessUrl)
    {
        return new GraphiGradeConfig
        {
            DbConnectionString = dbConnectionString,
            MailgunConfig = new MailgunConfig
            {
                MailgunApiKey = mailgunApiKey,
                MailgunEndpointUrl = mailgunEndpointUrl
            },
            RecaptchaConfig = new RecaptchaConfig
            {
                SiteKey = recaptchaSiteKey,
                SecretKey = recaptchaSecretKey,
                GoogleApiKey = googleApiKey,
                RecaptchaAssessUrl = recaptchaAssessUrl
            }
        };
    }
}
