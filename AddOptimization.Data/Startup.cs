using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Data.Repositories;

namespace AddOptimization.Data
{
    public static class Startup
    {
        public static void RegisterDataServices(this IServiceCollection services,string connectionString)
        {
            services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork,UnitOfWork>();
            services.AddDbContext<AddOptimizationContext>(x => x.UseSqlServer(connectionString));
        }
    }
}
