using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SurveyBasket.Api.Settings;

namespace SurveyBasket.Api.Health;

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
