using System.ComponentModel.DataAnnotations;

namespace Zynapse.Persistence.Postgres.Entities;

public class ProductEntity
{
    public int Id { get; set; }

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Description { get; set; } = string.Empty;

    [Range(0, (double)decimal.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, 99)]
    public int Stock { get; set; }
}