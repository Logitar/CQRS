namespace Logitar.CQRS.Tests;

internal class ArgumentOutOfRangeQueryHandler : IQueryHandler<Query, Unit>
{
  public Task<Unit> HandleAsync(Query query, CancellationToken cancellationToken)
  {
    throw new ArgumentOutOfRangeException(nameof(query));
  }
}
