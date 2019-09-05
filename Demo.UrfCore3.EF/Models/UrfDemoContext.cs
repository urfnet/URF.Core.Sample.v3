using Microsoft.EntityFrameworkCore;

namespace Demo.UrfCore3.EF.Models
{
    public class UrfDemoContext : DbContext
    {
        public UrfDemoContext(DbContextOptions<UrfDemoContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, ProductName = "Chai", UnitPrice = 1 },
                new Product { Id = 2, ProductName = "Chang", UnitPrice = 2 },
                new Product { Id = 3, ProductName = "Cappuccino", UnitPrice = 3 }
            );
        }
    }
}
