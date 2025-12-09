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

  /// <summary>
  /// Validates the retry settings.
  /// </summary>
  public void Validate()
  {
    List<string> errors = new(capacity: 7);

    if (Delay < 0)
    {
      errors.Add($"'{nameof(Delay)}' must be greater than or equal to 0.");
    }
    if (MaximumDelay < 0)
    {
      errors.Add($"'{nameof(MaximumDelay)}' must be greater than or equal to 0.");
    }

    switch (Algorithm)
    {
      case RetryAlgorithm.Exponential:
        if (Delay <= 0)
        {
          errors.Add($"'{nameof(Delay)}' must be greater than 0.");
        }
        if (ExponentialBase <= 1)
        {
          errors.Add($"'{nameof(ExponentialBase)}' must be greater than 1.");
        }
        break;
      case RetryAlgorithm.Linear:
        if (Delay <= 0)
        {
          errors.Add($"'{nameof(Delay)}' must be greater than 0.");
        }
        if (MaximumDelay > 0)
        {
          errors.Add($"'{nameof(Delay)}' must be 0 when '{nameof(Algorithm)}' is {Algorithm}.");
        }
        break;
      case RetryAlgorithm.Random:
        if (Delay <= 0)
        {
          errors.Add($"'{nameof(Delay)}' must be greater than 0.");
        }
        if (RandomVariation <= 0)
        {
          errors.Add($"'{nameof(RandomVariation)}' must be greater than 0.");
        }
        if (RandomVariation > Delay)
        {
          errors.Add($"'{nameof(RandomVariation)}' must be less than or equal to '{nameof(Delay)}'.");
        }
        if (MaximumDelay > 0)
        {
          errors.Add($"'{nameof(Delay)}' must be 0 when '{nameof(Algorithm)}' is {Algorithm}.");
        }
        break;
      case RetryAlgorithm.Fixed:
      case RetryAlgorithm.None:
        break;
      default:
        throw new ArgumentOutOfRangeException(nameof(Algorithm));
    }

    if (MaximumRetries < 0)
    {
      errors.Add($"'{nameof(MaximumRetries)}' must be greater than or equal to 0.");
    }

    if (errors.Count > 0)
    {
      StringBuilder message = new("Validation failed.");
      foreach (string error in errors)
      {
        message.AppendLine().Append(" - ").Append(error);
      }
      throw new InvalidOperationException(message.ToString());
    }
  }
}
