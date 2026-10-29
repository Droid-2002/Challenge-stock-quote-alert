using System.Text.Json;
using StockQuoteAlert.Models;

namespace StockQuoteAlert.Tests;

public class AppConfigTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"appsettings-{Guid.NewGuid()}.json");

    public void Dispose() => File.Delete(_path);

    [Fact]
    public void Load_MissingFile_ThrowsFileNotFound() =>
        Assert.Throws<FileNotFoundException>(() => AppConfig.Load(_path));

    [Fact]
    public void Load_ValidFile_ReadsValuesCaseInsensitive()
    {
        File.WriteAllText(_path, """
            {
              "email": { "from": "a@x.com", "to": "b@x.com" },
              "smtp":  { "host": "smtp.x.com", "port": 465, "enableSsl": false, "user": "u", "password": "p" },
              "api":   { "baseUrl": "https://brapi.dev/api/quote/", "token": "t", "intervalSeconds": 30 }
            }
            """);

        var config = AppConfig.Load(_path);

        Assert.Equal("a@x.com", config.Email.From);
        Assert.Equal("b@x.com", config.Email.To);
        Assert.Equal("smtp.x.com", config.Smtp.Host);
        Assert.Equal(465, config.Smtp.Port);
        Assert.False(config.Smtp.EnableSsl);
        Assert.Equal("https://brapi.dev/api/quote/", config.Api.BaseUrl);
        Assert.Equal("t", config.Api.Token);
        Assert.Equal(30, config.Api.IntervalSeconds);
    }

    [Fact]
    public void Load_MissingKeys_UsesDefaults()
    {
        File.WriteAllText(_path, "{}");

        var config = AppConfig.Load(_path);

        Assert.Equal(587, config.Smtp.Port);
        Assert.True(config.Smtp.EnableSsl);
        Assert.Equal(60, config.Api.IntervalSeconds);
        Assert.Equal("", config.Email.From);
    }

    [Fact]
    public void Load_InvalidJson_ThrowsJsonException()
    {
        File.WriteAllText(_path, "{ not json");
        Assert.ThrowsAny<JsonException>(() => AppConfig.Load(_path));
    }

    [Fact]
    public void Load_NullJson_ThrowsInvalidOperation()
    {
        File.WriteAllText(_path, "null");
        Assert.Throws<InvalidOperationException>(() => AppConfig.Load(_path));
    }
}
