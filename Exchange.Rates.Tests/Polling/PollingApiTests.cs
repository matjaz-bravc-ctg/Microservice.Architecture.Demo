using Exchange.Rates.CoinCap.Polling.Api.Services;
using Exchange.Rates.Ecb.Polling.Api.Services;
using Exchange.Rates.Tests.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace Exchange.Rates.Tests.Polling;

public class CoinCapPollingApiTests(CoinCapPollingApiFactory factory) : IClassFixture<CoinCapPollingApiFactory>
{
  private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

  [Fact]
  public async Task Root_ReturnsServiceName()
  {
    Assert.Equal("Exchange.Rates.CoinCap.Polling.Api", await factory.CreateClient().GetStringAsync("/", CancellationToken));
  }

  [Fact]
  public async Task Health_IsHealthy()
  {
    Assert.Equal("Healthy", await factory.CreateClient().GetStringAsync("/health", CancellationToken));
  }

  [Fact]
  public async Task AssetsApi_CallsCoinGeckoFromConfiguration()
  {
    using IServiceScope scope = factory.Services.CreateScope();
    ICoinCapAssetsApi api = scope.ServiceProvider.GetRequiredService<ICoinCapAssetsApi>();

    var result = await api.GetAssetData("Bitcoin", CancellationToken);

    Assert.IsType<CoinGeckoAssetsApi>(api);
    Assert.Equal("bitcoin", result.Data?.Id);
    Assert.Contains(new Uri("https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&ids=bitcoin"), factory.Handler.Requests);
  }
}

public class EcbPollingApiTests(EcbPollingApiFactory factory) : IClassFixture<EcbPollingApiFactory>
{
  private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

  [Fact]
  public async Task Root_ReturnsServiceName()
  {
    Assert.Equal("Exchange.Rates.Ecb.Polling.Api", await factory.CreateClient().GetStringAsync("/", CancellationToken));
  }

  [Fact]
  public async Task Health_IsHealthy()
  {
    Assert.Equal("Healthy", await factory.CreateClient().GetStringAsync("/health", CancellationToken));
  }

  [Fact]
  public async Task ExchangeRatesApi_CallsFrankfurterFromConfiguration()
  {
    using IServiceScope scope = factory.Services.CreateScope();
    IEcbExchangeRatesApi api = scope.ServiceProvider.GetRequiredService<IEcbExchangeRatesApi>();

    var result = await api.GetLatestRates("USD,CHF", CancellationToken);

    Assert.IsType<FrankfurterExchangeRatesApi>(api);
    Assert.Equal(2, result.Rates?.Count);
    Assert.Contains(new Uri("https://api.frankfurter.dev/v1/latest?base=EUR&symbols=USD,CHF"), factory.Handler.Requests);
  }
}
