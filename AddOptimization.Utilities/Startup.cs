using Microsoft.Extensions.DependencyInjection;
using AddOptimization.Utilities.Interface;
using AddOptimization.Utilities.Services;

namespace AddOptimization.Utilities;

public static class Startup
{
    public static void RegisterUtilityServices(this IServiceCollection services)
    {
        services.AddSingleton<CustomDataProtectionService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<ITemplateService, TemplateService>();
    }
}
