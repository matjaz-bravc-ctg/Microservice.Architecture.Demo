using System.Text.Json.Serialization;

namespace Exchange.Rates.CoinCap.Polling.Api.Services;

/// <summary>
/// One item of the CoinGecko /coins/markets response (only the fields this service uses).
/// </summary>
public sealed class CoinGeckoMarket
{
  [JsonPropertyName("id")]
  public string? Id { get; set; }

  [JsonPropertyName("symbol")]
  public string? Symbol { get; set; }

  [JsonPropertyName("name")]
  public string? Name { get; set; }

  [JsonPropertyName("market_cap_rank")]
  public int? MarketCapRank { get; set; }

  [JsonPropertyName("current_price")]
  public decimal? CurrentPrice { get; set; }

  [JsonPropertyName("market_cap")]
  public decimal? MarketCap { get; set; }

  [JsonPropertyName("total_volume")]
  public decimal? TotalVolume { get; set; }

  [JsonPropertyName("price_change_percentage_24h")]
  public decimal? PriceChangePercentage24H { get; set; }

  [JsonPropertyName("circulating_supply")]
  public decimal? CirculatingSupply { get; set; }

  [JsonPropertyName("max_supply")]
  public decimal? MaxSupply { get; set; }
}
