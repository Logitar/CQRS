namespace Logitar.CQRS.Tests;

internal class NotImplementedQueryHandler : IQueryHandler<Query, Unit>
{
  public Task<Unit> HandleAsync(Query query, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}
