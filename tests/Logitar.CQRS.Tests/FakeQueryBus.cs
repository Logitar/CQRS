namespace Logitar.CQRS.Tests;

internal class FakeQueryBus : QueryBus
{
  public FakeQueryBus(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override bool ShouldRetry<TResult>(IQuery<TResult> query, Exception exception)
  {
    if (exception is TargetInvocationException targetInvocation && targetInvocation.InnerException is not null)
    {
      exception = targetInvocation.InnerException;
    }
    return exception is not NotImplementedException;
  }

  public new async Task<object> GetHandlerAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
  {
    return await base.GetHandlerAsync(query, cancellationToken);
  }

  public new int CalculateMillisecondsDelay<TResult>(IQuery<TResult> query, Exception exception, int attempt)
  {
    return base.CalculateMillisecondsDelay(query, exception, attempt);
  }
}
