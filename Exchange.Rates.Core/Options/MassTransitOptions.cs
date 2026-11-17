using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Exchange.Rates.Core.Options;

/// <summary>
/// RabbitMQ connection and receive endpoint settings, bound from the "MassTransitOptions" section.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class MassTransitOptions
{
  public const string SectionName = "MassTransitOptions";

  [Required]
  public string Host { get; set; } = string.Empty;

  public string Username { get; set; } = "guest";

  public string Password { get; set; } = "guest";

  /// <summary>
  /// Queue the polling service consumes from. Not used by the OpenApi services.
  /// </summary>
  public string? QueueName { get; set; }

  [Range(1, 1000)]
  public int ReceiveEndpointPrefetchCount { get; set; } = 16;
}
