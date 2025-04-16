using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Zynapse.Backend.Api.DTO.Cart;

public class UpdateCartItemQuantityDto
{
    [Required]
    [Range(1, 99, ErrorMessage = "Quantity must be between 1 and 99")]
    [DefaultValue(1)]
    public int Quantity { get; set; } = 1;
}