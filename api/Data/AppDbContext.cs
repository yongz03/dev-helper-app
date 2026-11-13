using DevHelper.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DevHelper.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
}
