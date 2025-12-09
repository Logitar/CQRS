namespace Logitar.CQRS;

/// <summary>
/// Represents a handler for a specific query.
/// </summary>
/// <typeparam name="TQuery">The type of the query.</typeparam>
/// <typeparam name="TResult">The type of the query result.</typeparam>
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
  /// <summary>
  /// Handles the specified query and returns its result.
  /// </summary>
  /// <param name="query">The query to handle.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>The query result.</returns>
  Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
