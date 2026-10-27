using System.Text.Json;
using StockQuoteAlert.Connect;
using StockQuoteAlert.Interfaces;
using StockQuoteAlert.Services;

try
{
    var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    var config = AppConfig.Load(configPath);

    var symbol = args.Length > 0 ? args[0] : "PETR4";

    var apiConnect = new ApiConnect(config.Api);
    var apiServices = new ApiServices(apiConnect);

    var stock = await apiServices.GetQuoteAsync(symbol);

    Console.WriteLine($"Symbol: {stock.Symbol}");
    Console.WriteLine($"Price: {stock.Price:F2}");
    Console.WriteLine($"Updated at: {stock.UpdatedAt:O}");
}
catch (ArgumentException ex)
{
    Console.Error.WriteLine($"Invalid configuration or argument: {ex.Message}");
    Environment.ExitCode = 1;
}
catch (HttpRequestException ex)
{
    Console.Error.WriteLine($"The quote request failed: {ex.Message}");
    Environment.ExitCode = 1;
}
catch (InvalidOperationException ex)
{
    Console.Error.WriteLine($"The API returned invalid quote data: {ex.Message}");
    Environment.ExitCode = 1;
}
catch (JsonException ex)
{
    Console.Error.WriteLine($"The API returned invalid JSON: {ex.Message}");
    Environment.ExitCode = 1;
}