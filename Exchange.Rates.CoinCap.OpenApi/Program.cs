using Exchange.Rates.CoinCap.OpenApi.Filters;
using Exchange.Rates.Contracts.Messages;
using Exchange.Rates.Core.Options;
using MassTransit;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Serilog;
using System.Reflection;

const string ServiceName = "Exchange.Rates.CoinCap.OpenApi";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog((services, configuration) => configuration
  .ReadFrom.Configuration(builder.Configuration)
  .ReadFrom.Services(services)
  .Enrich.FromLogContext());

builder.Services.AddOptions<MassTransitOptions>()
  .Bind(builder.Configuration.GetSection(MassTransitOptions.SectionName))
  .ValidateDataAnnotations()
  .ValidateOnStart();

// Formats the endpoint names using kebab-case (dashed snake case)
builder.Services.AddSingleton<IEndpointNameFormatter>(KebabCaseEndpointNameFormatter.Instance);

// MassTransit also registers a bus health check, which /health reports
builder.Services.AddMassTransit(x =>
{
  x.AddRequestClient<ISubmitCoinCapAssetId>(RequestTimeout.After(s: 15));
  x.UsingRabbitMq((context, rabbit) =>
  {
    MassTransitOptions options = context.GetRequiredService<IOptions<MassTransitOptions>>().Value;
    rabbit.Host(new Uri(options.Host), host =>
    {
      host.Username(options.Username);
      host.Password(options.Password);
    });
  });
});

builder.Services.AddControllers()
  .ConfigureApiBehaviorOptions(options =>
  {
    options.SuppressConsumesConstraintForFormFileParameters = true;
    options.SuppressInferBindingSourcesForParameters = true;
    options.SuppressModelStateInvalidFilter = true; // The controller returns 400 with the model state itself
    options.SuppressMapClientErrors = true;
  })
  .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

builder.Services.AddSwaggerGen(options =>
{
  options.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = ServiceName,
    Description = "API for real-time pricing and market activity of cryptocurrencies"
  });
  options.EnableAnnotations();
  options.DocumentFilter<SwaggerDocumentFilter>();
  options.DescribeAllParametersInCamelCase();
  string xmlDocFile = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
  if (File.Exists(xmlDocFile))
  {
    options.IncludeXmlComments(xmlDocFile);
  }
});

builder.Services.AddHealthChecks();
builder.Services.AddCors();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", ServiceName));
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

app.MapControllers();
app.MapHealthChecks("/health");
// Redirect root to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

await app.RunAsync();
