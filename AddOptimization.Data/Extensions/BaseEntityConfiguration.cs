using Microsoft.EntityFrameworkCore;
using AddOptimization.Data.Common;
using System.Reflection;

namespace AddOptimization.Data.Extensions;

public static class BaseEntityConfiguration
{
    static void Configure<TEntity,TId>(ModelBuilder modelBuilder)
   where TEntity : BaseEntityNew<TId>
    {
        modelBuilder.Entity<TEntity>(builder =>
        {
            builder.HasOne(e => e.UpdatedByUser).WithMany().HasForeignKey(e => e.UpdatedByUserId).HasPrincipalKey(e => e.Id);
        });
    }
    public static ModelBuilder ApplyBaseEntityConfiguration(this ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyBaseEntityConfiguration<int>();
        modelBuilder.ApplyBaseEntityConfiguration<Guid>();
        return modelBuilder;
    }

        public static ModelBuilder ApplyBaseEntityConfiguration<TId>(this ModelBuilder modelBuilder)
    {
        var method = typeof(BaseEntityConfiguration).GetTypeInfo().DeclaredMethods
          .Single(m => m.Name == nameof(Configure));
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsBaseEntity())
                method.MakeGenericMethod(entityType.ClrType,typeof(TId)).Invoke(null, new[] { modelBuilder });
        }
        return modelBuilder;
    }

    static bool IsBaseEntity(this Type type)
    {
        return  type.BaseType == typeof(BaseEntityNew<>);
    }
   
}

