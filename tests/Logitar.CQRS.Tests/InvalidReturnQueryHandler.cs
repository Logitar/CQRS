namespace Logitar.CQRS.Tests;

internal class InvalidReturnQueryHandler
{
  public Unit HandleAsync(IQuery<Unit> query, CancellationToken cancellationToken) => Unit.Value;
}
