using Exchange.Rates.Contracts.Models;
using System.Globalization;
using System.Net.Http.Json;

namespace Exchange.Rates.CoinCap.Polling.Api.Services;

/// <summary>
/// Reads crypto market data from the public CoinGecko API v3 (no API key needed)
/// and maps it to the <see cref="CoinCapAsset"/> contract, which keeps the CoinCap API 2.0 shape.
/// </summary>
public sealed class CoinGeckoAssetsApi(HttpClient httpClient, ILogger<CoinGeckoAssetsApi> logger) : ICoinCapAssetsApi
{
  public async Task<CoinCapAsset> GetAssetData(string id, CancellationToken cancellationToken = default)
  {
    string requestUri = $"coins/markets?vs_currency=usd&ids={Uri.EscapeDataString(id.Trim().ToLowerInvariant())}";
    try
    {
      using HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);
      if (!response.IsSuccessStatusCode)
      {
        logger.LogError("CoinGecko API failed with HTTP status code {StatusCode} for asset {Id}", response.StatusCode, id);
        return new CoinCapAsset();
      }

      List<CoinGeckoMarket>? markets = await response.Content.ReadFromJsonAsync<List<CoinGeckoMarket>>(cancellationToken);
      CoinGeckoMarket? market = markets?.FirstOrDefault();
      if (market is null)
      {
        logger.LogWarning("CoinGecko API returned no data for asset {Id}", id);
        return new CoinCapAsset();
      }

      return new CoinCapAsset
      {
        Data = Map(market),
        TimeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
      };
    }
    // Network errors, invalid JSON, and resilience timeouts or open circuits all mean "not available"
    catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
    {
      logger.LogError(ex, "CoinGecko API request for asset {Id} failed", id);
      return new CoinCapAsset();
    }
  }

  private static CoinCapData Map(CoinGeckoMarket market) => new()
  {
    Id = market.Id,
    Rank = Format(market.MarketCapRank),
    Symbol = market.Symbol?.ToUpperInvariant(),
    Name = market.Name,
    Supply = Format(market.CirculatingSupply),
    MaxSupply = Format(market.MaxSupply),
    MarketCapUsd = Format(market.MarketCap),
    VolumeUsd24Hr = Format(market.TotalVolume),
    PriceUsd = Format(market.CurrentPrice),
    ChangePercent24Hr = Format(market.PriceChangePercentage24H)
  };

  private static string? Format<T>(T? value) where T : struct, IFormattable =>
    value?.ToString(null, CultureInfo.InvariantCulture);
}
