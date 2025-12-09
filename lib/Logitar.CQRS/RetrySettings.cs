using Microsoft.Extensions.Configuration;

namespace Logitar.CQRS;

/// <summary>
/// Represents configuration options for retry behaviour, including timing strategy, base delays, and operational limits.
/// </summary>
public record RetrySettings
{
  /// <summary>
  /// The configuration section key used to bind retry settings.
  /// </summary>
  public const string SectionKey = "Retry";

  /// <summary>
  /// The algorithm determining how retry delays are calculated.
  /// </summary>
  public RetryAlgorithm Algorithm { get; set; }
  /// <summary>
  /// The base delay, in milliseconds, applied before the first retry or used as the fixed interval depending on the algorithm.
  /// </summary>
  public int Delay { get; set; }
  /// <summary>
  /// The numeric base used when applying exponential backoff. For example, a base of 2 doubles the delay on each retry attempt.
  /// </summary>
  public int ExponentialBase { get; set; }
  /// <summary>
  /// The maximum random variation, in milliseconds, added or subtracted when using randomised retry delays.
  /// </summary>
  public int RandomVariation { get; set; }

  /// <summary>
  /// The maximum number of retry attempts allowed before the operation is considered failed. A value of 0 typically means no retries.
  /// </summary>
  public int MaximumRetries { get; set; }
  /// <summary>
  /// The maximum delay, in milliseconds, permitted between retry attempts. This acts as a safety cap for exponential or linear backoff strategies.
  /// </summary>
  public int MaximumDelay { get; set; }

  /// <summary>
  /// Initializes <see cref="RetrySettings"/> by binding the configuration section and applying environment variable overrides when present.
  /// </summary>
  /// <param name="configuration">The configuration.</param>
  /// <returns>The initialized settings.</returns>
  public static RetrySettings Initialize(IConfiguration configuration)
  {
    RetrySettings settings = configuration.GetSection(SectionKey).Get<RetrySettings>() ?? new();

    settings.Algorithm = EnvironmentHelper.GetEnum("RETRY_ALGORITHM", settings.Algorithm);
    settings.Delay = EnvironmentHelper.GetInt32("RETRY_DELAY", settings.Delay);
    settings.ExponentialBase = EnvironmentHelper.GetInt32("RETRY_EXPONENTIAL_BASE", settings.ExponentialBase);
    settings.RandomVariation = EnvironmentHelper.GetInt32("RETRY_RANDOM_VARIATION", settings.RandomVariation);

    settings.MaximumRetries = EnvironmentHelper.GetInt32("RETRY_MAXIMUM_RETRIES", settings.MaximumRetries);
    settings.MaximumDelay = EnvironmentHelper.GetInt32("RETRY_MAXIMUM_DELAY", settings.MaximumDelay);

    return settings;
  }
}
