using Exchange.Rates.Contracts.Messages.Base;

namespace Exchange.Rates.Contracts.Messages;

public interface IEcbExchangeRatesRejected : IBaseContract
{
  List<string> Symbols { get; }

  string Reason { get; }
}
