using StockQuoteAlert.Interfaces;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Services;

// Builds and sends the buy/sell alert emails.
// IEmailSender only knows how to deliver an email; this class decides what the message says.
public class EmailServices : IAlertNotifier
{
    private readonly IEmailSender _sender;
    private readonly EmailSettings _settings;

    public EmailServices(IEmailSender sender, EmailSettings settings)
    {
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        if (string.IsNullOrWhiteSpace(settings.From))
        {
            throw new ArgumentException("The sender email (Email.From) is not configured.", nameof(settings.From));
        }

        if (string.IsNullOrWhiteSpace(settings.To))
        {
            throw new ArgumentException("The recipient email (Email.To) is not configured.", nameof(settings.To));
        }

        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        _settings = settings;
    }

    public Task NotifyBuyAsync(Stock stock, decimal buyPrice, CancellationToken ct = default)
    {
        var subject = $"[StockQuoteAlert] Buy: {stock.Symbol}";
        var body =
            $"{stock.Symbol} is trading at R$ {stock.Price:F2}, " +
            $"below the buy reference price (R$ {buyPrice:F2}).\n\n" +
            $"Recommendation: BUY.\n" +
            $"Quote retrieved at: {stock.UpdatedAt:O}";

        return SendAsync(subject, body, ct);
    }

    public Task NotifySellAsync(Stock stock, decimal sellPrice, CancellationToken ct = default)
    {
        var subject = $"[StockQuoteAlert] Sell: {stock.Symbol}";
        var body =
            $"{stock.Symbol} is trading at R$ {stock.Price:F2}, " +
            $"above the sell reference price (R$ {sellPrice:F2}).\n\n" +
            $"Recommendation: SELL.\n" +
            $"Quote retrieved at: {stock.UpdatedAt:O}";

        return SendAsync(subject, body, ct);
    }

    private Task SendAsync(string subject, string body, CancellationToken ct) =>
        _sender.SendAsync(_settings.From, _settings.To, subject, body, ct);
}
