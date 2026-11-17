using Exchange.Rates.CoinCap.Polling.Api.Options;
using Exchange.Rates.CoinCap.Polling.Api.Services;
using Exchange.Rates.Core.Options;
using MassTransit;
using Microsoft.Extensions.Options;
using Serilog;
using System.Net.Http.Headers;

const string ServiceName = "Exchange.Rates.CoinCap.Polling.Api";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, configuration) => configuration
  .ReadFrom.Configuration(builder.Configuration)
  .ReadFrom.Services(services)
  .Enrich.FromLogContext());

builder.Services.AddOptions<CoinCapAssetsApiOptions>()
  .Bind(builder.Configuration.GetSection(CoinCapAssetsApiOptions.SectionName))
  .ValidateDataAnnotations()
  .ValidateOnStart();

builder.Services.AddOptions<MassTransitOptions>()
  .Bind(builder.Configuration.GetSection(MassTransitOptions.SectionName))
  .ValidateDataAnnotations()
  .ValidateOnStart();

// Typed HttpClient for the external API, with the standard resilience pipeline
// (retries on transient errors and 429, circuit breaker and timeouts)
builder.Services.AddHttpClient<ICoinCapAssetsApi, CoinGeckoAssetsApi>((services, client) =>
  {
    CoinCapAssetsApiOptions options = services.GetRequiredService<IOptions<CoinCapAssetsApiOptions>>().Value;
    client.BaseAddress = new Uri(options.Url.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(ServiceName.Replace('.', '-'), "1.0"));
  })
  .SetHandlerLifetime(TimeSpan.FromMinutes(builder.Configuration.GetValue($"{CoinCapAssetsApiOptions.SectionName}:HandlerLifetimeMinutes", 5)))
  .AddStandardResilienceHandler();

// Formats the endpoint names using kebab-case (dashed snake case)
builder.Services.AddSingleton<IEndpointNameFormatter>(KebabCaseEndpointNameFormatter.Instance);

// MassTransit also registers a bus health check, which /health reports
builder.Services.AddMassTransit(x =>
{
  // Discovers the consumers and their definitions (queue name, retry, outbox, prefetch)
  x.AddConsumers(typeof(Program).Assembly);
  x.UsingRabbitMq((context, rabbit) =>
  {
    MassTransitOptions options = context.GetRequiredService<IOptions<MassTransitOptions>>().Value;
    rabbit.Host(new Uri(options.Host), host =>
    {
      host.Username(options.Username);
      host.Password(options.Password);
    });
    rabbit.ConfigureEndpoints(context);
  });
});

builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/", () => ServiceName);

await app.RunAsync();
