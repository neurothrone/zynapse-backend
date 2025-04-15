using Zynapse.Backend.Api.Models;

namespace Zynapse.Backend.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/v1/products");

        group.MapPost(string.Empty, ProductHandlers.CreateProductAsync)
            .WithSummary("Create a new product.")
            .WithDescription("Creates a new product.")
            .Produces<Product>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        group.MapGet("", ProductHandlers.GetProductsAsync)
            .WithSummary("Get all products, optionally filtered by category.")
            .Produces<List<Product>>();

        group.MapGet("/{id:int:min(0)}", ProductHandlers.GetProductAsync)
            .WithSummary("Get a product by its ID.")
            .Produces<Product>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("random", ProductHandlers.GetRandomProductAsync)
            .WithSummary("Get a random product, optionally filtered by category.")
            .Produces<Product>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("categories", ProductHandlers.GetCategoriesAsync)
            .WithSummary("Get all product categories.")
            .Produces<List<string>>();

        group.MapPut("/{id:int:min(0)}", ProductHandlers.UpdateProductAsync)
            .WithSummary("Update a product by its ID.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        group.MapDelete("/{id:int:min(0)}", ProductHandlers.DeleteProductAsync)
            .WithSummary("Delete a product by its ID.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization();
    }
}