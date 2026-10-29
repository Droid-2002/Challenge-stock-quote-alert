using StockQuoteAlert.Interfaces;
using StockQuoteAlert.Models;
using System.Text.Json;

namespace StockQuoteAlert.Services;

// Sits between the raw HTTP layer (ApiConnect) and the rest of the app.
// ApiConnect only fetches the response text; this class turns it into a validated Stock.
// If the API changes its format, this is the only class that needs to change.
public class ApiServices : IQuoteService
{
    private readonly IQuoteApiClient _api;

    public ApiServices(IQuoteApiClient api)
    {
        // Null check: fail fast if the dependency is missing.
        _api = api ?? throw new ArgumentNullException(nameof(api));
    }

    // Optional CancellationToken so the call can be interrupted
    public async Task<Stock> GetQuoteAsync(string symbol, CancellationToken ct = default)
    {
        // Validate before the request: avoids a useless call.
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("The stock symbol cannot be empty.", nameof(symbol));
        }

        var raw = await _api.GetRawQuoteAsync(symbol, ct);

        // Parse the JSON document to validate each field before using it.
        // Expected shape: { "chart": { "result": [ { "meta": { "symbol", "regularMarketPrice", "regularMarketTime" } } ], "error": null } }
        using var document = JsonDocument.Parse(raw);

        if (!document.RootElement.TryGetProperty("chart", out var chart) || chart.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("The quote response does not contain a chart.");
        }

        // Yahoo reports problems (e.g. unknown symbol) in chart.error.
        if (chart.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object)
        {
            var description = error.TryGetProperty("description", out var d) ? d.GetString() : null;
            throw new InvalidOperationException($"The quote API returned an error: {description ?? "unknown error"}.");
        }

        if (!chart.TryGetProperty("result", out var results) ||
            results.ValueKind != JsonValueKind.Array ||
            results.GetArrayLength() == 0 ||
            !results[0].TryGetProperty("meta", out var meta) ||
            meta.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException("The quote response does not contain any results.");
        }

        // Use the symbol returned by the API (e.g. "PETR4.SA"), not the one requested.
        if (!meta.TryGetProperty("symbol", out var symbolElement) ||
            symbolElement.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(symbolElement.GetString()))
        {
            throw new InvalidOperationException("The quote response does not contain a valid symbol.");
        }

        // decimal instead of double to avoid rounding errors when comparing the price against the buy/sell thresholds.
        if (!meta.TryGetProperty("regularMarketPrice", out var priceElement) ||
            !priceElement.TryGetDecimal(out var price) ||
            price <= 0)
        {
            throw new InvalidOperationException("The quote response does not contain a valid market price.");
        }

        // Time of the quote (Unix seconds). Falls back to now if missing.
        var updatedAt = meta.TryGetProperty("regularMarketTime", out var timeElement) && timeElement.TryGetInt64(out var unix)
            ? DateTimeOffset.FromUnixTimeSeconds(unix).UtcDateTime
            : DateTime.UtcNow;

        return new Stock(symbolElement.GetString()!, price, updatedAt);
    }
}
