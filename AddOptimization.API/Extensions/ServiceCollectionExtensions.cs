namespace AddOptimization.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCors(this IServiceCollection services, WebApplicationBuilder builder,string allowOrigin)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(allowOrigin, policy =>
            {
                policy.WithOrigins(builder.Configuration["Cors:AllowOrigins"]).AllowAnyHeader().AllowAnyMethod();
            });
        });
    }
}
