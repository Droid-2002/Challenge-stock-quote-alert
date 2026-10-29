using StockQuoteAlert.Models;
using StockQuoteAlert.Services;

namespace StockQuoteAlert.Tests;

public class AlertServicesTests
{
    private static readonly AlertParameters Params = new("PETR4", SellPrice: 22.67m, BuyPrice: 22.59m);

    [Theory]
    [InlineData("22.68", AlertState.Sell)]
    [InlineData("22.58", AlertState.Buy)]
    [InlineData("22.63", AlertState.Neutral)]
    [InlineData("22.67", AlertState.Neutral)] // exactly the sell price: no alert (strict >)
    [InlineData("22.59", AlertState.Neutral)] // exactly the buy price: no alert (strict <)
    public void Evaluate_ReturnsExpectedState(string price, AlertState expected) =>
        Assert.Equal(expected, AlertServices.Evaluate(decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture), Params));
}
