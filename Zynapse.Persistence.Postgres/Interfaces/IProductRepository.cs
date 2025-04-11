using Zynapse.Persistence.Postgres.Entities;
using Zynapse.Shared.Utils;

namespace Zynapse.Persistence.Postgres.Interfaces;

public interface IProductRepository
{
    Task<Result<ProductEntity>> CreateProductAsync(ProductEntity product);
    Task<Result<List<ProductEntity>>> GetProductsAsync();
    Task<Result<List<ProductEntity>>> GetProductsByCategoryAsync(string category);
    Task<Result<ProductEntity>> GetProductAsync(int id);
    Task<Result<ProductEntity>> GetRandomProductAsync();
    Task<Result<ProductEntity>> GetRandomProductByCategoryAsync(string category);
    Task<Result<List<string>>> GetCategoriesAsync();
    Task<Result<ProductEntity>> UpdateProductAsync(int id, ProductEntity product);
    Task<Result<ProductEntity>> DeleteProductAsync(int id);
}