using Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
// ReSharper disable UnusedMethodReturnValue.Global

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRepositories(this IServiceCollection services) =>
        services.Scan(i =>
            i.FromCallingAssembly()
                .AddClasses(c => c.AssignableTo<IRepositoryService>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );
}
