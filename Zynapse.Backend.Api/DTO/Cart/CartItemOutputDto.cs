using Zynapse.Backend.Api.DTO.Product;

namespace Zynapse.Backend.Api.DTO.Cart;

public class CartItemOutputDto
{
    public int Id { get; set; }
    public ProductOutputDto Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
} 