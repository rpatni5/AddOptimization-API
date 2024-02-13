
using Microsoft.Extensions.Configuration;
using System;

namespace AddOptimization.Utilities.Extensions;

public static class ConfigurationExtension
{
    public static T ReadSection<T>(this IConfiguration configuration,string sectionName) where T : class
    {
        var instance = Activator.CreateInstance<T>();
        configuration.GetSection(sectionName).Bind(instance);
        return instance;
    }
}
