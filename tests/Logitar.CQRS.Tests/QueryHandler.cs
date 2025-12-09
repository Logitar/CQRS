namespace Logitar.CQRS.Tests;

internal class QueryHandler : IQueryHandler<Query, Unit>
{
  public Task<Unit> HandleAsync(Query query, CancellationToken cancellationToken) => Unit.CompletedTask;
}
