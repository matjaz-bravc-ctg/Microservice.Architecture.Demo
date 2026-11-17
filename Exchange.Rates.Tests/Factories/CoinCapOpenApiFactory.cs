using Exchange.Rates.CoinCap.OpenApi.Controllers;
using Exchange.Rates.CoinCap.Polling.Api.Consumers;
using Exchange.Rates.CoinCap.Polling.Api.Services;
using Exchange.Rates.Contracts.Messages;
using Exchange.Rates.Tests.TestDoubles;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Exchange.Rates.Tests.Factories;

/// <summary>
/// Hosts Exchange.Rates.CoinCap.OpenApi in memory. RabbitMQ is replaced by the MassTransit in-memory
/// test harness, and the real CoinCap polling consumer answers the requests using a fake market data API.
/// </summary>
public sealed class CoinCapOpenApiFactory : WebApplicationFactory<ExchangeRatesCoinCapController>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder) =>
    builder.ConfigureTestServices(services =>
    {
      services.AddSingleton<ICoinCapAssetsApi, FakeCoinCapAssetsApi>();
      services.AddMassTransitTestHarness(x =>
      {
        x.AddRequestClient<ISubmitCoinCapAssetId>();
        x.AddConsumer<SubmitCoinCapAssetConsumer>();
      });
    });
}
