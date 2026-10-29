using StockQuoteAlert.Models;

namespace StockQuoteAlert.Tests;

public class AlertParametersTests
{
    [Fact]
    public void Parse_ValidArgs_ReturnsNormalizedParameters()
    {
        var p = AlertParameters.Parse([" petr4 ", "22.67", "22.59"]);

        Assert.Equal("PETR4", p.Symbol);
        Assert.Equal(22.67m, p.SellPrice);
        Assert.Equal(22.59m, p.BuyPrice);
    }

    [Fact]
    public void Parse_IntegerPrices_AreAccepted()
    {
        var p = AlertParameters.Parse(["PETR4", "30", "20"]);

        Assert.Equal(30m, p.SellPrice);
        Assert.Equal(20m, p.BuyPrice);
    }

    [Theory]
    [InlineData()]
    [InlineData("PETR4")]
    [InlineData("PETR4", "22.67")]
    [InlineData("PETR4", "22.67", "22.59", "extra")]
    public void Parse_WrongNumberOfArgs_Throws(params string[] args) =>
        Assert.Throws<ArgumentException>(() => AlertParameters.Parse(args));

    [Fact]
    public void Parse_NullArgs_Throws() =>
        Assert.Throws<ArgumentException>(() => AlertParameters.Parse(null!));

    [Fact]
    public void Parse_EmptySymbol_Throws() =>
        Assert.Throws<ArgumentException>(() => AlertParameters.Parse(["  ", "22.67", "22.59"]));

    [Theory]
    [InlineData("22,67")]    // comma: rejected instead of misread
    [InlineData("1.000,50")]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("1e3")]
    public void Parse_InvalidSellPrice_Throws(string sell) =>
        Assert.Throws<ArgumentException>(() => AlertParameters.Parse(["PETR4", sell, "10"]));

    [Theory]
    [InlineData("22.59", "22.67")] // sell < buy
    [InlineData("22.67", "22.67")] // sell == buy
    public void Parse_SellNotGreaterThanBuy_Throws(string sell, string buy) =>
        Assert.Throws<ArgumentException>(() => AlertParameters.Parse(["PETR4", sell, buy]));
}
