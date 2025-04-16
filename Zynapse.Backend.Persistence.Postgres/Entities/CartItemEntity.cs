namespace Zynapse.Backend.Persistence.Postgres.Entities;

public class CartItemEntity
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public CartEntity Cart { get; set; } = null!;
    public int ProductId { get; set; }
    public ProductEntity Product { get; set; } = null!;
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}