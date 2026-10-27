using StockQuoteAlert.Connect;
using StockQuoteAlert.Interfaces;
using System.Text.Json;

namespace StockQuoteAlert.Services;

// Sits between the raw HTTP layer (ApiConnect) and the rest of the app.
// ApiConnect only fetches the response text; this class turns it into a validated Stock.
// If the API changes its format, this is the only class that needs to change.
public class ApiServices
{
    private readonly ApiConnect _api;
    // Validate the API connection before creating the service, so the app fails fast if misconfigured.

    public ApiServices(ApiConnect api)
    {
        _api = api;
    }

    // Optional CancellationToken so the call can be interrupted
    public async Task<Stock> GetQuoteAsync(string symbol, CancellationToken ct = default)
    {
        // Validate before the request: avoids wasting API quota.
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("The stock symbol cannot be empty.", nameof(symbol));
        }

        var raw = await _api.GetRawQuoteAsync(symbol, ct);

        // Parse the JSON document to validate each field before using it.
        using var document = JsonDocument.Parse(raw);

        // The API returns "results" as an array even for a single ticker.
        if (!document.RootElement.TryGetProperty("results", out var results) ||
            results.ValueKind != JsonValueKind.Array ||
            results.GetArrayLength() == 0)
        {
            throw new InvalidOperationException("The quote response does not contain any results.");
        }

        // We request a single symbol, so the first item is the one we want.
        var result = results[0];

        // Use the symbol returned by the API, not the one requested.
        if (!result.TryGetProperty("symbol", out var symbolElement) ||
            symbolElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(symbolElement.GetString()))
        {
            throw new InvalidOperationException("The quote response does not contain a valid symbol.");
        }

        // decimal instead of double to avoid rounding errors when comparing the price against the buy/sell thresholds.
        if (!result.TryGetProperty("regularMarketPrice", out var priceElement) ||
            !priceElement.TryGetDecimal(out var price) ||
            price <= 0)
        {
            throw new InvalidOperationException("The quote response does not contain a valid market price.");
        }

        return new Stock(symbolElement.GetString()!, price, DateTime.UtcNow);
    }
}
