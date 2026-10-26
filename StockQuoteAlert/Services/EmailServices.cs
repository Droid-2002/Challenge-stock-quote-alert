using StockQuoteAlert.Connect;
using StockQuoteAlert.Interfaces;

namespace StockQuoteAlert.Services;

// Builds and sends the buy/sell alert emails.
public class EmailServices
{
    private readonly EmailConnect _email;
    private readonly EmailSettings _settings;

    public EmailServices(EmailConnect email, EmailSettings settings)
    {
        _email = email;
        _settings = settings;
    }

    public Task SendBuyAlertAsync(Stock stock, decimal buyPrice, CancellationToken ct = default)
    {
        // MailMessage (From/To from _settings) advising to buy, then call _email.SendAsync.
        throw new NotImplementedException();
    }

    public Task SendSellAlertAsync(Stock stock, decimal sellPrice, CancellationToken ct = default)
    {
        // MailMessage (From/To from _settings) advising to sell, then call _email.SendAsync.
        throw new NotImplementedException();
    }
}
