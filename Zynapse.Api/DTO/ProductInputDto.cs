using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Zynapse.Api.DTO;

public record ProductInputDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    [DefaultValue("World of Warcraft")]
    public required string Name { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}