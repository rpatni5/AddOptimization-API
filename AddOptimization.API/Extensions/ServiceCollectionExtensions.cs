using AddOptimization.Contracts.Constants;
using AddOptimization.Contracts.Services;

namespace AddOptimization.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCors(this IServiceCollection services, WebApplicationBuilder builder, string allowOrigin)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(allowOrigin, policy =>
            {
                policy.WithOrigins(builder.Configuration["Cors:AllowOrigins"]).AllowAnyHeader().AllowAnyMethod();
            });
        });
    }

    public static void AddLoggingService(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {

            var a = builder.Services.BuildServiceProvider();
            using (var scope = a.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var logLevelService = services.GetRequiredService<ISettingService>();
                    var logLevelSetting = logLevelService.GetSettingByCode(SettingCodes.LOG_LEVEL).Result;
                    builder.AddFile($"logs/log_for.txt", logLevelSetting.Result!=null && logLevelSetting.Result.IsEnabled ? LogLevel.Information : LogLevel.Error);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while setting the log level.");
                }
            }
        });
    }
}
