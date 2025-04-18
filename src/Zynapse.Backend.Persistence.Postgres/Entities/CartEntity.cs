namespace Zynapse.Backend.Persistence.Postgres.Entities;

public class CartEntity
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<CartItemEntity> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
} 