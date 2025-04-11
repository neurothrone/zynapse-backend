using Zynapse.Api.DTO;
using Zynapse.Api.Models;
using Zynapse.Persistence.Postgres.Entities;

namespace Zynapse.Api.Extensions;

public static class ProductExtensions
{
    public static Product ToModel(this ProductEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        Price = entity.Price,
        Stock = entity.Stock
    };

    public static ProductEntity ToEntity(this Product model) => new()
    {
        Id = model.Id,
        Name = model.Name,
        Description = model.Description,
        Price = model.Price,
        Stock = model.Stock
    };

    public static ProductEntity ToEntity(this ProductInputDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        Price = dto.Price,
        Stock = dto.Stock
    };
}