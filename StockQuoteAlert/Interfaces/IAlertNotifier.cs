using StockQuoteAlert.Models;

namespace StockQuoteAlert.Interfaces;

// Contract for notifying the user about a buy or sell opportunity.
// The monitor only needs to know that someone was notified, not how (email, Telegram, console...).
public interface IAlertNotifier
{
    Task NotifyBuyAsync(Stock stock, decimal buyPrice, CancellationToken ct = default);
    Task NotifySellAsync(Stock stock, decimal sellPrice, CancellationToken ct = default);
}
