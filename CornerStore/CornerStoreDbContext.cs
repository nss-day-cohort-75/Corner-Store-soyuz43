using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
using System;

namespace CornerStore
{
    public class CornerStoreDbContext : DbContext
    {
        public DbSet<Cashier> Cashiers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }

        public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, CategoryName = "Snacks" },
                new Category { Id = 2, CategoryName = "Drinks" }
            );

            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, ProductName = "Chips", Price = 1.99m, Brand = "Lays", CategoryId = 1 },
                new Product { Id = 2, ProductName = "Cola", Price = 2.49m, Brand = "Coca-Cola", CategoryId = 2 }
            );

            // Seed Cashiers
            modelBuilder.Entity<Cashier>().HasData(
                new Cashier { Id = 1, FirstName = "Ernie", LastName = "Fairchild" }
            );

            // Seed Orders
            modelBuilder.Entity<Order>().HasData(
                new Order { Id = 1, CashierId = 1, PaidOnDate = DateTime.UtcNow }
            );

            // Seed OrderProducts
            modelBuilder.Entity<OrderProduct>().HasData(
                new OrderProduct { OrderId = 1, ProductId = 1, Quantity = 2 },
                new OrderProduct { OrderId = 1, ProductId = 2, Quantity = 1 }
            );
        }
    }
}
