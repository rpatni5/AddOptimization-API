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
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }
    public virtual DbSet<Role> Roles { get; set; }
    public virtual DbSet<UserRole> UserRoles { get; set; }
    public virtual DbSet<Screen> Screens { get; set; }
    public virtual DbSet<Field> Fields { get; set; }
    public virtual DbSet<RolePermission> RolePermissions { get; set; }
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public virtual DbSet<Customer> Customers { get; set; }
    public virtual DbSet<CustomerStatus> CustomerStatuses { get; set; }
    public virtual DbSet<Address> Addresses { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AS");
        modelBuilder.ApplyBaseEntityConfiguration();
        modelBuilder.Entity<ApplicationUser>(entity =>
        { 
            entity.HasMany(e => e.UserRoles).WithOne(c => c.User);
        });
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasOne(e => e.Customer).WithMany(c => c.Addresses);
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
    }
}
