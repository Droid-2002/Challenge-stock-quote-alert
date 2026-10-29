using System.Text.Json;
using StockQuoteAlert.Services;

namespace StockQuoteAlert.Tests;

public class ApiServicesTests
{
    // Trimmed version of a real Yahoo Finance chart response.
    private const string ValidJson =
        """{"chart":{"result":[{"meta":{"currency":"BRL","symbol":"PETR4.SA","regularMarketTime":1793282554,"regularMarketPrice":52.72}}],"error":null}}""";

    [Fact]
    public async Task GetQuoteAsync_ValidResponse_ReturnsStock()
    {
        var stock = await new ApiServices(new FakeApiClient(ValidJson)).GetQuoteAsync("petr4");

        Assert.Equal("PETR4.SA", stock.Symbol); // symbol from the API, not the one requested
        Assert.Equal(52.72m, stock.Price);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1793282554).UtcDateTime, stock.UpdatedAt); // quote time, not request time
    }

    [Fact]
    public async Task GetQuoteAsync_NoMarketTime_UsesNow()
    {
        var json = """{"chart":{"result":[{"meta":{"symbol":"PETR4.SA","regularMarketPrice":52.72}}],"error":null}}""";
        var before = DateTime.UtcNow;

        var stock = await new ApiServices(new FakeApiClient(json)).GetQuoteAsync("PETR4");

        Assert.InRange(stock.UpdatedAt, before, DateTime.UtcNow);
    }

    [Fact]
    public async Task GetQuoteAsync_ApiError_ThrowsWithDescription()
    {
        var json = """{"chart":{"result":null,"error":{"code":"Not Found","description":"No data found, symbol may be delisted"}}}""";

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => new ApiServices(new FakeApiClient(json)).GetQuoteAsync("XXXX99"));

        Assert.Contains("No data found, symbol may be delisted", ex.Message);
    }

    [Theory]
    [InlineData("""{}""")]
    [InlineData("""{"chart":[]}""")]
    [InlineData("""{"chart":{"result":[],"error":null}}""")]
    [InlineData("""{"chart":{"result":null,"error":null}}""")]
    [InlineData("""{"chart":{"result":[{}],"error":null}}""")]
    [InlineData("""{"chart":{"result":[{"meta":{"regularMarketPrice":52.72}}],"error":null}}""")]
    [InlineData("""{"chart":{"result":[{"meta":{"symbol":"","regularMarketPrice":52.72}}],"error":null}}""")]
    [InlineData("""{"chart":{"result":[{"meta":{"symbol":"PETR4.SA"}}],"error":null}}""")]
    [InlineData("""{"chart":{"result":[{"meta":{"symbol":"PETR4.SA","regularMarketPrice":"52.72"}}],"error":null}}""")]
    [InlineData("""{"chart":{"result":[{"meta":{"symbol":"PETR4.SA","regularMarketPrice":0}}],"error":null}}""")]
    [InlineData("""{"chart":{"result":[{"meta":{"symbol":"PETR4.SA","regularMarketPrice":-1}}],"error":null}}""")]
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
