namespace Logitar.CQRS;

/// <summary>
/// Represents a command without result.
/// </summary>
public interface ICommand : ICommand<Unit>;

/// <summary>
/// Represents a command returning a result.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface ICommand<TResult>;
