namespace StockQuoteAlert.Interfaces;

// Contract for the low-level HTTP access to the quote API.
// Returns the raw response body; parsing is not its job.
public interface IQuoteApiClient
{
    Task<string> GetRawQuoteAsync(string symbol, CancellationToken ct = default);
}
