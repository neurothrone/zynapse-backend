using Microsoft.EntityFrameworkCore;
using Zynapse.Backend.Persistence.Postgres.Entities;

namespace Zynapse.Backend.Persistence.Postgres.Data;

public class ZynapseDbContext(DbContextOptions<ZynapseDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<ProductEntity>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);

            // Explicitly set NUMERIC(10, 2) for Price
            entity.Property(e => e.Price)
                .HasColumnType("NUMERIC(10, 2)");

            // Ensure Description uses TEXT
            entity.Property(e => e.Description)
                .HasColumnType("TEXT");
        });
    }
}