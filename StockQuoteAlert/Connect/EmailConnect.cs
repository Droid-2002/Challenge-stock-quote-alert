using System.Net.Mail;
using StockQuoteAlert.Interfaces;

namespace StockQuoteAlert.Connect;

// Low-level SMTP access.
public class EmailConnect
{
    private readonly SmtpSettings _settings;

    public EmailConnect(SmtpSettings settings)
    {
        _settings = settings;
    }

    public Task SendAsync(MailMessage message, CancellationToken ct = default)
    {
        // SmtpClient from _settings (host, port, SSL, credentials) and send the message.
        throw new NotImplementedException();
    }
}
