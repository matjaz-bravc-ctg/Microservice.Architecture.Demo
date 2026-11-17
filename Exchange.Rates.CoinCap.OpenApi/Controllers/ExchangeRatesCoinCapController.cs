using Exchange.Rates.CoinCap.OpenApi.Models;
using Exchange.Rates.Contracts.Messages;
using Exchange.Rates.Contracts.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Exchange.Rates.CoinCap.OpenApi.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/[controller]")]
public class ExchangeRatesCoinCapController(
  ILogger<ExchangeRatesCoinCapController> logger,
  IRequestClient<ISubmitCoinCapAssetId> submitCoinCapAssetIdRequestClient)
  : ControllerBase
{
  /// <summary>
  /// Crypto AssetInfo endpoint
  /// </summary>
  /// <remarks>
  /// The asset price and market data in USD. Example Id: bitcoin
  /// </remarks>
  /// <param name="model">Unique identifier for asset. Ex: bitcoin</param>
  /// <param name="cancellationToken">Request cancellation</param>
  /// <response code="200">Returned if everything is ok</response>
  /// <response code="400">Returned if the asset Id is missing or the asset is not available</response>
  /// <response code="504">Returned if the polling service did not answer in time</response>
  [HttpGet("assetinfo")]
  [ProducesResponseType<CoinCapAsset>(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  [ProducesResponseType(StatusCodes.Status504GatewayTimeout)]
  public async Task<IActionResult> AssetInfo([FromQuery] AssetIdSubmissionModel model, CancellationToken cancellationToken)
  {
    if (!ModelState.IsValid)
    {
      logger.LogWarning("Invalid model: {Errors}", ModelState);
      return BadRequest(ModelState);
    }

    try
    {
      // https://masstransit.io/documentation/concepts/requests
      Response<ICoinCapAssetAccepted, ICoinCapAssetRejected> response =
        await submitCoinCapAssetIdRequestClient.GetResponse<ICoinCapAssetAccepted, ICoinCapAssetRejected>(new
        {
          EventId = NewId.NextGuid(),
          InVar.Timestamp,
          model.Id
        }, cancellationToken);

      if (response.Is(out Response<ICoinCapAssetAccepted>? accepted))
      {
        return Ok(accepted.Message.AssetData);
      }

      if (response.Is(out Response<ICoinCapAssetRejected>? rejected))
      {
        logger.LogWarning("Asset {Id} rejected: {Reason}", rejected.Message.Id, rejected.Message.Reason);
        return BadRequest(rejected.Message);
      }

      return Problem("Unexpected response from the CoinCap polling service.");
    }
    catch (RequestTimeoutException ex)
    {
      logger.LogError(ex, "CoinCap polling service did not answer in time");
      return Problem("The CoinCap polling service did not answer in time.", statusCode: StatusCodes.Status504GatewayTimeout);
    }
  }
}
