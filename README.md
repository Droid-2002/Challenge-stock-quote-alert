# Stock Quote Alert

> **Note:** This project was developed as part of a selective process.

![CI](https://github.com/Droid-2002/Challenge-stock-quote-alert/actions/workflows/ci.yml/badge.svg)

A .NET 10 console application that continuously monitors a B3 stock and sends an email alert when the price rises above a **sell** threshold or drops below a **buy** threshold.

```bash
stock-quote-alert PETR4 22.67 22.59
```

## Features

- Continuous monitoring at a configurable interval
- **Sell** email when the price goes above the sell price, **buy** email when it drops below the buy price
- No duplicate alerts: an email is sent only when the state changes (neutral → sell, sell → buy, ...)
- Keeps running on network, API or SMTP failures; a failed email is retried on the next check
- Clean shutdown with `Ctrl+C`
- Input and configuration validated before any request (clear messages, exit code `1`)
- Unit tests (xUnit) run automatically by GitHub Actions on every push and pull request to `main`

## Setup

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

1. Clone the repository.
2. In the `StockQuoteAlert` directory, copy the example configuration:
   ```bash
   cp appsettings.example.json appsettings.json
   ```
3. Fill in `appsettings.json` with your SMTP server details and the alert recipient.

```json
{
  "Email": { "From": "sender@gmail.com", "To": "alerts@example.com" },
  "Smtp":  { "Host": "smtp.gmail.com", "Port": 587, "EnableSsl": true,
             "User": "sender@gmail.com", "Password": "app-password" },
  "Api":   { "BaseUrl": "https://query1.finance.yahoo.com/v8/finance/chart/",
             "SymbolSuffix": ".SA", "UserAgent": "stock-quote-alert/1.0", "IntervalSeconds": 60 }
}
```
> **Note:** `appsettings.json` is in `.gitignore`, so your credentials are never committed.

### Quote API (Yahoo Finance)

Quotes come from the public **Yahoo Finance chart endpoint**. No account or token is needed.

> **Note:** this endpoint is public but not officially documented or supported by Yahoo, so it may change or be rate limited without notice. Only `Connect/ApiConnect.cs` (request) and `Services/ApiServices.cs` (JSON parsing) depend on it.

## Usage

The application takes 3 arguments: `[stock] [sell_price] [buy_price]`. Use a dot as the decimal separator; the sell price must be greater than the buy price.

**With `dotnet run`** (from the `StockQuoteAlert` directory):
```bash
dotnet run -- PETR4 22.67 22.59
```

**As an executable**:
```bash
dotnet publish -c Release -o ../publish
cd ../publish
./stock-quote-alert PETR4 22.67 22.59      # stock-quote-alert.exe on Windows
```
Keep the filled-in `appsettings.json` next to the executable.

To stop the application, press `Ctrl+C`.

## Tests and CI

The `StockQuoteAlert.Tests` project (xUnit) covers argument parsing, configuration loading, the alert rule, the Yahoo request and JSON parsing, the email content and the monitoring loop (no duplicate alerts, retry after an email failure, recovery after an API failure, `Ctrl+C`). The API and SMTP layers are replaced by in-memory fakes, so the tests need no network, no credentials and no `appsettings.json`.

Run them from the repository root:
```bash
dotnet test
```

**Continuous integration:** the GitHub Actions workflow [`.github/workflows/ci.yml`](.github/workflows/ci.yml) runs on every push and pull request to `main`, on `ubuntu-latest` with .NET 10:

1. `dotnet restore`
2. `dotnet build --no-restore -c Release`
3. `dotnet test --no-build -c Release`

A build error or a failing test fails the workflow; the badge at the top of this README shows the status of the last run.

## API Credits and references

- **Quote data:** [Yahoo Finance](https://finance.yahoo.com/) (public chart endpoint `query1.finance.yahoo.com/v8/finance/chart`)
- **SMTP:** [`System.Net.Mail.SmtpClient`](https://learn.microsoft.com/dotnet/api/system.net.mail.smtpclient), [dotnet/runtime — System.Net.Mail](https://github.com/dotnet/runtime/tree/main/src/libraries/System.Net.Mail), [jstedfast/MailKit](https://github.com/jstedfast/MailKit).

## AI usage

As allowed by the challenge, AI was used as a support tool. **The core logic, the architecture and the technical decisions are my own**, and every suggestion was reviewed, adapted and tested by me.
 
### Commit messages

To make code changes clearer, both for features and for core pending items, I used AI to suggest the wording of some comments and commit messages. The suggestions were reviewed and adjusted by me.

### Debugging

During development, I asked the AI to help me understand the cause of some bugs, such as inconsistent parameters read from the JSON configuration and passed to the email sending code. The diagnosis was used as support; the fix was made and validated by me.

### Researching methodologies

I used AI as a research tool to compare best approaches for the project, testing some methodologies before deciding which one to apply.

### Documentation

The structure of this README (run instructions and reference list) was organized with an AI from my notes and the project code. The reference links were checked.

### Tests and CI (GitHub Actions)

An LLM was used as support in the testing and CI part. The structure and the logic are my own: I created the xUnit test project and the solution, set up the GitHub Actions workflow (restore, build and test on every push and pull request to `main`), and designed the code around interfaces so that the API and email layers could be replaced by fakes in the tests.

The LLM proposed some of the test cases, mainly edge cases (invalid prices and arguments, malformed API responses, prices exactly equal to the thresholds, email failures followed by a retry). Every suggested test was reviewed, adjusted and run by me before being kept.
