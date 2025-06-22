using MailKit.Net.Smtp;
namespace SurveyBasket.Api.Health;

// we try to connect to the email provider and try to make authenticate
public class MailProviderHealthCheck(IOptions<MailSettings> options) : IHealthCheck
{
    private readonly MailSettings _options = options.Value;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient();
            client.Connect(_options.Host, _options.Port, SecureSocketOptions.StartTls, cancellationToken);
            client.Authenticate(_options.Mail, _options.Password, cancellationToken);
            return await Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception ex)
        {
            return await Task.FromResult(HealthCheckResult.Unhealthy(exception: ex));
        }
    }
}
