namespace NimbusBoard.Infrastructure.Services;

public sealed class SmtpOptions
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
