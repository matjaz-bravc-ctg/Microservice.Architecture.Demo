using Exchange.Rates.Contracts.Models;

namespace Exchange.Rates.CoinCap.Polling.Api.Services;

public interface ICoinCapAssetsApi
{
  /// <summary>
  /// Gets market data of a crypto asset. <see cref="CoinCapAsset.Data"/> is null when the asset is not available.
  /// </summary>
  Task<CoinCapAsset> GetAssetData(string id, CancellationToken cancellationToken = default);
}
