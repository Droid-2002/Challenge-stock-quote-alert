using StockQuoteAlert.Connect;
using StockQuoteAlert.Models;
using StockQuoteAlert.Services;

namespace StockQuoteAlert.Tests;

public class EmailServicesTests
{
    private static readonly EmailSettings Settings = new() { From = "from@x.com", To = "to@x.com" };
    private static readonly Stock Stock = new("PETR4", 22.50m, DateTime.UtcNow);

    [Fact]
    public async Task NotifyBuyAsync_SendsBuyEmail()
    {
        var sender = new FakeEmailSender();
        await new EmailServices(sender, Settings).NotifyBuyAsync(Stock, 22.59m);

        var email = Assert.Single(sender.Sent);
        Assert.Equal("from@x.com", email.From);
        Assert.Equal("to@x.com", email.To);
        Assert.Equal("[StockQuoteAlert] Buy: PETR4", email.Subject);
        Assert.Contains("BUY", email.Body);
        Assert.Contains(22.59m.ToString("F2"), email.Body);
    }

    [Fact]
    public async Task NotifySellAsync_SendsSellEmail()
    {
        var sender = new FakeEmailSender();
        await new EmailServices(sender, Settings).NotifySellAsync(Stock, 22.67m);

        var email = Assert.Single(sender.Sent);
        Assert.Equal("[StockQuoteAlert] Sell: PETR4", email.Subject);
        Assert.Contains("SELL", email.Body);
        Assert.Contains(22.67m.ToString("F2"), email.Body);
    }

    [Theory]
    [InlineData("", "to@x.com")]
    [InlineData("from@x.com", " ")]
    public void Constructor_MissingAddress_Throws(string from, string to) =>
        Assert.Throws<ArgumentException>(() => new EmailServices(new FakeEmailSender(), new EmailSettings { From = from, To = to }));

    [Fact]
    public void Constructor_NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new EmailServices(null!, Settings));
        Assert.Throws<ArgumentNullException>(() => new EmailServices(new FakeEmailSender(), null!));
    }
}

public class EmailConnectTests
{
    private static SmtpSettings Smtp(string host = "localhost", int port = 587) => new() { Host = host, Port = port };

    [Theory]
    [InlineData("", 587)]
    [InlineData("smtp.x.com", 0)]
    [InlineData("smtp.x.com", 70000)]
    public void Constructor_InvalidSettings_Throws(string host, int port) =>
        Assert.Throws<ArgumentException>(() => new EmailConnect(Smtp(host, port)));

    [Fact]
    public async Task SendAsync_InvalidAddress_ThrowsSendEmailException()
    {
        // MailMessage rejects the address before any network call.
        var ex = await Assert.ThrowsAsync<SendEmailException>(
            () => new EmailConnect(Smtp()).SendAsync("not-an-email", "to@x.com", "s", "b"));

        Assert.IsType<FormatException>(ex.InnerException);
    }

    [Fact]
    public async Task SendAsync_Cancelled_ThrowsOperationCanceled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => new EmailConnect(Smtp()).SendAsync("from@x.com", "to@x.com", "s", "b", cts.Token));
    }
}
