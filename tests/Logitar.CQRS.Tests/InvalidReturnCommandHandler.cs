namespace Logitar.CQRS.Tests;

internal class InvalidReturnCommandHandler
{
  public Unit HandleAsync(ICommand command, CancellationToken cancellationToken) => Unit.Value;
}
