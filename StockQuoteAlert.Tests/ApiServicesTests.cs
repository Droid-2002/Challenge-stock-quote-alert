using System.Text.Json;
using StockQuoteAlert.Services;

namespace StockQuoteAlert.Tests;

public class ApiServicesTests
{
    [Fact]
    public async Task GetQuoteAsync_ValidResponse_ReturnsStock()
    {
        var service = new ApiServices(new FakeApiClient("""{"results":[{"symbol":"PETR4","regularMarketPrice":22.65}]}"""));

        var stock = await service.GetQuoteAsync("petr4");

        Assert.Equal("PETR4", stock.Symbol); // symbol from the API, not the one requested
        Assert.Equal(22.65m, stock.Price);
    }

    [Theory]
    [InlineData("""{}""")]
    [InlineData("""{"results":[]}""")]
    [InlineData("""{"results":{}}""")]
    [InlineData("""{"results":[{"regularMarketPrice":22.65}]}""")]
    [InlineData("""{"results":[{"symbol":"","regularMarketPrice":22.65}]}""")]
    [InlineData("""{"results":[{"symbol":"PETR4"}]}""")]
    [InlineData("""{"results":[{"symbol":"PETR4","regularMarketPrice":"22.65"}]}""")]
    [InlineData("""{"results":[{"symbol":"PETR4","regularMarketPrice":0}]}""")]
    [InlineData("""{"results":[{"symbol":"PETR4","regularMarketPrice":-1}]}""")]
    public async Task GetQuoteAsync_InvalidResponse_Throws(string json)
    {
        var service = new ApiServices(new FakeApiClient(json));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetQuoteAsync("PETR4"));
    }

    [Fact]
    public async Task GetQuoteAsync_NotJson_ThrowsJsonException()
    {
        var service = new ApiServices(new FakeApiClient("<html>error</html>"));
        await Assert.ThrowsAnyAsync<JsonException>(() => service.GetQuoteAsync("PETR4"));
    }

    [Fact]
    public async Task GetQuoteAsync_EmptySymbol_ThrowsWithoutCallingApi()
    {
        var api = new FakeApiClient("{}");
        var service = new ApiServices(api);

        await Assert.ThrowsAsync<ArgumentException>(() => service.GetQuoteAsync(" "));
        Assert.Equal(0, api.Calls);
    }

    [Fact]
    public void Constructor_NullClient_Throws() =>
        Assert.Throws<ArgumentNullException>(() => new ApiServices(null!));
}
