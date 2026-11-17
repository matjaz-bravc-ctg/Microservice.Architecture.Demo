namespace Exchange.Rates.Tests;

/// <summary>
/// Trimmed copies of real responses from the external APIs.
/// </summary>
internal static class TestData
{
  public const string CoinGeckoBitcoinJson = """
    [{"id":"bitcoin","symbol":"btc","name":"Bitcoin","current_price":63877.5,"market_cap":1284774797857,
      "market_cap_rank":1,"total_volume":35077477266,"price_change_percentage_24h":-1.33994,
      "circulating_supply":20113378.0,"max_supply":21000000.0,"roi":null}]
    """;

  public const string FrankfurterUsdChfJson = """
    {"amount":1.0,"base":"EUR","date":"2026-11-17","rates":{"CHF":0.9233,"USD":1.0989}}
    """;
}
