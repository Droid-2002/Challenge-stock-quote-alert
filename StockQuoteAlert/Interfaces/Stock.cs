namespace StockQuoteAlert.Interfaces;

public record Stock(string Symbol, decimal Price, DateTime UpdatedAt);
