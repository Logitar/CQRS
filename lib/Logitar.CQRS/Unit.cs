namespace Logitar.CQRS;

/// <summary>
/// Represents a void type, since <see cref="Void"/> is not a valid return type in C#.
/// </summary>
public readonly struct Unit
{
  /// <summary>
  /// Gets the default and only value of a unit.
  /// </summary>
  public static readonly Unit Value = new();

  /// <summary>
  /// Gets a completed task from the default and only unit value.
  /// </summary>
  public static Task<Unit> CompletedTask => Task.FromResult(Value);

  /// <summary>
  /// Returns a value indicating whether or not the specified units are equal.
  /// </summary>
  /// <param name="left">The first unit to compare.</param>
  /// <param name="right">The other unit to compare.</param>
  /// <returns>True if the units are equal.</returns>
  public static bool operator ==(Unit left, Unit right) => left.Equals(right);
  /// <summary>
  /// Returns a value indicating whether or not the specified units are different.
  /// </summary>
  /// <param name="left">The first unit to compare.</param>
  /// <param name="right">The other unit to compare.</param>
  /// <returns>True if the units are different.</returns>
  public static bool operator !=(Unit left, Unit right) => !(left == right);

  /// <summary>
  /// Returns a value indicating whether or not the specified object is equal to the unit.
  /// </summary>
  /// <param name="obj">The object to be compared to.</param>
  /// <returns>True if the object is equal to the unit.</returns>
  public override bool Equals([NotNullWhen(true)] object? obj) => obj is Unit;
  /// <summary>
  /// Returns the hash code of the current unit.
  /// </summary>
  /// <returns>The hash code.</returns>
  public override int GetHashCode() => 0;
  /// <summary>
  /// Returns a string representation of the unit.
  /// </summary>
  /// <returns>The string representation.</returns>
  public override string ToString() => "()";
}
