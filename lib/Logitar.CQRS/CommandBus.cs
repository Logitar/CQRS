using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logitar.CQRS;

/// <summary>
/// Represents an in-memory bus in which sent commands are executed synchronously.
/// </summary>
public class CommandBus : ICommandBus
{
  /// <summary>
  /// The name of the command handler method.
  /// </summary>
  protected const string HandlerName = nameof(ICommandHandler<,>.HandleAsync);

  /// <summary>
  /// Gets the logger.
  /// </summary>
  protected virtual ILogger<CommandBus>? Logger { get; }
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
  /// Initializes a new instance of the <see cref="CommandBus"/> class.
  /// </summary>
  /// <param name="serviceProvider">The service provider.</param>
  public CommandBus(IServiceProvider serviceProvider)
  {
    Logger = serviceProvider.GetService<ILogger<CommandBus>>();
    ServiceProvider = serviceProvider;
    Settings = serviceProvider.GetService<RetrySettings>() ?? new();
  }

  /// <summary>
  /// Executes the specified command.
  /// </summary>
  /// <typeparam name="TResult">The type of the command result.</typeparam>
  /// <param name="command">The command to execute.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>The command result.</returns>
  /// <exception cref="InvalidOperationException">The handler did not define the handle method, or it did not return a task.</exception>
  public virtual async Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
  {
    Settings.Validate();

    object handler = await GetHandlerAsync(command, cancellationToken);

    Type handlerType = handler.GetType();
    Type commandType = command.GetType();
    Type[] parameterTypes = [commandType, typeof(CancellationToken)];
    MethodInfo handle = handlerType.GetMethod(HandlerName, parameterTypes)
      ?? throw new InvalidOperationException($"The handler {handlerType} must define a '{HandlerName}' method.");

    object[] parameters = [command, cancellationToken];
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
        if (!ShouldRetry(command, exception))
        {
          throw;
        }
        innerException = exception;

        int millisecondsDelay = CalculateMillisecondsDelay(command, exception, attempt);
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
          Logger.LogWarning(exception, "Command '{Command}' execution failed at attempt {Attempt}, will retry in {Delay}ms.", commandType, attempt, millisecondsDelay);
        }

        if (millisecondsDelay > 0)
        {
          await Task.Delay(millisecondsDelay, cancellationToken);
        }
      }
    }

    throw new InvalidOperationException($"Command '{commandType}' execution failed after {attempt} attempts. See inner exception for more detail.", innerException);
  }

  /// <summary>
  /// Finds the handler for the specified command.
  /// </summary>
  /// <typeparam name="TResult">The type of the command result.</typeparam>
  /// <param name="command">The command.</param>
  /// <param name="cancellationToken">The cancellation token.</param>
  /// <returns>The command handler.</returns>
  /// <exception cref="InvalidOperationException">There is no handler or many handlers for the specified command.</exception>
  protected virtual async Task<object> GetHandlerAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
  {
    Type commandType = command.GetType();
    IEnumerable<object> handlers = ServiceProvider.GetServices(typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult)))
      .Where(handler => handler is not null)
      .Select(handler => handler!);
    int count = handlers.Count();
    if (count != 1)
    {
      StringBuilder message = new StringBuilder("Exactly one handler was expected for command of type '").Append(commandType).Append("', but ");
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
  /// Determines if the command execution should be retried or not.
  /// </summary>
  /// <typeparam name="TResult">The type of the command result.</typeparam>
  /// <param name="command">The command to execute.</param>
  /// <param name="exception">The exception.</param>
  /// <returns>A value indicating whether or not the command execution should be retried.</returns>
  protected virtual bool ShouldRetry<TResult>(ICommand<TResult> command, Exception exception)
  {
    return true;
  }

  /// <summary>
  /// Calculates the delay, in milliseconds, to wait before retrying the execution of a command after a failure.
  /// The delay is computed according to the retry algorithm and configuration defined in <see cref="RetrySettings"/>.
  /// </summary>
  /// <typeparam name="TResult">The type of the command result.</typeparam>
  /// <param name="command">The command to execute.</param>
  /// <param name="exception">The exception.</param>
  /// <param name="attempt">The current retry attempt number, starting at 1.</param>
  /// <returns>The number of milliseconds to wait before retrying. Returns <c>0</c> when retrying should occur immediately or when the configured delay or algorithm does not produce a positive value.</returns>
  protected virtual int CalculateMillisecondsDelay<TResult>(ICommand<TResult> command, Exception exception, int attempt)
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
