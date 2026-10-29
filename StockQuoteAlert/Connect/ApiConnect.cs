using StockQuoteAlert.Interfaces;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Connect;

// Low-level HTTP access to the Yahoo Finance chart API:
// GET {BaseUrl}{SYMBOL}{SymbolSuffix}?interval=1d&range=1d
public class ApiConnect : IQuoteApiClient
{
    private readonly HttpClient _http;
    private readonly ApiSettings _settings;

    public ApiConnect(ApiSettings settings) : this(settings, new HttpClientHandler()) { }

    // internal: lets tests replace the network with a fake HttpMessageHandler.
    internal ApiConnect(ApiSettings settings, HttpMessageHandler handler)
    {
        // Validate the settings before creating the HttpClient, so the app fails fast if misconfigured.
        if (settings is null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            throw new ArgumentException(
                "The API base URL is not configured.",
                nameof(settings.BaseUrl));
        }

        // Without the trailing "/", ".../v8/finance/chart" + "PETR4.SA" would become ".../v8/finance/PETR4.SA".
        var baseUrl = settings.BaseUrl.EndsWith('/') ? settings.BaseUrl : settings.BaseUrl + "/";

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new ArgumentException(
                "The API base URL must be a valid absolute URL.",
                nameof(settings.BaseUrl));
        }

        _settings = settings;
        _http = new HttpClient(handler ?? throw new ArgumentNullException(nameof(handler))) { BaseAddress = baseUri };

        // Yahoo answers 429 to requests without a User-Agent.
        if (!string.IsNullOrWhiteSpace(settings.UserAgent))
        {
            _http.DefaultRequestHeaders.UserAgent.ParseAdd(settings.UserAgent);
        }
    }

    // Defining the method as async allows the caller to cancel it, with Ctrl+C in the monitoring loop.
    public async Task<string> GetRawQuoteAsync(string symbol, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException(
                "The stock symbol must be provided.",
                nameof(symbol));
        }

        var yahooSymbol = ToYahooSymbol(symbol);

        // range=1d: only today's data; the current price is in chart.result[0].meta.
        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"{Uri.EscapeDataString(yahooSymbol)}?interval=1d&range=1d");

        // The response is disposed at the end of this method, after the content is read.
        using var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }

    // "petr4" -> "PETR4.SA". A symbol that already has a suffix ("AAPL" with SymbolSuffix "", "PETR4.SA") is kept.
    private string ToYahooSymbol(string symbol)
    {
        var normalized = symbol.Trim().ToUpperInvariant();
        var suffix = _settings.SymbolSuffix?.Trim().ToUpperInvariant() ?? "";

        return normalized.Contains('.') ? normalized : normalized + suffix;
    }
}
