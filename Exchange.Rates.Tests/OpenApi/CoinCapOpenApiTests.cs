using Exchange.Rates.Tests.Factories;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Exchange.Rates.Tests.OpenApi;

public class CoinCapOpenApiTests(CoinCapOpenApiFactory factory) : IClassFixture<CoinCapOpenApiFactory>
{
  private const string AssetInfoUrl = "/api/exchangeratescoincap/assetinfo";

  private readonly HttpClient _client = factory.CreateClient();

  private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

  [Fact]
  public async Task AssetInfo_ReturnsAssetData_WhenAssetIsAvailable()
  {
    HttpResponseMessage response = await _client.GetAsync($"{AssetInfoUrl}?id=bitcoin", CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    JObject body = JObject.Parse(await response.Content.ReadAsStringAsync(CancellationToken));
    Assert.Equal("bitcoin", (string?)body["data"]?["id"]);
    Assert.Equal("BTC", (string?)body["data"]?["symbol"]);
    Assert.Equal("63877", (string?)body["data"]?["priceUsd"]);
    Assert.Equal(1_700_000_000_000, (long?)body["timestamp"]);
  }

  [Fact]
  public async Task AssetInfo_ReturnsRejectionReason_WhenAssetIsNotAvailable()
  {
    // Repeated because the rejection used to be lost in a race about half of the time
    for (int i = 0; i < 10; i++)
    {
      HttpResponseMessage response = await _client.GetAsync($"{AssetInfoUrl}?id=xyz", CancellationToken);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      JObject body = JObject.Parse(await response.Content.ReadAsStringAsync(CancellationToken));
      Assert.Equal("xyz", (string?)body["id"]);
      Assert.Equal("Asset Data for a xyz is not available", (string?)body["reason"]);
    }
  }

  [Fact]
  public async Task AssetInfo_ReturnsBadRequest_WhenIdIsMissing()
  {
    HttpResponseMessage response = await _client.GetAsync(AssetInfoUrl, CancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Contains("The Id field is required.", await response.Content.ReadAsStringAsync(CancellationToken));
  }

  [Fact]
  public async Task Health_IsHealthy()
  {
    Assert.Equal("Healthy", await _client.GetStringAsync("/health", CancellationToken));
  }

  [Fact]
  public async Task Root_RedirectsToSwaggerUi()
  {
    HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    HttpResponseMessage response = await client.GetAsync("/", CancellationToken);

    Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    Assert.Equal("/swagger/index.html", response.Headers.Location?.OriginalString);
  }

  [Fact]
  public async Task Swagger_DescribesAssetInfoEndpoint()
  {
    string swagger = await _client.GetStringAsync("/swagger/v1/swagger.json", CancellationToken);

    Assert.Contains(AssetInfoUrl, swagger);
    Assert.Contains("Unique identifier for asset", swagger); // from the XML documentation
  }

  [Fact]
  public async Task Swagger_TagDescriptionMatchesControllerTag()
  {
    JObject swagger = JObject.Parse(await _client.GetStringAsync("/swagger/v1/swagger.json", CancellationToken));

    // The document filter's tag must use the controller's name, otherwise Swagger UI shows its description under an empty group
    JToken tag = Assert.Single(swagger["tags"]!);
    Assert.Equal("ExchangeRatesCoinCap", (string?)tag["name"]);
    Assert.Contains("CoinGecko", (string?)tag["description"]);
    Assert.All(swagger["paths"]!.Values().SelectMany(path => path.Values()),
      operation => Assert.Equal(["ExchangeRatesCoinCap"], operation["tags"]?.ToObject<string[]>() ?? []));
  }
}
