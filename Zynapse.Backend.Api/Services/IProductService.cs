using Zynapse.Backend.Api.DTO.Product;
using Zynapse.Backend.Api.Models;
using Zynapse.Backend.Shared.Utils;

namespace Zynapse.Backend.Api.Services;

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