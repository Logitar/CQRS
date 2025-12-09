using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logitar.CQRS;

/// <summary>
/// Provides extension methods for registering the Logitar CQRS pattern into a dependency injection container.
/// </summary>
public static class DependencyInjectionExtensions
{
  /// <summary>
  /// Registers the command and query buses, along with their supporting configuration such as <see cref="RetrySettings"/>, into the service collection.
  /// This enables Logitar's CQRS handling throughout the application.
  /// </summary>
  /// <param name="services">The service collection.</param>
  /// <returns>The service collection.</returns>
  public static IServiceCollection AddLogitarCQRS(this IServiceCollection services)
  {
    return services
      .AddSingleton(serviceProvider => RetrySettings.Initialize(serviceProvider.GetRequiredService<IConfiguration>()))
      .AddTransient<ICommandBus, CommandBus>()
      .AddTransient<IQueryBus, QueryBus>();
  }
}
