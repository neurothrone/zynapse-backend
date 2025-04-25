using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Zynapse.Backend.Api.DTO.Product;

public record ProductInputDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    [DefaultValue("World of Warcraft")]
    public required string Name { get; set; }

    public string Description { get; set; } = string.Empty;

    [Range(0, (double)decimal.MaxValue)]
    [DefaultValue(9.99)]
    public decimal Price { get; set; }

    [Range(0, 99)]
    [DefaultValue(50)]
    public int Stock { get; set; }

    public string? ImageUrl { get; set; }
    public string? SteamUrl { get; set; }
}