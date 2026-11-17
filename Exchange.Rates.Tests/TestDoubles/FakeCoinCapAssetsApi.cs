using Exchange.Rates.CoinCap.Polling.Api.Services;
using Exchange.Rates.Contracts.Models;

namespace Exchange.Rates.Tests.TestDoubles;

/// <summary>
/// Knows only "bitcoin"; every other asset is not available.
/// </summary>
public sealed class FakeCoinCapAssetsApi : ICoinCapAssetsApi
{
  public Task<CoinCapAsset> GetAssetData(string id, CancellationToken cancellationToken = default) =>
    Task.FromResult(id == "bitcoin"
      ? new CoinCapAsset
      {
        TimeStamp = 1_700_000_000_000,
        Data = new CoinCapData { Id = "bitcoin", Symbol = "BTC", Name = "Bitcoin", Rank = "1", PriceUsd = "63877" }
      }
      : new CoinCapAsset());
}
