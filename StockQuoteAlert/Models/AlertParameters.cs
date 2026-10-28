using System.Globalization;

namespace StockQuoteAlert.Models;

// The 3 command line parameters.
// Validation is done in the Parse method, which guarantees that:
//  - the symbol is not empty;
//  - both prices are positive numbers;
//  - the sell price is greater than the buy price.
public record AlertParameters(string Symbol, decimal SellPrice, decimal BuyPrice)
{
    public const string Usage =
        "\n usage: stock-quote-alert <STOCK> <SELL_PRICE> <BUY_PRICE>";

    public static AlertParameters Parse(string[] args)
    {
        if (args is null || args.Length != 3)
        {
            throw new ArgumentException($"Null or invalid number of arguments. {Usage}");
        }

        var symbol = args[0].Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException($"The symbol cannot be empty. {Usage}");
        }

        var sellPrice = ParsePrice(args[1], "sell price");
        var buyPrice = ParsePrice(args[2], "buy price");

        if (sellPrice <= buyPrice)
        {
            throw new ArgumentException(
                $"The sell price ({sellPrice}) must be greater than the buy price ({buyPrice}).");
        }

        return new AlertParameters(symbol, sellPrice, buyPrice);
    }

    private static decimal ParsePrice(string value, string name)
    {
        // InvariantCulture: "22.67" is always 22.67, even on a pt-BR Windows.
        // AllowDecimalPoint only: rejects "22,67" and "1.000,50" instead of misreading them.
        if (!decimal.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var price)
            || price <= 0)
        {
            throw new ArgumentException(
                $"Invalid {name} '{value}'. Use a positive number with a decimal point (e.g. 22.67).");
        }

        return price;
    }
}
