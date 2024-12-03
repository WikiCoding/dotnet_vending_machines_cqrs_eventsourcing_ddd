using Microsoft.EntityFrameworkCore;
using vendingmachines.queries.entities;

namespace vendingmachines.queries.repository;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Machine> Machines { get; set; }
    public DbSet<Product> Projects { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().HasOne<Machine>().WithMany(m => m.Products).HasForeignKey(p => p.MachineId);
    }
}
