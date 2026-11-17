using Exchange.Rates.Core.Options;
using MassTransit;
using Microsoft.Extensions.Options;

namespace Exchange.Rates.Ecb.Polling.Api.Consumers.Definitions;

// Consumer definitions are discovered together with the consumers by AddConsumers
public class SubmitExchangeRateSymbolsDefinition : ConsumerDefinition<SubmitExchangeRateSymbolsConsumer>
{
  private readonly MassTransitOptions _options;

  public SubmitExchangeRateSymbolsDefinition(IOptions<MassTransitOptions> options)
  {
    _options = options.Value;

    // Use the queue name from configuration instead of the generated one
    if (!string.IsNullOrWhiteSpace(_options.QueueName))
    {
      EndpointName = _options.QueueName;
    }

    // Limit the number of messages consumed concurrently
    // this applies to the consumer only, not the endpoint
    ConcurrentMessageLimit = 16;
  }

  protected override void ConfigureConsumer(IReceiveEndpointConfigurator endpointConfigurator,
      IConsumerConfigurator<SubmitExchangeRateSymbolsConsumer> consumerConfigurator, IRegistrationContext context)
  {
    endpointConfigurator.PrefetchCount = _options.ReceiveEndpointPrefetchCount;
    endpointConfigurator.UseMessageRetry(r => r.Interval(5, 1000));
    endpointConfigurator.UseInMemoryOutbox(context);
  }
}
