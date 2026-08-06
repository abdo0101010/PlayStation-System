using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace PlaystationSystem.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
       
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Session> Sessions { get; set; } = null!;
        public DbSet<SessionOrder> SessionOrders { get; set; } = null!;
        public DbSet<Shifts> Shifts { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
            modelBuilder.Entity<Device>()
                .Property(d => d.HourPriceMulti )
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
            
            // Configure relationships and constraints here if needed
        }
    }
} 
