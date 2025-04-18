using Zynapse.Backend.Persistence.Postgres.Entities;
using Zynapse.Backend.Shared.Utils;

namespace Zynapse.Backend.Persistence.Postgres.Interfaces;

public interface ICartRepository
{
    Task<Result<CartEntity>> GetCartAsync(string userId);
    Task<Result<CartEntity>> AddItemToCartAsync(string userId, int productId, int quantity);
    Task<Result<CartEntity>> UpdateItemQuantityAsync(string userId, int cartItemId, int quantity);
    Task<Result<CartEntity>> RemoveItemFromCartAsync(string userId, int cartItemId, int quantity = 1);
    Task<Result<CartEntity>> ClearCartAsync(string userId);
} 