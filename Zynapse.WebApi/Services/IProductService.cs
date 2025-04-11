using Zynapse.Shared.Utils;
using Zynapse.WebApi.DTO;
using Zynapse.WebApi.Models;

namespace Zynapse.WebApi.Services;

public interface IProductService
{
    Task<Result<Product>> CreateProductAsync(ProductInputDto product);
    Task<Result<List<Product>>> GetProductsAsync();
    Task<Result<List<Product>>> GetProductsByCategoryAsync(string category);
    Task<Result<Product>> GetProductAsync(int id);
    Task<Result<Product>> GetRandomProductAsync();
    Task<Result<Product>> GetRandomProductByCategoryAsync(string category);
    Task<Result<List<string>>> GetCategoriesAsync();
    Task<Result<Product>> UpdateProductAsync(int id, ProductInputDto product);
    Task<Result<Product>> DeleteProductAsync(int id);
}