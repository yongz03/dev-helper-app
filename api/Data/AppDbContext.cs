using DevHelper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevHelper.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>()
            .HasIndex(customer => customer.Email)
            .IsUnique();
    }
}
