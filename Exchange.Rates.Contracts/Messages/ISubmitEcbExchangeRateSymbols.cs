using Exchange.Rates.Contracts.Messages.Base;

namespace Exchange.Rates.Contracts.Messages;

public interface ISubmitEcbExchangeRateSymbols : IBaseContract
{
  List<string> Symbols { get; }
}
