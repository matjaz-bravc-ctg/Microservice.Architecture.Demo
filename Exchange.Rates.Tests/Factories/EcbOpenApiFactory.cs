using Exchange.Rates.Contracts.Messages;
using Exchange.Rates.Ecb.OpenApi.Controllers;
using Exchange.Rates.Ecb.Polling.Api.Consumers;
using Exchange.Rates.Ecb.Polling.Api.Services;
using Exchange.Rates.Tests.TestDoubles;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Exchange.Rates.Tests.Factories;

/// <summary>
/// Hosts Exchange.Rates.Ecb.OpenApi in memory. RabbitMQ is replaced by the MassTransit in-memory
/// test harness, and the real ECB polling consumer answers the requests using a fake rates API.
/// </summary>
public sealed class EcbOpenApiFactory : WebApplicationFactory<ExchangeRatesEcbController>
{
  protected override void ConfigureWebHost(IWebHostBuilder builder) =>
    builder.ConfigureTestServices(services =>
    {
      services.AddSingleton<IEcbExchangeRatesApi, FakeEcbExchangeRatesApi>();
      services.AddMassTransitTestHarness(x =>
      {
        x.AddRequestClient<ISubmitEcbExchangeRateSymbols>();
        x.AddConsumer<SubmitExchangeRateSymbolsConsumer>();
      });
    });
}
