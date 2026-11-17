using Exchange.Rates.CoinCap.Polling.Api.Services;
using Exchange.Rates.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace Exchange.Rates.Tests.Services;

public class CoinGeckoAssetsApiTests
{
  private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

  private static (CoinGeckoAssetsApi Api, StubHttpMessageHandler Handler) Create(HttpStatusCode statusCode, string json)
  {
    var handler = new StubHttpMessageHandler(statusCode, json);
    var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test/api/v3/") };
    return (new CoinGeckoAssetsApi(client, NullLogger<CoinGeckoAssetsApi>.Instance), handler);
  }

  [Fact]
  public async Task GetAssetData_MapsCoinGeckoMarketToCoinCapContract()
  {
    var (api, handler) = Create(HttpStatusCode.OK, TestData.CoinGeckoBitcoinJson);

    var result = await api.GetAssetData(" Bitcoin ", CancellationToken);

    Assert.Equal(new Uri("https://api.example.test/api/v3/coins/markets?vs_currency=usd&ids=bitcoin"), Assert.Single(handler.Requests));
    Assert.NotNull(result.Data);
    Assert.Equal("bitcoin", result.Data.Id);
    Assert.Equal("1", result.Data.Rank);
    Assert.Equal("BTC", result.Data.Symbol);
    Assert.Equal("Bitcoin", result.Data.Name);
    Assert.Equal("63877.5", result.Data.PriceUsd);
    Assert.Equal("1284774797857", result.Data.MarketCapUsd);
    Assert.Equal("35077477266", result.Data.VolumeUsd24Hr);
    Assert.Equal("-1.33994", result.Data.ChangePercent24Hr);
    Assert.Equal("20113378.0", result.Data.Supply);
    Assert.Equal("21000000.0", result.Data.MaxSupply);
    Assert.Null(result.Data.Vwap24Hr);
    Assert.True(result.TimeStamp > 0);
  }

  [Fact]
  public async Task GetAssetData_ReturnsNoData_WhenAssetIsUnknown()
  {
    var (api, _) = Create(HttpStatusCode.OK, "[]");

    var result = await api.GetAssetData("xyz", CancellationToken);

    Assert.Null(result.Data);
  }

  [Theory]
  [InlineData(HttpStatusCode.TooManyRequests, "{}")]
  [InlineData(HttpStatusCode.InternalServerError, "{}")]
  [InlineData(HttpStatusCode.OK, "not json")]
  public async Task GetAssetData_ReturnsNoData_WhenApiFails(HttpStatusCode statusCode, string json)
  {
    var (api, _) = Create(statusCode, json);

    var result = await api.GetAssetData("bitcoin", CancellationToken);

    Assert.Null(result.Data);
  }
}
