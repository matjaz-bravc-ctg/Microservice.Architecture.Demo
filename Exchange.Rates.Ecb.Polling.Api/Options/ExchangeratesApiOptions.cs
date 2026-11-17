using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Exchange.Rates.Ecb.Polling.Api.Options;

/// <summary>
/// Settings of the ECB exchange rates API, bound from the "ExchangeratesApiOptions" section.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ExchangeratesApiOptions
{
  public const string SectionName = "ExchangeratesApiOptions";

  [Range(1, 1440)]
  public int HandlerLifetimeMinutes { get; set; } = 5;

  public string? Name { get; set; }

  /// <summary>
  /// Base URL of the Frankfurter API, e.g. https://api.frankfurter.dev/v1
  /// </summary>
  [Required, Url]
  public string Url { get; set; } = string.Empty;
}
