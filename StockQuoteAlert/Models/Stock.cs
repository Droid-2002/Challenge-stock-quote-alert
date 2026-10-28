namespace StockQuoteAlert.Models;

public record Stock(string Symbol, decimal Price, DateTime UpdatedAt);
