using Zynapse.Backend.Persistence.Postgres.Entities;
using Zynapse.Backend.Shared.Utils;

namespace Zynapse.Backend.Persistence.Postgres.Interfaces;

public interface ICartRepository
{
    Task<Result<CartEntity>> GetCartAsync(string userId);
    Task<Result<CartEntity>> AddItemToCartAsync(string userId, int productId, int quantity);
    Task<Result<CartEntity>> RemoveItemFromCartAsync(string userId, int cartItemId);
    Task<Result<CartEntity>> ClearCartAsync(string userId);
} 