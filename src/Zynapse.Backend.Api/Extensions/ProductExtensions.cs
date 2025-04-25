using Zynapse.Backend.Api.DTO;
using Zynapse.Backend.Api.DTO.Product;
using Zynapse.Backend.Api.Models;
using Zynapse.Backend.Persistence.Postgres.Entities;

namespace Zynapse.Backend.Api.Extensions;

public static class ProductExtensions
{
    public static Product ToModel(this ProductEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        Price = entity.Price,
        Stock = entity.Stock,
        SteamLink = entity.SteamLink
    };

    public static ProductEntity ToEntity(this Product model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description,
        Price = model.Price,
        Stock = model.Stock,
        SteamLink = model.SteamLink
    };

    public static ProductEntity ToEntity(this ProductInputDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        Stock = dto.Stock,
        SteamLink = dto.SteamUrl
    };
}