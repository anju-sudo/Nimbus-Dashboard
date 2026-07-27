using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NimbusBoard.Application.Common.Interfaces;
using System.Net;
using System.Net.Mail;

namespace NimbusBoard.Infrastructure.Services;

public class SmtpOptions
{
    public const string SectionName = "Smtp";
    public bool Enabled { get; set; }
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public string? User { get; set; }
    public string? Password { get; set; }
    public string From { get; set; } = "nimbus@localhost";
    public bool UseSsl { get; set; }
}

public class SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly SmtpOptions _options = options.Value;

    public bool IsEnabled => _options.Enabled;

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("Email (disabled): To={To} Subject={Subject} Body={Body}", to, subject, body);
            return;
        }

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.User))
        {
            client.Credentials = new NetworkCredential(_options.User, _options.Password);
        }

        using var message = new MailMessage(_options.From, to, subject, body);
        await client.SendMailAsync(message, cancellationToken);
        logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
    }
}
