using Zynapse.Persistence.Postgres.Interfaces;
using Zynapse.Shared.Utils;
using Zynapse.WebApi.DTO;
using Zynapse.WebApi.Extensions;
using Zynapse.WebApi.Models;

namespace Zynapse.WebApi.Services;

public class ProductService(IProductRepository repository) : IProductService
{
    public async Task<Result<Product>> CreateProductAsync(ProductInputDto product)
    {
        var result = await repository.CreateProductAsync(product.ToEntity());
        return result.Match<Result<Product>>(
            onSuccess: createdProduct => Result<Product>.Success(createdProduct.ToModel()),
            onFailure: Result<Product>.Failure
        );
    }

    public async Task<Result<List<Product>>> GetProductsAsync()
    {
        var result = await repository.GetProductsAsync();
        return result.Match<Result<List<Product>>>(
            onSuccess: products => Result<List<Product>>.Success(products
                .Select(p => p.ToModel())
                .ToList()),
            onFailure: Result<List<Product>>.Failure
        );
    }

    public async Task<Result<List<Product>>> GetProductsByCategoryAsync(string category)
    {
        var result = await repository.GetProductsByCategoryAsync(category);
        return result.Match<Result<List<Product>>>(
            onSuccess: products => Result<List<Product>>.Success(products
                .Select(p => p.ToModel())
                .ToList()),
            onFailure: Result<List<Product>>.Failure
        );
    }

    public async Task<Result<Product>> GetProductAsync(int id)
    {
        var result = await repository.GetProductAsync(id);
        return result.Match<Result<Product>>(
            onSuccess: product => Result<Product>.Success(product.ToModel()),
            onFailure: Result<Product>.Failure
        );
    }

    public async Task<Result<Product>> GetRandomProductAsync()
    {
        var result = await repository.GetRandomProductAsync();
        return result.Match<Result<Product>>(
            onSuccess: product => Result<Product>.Success(product.ToModel()),
            onFailure: Result<Product>.Failure
        );
    }

    public async Task<Result<Product>> GetRandomProductByCategoryAsync(string category)
    {
        var result = await repository.GetRandomProductByCategoryAsync(category);
        return result.Match<Result<Product>>(
            onSuccess: product => Result<Product>.Success(product.ToModel()),
            onFailure: Result<Product>.Failure
        );
    }

    public Task<Result<List<string>>> GetCategoriesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Result<Product>> UpdateProductAsync(int id, ProductInputDto product)
    {
        var result = await repository.UpdateProductAsync(id, product.ToEntity());
        return result.Match<Result<Product>>(
            onSuccess: updatedProduct => Result<Product>.Success(updatedProduct.ToModel()),
            onFailure: Result<Product>.Failure
        );
    }

    public async Task<Result<Product>> DeleteProductAsync(int id)
    {
        var result = await repository.DeleteProductAsync(id);
        return result.Match<Result<Product>>(
            onSuccess: deletedProduct => Result<Product>.Success(deletedProduct.ToModel()),
            onFailure: Result<Product>.Failure
        );
    }
}