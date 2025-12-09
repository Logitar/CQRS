using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logitar.CQRS;

/// <summary>
/// Represents an in-memory bus in which sent querys are executed synchronously.
/// </summary>
public class QueryBus : IQueryBus
{
  /// <summary>
  /// The name of the query handler method.
  /// </summary>
  protected const string HandlerName = nameof(IQueryHandler<,>.HandleAsync);

  /// <summary>
  /// Gets the logger.
  /// </summary>
  protected virtual ILogger<QueryBus>? Logger { get; }
  /// <summary>
  /// Gets the pseudo-random number generator.
  /// </summary>
  protected virtual Random Random { get; } = new();
  /// <summary>
  /// Gets the service provider.
  /// </summary>
  protected virtual IServiceProvider ServiceProvider { get; }
  /// <summary>
  /// Gets the retry settings.
  /// </summary>
  protected virtual RetrySettings Settings { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="QueryBus"/> class.
  /// </summary>
  /// <param name="serviceProvider">The service provider.</param>
  public QueryBus(IServiceProvider serviceProvider)
  {
    Logger = serviceProvider.GetService<ILogger<QueryBus>>();
    ServiceProvider = serviceProvider;
    Settings = serviceProvider.GetService<RetrySettings>() ?? new();
  }

  /// <summary>
  /// Executes the specified query.
  /// </summary>
  /// <typeparam name="TResult">The type of the query result.</typeparam>
  /// <param name="query">The query to execute.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>The query result.</returns>
  /// <exception cref="InvalidOperationException">The handler did not define the handle method, or it did not return a task.</exception>
  public virtual async Task<TResult> ExecuteAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
  {
    Settings.Validate();

    object handler = await GetHandlerAsync(query, cancellationToken);

    Type handlerType = handler.GetType();
    Type queryType = query.GetType();
    Type[] parameterTypes = [queryType, typeof(CancellationToken)];
    MethodInfo handle = handlerType.GetMethod(HandlerName, parameterTypes)
      ?? throw new InvalidOperationException($"The handler {handlerType} must define a '{HandlerName}' method.");

    object[] parameters = [query, cancellationToken];
    Exception? innerException;
    int attempt = 0;
    while (true)
    {
      attempt++;
      try
      {
        object? result = handle.Invoke(handler, parameters);
        if (result is not Task<TResult> task)
        {
          throw new InvalidOperationException($"The handler {handlerType} {HandlerName} method must return a {nameof(Task)}.");
        }
        return await task;
      }
      catch (Exception exception)
      {
        if (!ShouldRetry(query, exception))
        {
          throw;
        }
        innerException = exception;

        int millisecondsDelay = CalculateMillisecondsDelay(query, exception, attempt);
        if (millisecondsDelay < 0)
        {
          throw new InvalidOperationException($"The retry delay '{millisecondsDelay}' should be greater than or equal to 0ms.");
        }

        if (Settings.Algorithm == RetryAlgorithm.None
          || (Settings.MaximumRetries > 0 && attempt > Settings.MaximumRetries)
          || (Settings.MaximumDelay > 0 && millisecondsDelay > Settings.MaximumDelay))
        {
          break;
        }

        if (Logger is not null && Logger.IsEnabled(LogLevel.Warning))
        {
          Logger.LogWarning(exception, "Query '{Query}' execution failed at attempt {Attempt}, will retry in {Delay}ms.", queryType, attempt, millisecondsDelay);
        }

        if (millisecondsDelay > 0)
        {
          await Task.Delay(millisecondsDelay, cancellationToken);
        }
      }
    }

    throw new InvalidOperationException($"Query '{queryType}' execution failed after {attempt} attempts. See inner exception for more detail.", innerException);
  }

  /// <summary>
  /// Finds the handler for the specified query.
  /// </summary>
  /// <typeparam name="TResult">The type of the query result.</typeparam>
  /// <param name="query">The query.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>The query handler.</returns>
  /// <exception cref="InvalidOperationException">There is no handler or many handlers for the specified query.</exception>
  protected virtual async Task<object> GetHandlerAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
  {
    Type queryType = query.GetType();
    IEnumerable<object> handlers = ServiceProvider.GetServices(typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResult)))
      .Where(handler => handler is not null)
      .Select(handler => handler!);
    int count = handlers.Count();
    if (count != 1)
    {
      StringBuilder message = new StringBuilder("Exactly one handler was expected for query of type '").Append(queryType).Append("', but ");
      if (count < 1)
      {
        message.Append("none was found.");
      }
      else
      {
        message.Append(count).Append(" were found.");
      }
      throw new InvalidOperationException(message.ToString());
    }
    return handlers.Single();
  }

  /// <summary>
  /// Determines if the query execution should be retried or not.
  /// </summary>
  /// <typeparam name="TResult">The type of the query result.</typeparam>
  /// <param name="query">The query to execute.</param>
  /// <param name="exception">The exception.</param>
  /// <returns>A value indicating whether or not the query execution should be retried.</returns>
  protected virtual bool ShouldRetry<TResult>(IQuery<TResult> query, Exception exception)
  {
    return true;
  }

  /// <summary>
  /// Calculates the delay, in milliseconds, to wait before retrying the execution of a query after a failure.
  /// The delay is computed according to the retry algorithm and configuration defined in <see cref="RetrySettings"/>.
  /// </summary>
  /// <typeparam name="TResult">The type of the query result.</typeparam>
  /// <param name="query">The query to execute.</param>
  /// <param name="exception">The exception.</param>
  /// <param name="attempt">The current retry attempt number, starting at 1.</param>
  /// <returns>The number of milliseconds to wait before retrying. Returns <c>0</c> when retrying should occur immediately or when the configured delay or algorithm does not produce a positive value.</returns>
  protected virtual int CalculateMillisecondsDelay<TResult>(IQuery<TResult> query, Exception exception, int attempt)
  {
    if (Settings.Delay > 0)
    {
      switch (Settings.Algorithm)
      {
        case RetryAlgorithm.Exponential:
          if (Settings.ExponentialBase > 1)
          {
            return (int)Math.Pow(Settings.ExponentialBase, attempt - 1) * Settings.Delay;
          }
          break;
        case RetryAlgorithm.Fixed:
          return Settings.Delay;
        case RetryAlgorithm.Linear:
          return attempt * Settings.Delay;
        case RetryAlgorithm.Random:
          if (Settings.RandomVariation > 0 && Settings.RandomVariation < Settings.Delay)
          {
            int minimum = Settings.Delay - Settings.RandomVariation;
            int maximum = Settings.Delay + Settings.RandomVariation;
            return Random.Next(minimum, maximum + 1);
          }
          break;
      }
    }
    return 0;
  }
}
