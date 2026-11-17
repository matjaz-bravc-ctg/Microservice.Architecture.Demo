using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.QualityOfService.Polly;

const string ServiceName = "Exchange.Rates.Gateway";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Configuration
  .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
  .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
  .AddEnvironmentVariables();

builder.Services
  .AddOcelot(builder.Configuration)
  // In-memory response cache (CacheManager dictionary handle) for routes with CacheOptions in ocelot.json
  .AddCacheManager(cache => cache.WithDictionaryHandle())
  // Timeouts and circuit breaker for routes with QoSOptions in ocelot.json
  .AddPolly();

builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

// The gateway's own endpoints; every other request goes to Ocelot
app.MapGet("/", () => ServiceName);
app.MapHealthChecks("/health");
app.UseRouting();
app.UseEndpoints(_ => { });

await app.UseOcelot();
await app.RunAsync();
