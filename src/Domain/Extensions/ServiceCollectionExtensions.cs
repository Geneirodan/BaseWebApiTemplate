using Infrastructure;
using Infrastructure.Entities;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessLogicServices(this IServiceCollection services)
    {
        services
            .AddIdentity<User, Role>()
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders();
        
        services.AddRepositories();
        return services.Scan(i => i.FromCallingAssembly()
            .AddClasses(c => c.WithAttribute<TransientServiceAttribute>())
            .AsImplementedInterfaces()
            .WithTransientLifetime()
            .AddClasses(c => c.WithAttribute<ScopedServiceAttribute>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(c => c.WithAttribute<SingletonServiceAttribute>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime()
        );
    }
}
