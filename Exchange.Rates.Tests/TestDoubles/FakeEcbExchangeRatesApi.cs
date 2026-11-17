using Exchange.Rates.Contracts.Models;
using Exchange.Rates.Ecb.Polling.Api.Services;

namespace Exchange.Rates.Tests.TestDoubles;

/// <summary>
/// Knows only USD and CHF; any other symbol makes the rates unavailable.
/// </summary>
public sealed class FakeEcbExchangeRatesApi : IEcbExchangeRatesApi
{
  private static readonly Dictionary<string, decimal> KnownRates = new() { ["USD"] = 1.0989m, ["CHF"] = 0.9233m };

  public Task<EcbCurrencyExchange> GetLatestRates(string symbols = "USD", CancellationToken cancellationToken = default)
  {
    string[] requested = symbols.Split(',');
    return Task.FromResult(requested.All(KnownRates.ContainsKey)
      ? new EcbCurrencyExchange
      {
        Base = "EUR",
        Date = new DateTime(2026, 11, 17),
        Rates = requested.ToDictionary(symbol => symbol, symbol => KnownRates[symbol])
      }
      : new EcbCurrencyExchange());
  }
}
