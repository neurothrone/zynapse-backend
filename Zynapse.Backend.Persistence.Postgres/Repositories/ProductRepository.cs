using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zynapse.Backend.Persistence.Postgres.Data;
using Zynapse.Backend.Persistence.Postgres.Entities;
using Zynapse.Backend.Persistence.Postgres.Interfaces;
using Zynapse.Backend.Persistence.Postgres.Utils;
using Zynapse.Backend.Shared.Utils;

namespace Zynapse.Backend.Persistence.Postgres.Repositories;

public class ProductRepository(
    ZynapseDbContext context,
    ILogger<ProductRepository> logger
) : IProductRepository
{
    public async Task<Result<ProductEntity>> CreateProductAsync(ProductEntity product)
    {
        try
        {
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();
            return Result<ProductEntity>.Success(product);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to create ProductEntity in {MethodName}. Exception: {ExceptionMessage}",
                nameof(GetProductsAsync), ex.Message);
            return Result<ProductEntity>.Failure(ProductMessages.DbUpdateFailed);
        }
    }

    public async Task<Result<List<ProductEntity>>> GetProductsAsync()
    {
        try
        {
            var products = await context.Products
                .AsNoTracking()
                .ToListAsync();
            return Result<List<ProductEntity>>.Success(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve products in {MethodName}. Exception: {ExceptionMessage}",
                nameof(GetProductsAsync), ex.Message);
            return Result<List<ProductEntity>>.Failure(ProductMessages.DbReadFailed);
        }
    }

    public async Task<Result<List<ProductEntity>>> GetProductsByCategoryAsync(string category)
    {
        try
        {
            var products = await context.Products
                .AsNoTracking()
                // .Where(e => e.Category == category)
                .ToListAsync();
            return Result<List<ProductEntity>>.Success(products);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to get Products by category in {MethodName}. Exception: {ExceptionMessage}",
                nameof(GetProductsAsync), ex.Message);
            return Result<List<ProductEntity>>.Failure(ProductMessages.DbReadFailed);
        }
    }

    public async Task<Result<ProductEntity>> GetProductAsync(int id)
    {
        try
        {
            var product = await context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);
            return product is not null
                ? Result<ProductEntity>.Success(product)
                : Result<ProductEntity>.Failure(ProductMessages.ProductNotFound);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get ProductEntity in {MethodName}. Exception: {ExceptionMessage}",
                nameof(GetProductsAsync), ex.Message);
            return Result<ProductEntity>.Failure(ProductMessages.DbReadFailed);
        }
    }

    public async Task<Result<ProductEntity>> GetRandomProductAsync()
    {
        try
        {
            var product = await context.Products
                .AsNoTracking()
                .OrderBy(_ => Guid.NewGuid())
                .Take(1)
                .FirstOrDefaultAsync();
            return product is not null
                ? Result<ProductEntity>.Success(product)
                : Result<ProductEntity>.Failure(ProductMessages.NoProducts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get random ProductEntity in {MethodName}. Exception: {ExceptionMessage}",
                nameof(GetProductsAsync), ex.Message);
            return Result<ProductEntity>.Failure(ProductMessages.DbReadFailed);
        }
    }

    public async Task<Result<ProductEntity>> GetRandomProductByCategoryAsync(string category)
    {
        try
        {
            var product = await context.Products
                .AsNoTracking()
                // .Where(e => e.Category == category)
                .OrderBy(_ => Guid.NewGuid())
                .Take(1)
                .FirstOrDefaultAsync();
            return product is not null
                ? Result<ProductEntity>.Success(product)
                : Result<ProductEntity>.Failure(ProductMessages.NoProductsForCategory(category));
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to get random ProductEntity by category in {MethodName}. Exception: {ExceptionMessage}",
                nameof(GetProductsAsync), ex.Message);
            return Result<ProductEntity>.Failure(ProductMessages.DbReadFailed);
        }
    }

    public async Task<Result<List<string>>> GetCategoriesAsync()
    {
        try
        {
            // var categories = await context.Products
            //     .AsNoTracking()
            //     .Select(e => e.Category)
            //     .Distinct()
            //     .ToListAsync();
            // return Result<List<string>>.Success(categories);
            return Result<List<string>>.Success([]);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get ProductEntity categories in {MethodName}. Exception: {ExceptionMessage}",
                nameof(GetProductsAsync), ex.Message);
            return Result<List<string>>.Failure(ProductMessages.DbReadFailed);
        }
    }

    public async Task<Result<ProductEntity>> UpdateProductAsync(int id, ProductEntity product)
    {
        try
        {
            var existingProductEntity = await context.Products.FindAsync(id);
            if (existingProductEntity is null)
                return Result<ProductEntity>.Failure(ProductMessages.ProductNotFound);

            existingProductEntity.Name = product.Name;
            existingProductEntity.Description = product.Description;
            existingProductEntity.Price = product.Price;
            existingProductEntity.Stock = product.Stock;
            await context.SaveChangesAsync();

            return Result<ProductEntity>.Success(existingProductEntity);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to update ProductEntity in {MethodName}. Exception: {ExceptionMessage}",
                nameof(GetProductsAsync), ex.Message);
            return Result<ProductEntity>.Failure(ProductMessages.DbUpdateFailed);
        }
    }

    public async Task<Result<ProductEntity>> DeleteProductAsync(int id)
    {
        try
        {
            var product = await context.Products.FindAsync(id);
            if (product is null)
                return Result<ProductEntity>.Failure(ProductMessages.ProductNotFound);

            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return Result<ProductEntity>.Success(product);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Failed to delete ProductEntity in {MethodName}. Exception: {ExceptionMessage}",
                nameof(GetProductsAsync), ex.Message);
            return Result<ProductEntity>.Failure(ProductMessages.DbUpdateFailed);
        }
    }
}