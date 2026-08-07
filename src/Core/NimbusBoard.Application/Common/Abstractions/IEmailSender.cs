namespace NimbusBoard.Application.Common.Abstractions;

public interface IEmailSender
{
    bool IsEnabled { get; }
    Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
