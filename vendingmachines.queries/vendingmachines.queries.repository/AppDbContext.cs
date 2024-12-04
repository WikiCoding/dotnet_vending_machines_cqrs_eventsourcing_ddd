using Microsoft.EntityFrameworkCore;
using vendingmachines.queries.entities;

namespace vendingmachines.queries.repository;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Machine> Machines { get; set; }
    public DbSet<Product> Products { get; set; }

    // nice idea but messages are not necessarily stored in order, and it's possible to try to create a product with a
    // machineId which is still in transaction state coming causing FK violation.
    // Stays here as example for other projects
    // protected override void OnModelCreating(ModelBuilder modelBuilder)
    // {
    //     base.OnModelCreating(modelBuilder);
    //
    //     modelBuilder.Entity<Product>().HasOne<Machine>().WithMany(m => m.Products).HasForeignKey(p => p.MachineId);
    // }
}
