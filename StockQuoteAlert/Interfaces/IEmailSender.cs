namespace StockQuoteAlert.Interfaces;

// Contract for delivering an email.
// Uses plain strings instead of MailMessage, so the contract is not tied to System.Net.Mail.
public interface IEmailSender
{
    Task SendAsync(string from, string to, string subject, string body, CancellationToken ct = default);
}
