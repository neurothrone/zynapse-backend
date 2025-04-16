using Microsoft.EntityFrameworkCore;
using Zynapse.Backend.Persistence.Postgres.Data;
using Zynapse.Backend.Persistence.Postgres.Entities;
using Zynapse.Backend.Persistence.Postgres.Interfaces;
using Zynapse.Backend.Shared.Utils;

namespace Zynapse.Backend.Persistence.Postgres.Repositories;

public class CartRepository(ZynapseDbContext context) : ICartRepository
{
    public async Task<Result<CartEntity>> GetCartAsync(string userId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId);
            return Result<CartEntity>.Success(cart);
        }
        catch (Exception ex)
        {
            return Result<CartEntity>.Failure($"Failed to get cart: {ex.Message}");
        }
    }

    public async Task<Result<CartEntity>> AddItemToCartAsync(string userId, int productId, int quantity)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId);

            // Check if product exists
            var product = await context.Products.FindAsync(productId);
            if (product is null)
                return Result<CartEntity>.Failure("Product not found.");

            // Check if item already exists in cart
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem is not null)
            {
                // Update quantity
                existingItem.Quantity += quantity;
                existingItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Add new item
                var cartItem = new CartItemEntity
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                cart.Items.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // Reload cart with products
            return await GetCartAsync(userId);
        }
        catch (Exception ex)
        {
            return Result<CartEntity>.Failure($"Failed to add item to cart: {ex.Message}");
        }
    }

    public async Task<Result<CartEntity>> UpdateItemQuantityAsync(string userId, int cartItemId, int quantity)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId);

            // Find the item
            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (item == null)
                return Result<CartEntity>.Failure("Item not found in cart.");

            // Set the exact quantity specified
            item.Quantity = quantity;
            
            if (item.Quantity <= 0)
            {
                // Remove item if quantity zero or negative
                context.Entry(item).State = EntityState.Deleted;
            }

            item.UpdatedAt = DateTime.UtcNow;
            cart.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // Reload cart
            return await GetCartAsync(userId);
        }
        catch (Exception ex)
        {
            return Result<CartEntity>.Failure($"Failed to update item quantity: {ex.Message}");
        }
    }

    public async Task<Result<CartEntity>> RemoveItemFromCartAsync(string userId, int cartItemId, int quantity = 1)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId);

            // Find the item
            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);
            if (item == null)
                return Result<CartEntity>.Failure("Item not found in cart.");

            // Decrement quantity
            item.Quantity -= quantity;
            
            // Remove item if quantity is zero or less
            if (item.Quantity <= 0)
            {
                context.Entry(item).State = EntityState.Deleted;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // Reload cart
            return await GetCartAsync(userId);
        }
        catch (Exception ex)
        {
            return Result<CartEntity>.Failure($"Failed to remove item from cart: {ex.Message}");
        }
    }

    public async Task<Result<CartEntity>> ClearCartAsync(string userId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId);

            // Remove all items
            foreach (var item in cart.Items.ToList())
            {
                context.Entry(item).State = EntityState.Deleted;
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();

            // Reload cart
            return await GetCartAsync(userId);
        }
        catch (Exception ex)
        {
            return Result<CartEntity>.Failure($"Failed to clear cart: {ex.Message}");
        }
    }

    private async Task<CartEntity> GetOrCreateCartAsync(string userId)
    {
        // Get cart with items and products
        var cart = await context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is not null)
            return cart;

        // Create new cart
        cart = new CartEntity
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Carts.Add(cart);
        await context.SaveChangesAsync();

        return cart;
    }
}