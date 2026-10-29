using System.Net;
using StockQuoteAlert.Connect;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Tests;

public class ApiConnectTests
{
    private static ApiSettings Settings(string baseUrl, string token = "") =>
        new() { BaseUrl = baseUrl, Token = token };

    [Theory]
    [InlineData("https://brapi.dev/api/quote/")]
    [InlineData("https://brapi.dev/api/quote")] // without the trailing "/" the URL must stay the same
    public async Task GetRawQuoteAsync_BuildsUrlWithSymbol(string baseUrl)
    {
        var handler = new FakeHttpHandler(HttpStatusCode.OK, "{}");
        var api = new ApiConnect(Settings(baseUrl), handler);

        await api.GetRawQuoteAsync(" petr4 ");

        Assert.Equal("https://brapi.dev/api/quote/PETR4", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
    }

    [Fact]
    public async Task GetRawQuoteAsync_WithToken_SendsBearerHeader()
    {
        var handler = new FakeHttpHandler(HttpStatusCode.OK, "{}");
        var api = new ApiConnect(Settings("https://brapi.dev/api/quote/", "abc"), handler);

        await api.GetRawQuoteAsync("PETR4");

        Assert.Equal("Bearer", handler.LastRequest!.Headers.Authorization!.Scheme);
        Assert.Equal("abc", handler.LastRequest.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task GetRawQuoteAsync_WithoutToken_SendsNoAuthHeader()
    {
        var handler = new FakeHttpHandler(HttpStatusCode.OK, "{}");
        var api = new ApiConnect(Settings("https://brapi.dev/api/quote/"), handler);

        await api.GetRawQuoteAsync("PETR4");

        Assert.Null(handler.LastRequest!.Headers.Authorization);
    }

    [Fact]
    public async Task GetRawQuoteAsync_ReturnsBody()
    {
        var api = new ApiConnect(Settings("https://brapi.dev/api/quote/"), new FakeHttpHandler(HttpStatusCode.OK, "body"));
        Assert.Equal("body", await api.GetRawQuoteAsync("PETR4"));
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task GetRawQuoteAsync_HttpError_Throws(HttpStatusCode status)
    {
        var api = new ApiConnect(Settings("https://brapi.dev/api/quote/"), new FakeHttpHandler(status, ""));
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
        var api = new ApiConnect(Settings("https://brapi.dev/api/quote/"), new FakeHttpHandler(HttpStatusCode.OK, "{}"));
        await Assert.ThrowsAsync<ArgumentException>(() => api.GetRawQuoteAsync(""));
    }
}
