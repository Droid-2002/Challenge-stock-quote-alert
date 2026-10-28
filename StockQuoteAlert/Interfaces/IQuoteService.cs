using StockQuoteAlert.Models;

namespace StockQuoteAlert.Interfaces;

// Contract for anything that can provide a validated quote for a symbol.
// MonitorServices depends on this, not on ApiServices.
public interface IQuoteService
{
    Task<Stock> GetQuoteAsync(string symbol, CancellationToken ct = default);
}
