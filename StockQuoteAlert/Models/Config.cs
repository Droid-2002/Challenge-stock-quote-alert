using System.Text.Json;

namespace StockQuoteAlert.Models;

public class AppConfig
{
    public EmailSettings Email { get; set; } = new();
    public SmtpSettings Smtp { get; set; } = new();
    public ApiSettings Api { get; set; } = new();

// Reads the configuration from the JSON config file and returns an AppConfig object.
//  Called from StockQuoteAlert\Program.cs
    public static AppConfig Load(string path) 
    {
        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Config file not found: {path}. Copy appsettings.example.json to appsettings.json."); 

        var json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        return JsonSerializer.Deserialize<AppConfig>(json, options)
            ?? throw new InvalidOperationException("Could not read config file.");
    }
}

public class EmailSettings // Email settings for sending email alerts. Called from StockQuoteAlert\appsettings.example.json
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
}

public class SmtpSettings // SMTP settings for sending email alerts.
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string User { get; set; } = "";
    public string Password { get; set; } = "";
}

public class ApiSettings // API settings for fetching stock quotes.
{
    public string BaseUrl { get; set; } = ""; 
    public string Token { get; set; } = "";
    public int IntervalSeconds { get; set; } = 60; // Default to 60 seconds between API calls.
}
