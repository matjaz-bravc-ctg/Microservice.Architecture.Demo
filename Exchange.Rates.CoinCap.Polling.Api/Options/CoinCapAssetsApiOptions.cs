using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Exchange.Rates.CoinCap.Polling.Api.Options;

/// <summary>
/// Settings of the crypto market data API, bound from the "CoinCapAssetsApiOptions" section.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class CoinCapAssetsApiOptions
{
  public const string SectionName = "CoinCapAssetsApiOptions";

  [Range(1, 1440)]
  public int HandlerLifetimeMinutes { get; set; } = 5;

  public string? Name { get; set; }

  /// <summary>
  /// Base URL of the CoinGecko API v3, e.g. https://api.coingecko.com/api/v3
  /// </summary>
  [Required, Url]
  public string Url { get; set; } = string.Empty;
}
