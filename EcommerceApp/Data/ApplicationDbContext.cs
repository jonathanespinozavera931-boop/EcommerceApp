using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcommerceApp.Models;

namespace EcommerceApp.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Precisión para precios (evita errores con PostgreSQL)
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.Subtotal).HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.Envio).HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.Descuento).HasPrecision(18, 2);
            modelBuilder.Entity<Order>()
                .Property(o => o.Total).HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetail>()
                .Property(d => d.Price).HasPrecision(18, 2);
            modelBuilder.Entity<OrderDetail>()
                .Property(d => d.Subtotal).HasPrecision(18, 2);
        }
    }
}