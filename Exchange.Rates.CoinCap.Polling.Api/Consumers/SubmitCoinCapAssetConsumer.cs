using Exchange.Rates.CoinCap.Polling.Api.Services;
using Exchange.Rates.Contracts.Messages;
using MassTransit;

namespace Exchange.Rates.CoinCap.Polling.Api.Consumers;

public sealed class SubmitCoinCapAssetConsumer(
  ILogger<SubmitCoinCapAssetConsumer> logger,
  ICoinCapAssetsApi coinCapAssetsApi)
  : IConsumer<ISubmitCoinCapAssetId>
{
  public async Task Consume(ConsumeContext<ISubmitCoinCapAssetId> context)
  {
    logger.LogDebug("Asset {Id} requested", context.Message.Id);
    if (context.RequestId is null)
    {
      return; // Not a request, nobody waits for an answer
    }

    if (string.IsNullOrWhiteSpace(context.Message.Id))
    {
      await RespondRejected(context, "Asset Id is required");
      return;
    }

    var result = await coinCapAssetsApi.GetAssetData(context.Message.Id, context.CancellationToken);
    if (result.Data is null)
    {
      await RespondRejected(context, $"Asset Data for a {context.Message.Id} is not available");
      return;
    }

    await context.RespondAsync<ICoinCapAssetAccepted>(new
    {
      context.Message.EventId,
      InVar.Timestamp,
      context.Message.Id,
      AssetData = result,
      Message = "Asset Data"
    });
  }

  private static Task RespondRejected(ConsumeContext<ISubmitCoinCapAssetId> context, string reason) =>
    context.RespondAsync<ICoinCapAssetRejected>(new
    {
      context.Message.EventId,
      InVar.Timestamp,
      context.Message.Id,
      Reason = reason
    });
}
