using Exchange.Rates.Tests.Factories;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Exchange.Rates.Tests.OpenApi;

public class EcbOpenApiTests(EcbOpenApiFactory factory) : IClassFixture<EcbOpenApiFactory>
{
  private const string EurBaseRatesUrl = "/api/exchangeratesecb/eurbaserates";

  private readonly HttpClient _client = factory.CreateClient();

  private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

  [Fact]
  public async Task EurBaseRates_ReturnsRates_WhenSymbolsAreAvailable()
  {
    HttpResponseMessage response = await _client.GetAsync($"{EurBaseRatesUrl}?symbols=usd, CHF", CancellationToken);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    JObject body = JObject.Parse(await response.Content.ReadAsStringAsync(CancellationToken));
    Assert.Equal("EUR", (string?)body["base"]);
    Assert.Equal(1.0989m, (decimal?)body["rates"]?["USD"]);
    Assert.Equal(0.9233m, (decimal?)body["rates"]?["CHF"]);
  }

  [Fact]
  public async Task EurBaseRates_ReturnsRejectionReasonAndSymbols_WhenRatesAreNotAvailable()
  {
    // Repeated because the rejection used to be lost in a race about half of the time
    for (int i = 0; i < 10; i++)
    {
      HttpResponseMessage response = await _client.GetAsync($"{EurBaseRatesUrl}?symbols=USD,XYZ", CancellationToken);

      Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
      JObject body = JObject.Parse(await response.Content.ReadAsStringAsync(CancellationToken));
      Assert.Equal(["USD", "XYZ"], body["symbols"]?.ToObject<string[]>() ?? []);
      Assert.Equal("Exchange Rates for a USD,XYZ are not available", (string?)body["reason"]);
    }
  }

  [Fact]
  public async Task EurBaseRates_IsRejected_WhenSymbolsContainNoCurrency()
  {
    HttpResponseMessage response = await _client.GetAsync($"{EurBaseRatesUrl}?symbols=,", CancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    JObject body = JObject.Parse(await response.Content.ReadAsStringAsync(CancellationToken));
    Assert.Equal("At least one currency symbol is required", (string?)body["reason"]);
  }

  [Fact]
  public async Task EurBaseRates_ReturnsBadRequest_WhenSymbolsAreMissing()
  {
    HttpResponseMessage response = await _client.GetAsync(EurBaseRatesUrl, CancellationToken);

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    Assert.Contains("The Symbols field is required.", await response.Content.ReadAsStringAsync(CancellationToken));
  }

  [Fact]
  public async Task Health_IsHealthy()
  {
    Assert.Equal("Healthy", await _client.GetStringAsync("/health", CancellationToken));
  }

  [Fact]
  public async Task Swagger_TagDescriptionMatchesControllerTag()
  {
    JObject swagger = JObject.Parse(await _client.GetStringAsync("/swagger/v1/swagger.json", CancellationToken));

    // The document filter's tag must use the controller's name, otherwise Swagger UI shows its description under an empty group
    JToken tag = Assert.Single(swagger["tags"]!);
    Assert.Equal("ExchangeRatesEcb", (string?)tag["name"]);
    Assert.Contains("Frankfurter", (string?)tag["description"]);
    Assert.All(swagger["paths"]!.Values().SelectMany(path => path.Values()),
      operation => Assert.Equal(["ExchangeRatesEcb"], operation["tags"]?.ToObject<string[]>() ?? []));
  }
}
