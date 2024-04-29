using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AddOptimization.Data.Common;
using AddOptimization.Data.Extensions;
using AddOptimization.Utilities.Extensions;
using NPOI.SS.UserModel;

namespace AddOptimization.Data.Entities;

public partial class AddOptimizationContext : DbContext
{
    public string CurrentUserEmail { get; set; }
    public List<string> CurrentUserRoles { get; set; }

    public AddOptimizationContext(IHttpContextAccessor httpContextAccessor)
    {
        CurrentUserEmail = httpContextAccessor.HttpContext.GetCurrentUserEmail();
        CurrentUserRoles = httpContextAccessor.HttpContext.GetCurrentUserRoles();

    }

    public AddOptimizationContext(DbContextOptions<AddOptimizationContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        CurrentUserEmail = httpContextAccessor.HttpContext.GetCurrentUserEmail();
        CurrentUserRoles = httpContextAccessor.HttpContext.GetCurrentUserRoles();
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
    public virtual DbSet<License> Licenses { get; set; }
    public virtual DbSet<LicenseDevice> LicenseDevices { get; set; }
    public virtual DbSet<GuiVersion> GuiVersions { get; set; }

    public virtual DbSet<PublicHoliday> PublicHolidays { get; set; }
    public virtual DbSet<Country> Country { get; set; }
    public virtual DbSet<Schedulers> SchedulerEvents { get; set; }
    public virtual DbSet<SchedulerStatus> SchedulerStatus { get; set; }
    public virtual DbSet<SchedulerEventType> schedulereventtypes { get; set; }



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
        modelBuilder.Entity<Customer>().Property(b => b.CustomerStatusId)
    .IsRequired()
    .HasDefaultValue(new Guid("17756728-9DE6-409F-9D23-B8B5BA253F0E"));
        //modelBuilder.Entity<License>(entity =>
        //{
        //    entity.HasOne(e => e.Customer).WithMany(c => c.Licenses);
        //});

        modelBuilder.Entity<Customer>(entity =>
              {
                  entity.HasQueryFilter(e => e.Email == CurrentUserEmail);
              });

        modelBuilder.Entity<License>(entity =>
        {
            entity.HasQueryFilter(e => e.Customer != null ? e.Customer.Email == CurrentUserEmail : true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Conventions.Add(_ => new BlankTriggerAddingConvention());
    }
}
