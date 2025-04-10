using Microsoft.EntityFrameworkCore;
using Zynapse.Persistence.Postgres.Entities;

namespace Zynapse.Persistence.Postgres.Data;

public class ZynapseDbContext(DbContextOptions<ZynapseDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            // Explicitly set NUMERIC(10, 2) for Price
            entity.Property(e => e.Price)
                .HasColumnType("NUMERIC(10, 2)");

            // Ensure Description uses TEXT
            entity.Property(e => e.Description)
                .HasColumnType("TEXT");
        });
    }
}