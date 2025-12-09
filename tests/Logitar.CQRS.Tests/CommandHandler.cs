namespace Logitar.CQRS.Tests;

internal class CommandHandler : ICommandHandler<Command, Unit>
{
  public Task<Unit> HandleAsync(Command command, CancellationToken cancellationToken) => Unit.CompletedTask;
}
