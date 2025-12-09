namespace Logitar.CQRS;

/// <summary>
/// Represents a bus in which commands are sent.
/// </summary>
public interface ICommandBus
{
  /// <summary>
  /// Executes the specified command.
  /// </summary>
  /// <typeparam name="TResult">The type of the command result.</typeparam>
  /// <param name="command">The command to execute.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>The command result.</returns>
  Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
}
