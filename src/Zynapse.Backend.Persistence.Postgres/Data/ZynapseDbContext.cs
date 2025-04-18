using Microsoft.EntityFrameworkCore;
using Zynapse.Backend.Persistence.Postgres.Entities;

namespace Zynapse.Backend.Persistence.Postgres.Data;

public class ZynapseDbContext(DbContextOptions<ZynapseDbContext> options) : DbContext(options)
{
    public DbSet<ProductEntity> Products => Set<ProductEntity>();
    public DbSet<CartEntity> Carts => Set<CartEntity>();
    public DbSet<CartItemEntity> CartItems => Set<CartItemEntity>();

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

        modelBuilder.Entity<CartEntity>(entity =>
        {
            entity.ToTable("carts");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.UserId)
                .IsRequired();
                
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            // Add index on UserId for faster lookups
            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("IX_carts_UserId");
        });

        modelBuilder.Entity<CartItemEntity>(entity =>
        {
            entity.ToTable("cart_items");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Quantity)
                .IsRequired();
                
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
                
            // Relationships
            entity.HasOne(e => e.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(e => e.CartId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}