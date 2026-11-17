using Exchange.Rates.Contracts.Messages;
using MassTransit;

namespace Exchange.Rates.CoinCap.Polling.Api.Consumers;

public class SubmitCoinCapAssetFaultConsumer(ILogger<SubmitCoinCapAssetFaultConsumer> logger)
  : IConsumer<Fault<ISubmitCoinCapAssetId>>
{
  public Task Consume(ConsumeContext<Fault<ISubmitCoinCapAssetId>> context)
  {
    logger.LogError("There was an error with requesting asset {Id}: {Exceptions}",
      context.Message.Message.Id,
      string.Join("; ", context.Message.Exceptions.Select(exception => exception.Message)));
    return Task.CompletedTask;
  }
}
