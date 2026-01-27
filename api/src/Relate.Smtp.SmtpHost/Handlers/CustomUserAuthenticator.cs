using SmtpServer;
using SmtpServer.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Relate.Smtp.SmtpHost.Handlers;

public class CustomUserAuthenticator : IUserAuthenticator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CustomUserAuthenticator> _logger;

    public CustomUserAuthenticator(IConfiguration configuration, ILogger<CustomUserAuthenticator> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<bool> AuthenticateAsync(
        ISessionContext context,
        string user,
        string password,
        CancellationToken cancellationToken)
    {
        // Get SMTP credentials from configuration
        var smtpUser = _configuration["Smtp:Username"];
        var smtpPassword = _configuration["Smtp:Password"];

        if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
        {
            _logger.LogWarning("SMTP authentication is not configured. Set Smtp:Username and Smtp:Password in configuration.");
            return Task.FromResult(false);
        }

        var isAuthenticated = string.Equals(user, smtpUser, StringComparison.OrdinalIgnoreCase)
            && string.Equals(password, smtpPassword, StringComparison.Ordinal);

        if (isAuthenticated)
        {
            _logger.LogInformation("SMTP user authenticated: {User}", user);
        }
        else
        {
            _logger.LogWarning("SMTP authentication failed for user: {User}", user);
        }

        return Task.FromResult(isAuthenticated);
    }
}
