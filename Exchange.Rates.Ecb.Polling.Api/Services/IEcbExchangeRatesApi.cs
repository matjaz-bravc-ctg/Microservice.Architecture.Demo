using Exchange.Rates.Contracts.Models;

namespace Exchange.Rates.Ecb.Polling.Api.Services;

public interface IEcbExchangeRatesApi
{
  /// <summary>
  /// Gets the latest EUR based rates. <see cref="EcbCurrencyExchange.Rates"/> is null when the rates are not available.
  /// </summary>
  Task<EcbCurrencyExchange> GetLatestRates(string symbols = "USD", CancellationToken cancellationToken = default);
}
