namespace Logitar.CQRS.Tests;

internal class InvalidQueryBus : QueryBus
{
  private readonly object _handler;
  private readonly int _millisecondsDelay;

  public InvalidQueryBus(IServiceProvider serviceProvider, object handler, int millisecondsDelay = 0) : base(serviceProvider)
  {
    _handler = handler;
    _millisecondsDelay = millisecondsDelay;
  }

  protected override Task<object> GetHandlerAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
  {
    return Task.FromResult(_handler);
  }

  protected override int CalculateMillisecondsDelay<TResult>(IQuery<TResult> query, Exception exception, int attempt)
  {
    return _millisecondsDelay;
  }
}
