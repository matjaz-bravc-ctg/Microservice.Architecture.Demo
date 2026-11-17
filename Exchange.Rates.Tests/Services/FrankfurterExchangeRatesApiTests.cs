using Exchange.Rates.Ecb.Polling.Api.Services;
using Exchange.Rates.Tests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace Exchange.Rates.Tests.Services;

public class FrankfurterExchangeRatesApiTests
{
  private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

  private static (FrankfurterExchangeRatesApi Api, StubHttpMessageHandler Handler) Create(HttpStatusCode statusCode, string json)
  {
    var handler = new StubHttpMessageHandler(statusCode, json);
    var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.example.test/v1/") };
    return (new FrankfurterExchangeRatesApi(client, NullLogger<FrankfurterExchangeRatesApi>.Instance), handler);
  }

  [Fact]
  public async Task GetLatestRates_ReturnsEurBasedRates()
  {
    var (api, handler) = Create(HttpStatusCode.OK, TestData.FrankfurterUsdChfJson);

    var result = await api.GetLatestRates("usd, chf", CancellationToken);

    Assert.Equal(new Uri("https://api.example.test/v1/latest?base=EUR&symbols=USD,CHF"), Assert.Single(handler.Requests));
    Assert.Equal("EUR", result.Base);
    Assert.Equal(new DateTime(2026, 11, 17), result.Date);
    Assert.Equal(1.0989m, result.Rates?["USD"]);
    Assert.Equal(0.9233m, result.Rates?["CHF"]);
  }

  [Theory]
  [InlineData(HttpStatusCode.NotFound, "{\"message\":\"not found\"}")]
  [InlineData(HttpStatusCode.InternalServerError, "{}")]
  [InlineData(HttpStatusCode.OK, "{\"base\":\"EUR\",\"rates\":{}}")]
  [InlineData(HttpStatusCode.OK, "not json")]
  public async Task GetLatestRates_ReturnsNoRates_WhenApiFailsOrHasNoRates(HttpStatusCode statusCode, string json)
  {
    var (api, _) = Create(statusCode, json);

    var result = await api.GetLatestRates("XYZ", CancellationToken);

    Assert.Null(result.Rates);
  }
}
