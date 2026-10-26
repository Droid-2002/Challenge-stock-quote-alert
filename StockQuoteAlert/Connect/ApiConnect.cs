using StockQuoteAlert.Interfaces;

namespace StockQuoteAlert.Connect;

// Low-level HTTP access to the quote API.
public class ApiConnect
{
    private readonly HttpClient _http;
    private readonly ApiSettings _settings;

    public ApiConnect(ApiSettings settings)
    {
        _settings = settings;
        _http = new HttpClient { BaseAddress = new Uri(settings.BaseUrl) };
    }

    public Task<string> GetRawQuoteAsync(string symbol, CancellationToken ct = default)
    {
        // request (symbol + token from _settings) and return the response body.
        throw new NotImplementedException();
    }
}
