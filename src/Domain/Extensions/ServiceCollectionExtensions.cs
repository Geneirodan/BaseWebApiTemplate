using Geneirodan.Generics.CrudService.Extensions;
using Geneirodan.Generics.Repository.Extensions;
using Infrastructure;
using Infrastructure.Entities;
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
        
        return services
            .AddRepositoriesFromAssemblyOf<ApplicationContext>()
            .AddServicesFromAssemblyOf<AssemblyReference>();
    }
}
