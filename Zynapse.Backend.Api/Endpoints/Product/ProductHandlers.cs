using System.ComponentModel.DataAnnotations;
using Zynapse.Backend.Api.DTO.Product;
using Zynapse.Backend.Api.Services;

namespace Zynapse.Backend.Api.Endpoints.Product;

// !: TypedResults API
// Source: https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-9.0&tabs=visual-studio#use-the-typedresults-api

public static class ProductHandlers
{
    public static async Task<IResult> CreateProductAsync(ProductInputDto product, IProductService service)
    {
        if (!Validator.TryValidateObject(product, new ValidationContext(product), null, true))
            return TypedResults.BadRequest("Invalid product data.");

        var result = await service.CreateProductAsync(product);
        return result.Match<IResult>(
            onSuccess: createdProduct => TypedResults.Created($"api/products/{createdProduct.Id}", createdProduct),
            onFailure: TypedResults.BadRequest
        );
    }

    public static async Task<IResult> GetProductsAsync(string? category, IProductService service)
    {
        var result = !string.IsNullOrEmpty(category)
            ? await service.GetProductsByCategoryAsync(category)
            : await service.GetProductsAsync();
        return result.Match<IResult>(
            onSuccess: TypedResults.Ok,
            onFailure: error => TypedResults.Problem(error, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    public static async Task<IResult> GetProductAsync(int id, IProductService service)
    {
        var result = await service.GetProductAsync(id);
        return result.Match<IResult>(
            onSuccess: TypedResults.Ok,
            onFailure: TypedResults.NotFound
        );
    }

    public static async Task<IResult> UpdateProductAsync(int id, ProductInputDto product, IProductService service)
    {
        if (!Validator.TryValidateObject(product, new ValidationContext(product), null, true))
            return TypedResults.BadRequest("Invalid product data.");

        var result = await service.UpdateProductAsync(id, product);
        return result.Match<IResult>(
            onSuccess: updatedProduct => TypedResults.Ok(new
            {
                message = "The product has been successfully updated.",
                product = updatedProduct
            }),
            onFailure: TypedResults.NotFound
        );
    }

    public static async Task<IResult> DeleteProductAsync(int id, IProductService service)
    {
        var result = await service.DeleteProductAsync(id);
        return result.Match<IResult>(
            onSuccess: deletedProduct => TypedResults.Ok(new
            {
                message = "The product has been successfully deleted.",
                product = deletedProduct
            }),
            onFailure: TypedResults.NotFound
        );
    }

    public static async Task<IResult> GetRandomProductAsync(string? category, IProductService service)
    {
        var result = !string.IsNullOrEmpty(category)
            ? await service.GetRandomProductByCategoryAsync(category)
            : await service.GetRandomProductAsync();
        return result.Match<IResult>(
            onSuccess: TypedResults.Ok,
            onFailure: TypedResults.NotFound
        );
    }

    public static async Task<IResult> GetCategoriesAsync(IProductService service)
    {
        var result = await service.GetCategoriesAsync();
        return result.Match<IResult>(
            onSuccess: TypedResults.Ok,
            onFailure: error => TypedResults.Problem(error, statusCode: StatusCodes.Status500InternalServerError)
        );
    }
}