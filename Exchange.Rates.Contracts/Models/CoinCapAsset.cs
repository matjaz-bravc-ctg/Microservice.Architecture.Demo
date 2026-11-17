using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace Exchange.Rates.Contracts.Models;

/// <summary>
/// Crypto asset market data. Keeps the response shape of the original CoinCap API 2.0.
/// </summary>
[ExcludeFromCodeCoverage]
public class CoinCapAsset
{
  [JsonProperty("data")]
  public CoinCapData? Data { get; set; }

  /// <summary>
  /// Unix time in milliseconds when the data was retrieved.
  /// </summary>
  [JsonProperty("timestamp")]
  public long TimeStamp { get; set; }
}
