namespace Zynapse.Backend.Api.Models.Cart;

public class Cart
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}