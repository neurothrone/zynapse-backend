using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Zynapse.Backend.Api.DTO.Cart;
using Zynapse.Backend.Api.Services.Cart;

namespace Zynapse.Backend.Api.Endpoints.Cart;

/// <summary>
/// Handlers for cart-related endpoints
/// </summary>
public static class CartHandlers
{
    public static async Task<IResult> GetCartAsync(ClaimsPrincipal user, ICartService service)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.Unauthorized();

        var result = await service.GetCartAsync(userId);
        return result.Match<IResult>(
            onSuccess: TypedResults.Ok,
            onFailure: error => TypedResults.Problem(error, statusCode: StatusCodes.Status500InternalServerError)
        );
    }

    public static async Task<IResult> AddItemToCartAsync(ClaimsPrincipal user, CartItemInputDto item, ICartService service)
    {
        if (!Validator.TryValidateObject(item, new ValidationContext(item), null, true))
            return TypedResults.BadRequest("Invalid item data.");

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.Unauthorized();

        var result = await service.AddItemToCartAsync(userId, item);
        return result.Match<IResult>(
            onSuccess: TypedResults.Ok,
            onFailure: TypedResults.BadRequest
        );
    }

    public static async Task<IResult> RemoveItemFromCartAsync(int id, ClaimsPrincipal user, ICartService service)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.Unauthorized();

        var result = await service.RemoveItemFromCartAsync(userId, id);
        return result.Match<IResult>(
            onSuccess: TypedResults.Ok,
            onFailure: TypedResults.NotFound
        );
    }

    public static async Task<IResult> ClearCartAsync(ClaimsPrincipal user, ICartService service)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return TypedResults.Unauthorized();

        var result = await service.ClearCartAsync(userId);
        return result.Match<IResult>(
            onSuccess: TypedResults.Ok,
            onFailure: error => TypedResults.Problem(error, statusCode: StatusCodes.Status500InternalServerError)
        );
    }
} 