using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Services;

namespace PlaystationSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly ICurrentTenantService _currentTenantService;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ICurrentTenantService currentTenantService) : base(options)
        {
            _currentTenantService = currentTenantService;
        }

        // جداول النظام
        public DbSet<Tenant> Tenants { get; set; } = null!;
        public DbSet<Device> Devices { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<SessionOrder> SessionOrders { get; set; } = null!;
        public DbSet<Shifts> Shifts { get; set; } = null!;
        public DbSet<Expense> Expenses { get; set; } = null!;
        public DbSet<DebtPayment> DebtPayments { get; set; } = null!;

        // خصائص مساعدة للفلترة
        public string? CurrentTenantId => _currentTenantService.TenantId;
        public bool IsSuperAdmin => _currentTenantService.IsSuperAdmin;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. منع الحذف التتابعي (Restrict Cascade Delete)
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // 2. إعدادات دقة الأرقام العشرية (Decimals Precision)
            modelBuilder.Entity<Device>()
                .Property(d => d.HourPriceMulti)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Device>()
                .Property(d => d.HourPriceSingle)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.SellingPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.PurchasePrice)
                .HasPrecision(18, 2);

            // 3. تطبيق الفلتر فقط على الجداول التي ترث ITenantEntity مع استثناء كلاسات الـ Identity
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType) &&
                    !typeof(IdentityUser).IsAssignableFrom(entityType.ClrType) &&
                    !typeof(IdentityRole).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var tenantIdProperty = Expression.Property(parameter, nameof(ITenantEntity.TenantId));

                    var currentTenantExpr = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
                    var isSuperAdminExpr = Expression.Property(Expression.Constant(this), nameof(IsSuperAdmin));

                    // الشرط: IsSuperAdmin == true OR TenantId == CurrentTenantId
                    var equalsExpr = Expression.Equal(tenantIdProperty, currentTenantExpr);
                    var combinedExpr = Expression.OrElse(isSuperAdminExpr, equalsExpr);

                    var lambda = Expression.Lambda(combinedExpr, parameter);
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        // 4. الحقن التلقائي للـ TenantId عند إضافة أي سجل جديد
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTenantId();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            ApplyTenantId();
            return base.SaveChanges();
        }

        private void ApplyTenantId()
        {
            if (!IsSuperAdmin && !string.IsNullOrEmpty(CurrentTenantId))
            {
                foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
                {
                    if (entry.State == EntityState.Added)
                    {
                        entry.Entity.TenantId = CurrentTenantId;
                    }
                }
            }
        }
    }
}