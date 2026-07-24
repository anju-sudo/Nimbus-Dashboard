namespace NimbusBoard.Application.Common.Interfaces;

public interface IEmailSender
{
    bool IsEnabled { get; }
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
