using System.Net;
using StockQuoteAlert.Interfaces;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Tests;

// In-memory replacements for the HTTP and SMTP layers: no network in the tests.

// Returns the queued items in order: a decimal becomes a Stock, an Exception is thrown.
internal class FakeQuoteService : IQuoteService
{
    private readonly Queue<object> _responses = new();
    public int Calls { get; private set; }

    public FakeQuoteService Returns(params object[] responses)
    {
        foreach (var r in responses) _responses.Enqueue(r);
        return this;
    }

    public Task<Stock> GetQuoteAsync(string symbol, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        Calls++;
        var next = _responses.Dequeue();
        if (next is Exception ex) throw ex;
        return Task.FromResult(new Stock(symbol, (decimal)next, DateTime.UtcNow));
    }
}

internal class FakeNotifier : IAlertNotifier
{
    public List<string> Sent { get; } = new();
    public int FailuresLeft { get; set; }

    public Task NotifyBuyAsync(Stock stock, decimal buyPrice, CancellationToken ct = default) => Record("Buy");
    public Task NotifySellAsync(Stock stock, decimal sellPrice, CancellationToken ct = default) => Record("Sell");

    private Task Record(string kind)
    {
        if (FailuresLeft > 0)
        {
            FailuresLeft--;
            throw new InvalidOperationException("SMTP down");
        }

        Sent.Add(kind);
        return Task.CompletedTask;
    }
}

internal class FakeApiClient(string json) : IQuoteApiClient
{
    public int Calls { get; private set; }

    public Task<string> GetRawQuoteAsync(string symbol, CancellationToken ct = default)
    {
        Calls++;
        return Task.FromResult(json);
    }
}

internal class FakeEmailSender : IEmailSender
{
    public List<(string From, string To, string Subject, string Body)> Sent { get; } = new();

    public Task SendAsync(string from, string to, string subject, string body, CancellationToken ct = default)
    {
        Sent.Add((from, to, subject, body));
        return Task.CompletedTask;
    }
}

// Replaces the network under HttpClient and records the last request.
internal class FakeHttpHandler(HttpStatusCode status, string body) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        LastRequest = request;
        return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
    }
}
