using System.Net.Mail;
using StockQuoteAlert.Connect;
using StockQuoteAlert.Interfaces;

namespace StockQuoteAlert.Services;

// Builds and sends the buy/sell alert emails.
// EmailConnect only knows how to deliver a MailMessage; this class decides what the message says.
public class EmailServices
{
    private readonly EmailConnect _email;
    private readonly EmailSettings _settings;

    public EmailServices(EmailConnect email, EmailSettings settings)
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

        _email = email ?? throw new ArgumentNullException(nameof(email));
        _settings = settings;
    }

    public Task SendBuyAlertAsync(Stock stock, decimal buyPrice, CancellationToken ct = default)
    {
        var subject = $"[StockQuoteAlert] Compra: {stock.Symbol}";
        var body =
            $"O ativo {stock.Symbol} esta cotado a R$ {stock.Price:F2}, " +
            $"abaixo do preco de referencia para compra (R$ {buyPrice:F2}).\n\n" +
            $"Recomendacao: COMPRAR.\n" +
            $"Cotacao obtida em: {stock.UpdatedAt:O}";

        return SendAsync(subject, body, ct);
    }

    public Task SendSellAlertAsync(Stock stock, decimal sellPrice, CancellationToken ct = default)
    {
        var subject = $"[StockQuoteAlert] Venda: {stock.Symbol}";
        var body =
            $"O ativo {stock.Symbol} esta cotado a R$ {stock.Price:F2}, " +
            $"acima do preco de referencia para venda (R$ {sellPrice:F2}).\n\n" +
            $"Recomendacao: VENDER.\n" +
            $"Cotacao obtida em: {stock.UpdatedAt:O}";

        return SendAsync(subject, body, ct);
    }

    private async Task SendAsync(string subject, string body, CancellationToken ct)
    {
        // MailMessage is IDisposable, so dispose it after sending.
        using var message = new MailMessage(_settings.From, _settings.To, subject, body);
        await _email.SendAsync(message, ct);
    }
}
