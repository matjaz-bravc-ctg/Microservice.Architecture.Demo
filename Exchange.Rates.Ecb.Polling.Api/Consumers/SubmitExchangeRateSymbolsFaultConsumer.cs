using Exchange.Rates.Contracts.Messages;
using MassTransit;

namespace Exchange.Rates.Ecb.Polling.Api.Consumers;

public class SubmitExchangeRateSymbolsFaultConsumer(ILogger<SubmitExchangeRateSymbolsFaultConsumer> logger)
  : IConsumer<Fault<ISubmitEcbExchangeRateSymbols>>
{
  public Task Consume(ConsumeContext<Fault<ISubmitEcbExchangeRateSymbols>> context)
  {
    logger.LogError("There was an error with requesting exchange rates for {Symbols}: {Exceptions}",
      context.Message.Message.Symbols,
      string.Join("; ", context.Message.Exceptions.Select(exception => exception.Message)));
    return Task.CompletedTask;
  }
}
