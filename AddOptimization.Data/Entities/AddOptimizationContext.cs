using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AddOptimization.Data.Common;
using AddOptimization.Data.Extensions;

namespace AddOptimization.Data.Entities;

public partial class AddOptimizationContext : DbContext
{
    public AddOptimizationContext(IHttpContextAccessor httpContextAccessor)
    {
    }

    public AddOptimizationContext(DbContextOptions<AddOptimizationContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
    }

    public virtual DbSet<ApplicationUser> ApplicationUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AS");
        modelBuilder.ApplyBaseEntityConfiguration();
        
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
    }
}
