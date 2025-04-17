using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Zynapse.Backend.Persistence.Postgres.Data;
using Zynapse.Backend.Persistence.Postgres.Entities;

namespace Zynapse.Backend.Persistence.Postgres.Utils;

public class ProductSeedDto
{
    [JsonPropertyName("product")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("inventory")]
    public int? Stock { get; set; }

    [JsonPropertyName("steam-link")]
    public string? SteamLink { get; set; }
}

public static class ProductSeeder
{
    public static async Task SeedAsync(ZynapseDbContext context, string jsonPath)
    {
        if (await context.Products.AnyAsync())
            return;

        var json = await File.ReadAllTextAsync(jsonPath);
        var items = JsonSerializer.Deserialize<List<ProductSeedDto>>(json);

        if (items is null)
            return;

        var entities = items.Select(p => new ProductEntity
        {
            Name = p.Name,
            Description = p.Description ?? string.Empty,
            Price = p.Price,
            Stock = p.Stock ?? 0,
            SteamLink = p.SteamLink
        });

        await context.Products.AddRangeAsync(entities);
        await context.SaveChangesAsync();
    }
}