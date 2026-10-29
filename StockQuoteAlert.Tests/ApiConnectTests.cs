using System.Net;
using StockQuoteAlert.Connect;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Tests;

public class ApiConnectTests
{
    private const string Yahoo = "https://query1.finance.yahoo.com/v8/finance/chart/";

    private static ApiSettings Settings(string baseUrl = Yahoo, string suffix = ".SA", string userAgent = "stock-quote-alert/1.0") =>
        new() { BaseUrl = baseUrl, SymbolSuffix = suffix, UserAgent = userAgent };

    private static async Task<HttpRequestMessage> SendAndCapture(ApiSettings settings, string symbol)
    {
        var handler = new FakeHttpHandler(HttpStatusCode.OK, "{}");
        await new ApiConnect(settings, handler).GetRawQuoteAsync(symbol);
        return handler.LastRequest!;
    }

    [Theory]
    [InlineData(Yahoo)]
    [InlineData("https://query1.finance.yahoo.com/v8/finance/chart")] // without the trailing "/" the URL must stay the same
    public async Task GetRawQuoteAsync_BuildsYahooUrl(string baseUrl)
    {
        var request = await SendAndCapture(Settings(baseUrl), " petr4 ");

        Assert.Equal(Yahoo + "PETR4.SA?interval=1d&range=1d", request.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Get, request.Method);
    }

    [Theory]
    [InlineData("PETR4.SA", ".SA", "PETR4.SA")] // suffix already present: not duplicated
    [InlineData("AAPL", "", "AAPL")]            // no suffix configured
    [InlineData("vale3", ".sa", "VALE3.SA")]
    public async Task GetRawQuoteAsync_AppliesSymbolSuffix(string symbol, string suffix, string expected)
    {
        var request = await SendAndCapture(Settings(suffix: suffix), symbol);
        Assert.Equal(Yahoo + expected + "?interval=1d&range=1d", request.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetRawQuoteAsync_SendsUserAgent()
    {
        var request = await SendAndCapture(Settings(), "PETR4");
        Assert.Contains("stock-quote-alert/1.0", request.Headers.UserAgent.ToString());
    }

    [Fact]
    public async Task GetRawQuoteAsync_ReturnsBody()
    {
        var api = new ApiConnect(Settings(), new FakeHttpHandler(HttpStatusCode.OK, "body"));
        Assert.Equal("body", await api.GetRawQuoteAsync("PETR4"));
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]           // unknown symbol
    [InlineData(HttpStatusCode.TooManyRequests)]    // rate limit
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetRawQuoteAsync_HttpError_Throws(HttpStatusCode status)
    {
        var api = new ApiConnect(Settings(), new FakeHttpHandler(status, ""));
        await Assert.ThrowsAsync<HttpRequestException>(() => api.GetRawQuoteAsync("PETR4"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a url")]
    public void Constructor_InvalidBaseUrl_Throws(string baseUrl) =>
        Assert.Throws<ArgumentException>(() => new ApiConnect(Settings(baseUrl)));

    [Fact]
    public async Task GetRawQuoteAsync_EmptySymbol_Throws()
    {
        var api = new ApiConnect(Settings(), new FakeHttpHandler(HttpStatusCode.OK, "{}"));
        await Assert.ThrowsAsync<ArgumentException>(() => api.GetRawQuoteAsync(""));
    }
}
