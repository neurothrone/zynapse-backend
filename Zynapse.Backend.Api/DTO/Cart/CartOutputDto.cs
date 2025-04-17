namespace Zynapse.Backend.Api.DTO.Cart;

public class CartOutputDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public List<CartItemOutputDto> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
    public int TotalItems { get; set; }
}