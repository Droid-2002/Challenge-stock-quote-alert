using StockQuoteAlert.Interfaces;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Services;

// Monitoring loop: fetch quote -> evaluate -> send email when the state changes.
public class MonitorServices
{
    private readonly IQuoteService _api;
    private readonly EmailServices _email;
    private readonly AlertParameters _parameters;
    private readonly TimeSpan _interval;

    // Last state that was already notified. Avoids sending the same alert on every check.
    private AlertState _lastState = AlertState.Neutral;

    public MonitorServices(IQuoteService api, EmailServices email, AlertParameters parameters, int intervalSeconds)
    {
        if (intervalSeconds <= 0)
        {
            throw new ArgumentException("Api.IntervalSeconds must be greater than zero.", nameof(intervalSeconds));
        }

        _api = api ?? throw new ArgumentNullException(nameof(api));
        _email = email ?? throw new ArgumentNullException(nameof(email));
        _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _interval = TimeSpan.FromSeconds(intervalSeconds);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(_interval);

        // do/while: check right away, then once per interval, until Ctrl+C.
        do
        {
            await CheckOnceAsync(ct);
        }
        while (await timer.WaitForNextTickAsync(ct));
    }

    private async Task CheckOnceAsync(CancellationToken ct)
    {
        try
        {
            var stock = await _api.GetQuoteAsync(_parameters.Symbol, ct);
            var state = AlertServices.Evaluate(stock.Price, _parameters);

            Log($"{stock.Symbol} = {stock.Price:F2} (sell > {_parameters.SellPrice:F2} | buy < {_parameters.BuyPrice:F2}) -> {state}");

            if (state == _lastState)
            {
                return;
            }

            if (state == AlertState.Sell)
            {
                await _email.SendSellAlertAsync(stock, _parameters.SellPrice, ct);
                Log("SELL email sent.");
            }
            else if (state == AlertState.Buy)
            {
                await _email.SendBuyAlertAsync(stock, _parameters.BuyPrice, ct);
                Log("BUY email sent.");
            }

            // Only updated after a successful send: if the email fails, it is retried on the next check.
            _lastState = state;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // Ctrl+C: stop the loop.
        }
        catch (Exception ex)
        {
            // Network, API or SMTP failure: log and keep monitoring.
            var detail = ex.InnerException is null ? "" : $" ({ex.InnerException.Message})";
            Console.Error.WriteLine($"[{DateTime.Now:HH:mm:ss}] Error: {ex.Message}{detail}");
        }
    }

    private static void Log(string message) =>
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
}
