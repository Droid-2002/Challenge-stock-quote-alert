using System.Net.Http.Headers;
using StockQuoteAlert.Interfaces;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Connect;

// Low-level HTTP access to the quote API.
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

        // Without the trailing "/", "https://brapi.dev/api/quote" + "PETR4" would become ".../api/PETR4".
        var baseUrl = settings.BaseUrl.EndsWith('/') ? settings.BaseUrl : settings.BaseUrl + "/";

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            throw new ArgumentException(
                "The API base URL must be a valid absolute URL.",
                nameof(settings.BaseUrl));
        }

        _settings = settings;
        _http = new HttpClient(handler ?? throw new ArgumentNullException(nameof(handler))) { BaseAddress = baseUri };
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

        // Normalize the symbol: trim whitespace and convert to uppercase.
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var encodedSymbol = Uri.EscapeDataString(normalizedSymbol);

        // Gets the quote for a single symbol.
        using var request = new HttpRequestMessage(HttpMethod.Get, encodedSymbol);

        if (!string.IsNullOrWhiteSpace(_settings.Token))
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _settings.Token);
        }

        // The response is disposed at the end of this method, after the content is read.
        using var response = await _http.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }
}
