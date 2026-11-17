using Exchange.Rates.Contracts.Messages;
using Exchange.Rates.Contracts.Models;
using Exchange.Rates.Ecb.OpenApi.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Exchange.Rates.Ecb.OpenApi.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/[controller]")]
public class ExchangeRatesEcbController(
  ILogger<ExchangeRatesEcbController> logger,
  IRequestClient<ISubmitEcbExchangeRateSymbols> submitEcbExchangeRateRequestClient)
  : ControllerBase
{
  /// <summary>
  /// EurBaseRates endpoint
  /// </summary>
  /// <remarks>
  /// Exchange rates quoted against the EUR. Example Symbols list: USD,CHF,CZK
  /// </remarks>
  /// <param name="model">Currency symbols</param>
  /// <param name="cancellationToken">Request cancellation</param>
  /// <response code="200">Returned if everything is ok</response>
  /// <response code="400">Returned if the symbols are missing or the rates are not available</response>
  /// <response code="504">Returned if the polling service did not answer in time</response>
  [HttpGet("eurbaserates")]
  [ProducesResponseType<EcbCurrencyExchange>(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
  public async Task<IActionResult> EurBaseRates([FromQuery] SymbolsSubmissionModel model, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      logger.LogWarning("Invalid model: {Errors}", ModelState);
      return BadRequest(ModelState);
    }

    try
    {
      // https://masstransit.io/documentation/concepts/requests
      Response<IEcbExchangeRatesAccepted, IEcbExchangeRatesRejected> response =
        await submitEcbExchangeRateRequestClient.GetResponse<IEcbExchangeRatesAccepted, IEcbExchangeRatesRejected>(new
        {
          EventId = NewId.NextGuid(),
          InVar.Timestamp,
          Symbols = model.Symbols
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(symbol => symbol.ToUpperInvariant())
            .ToList()
        }, cancellationToken);

      if (response.Is(out Response<IEcbExchangeRatesAccepted>? accepted))
      {
        return Ok(accepted.Message.CurrencyExchange);
      }

      if (response.Is(out Response<IEcbExchangeRatesRejected>? rejected))
      {
        logger.LogWarning("Symbols {Symbols} rejected: {Reason}", rejected.Message.Symbols, rejected.Message.Reason);
        return BadRequest(rejected.Message);
      }

      return Problem("Unexpected response from the ECB polling service.");
    }
    catch (RequestTimeoutException ex)
    {
      logger.LogError(ex, "ECB polling service did not answer in time");
      return Problem("The ECB polling service did not answer in time.", statusCode: StatusCodes.Status504GatewayTimeout);
    }
  }
}
