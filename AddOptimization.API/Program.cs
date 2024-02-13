using AutoMapper;
using Microsoft.EntityFrameworkCore;
using AddOptimization.API.Common;
using AddOptimization.API.Extensions;
using AddOptimization.Services.Mappings;
using AddOptimization.Services;
using AddOptimization.Utilities;
using AddOptimization.Data;

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
services.AddLogging(builder =>
{
    builder.AddFile($"logs/log_for.txt",LogLevel.Error);
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
app.Use((ctx, next) => {
    ctx.Response.Headers.Add("Access-Control-Expose-Headers", "*");
    return next();
    });
app.Run();
#endregion


