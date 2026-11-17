using Exchange.Rates.Tests.TestDoubles;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Exchange.Rates.Tests.Factories;

/// <summary>
/// Hosts a polling service in memory with the in-memory MassTransit test harness instead of RabbitMQ.
/// The typed HttpClient of <typeparamref name="TApi"/> keeps its real configuration, but its requests
/// are answered by <see cref="Handler"/> instead of the external API.
/// </summary>
/// <typeparam name="TEntryPoint">Any type from the polling service's assembly.</typeparam>
/// <typeparam name="TApi">The service's external API client interface.</typeparam>
/// <typeparam name="TApiImplementation">The typed HttpClient implementation of <typeparamref name="TApi"/>.</typeparam>
public class PollingApiFactory<TEntryPoint, TApi, TApiImplementation>(string responseJson) : WebApplicationFactory<TEntryPoint>
  where TEntryPoint : class
  where TApi : class
  where TApiImplementation : class, TApi
{
  public StubHttpMessageHandler Handler { get; } = new(HttpStatusCode.OK, responseJson);

  protected override void ConfigureWebHost(IWebHostBuilder builder) =>
    builder.ConfigureTestServices(services =>
    {
      services.AddHttpClient<TApi, TApiImplementation>().ConfigurePrimaryHttpMessageHandler(() => Handler);
      services.AddMassTransitTestHarness();
    });
}

public sealed class CoinCapPollingApiFactory()
  : PollingApiFactory<CoinCap.Polling.Api.Consumers.SubmitCoinCapAssetConsumer,
    CoinCap.Polling.Api.Services.ICoinCapAssetsApi,
    CoinCap.Polling.Api.Services.CoinGeckoAssetsApi>(TestData.CoinGeckoBitcoinJson);

public sealed class EcbPollingApiFactory()
  : PollingApiFactory<Ecb.Polling.Api.Consumers.SubmitExchangeRateSymbolsConsumer,
    Ecb.Polling.Api.Services.IEcbExchangeRatesApi,
    Ecb.Polling.Api.Services.FrankfurterExchangeRatesApi>(TestData.FrankfurterUsdChfJson);
