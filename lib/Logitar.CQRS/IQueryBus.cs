namespace Logitar.CQRS;

/// <summary>
/// Represents a bus in which queries are sent.
/// </summary>
public interface IQueryBus
{
  /// <summary>
  /// Executes the specified query.
  /// </summary>
  /// <typeparam name="TResult">The type of the query result.</typeparam>
  /// <param name="query">The query to execute.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>The query result.</returns>
  Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
