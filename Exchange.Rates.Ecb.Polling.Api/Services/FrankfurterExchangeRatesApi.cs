using Exchange.Rates.Contracts.Models;
using System.Net.Http.Json;

namespace Exchange.Rates.Ecb.Polling.Api.Services;

/// <summary>
/// Reads the ECB reference rates from the public Frankfurter API (no API key needed).
/// </summary>
public sealed class FrankfurterExchangeRatesApi(HttpClient httpClient, ILogger<FrankfurterExchangeRatesApi> logger) : IEcbExchangeRatesApi
{
  public async Task<EcbCurrencyExchange> GetLatestRates(string symbols = "USD", CancellationToken cancellationToken = default)
  {
    string encodedSymbols = string.Join(',', symbols
      .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
      .Select(symbol => Uri.EscapeDataString(symbol.ToUpperInvariant())));
    string requestUri = $"latest?base=EUR&symbols={encodedSymbols}";
    try
    {
      using HttpResponseMessage response = await httpClient.GetAsync(requestUri, cancellationToken);
      if (!response.IsSuccessStatusCode)
      {
        logger.LogError("Frankfurter API failed with HTTP status code {StatusCode} for symbols {Symbols}", response.StatusCode, symbols);
        return new EcbCurrencyExchange();
      }

      EcbCurrencyExchange? result = await response.Content.ReadFromJsonAsync<EcbCurrencyExchange>(cancellationToken);
      if (result?.Rates is null || result.Rates.Count == 0)
      {
        logger.LogWarning("Frankfurter API returned no rates for symbols {Symbols}", symbols);
        return new EcbCurrencyExchange();
      }

      return result;
    }
    // Network errors, invalid JSON, and resilience timeouts or open circuits all mean "not available"
    catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
    {
      logger.LogError(ex, "Frankfurter API request for symbols {Symbols} failed", symbols);
      return new EcbCurrencyExchange();
    }
  }
}
