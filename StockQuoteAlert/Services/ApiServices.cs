using StockQuoteAlert.Connect;
using StockQuoteAlert.Interfaces;

namespace StockQuoteAlert.Services;

// Turns the raw API response into a Stock.
public class ApiServices
{
    private readonly ApiConnect _api;

    public ApiServices(ApiConnect api)
    {
        _api = api;
    }

    public async Task<Stock> GetQuoteAsync(string symbol, CancellationToken ct = default)
    {
        var raw = await _api.GetRawQuoteAsync(symbol, ct);

        // parse the JSON in `raw` and return the current price.
        throw new NotImplementedException();
    }
}
