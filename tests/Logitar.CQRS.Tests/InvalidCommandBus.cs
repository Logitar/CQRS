namespace Logitar.CQRS.Tests;

internal class InvalidCommandBus : CommandBus
{
  private readonly object _handler;
  private readonly int _millisecondsDelay;

  public InvalidCommandBus(IServiceProvider serviceProvider, object handler, int millisecondsDelay = 0) : base(serviceProvider)
  {
    _handler = handler;
    _millisecondsDelay = millisecondsDelay;
  }

  protected override Task<object> GetHandlerAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
  {
    return Task.FromResult(_handler);
  }

  protected override int CalculateMillisecondsDelay<TResult>(ICommand<TResult> command, Exception exception, int attempt)
  {
    return _millisecondsDelay;
  }
}
