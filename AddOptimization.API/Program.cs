using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AddOptimization.API.Common;
using AddOptimization.API.Extensions;
using AddOptimization.Services.Mappings;
using AddOptimization.Services;
using AddOptimization.Utilities;
using AddOptimization.Data;
using AddOptimization.API.HostedService.BackgroundServices;
using AddOptimization.Contracts.Services;
using AddOptimization.Services.NotificationHelpers;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

#region configure services
// Add services to the container.
IServiceCollection services = builder.Services;
services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new UtcToCstDateTimeConverter());
});
services.AddCors(builder,MyAllowSpecificOrigins);
services.AddAuth(builder);
services.AddEndpointsApiExplorer();
services.AddSwagger();
services.AddAuthorization();
services.AddHttpContextAccessor();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var mappingProfile = new MappingProfile();
var mappingConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(mappingProfile);
});
IMapper mapper = mappingConfig.CreateMapper();
services.AddSingleton(mapper);
services.AddMemoryCache();
services.RegisterUtilityServices();
services.RegisterDomainServices();
services.RegisterDataServices(connectionString);
services.AddHostedService<LicenseRenewalEmailBackgroundService>();
services.AddHostedService<FillTimesheetReminderEmailBackgroundService>();
services.AddHostedService<PendingTimesheetReminderToCustomerBackgroundService>();
services.AddHostedService<GenerateInvoiceBackgroundService>();
services.AddHostedService<UnpaidInvoiceReminderToCustomerBackgroundService>();
services.AddHostedService<OverdueNotificationBackgroundService>();
services.AddLoggingService();
services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(10);
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(60);
    options.HandshakeTimeout = TimeSpan.FromMinutes(2);
});
#endregion
#region configure
var app = builder.Build();
// Configure the HTTP request pipeline.
app.UseDeveloperExceptionPage();
if (app.Environment.IsDevelopment())
{
  
    app.UseCors(MyAllowSpecificOrigins);
}
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (!Path.HasExtension(context.Request.Path.Value))
    {
        context.Request.Path = "/index.html";
        await next();
    }
    else
    {
        await next();
    }
});
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub");
app.Use((ctx, next) => {
    ctx.Response.Headers.Add("Access-Control-Expose-Headers", "*");
    return next();
    });
app.Run();
#endregion


