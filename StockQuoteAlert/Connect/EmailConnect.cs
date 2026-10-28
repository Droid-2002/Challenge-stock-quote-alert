using System.Net;
using System.Net.Mail;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Connect;

// Thrown when an email could not be sent through the SMTP server.
public class SendEmailException : Exception
{
    public SendEmailException(string? message, Exception? innerException = null)
        : base(message, innerException) { }
}

// Low-level SMTP access.
public class EmailConnect
{
    private readonly SmtpClient _client;

    public EmailConnect(SmtpSettings settings)
    {
        // Validate the settings before creating the SmtpClient, so the app fails fast if misconfigured.
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        if (string.IsNullOrWhiteSpace(settings.Host))
        {
            throw new ArgumentException("The SMTP host is not configured.", nameof(settings.Host));
        }

        if (settings.Port <= 0 || settings.Port > 65535)
        {
            throw new ArgumentException("The SMTP port must be between 1 and 65535.", nameof(settings.Port));
        }

        _client = new SmtpClient(settings.Host, settings.Port)
        {
            EnableSsl = settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(settings.User, settings.Password)
        };
    }

    public async Task SendAsync(MailMessage message, CancellationToken ct = default)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        try
        {
            await _client.SendMailAsync(message, ct);
        }
        catch (OperationCanceledException)
        {
            // Let cancellation (Ctrl+C) propagate as is.
            throw;
        }
        catch (Exception ex)
        {
            // Keep the original error as InnerException to help debugging (auth, host, SSL...).
            throw new SendEmailException("Could not send the email.", ex);
        }
    }
}
