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
        public DbSet<Admin> Admins { get; set; }
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
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Status)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StatusID);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Store)
                .WithMany(s => s.Orders)
                .HasForeignKey(o => o.StoreID);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Florist)
                .WithMany(f => f.Orders)
                .HasForeignKey(o => o.FloristID);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Deliverer)
                .WithMany(d => d.Orders)
                .HasForeignKey(o => o.DelivererID);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Store)
                .WithMany(s => s.Products)
                .HasForeignKey(p => p.StoreID);

            modelBuilder.Entity<Florist>()
                .HasOne(f => f.Store)
                .WithMany(s => s.Florists)
                .HasForeignKey(f => f.StoreID);
        }
    }
}
