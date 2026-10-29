using StockQuoteAlert.Models;
using StockQuoteAlert.Services;

namespace StockQuoteAlert.Tests;

public class MonitorServicesTests
{
    // Sell above 22.67, buy below 22.59. 23 = Sell, 22 = Buy, 22.63 = Neutral.
    private static readonly AlertParameters Params = new("PETR4", 22.67m, 22.59m);

    private static async Task<FakeNotifier> RunChecks(params object[] quotes) =>
        await RunChecks(new FakeNotifier(), quotes);

    private static async Task<FakeNotifier> RunChecks(FakeNotifier notifier, params object[] quotes)
    {
        var monitor = new MonitorServices(new FakeQuoteService().Returns(quotes), notifier, Params, 60);
        foreach (var _ in quotes) await monitor.CheckOnceAsync(CancellationToken.None);
        return notifier;
    }

    [Fact]
    public async Task Neutral_SendsNothing() =>
        Assert.Empty((await RunChecks(22.63m)).Sent);

    [Fact]
    public async Task AboveSell_SendsSell() =>
        Assert.Equal(["Sell"], (await RunChecks(23m)).Sent);

    [Fact]
    public async Task BelowBuy_SendsBuy() =>
        Assert.Equal(["Buy"], (await RunChecks(22m)).Sent);

    [Fact]
    public async Task SameStateRepeated_SendsOnlyOnce() =>
        Assert.Equal(["Sell"], (await RunChecks(23m, 23.5m, 24m)).Sent);

    [Fact]
    public async Task BackToNeutralThenCrossAgain_SendsAgain() =>
        Assert.Equal(["Sell", "Sell"], (await RunChecks(23m, 22.63m, 23m)).Sent);

    [Fact]
    public async Task SellThenBuy_SendsBoth() =>
        Assert.Equal(["Sell", "Buy"], (await RunChecks(23m, 22m)).Sent);

    [Fact]
    public async Task EmailFailure_IsRetriedOnNextCheck()
    {
        var notifier = await RunChecks(new FakeNotifier { FailuresLeft = 1 }, 23m, 23m);
        Assert.Equal(["Sell"], notifier.Sent);
    }

    [Fact]
    public async Task ApiFailure_IsLoggedAndMonitoringContinues()
    {
        var notifier = await RunChecks(new HttpRequestException("offline"), 23m);
        Assert.Equal(["Sell"], notifier.Sent);
    }

    [Fact]
    public async Task ApiFailure_DoesNotResetState() =>
        Assert.Equal(["Sell"], (await RunChecks(23m, new HttpRequestException("offline"), 23m)).Sent);

    [Fact]
    public async Task CheckOnceAsync_Cancelled_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var monitor = new MonitorServices(new FakeQuoteService(), new FakeNotifier(), Params, 60);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => monitor.CheckOnceAsync(cts.Token));
    }

    [Fact]
    public async Task RunAsync_ChecksImmediatelyAndStopsOnCancel()
    {
        using var cts = new CancellationTokenSource();
        var quotes = new FakeQuoteService().Returns(23m);
        var notifier = new FakeNotifier();
        var monitor = new MonitorServices(quotes, notifier, Params, 3600);

        var run = monitor.RunAsync(cts.Token);
        await Task.Delay(200);
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => run);
        Assert.Equal(1, quotes.Calls);      // first check runs before the first interval
        Assert.Equal(["Sell"], notifier.Sent);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidInterval_Throws(int interval) =>
        Assert.Throws<ArgumentException>(() => new MonitorServices(new FakeQuoteService(), new FakeNotifier(), Params, interval));

    [Fact]
    public void Constructor_NullDependencies_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new MonitorServices(null!, new FakeNotifier(), Params, 60));
        Assert.Throws<ArgumentNullException>(() => new MonitorServices(new FakeQuoteService(), null!, Params, 60));
        Assert.Throws<ArgumentNullException>(() => new MonitorServices(new FakeQuoteService(), new FakeNotifier(), null!, 60));
    }
}
