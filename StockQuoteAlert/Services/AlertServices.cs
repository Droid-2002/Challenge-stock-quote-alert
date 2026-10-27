using StockQuoteAlert.Interfaces;

namespace StockQuoteAlert.Services;

public enum AlertState
{
    Neutral, // price between buy and sell references: no alert
    Buy,     // price below the buy reference
    Sell     // price above the sell reference
}

// Pure decision rule: no HTTP, no SMTP. Easy to unit test.
public static class AlertServices
{
    public static AlertState Evaluate(decimal price, AlertParameters parameters)
    {
        if (price > parameters.SellPrice)
        {
            return AlertState.Sell;
        }

        if (price < parameters.BuyPrice)
        {
            return AlertState.Buy;
        }

        return AlertState.Neutral;
    }
}
