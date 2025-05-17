using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace STOREFLOWER.Models
{
    public class StoreFlowerContext : DbContext
    {
        public DbSet<Admin> Admins { get; set; } // Добавь это, если его нет
        public DbSet<OrderStatus> OrderStatuses { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Florist> Florists { get; set; }
        public DbSet<Deliverer> Deliverers { get; set; }

        public StoreFlowerContext() : base()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=ANASTASIA;Database=StoreFlower;Trusted_Connection=True;TrustServerCertificate=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Связь Orders и OrderStatuses
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Status)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StatusID);

            // Связь Orders и Stores
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Store)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StoreID);

            // Связь Orders и Florists
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Florist)
                .WithMany(f => f.Orders)
                .HasForeignKey(o => o.FloristID);

            // Связь Orders и Deliverers
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Deliverer)
                .WithMany(d => d.Orders)
                .HasForeignKey(o => o.DelivererID);

            // Связь Products и Stores
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Store)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.StoreID);

            // Связь Florists и Stores
            modelBuilder.Entity<Florist>()
                .HasOne(f => f.Store)
                .WithMany(s => s.Florists)
                .HasForeignKey(f => f.StoreID);
        }
    }
}
