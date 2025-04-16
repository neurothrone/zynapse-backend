using Zynapse.Backend.Api.DTO.Cart;

namespace Zynapse.Backend.Api.Endpoints.Cart;

/// <summary>
/// Endpoint mapping for cart-related API routes
/// </summary>
public static class CartEndpoints
{
    public static void MapCartEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/v1/cart").RequireAuthorization();

        // GET cart
        group.MapGet(string.Empty, CartHandlers.GetCartAsync)
            .WithSummary("Get the user's cart")
            .Produces<CartOutputDto>()
            .Produces(StatusCodes.Status401Unauthorized);

        // ADD item to cart
        group.MapPost("items", CartHandlers.AddItemToCartAsync)
            .WithSummary("Add an item to the cart")
            .Produces<CartOutputDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // UPDATE item quantity
        group.MapPatch("items/{id:int:min(0)}", CartHandlers.UpdateItemQuantityAsync)
            .WithSummary("Update the quantity of an item in the cart")
            .Produces<CartOutputDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        // REMOVE item from cart
        group.MapDelete("items/{id:int:min(0)}", CartHandlers.RemoveItemFromCartAsync)
            .WithSummary("Remove an item from the cart. By default, decrements quantity by 1. If quantity reaches 0, removes the item entirely.")
            .WithDescription("Specify a 'quantity' query parameter to decrement by more than 1.")
            .Produces<CartOutputDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        // CLEAR cart
        group.MapDelete(string.Empty, CartHandlers.ClearCartAsync)
            .WithSummary("Clear the cart")
            .Produces<CartOutputDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}