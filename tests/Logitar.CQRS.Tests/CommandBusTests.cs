using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Logitar.CQRS.Tests;

[Trait(Traits.Category, Categories.Unit)]
public class CommandBusTests
{
  private readonly CancellationToken _cancellationToken = default;

  [Fact(DisplayName = "CalculateMillisecondsDelay: it should return 0 when the algorithm is None.")]
  public void Given_AlgorithmIsNone_When_CalculateMillisecondsDelay_Then_ZeroReturned()
  {
    ServiceCollection services = new();
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.None,
      Delay = 100
    };
    services.AddSingleton(settings);
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    Exception exception = new();
    int attempt = 1;
    Assert.Equal(0, commandBus.CalculateMillisecondsDelay(command, exception, attempt));
  }

  [Theory(DisplayName = "CalculateMillisecondsDelay: it should return 0 when the delay is zero or negative.")]
  [InlineData(0)]
  [InlineData(-1)]
  public void Given_ZeroOrNegativeDelay_When_CalculateMillisecondsDelay_Then_ZeroReturned(int delay)
  {
    Assert.True(delay <= 0);

    ServiceCollection services = new();
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Fixed,
      Delay = delay
    };
    services.AddSingleton(settings);
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    Exception exception = new();
    int attempt = 1;
    Assert.Equal(0, commandBus.CalculateMillisecondsDelay(command, exception, attempt));
  }

  [Theory(DisplayName = "CalculateMillisecondsDelay: it should return 0 when the exponential base is less than 2.")]
  [InlineData(-1)]
  [InlineData(0)]
  [InlineData(1)]
  public void Given_ExponentialBaseLessThanTwo_When_CalculateMillisecondsDelay_Then_ZeroReturned(int exponentialBase)
  {
    Assert.True(exponentialBase < 2);

    ServiceCollection services = new();
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Exponential,
      Delay = 100,
      ExponentialBase = exponentialBase
    };
    services.AddSingleton(settings);
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    Exception exception = new();
    int attempt = 1;
    Assert.Equal(0, commandBus.CalculateMillisecondsDelay(command, exception, attempt));
  }

  [Theory(DisplayName = "CalculateMillisecondsDelay: it should return 0 when the random variation is greater than or equal to the delay.")]
  [InlineData(100, 100)]
  [InlineData(100, 1000)]
  public void Given_RandomVariationGreaterThanOrEqualToDelay_When_CalculateMillisecondsDelay_Then_ZeroReturned(int delay, int randomVariation)
  {
    Assert.True(delay > 0);
    Assert.True(randomVariation > 0);
    Assert.True(delay <= randomVariation);

    ServiceCollection services = new();
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Random,
      Delay = 100,
      RandomVariation = randomVariation
    };
    services.AddSingleton(settings);
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    Exception exception = new();
    int attempt = 1;
    Assert.Equal(0, commandBus.CalculateMillisecondsDelay(command, exception, attempt));
  }

  [Theory(DisplayName = "CalculateMillisecondsDelay: it should return 0 when the random variation is zero or negative.")]
  [InlineData(0)]
  [InlineData(-1)]
  public void Given_ZeroOrNegativeRandomVariation_When_CalculateMillisecondsDelay_Then_ZeroReturned(int randomVariation)
  {
    Assert.True(randomVariation <= 0);

    ServiceCollection services = new();
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Random,
      Delay = 100,
      RandomVariation = randomVariation
    };
    services.AddSingleton(settings);
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    Exception exception = new();
    int attempt = 1;
    Assert.Equal(0, commandBus.CalculateMillisecondsDelay(command, exception, attempt));
  }

  [Theory(DisplayName = "CalculateMillisecondsDelay: it should return the correct exponential delay.")]
  [InlineData(100, 10, 2, 1000)]
  [InlineData(100, 2, 5, 1600)]
  public void Given_Exponential_When_CalculateMillisecondsDelay_Then_CorrectDelay(int delay, int exponentialBase, int attempt, int millisecondsDelay)
  {
    Assert.True(delay > 0);
    Assert.True(exponentialBase > 1);
    Assert.True(attempt > 0);
    Assert.True(millisecondsDelay > 0);

    ServiceCollection services = new();
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Exponential,
      Delay = delay,
      ExponentialBase = exponentialBase
    };
    services.AddSingleton(settings);
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    Exception exception = new();
    Assert.Equal(millisecondsDelay, commandBus.CalculateMillisecondsDelay(command, exception, attempt));
  }

  [Fact(DisplayName = "CalculateMillisecondsDelay: it should return the correct fixed delay.")]
  public void Given_Fixed_When_CalculateMillisecondsDelay_Then_CorrectDelay()
  {
    ServiceCollection services = new();
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Fixed,
      Delay = 100
    };
    services.AddSingleton(settings);
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    Exception exception = new();
    int attempt = 10;
    Assert.Equal(settings.Delay, commandBus.CalculateMillisecondsDelay(command, exception, attempt));
  }

  [Fact(DisplayName = "CalculateMillisecondsDelay: it should return the correct linear delay.")]
  public void Given_Linear_When_CalculateMillisecondsDelay_Then_CorrectDelay()
  {
    ServiceCollection services = new();
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Linear,
      Delay = 100
    };
    services.AddSingleton(settings);
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    Exception exception = new();
    int attempt = 5;
    Assert.Equal(settings.Delay * attempt, commandBus.CalculateMillisecondsDelay(command, exception, attempt));
  }

  [Fact(DisplayName = "CalculateMillisecondsDelay: it should return the correct random delay.")]
  public void Given_Random_When_CalculateMillisecondsDelay_Then_CorrectDelay()
  {
    ServiceCollection services = new();
    RetrySettings settings = new()
    {
      Algorithm = RetryAlgorithm.Random,
      Delay = 500,
      RandomVariation = 450
    };
    services.AddSingleton(settings);
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    Exception exception = new();
    int attempt = 5;
    int delay = commandBus.CalculateMillisecondsDelay(command, exception, attempt);

    int minimum = settings.Delay - settings.RandomVariation;
    int maximum = settings.Delay + settings.RandomVariation;
    Assert.True(minimum <= delay && delay <= maximum);
  }

  [Fact(DisplayName = "ExecuteAsync: it should log a warning when an execution is retried.")]
  public async Task Given_Retry_When_ExecuteAsync_Then_WarningLogged()
  {
    Mock<ILogger<CommandBus>> logger = new();
    logger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);

    ServiceCollection services = new();
    services.AddSingleton<ICommandHandler<Command, Unit>, NotImplementedCommandHandler>();
    services.AddSingleton(logger.Object);
    services.AddSingleton(new RetrySettings
    {
      Algorithm = RetryAlgorithm.Fixed,
      Delay = 100,
      MaximumRetries = 2
    });
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    CommandBus commandBus = new(serviceProvider);

    Command command = new();
    await Assert.ThrowsAsync<InvalidOperationException>(async () => await commandBus.ExecuteAsync(command, _cancellationToken));

    logger.Verify(x => x.Log(
      LogLevel.Warning,
      It.IsAny<EventId>(),
      It.IsAny<It.IsAnyType>(),
      It.IsAny<Exception>(),
      (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.AtLeastOnce());
  }

  [Fact(DisplayName = "ExecuteAsync: it should rethrow the exception when it should not be retried.")]
  public async Task Given_ExceptionNotRetried_When_ExecuteAsync_Then_ExceptionRethrown()
  {
    ServiceCollection services = new();
    services.AddSingleton<ICommandHandler<Command, Unit>, NotImplementedCommandHandler>();
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    var exception = await Assert.ThrowsAsync<TargetInvocationException>(async () => await commandBus.ExecuteAsync(command, _cancellationToken));
    Assert.IsType<NotImplementedException>(exception.InnerException);
  }

  [Fact(DisplayName = "ExecuteAsync: it should retry the execution given a maximum delay.")]
  public async Task Given_MaximumDelay_When_ExecuteAsync_Then_RetriedUntilReached()
  {
    Mock<ILogger<CommandBus>> logger = new();
    logger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);

    ServiceCollection services = new();
    services.AddSingleton<ICommandHandler<Command, Unit>, NotImplementedCommandHandler>();
    services.AddSingleton(logger.Object);
    services.AddSingleton(new RetrySettings
    {
      Algorithm = RetryAlgorithm.Exponential,
      Delay = 100,
      ExponentialBase = 2,
      MaximumDelay = 1000
    });
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    CommandBus commandBus = new(serviceProvider);

    Command command = new();
    await Assert.ThrowsAsync<InvalidOperationException>(async () => await commandBus.ExecuteAsync(command, _cancellationToken));

    logger.Verify(x => x.Log(
      LogLevel.Warning,
      It.IsAny<EventId>(),
      It.IsAny<It.IsAnyType>(),
      It.IsAny<Exception>(),
      (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Exactly(4)); // NOTE(fpion): 100, 200, 400, 800
  }

  [Theory(DisplayName = "ExecuteAsync: it should retry the execution given maximum retries.")]
  [InlineData(1)]
  [InlineData(5)]
  public async Task Given_MaximumRetries_When_ExecuteAsync_Then_RetriedUntilReached(int maximumRetries)
  {
    Mock<ILogger<CommandBus>> logger = new();
    logger.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);

    ServiceCollection services = new();
    services.AddSingleton<ICommandHandler<Command, Unit>, NotImplementedCommandHandler>();
    services.AddSingleton(logger.Object);
    services.AddSingleton(new RetrySettings
    {
      Algorithm = RetryAlgorithm.Fixed,
      Delay = 100,
      MaximumRetries = maximumRetries
    });
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    CommandBus commandBus = new(serviceProvider);

    Command command = new();
    await Assert.ThrowsAsync<InvalidOperationException>(async () => await commandBus.ExecuteAsync(command, _cancellationToken));

    logger.Verify(x => x.Log(
      LogLevel.Warning,
      It.IsAny<EventId>(),
      It.IsAny<It.IsAnyType>(),
      It.IsAny<Exception>(),
      (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.Exactly(maximumRetries));
  }

  [Fact(DisplayName = "ExecuteAsync: it should return the execution result when it succeeded.")]
  public async Task Given_Success_When_ExecuteAsync_Then_ExecutionResult()
  {
    ServiceCollection services = new();
    services.AddSingleton<ICommandHandler<Command, Unit>, CommandHandler>();
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    Unit result = await commandBus.ExecuteAsync(command, _cancellationToken);
    Assert.Equal(Unit.Value, result);
  }

  [Fact(DisplayName = "ExecuteAsync: it should throw InvalidOperationException when the execution failed.")]
  public async Task Given_ExecutionFailed_When_ExecuteAsync_Then_InvalidOperationException()
  {
    ServiceCollection services = new();
    services.AddSingleton<ICommandHandler<Command, Unit>, NotImplementedCommandHandler>();
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    CommandBus commandBus = new(serviceProvider);

    Command command = new();
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await commandBus.ExecuteAsync(command, _cancellationToken));
    Assert.Equal($"Command '{command.GetType()}' execution failed after 1 attempts. See inner exception for more detail.", exception.Message);
    Assert.IsType<TargetInvocationException>(exception.InnerException);
    Assert.IsType<NotImplementedException>(exception.InnerException.InnerException);
  }

  [Fact(DisplayName = "ExecuteAsync: it should throw InvalidOperationException when the handler does not have a handle.")]
  public async Task Given_HandlerWithoutHandle_When_ExecuteAsync_Then_InvalidOperationException()
  {
    ServiceCollection services = new();
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    InvalidCommandBus commandBus = new(serviceProvider, new HandlelessCommandHandler());

    Command command = new();
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await commandBus.ExecuteAsync(command, _cancellationToken));
    Assert.Equal($"The handler {typeof(HandlelessCommandHandler)} must define a 'HandleAsync' method.", exception.Message);
  }

  [Fact(DisplayName = "ExecuteAsync: it should throw InvalidOperationException when the handler does not return a task.")]
  public async Task Given_HandlerDoesNotReturnTask_When_ExecuteAsync_Then_InvalidOperationException()
  {
    ServiceCollection services = new();
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    InvalidCommandBus commandBus = new(serviceProvider, new InvalidReturnCommandHandler());

    Command command = new();
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await commandBus.ExecuteAsync(command, _cancellationToken));
    Assert.IsType<InvalidOperationException>(exception.InnerException);
    Assert.Equal($"The handler {typeof(InvalidReturnCommandHandler)} HandleAsync method must return a Task.", exception.InnerException.Message);
  }

  [Fact(DisplayName = "ExecuteAsync: it should throw InvalidOperationException when the milliseconds delay is negative.")]
  public async Task Given_NegativeDelay_When_ExecuteAsync_Then_InvalidOperationException()
  {
    ServiceCollection services = new();
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    InvalidCommandBus commandBus = new(serviceProvider, new NotImplementedCommandHandler(), millisecondsDelay: -1);

    Command command = new();
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await commandBus.ExecuteAsync(command, _cancellationToken));
    Assert.Equal("The retry delay '-1' should be greater than or equal to 0ms.", exception.Message);
  }

  [Fact(DisplayName = "ExecuteAsync: it should throw InvalidOperationException when the settings are not valid.")]
  public async Task Given_InvalidSettings_When_ExecuteAsync_Then_InvalidOperationException()
  {
    ServiceCollection services = new();
    services.AddSingleton(new RetrySettings
    {
      Delay = -1
    });
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    CommandBus commandBus = new(serviceProvider);

    Command command = new();
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await commandBus.ExecuteAsync(command, _cancellationToken));
    string[] lines = exception.Message.Remove("\r").Split('\n');
    Assert.Equal(2, lines.Length);
    Assert.Equal("Validation failed.", lines[0]);
    Assert.Equal(" - 'Delay' must be greater than or equal to 0.", lines[1]);
  }

  [Fact(DisplayName = "GetHandlerAsync: it should return the handler found.")]
  public async Task Given_SingleHandler_When_GetHandlerAsync_Then_HandlerReturned()
  {
    ServiceCollection services = new();
    services.AddSingleton<ICommandHandler<Command, Unit>, CommandHandler>();
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    object handler = await commandBus.GetHandlerAsync(command, _cancellationToken);
    Assert.IsType<CommandHandler>(handler);
  }

  [Fact(DisplayName = "GetHandlerAsync: it should throw InvalidOperationException when many handlers were found.")]
  public async Task Given_ManyHandlers_When_GetHandlerAsync_Then_InvalidOperationException()
  {
    ServiceCollection services = new();
    services.AddSingleton<ICommandHandler<Command, Unit>, CommandHandler>();
    services.AddSingleton<ICommandHandler<Command, Unit>, CommandHandler>();
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await commandBus.GetHandlerAsync(command, _cancellationToken));
    Assert.Equal($"Exactly one handler was expected for command of type '{command.GetType()}', but 2 were found.", exception.Message);
  }

  [Fact(DisplayName = "GetHandlerAsync: it should throw InvalidOperationException when no handler was found.")]
  public async Task Given_NoHandler_When_GetHandlerAsync_Then_InvalidOperationException()
  {
    ServiceCollection services = new();
    IServiceProvider serviceProvider = services.BuildServiceProvider();
    FakeCommandBus commandBus = new(serviceProvider);

    Command command = new();
    var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await commandBus.GetHandlerAsync(command, _cancellationToken));
    Assert.Equal($"Exactly one handler was expected for command of type '{command.GetType()}', but none was found.", exception.Message);
  }
}
