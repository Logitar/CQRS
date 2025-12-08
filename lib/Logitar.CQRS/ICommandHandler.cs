namespace Logitar.CQRS;

/// <summary>
/// Represents a handler for a specific command.
/// </summary>
/// <typeparam name="TCommand">The type of the command.</typeparam>
/// <typeparam name="TResult">The type of the command result.</typeparam>
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
  /// <summary>
  /// Handles the specified command and returns its result.
  /// </summary>
  /// <param name="command">The command to handle.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>The command result.</returns>
  Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
