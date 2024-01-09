using DataAccess.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccess.Extensions;

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
