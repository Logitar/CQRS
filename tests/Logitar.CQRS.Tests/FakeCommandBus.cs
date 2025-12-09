namespace Logitar.CQRS.Tests;

internal class FakeCommandBus : CommandBus
{
  public FakeCommandBus(IServiceProvider serviceProvider) : base(serviceProvider)
  {
  }

  protected override bool ShouldRetry<TResult>(ICommand<TResult> command, Exception exception)
  {
    if (exception is TargetInvocationException targetInvocation && targetInvocation.InnerException is not null)
    {
      exception = targetInvocation.InnerException;
    }
    return exception is not NotImplementedException;
  }

  public new async Task<object> GetHandlerAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
  {
    return await base.GetHandlerAsync(command, cancellationToken);
  }

  public new int CalculateMillisecondsDelay<TResult>(ICommand<TResult> command, Exception exception, int attempt)
  {
    return base.CalculateMillisecondsDelay(command, exception, attempt);
  }
}
