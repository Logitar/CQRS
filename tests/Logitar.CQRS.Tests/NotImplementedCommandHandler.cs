namespace Logitar.CQRS.Tests;

internal class NotImplementedCommandHandler : ICommandHandler<Command, Unit>
{
  public Task<Unit> HandleAsync(Command command, CancellationToken cancellationToken)
  {
    throw new NotImplementedException();
  }
}
