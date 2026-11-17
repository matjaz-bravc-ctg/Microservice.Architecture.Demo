using Exchange.Rates.Contracts.Messages;
using Exchange.Rates.Ecb.Polling.Api.Services;
using MassTransit;

namespace Exchange.Rates.Ecb.Polling.Api.Consumers;

public sealed class SubmitExchangeRateSymbolsConsumer(
  ILogger<SubmitExchangeRateSymbolsConsumer> logger,
  IEcbExchangeRatesApi ecbExchangeRatesApi)
  : IConsumer<ISubmitEcbExchangeRateSymbols>
{
  public async Task Consume(ConsumeContext<ISubmitEcbExchangeRateSymbols> context)
  {
    logger.LogDebug("Exchange rates for {Symbols} requested", context.Message.Symbols);
    if (context.RequestId is null)
    {
      return; // Not a request, nobody waits for an answer
    }

    List<string> symbols = context.Message.Symbols ?? [];
    if (symbols.Count == 0)
    {
      await RespondRejected(context, symbols, "At least one currency symbol is required");
      return;
    }

    string joinedSymbols = string.Join(",", symbols);
    var result = await ecbExchangeRatesApi.GetLatestRates(joinedSymbols, context.CancellationToken);
    if (result.Rates is null)
    {
      await RespondRejected(context, symbols, $"Exchange Rates for a {joinedSymbols} are not available");
      return;
    }

    await context.RespondAsync<IEcbExchangeRatesAccepted>(new
    {
      context.Message.EventId,
      InVar.Timestamp,
      Symbols = symbols,
      CurrencyExchange = result,
      Message = "Exchange Rates Symbols"
    });
  }

  private static Task RespondRejected(ConsumeContext<ISubmitEcbExchangeRateSymbols> context, List<string> symbols, string reason) =>
    context.RespondAsync<IEcbExchangeRatesRejected>(new
    {
      context.Message.EventId,
      InVar.Timestamp,
      Symbols = symbols,
      Reason = reason
    });
}
