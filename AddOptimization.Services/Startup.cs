using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AddOptimization.Services;

public static class Startup
{
    public static void RegisterDomainServices(this IServiceCollection services)
    {
        var contractNamespace = "AddOptimization.Contracts";
        var implementationNamespace = "AddOptimization.Services";

        var interfaceAssembly = Assembly.Load(contractNamespace);
        var implementationAssembly = Assembly.Load(implementationNamespace);

        var interfaceTypes = interfaceAssembly.GetTypes()
            .Where(type => type.IsInterface);

        foreach (var interfaceType in interfaceTypes)
        {
            var implementationType = implementationAssembly.GetTypes()
                .FirstOrDefault(interfaceType.IsAssignableFrom);
            if (implementationType != null)
            {
                services.AddScoped(interfaceType, implementationType);
            }
        }
    }
}