namespace Logitar.CQRS.Tests;

internal class ArgumentOutOfRangeCommandHandler : ICommandHandler<Command, Unit>
{
  public Task<Unit> HandleAsync(Command command, CancellationToken cancellationToken)
  {
    throw new ArgumentOutOfRangeException(nameof(command));
  }
}
