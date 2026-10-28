using System.Text.Json;
using StockQuoteAlert.Connect;
using StockQuoteAlert.Models;
using StockQuoteAlert.Services;

// Ctrl+C cancels the token instead of killing the process, so the loop ends cleanly.
using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    var parameters = AlertParameters.Parse(args);

    var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    var config = AppConfig.Load(configPath);

    var apiServices = new ApiServices(new ApiConnect(config.Api));
    var emailServices = new EmailServices(new EmailConnect(config.Smtp), config.Email);
    var monitor = new MonitorServices(apiServices, emailServices, parameters, config.Api.IntervalSeconds);

    Console.WriteLine(
        $"Monitoring {parameters.Symbol} every {config.Api.IntervalSeconds}s. " +
        $"Sell above {parameters.SellPrice:F2}, buy below {parameters.BuyPrice:F2}. Ctrl+C to exit.");

    await monitor.RunAsync(cts.Token);
}
catch (OperationCanceledException) when (cts.IsCancellationRequested)
{
    Console.WriteLine("Monitoring ended.");
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Invalid parameters or configuration: {ex.Message}");
    Environment.ExitCode = 1;
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine(ex.Message);
    Environment.ExitCode = 1;
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"The appsettings.json file is not valid JSON: {ex.Message}");
    Environment.ExitCode = 1;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"Error reading the configuration: {ex.Message}");
    Environment.ExitCode = 1;
}
