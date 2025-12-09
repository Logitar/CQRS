namespace Logitar.CQRS;

/// <summary>
/// Defines retry scheduling strategies used to determine when subsequent attempts are executed.
/// </summary>
public enum RetryAlgorithm
{
  /// <summary>
  /// No retry is performed. The operation fails immediately on the first error.
  /// </summary>
  None = 0,

  /// <summary>
  /// Each retry delay increases exponentially, typically doubling after every failed attempt
  /// (e.g., 1s, 2s, 4s, 8s). Useful for reducing load on congested systems.
  /// </summary>
  Exponential = 1,

  /// <summary>
  /// A constant delay is applied between every retry attempt (e.g., always 5s).
  /// Suitable for predictable and steady retry pacing.
  /// </summary>
  Fixed = 2,

  /// <summary>
  /// The delay grows linearly with each retry attempt (e.g., 1s, 2s, 3s, 4s).
  /// Provides gradual backoff without the rapid escalation of exponential strategies.
  /// </summary>
  Linear = 3,

  /// <summary>
  /// Each retry delay is selected randomly within a defined range.
  /// Helps reduce synchronized retry storms across multiple clients.
  /// </summary>
  Random = 4
}
