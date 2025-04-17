using Zynapse.Backend.Api.DTO.Cart;
using Zynapse.Backend.Shared.Utils;

namespace Zynapse.Backend.Api.Services.Cart;

public interface ICartService
{
    Task<Result<CartOutputDto>> GetCartAsync(string userId);
    Task<Result<CartOutputDto>> AddItemToCartAsync(string userId, CartItemInputDto item);
    Task<Result<CartOutputDto>> UpdateItemQuantityAsync(string userId, int cartItemId, int quantity);
    Task<Result<CartOutputDto>> RemoveItemFromCartAsync(string userId, int cartItemId, int quantity = 1);
    Task<Result<CartOutputDto>> ClearCartAsync(string userId);
}