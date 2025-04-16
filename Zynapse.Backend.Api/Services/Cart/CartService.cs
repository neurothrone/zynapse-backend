using Zynapse.Backend.Api.DTO.Cart;
using Zynapse.Backend.Api.DTO.Product;
using Zynapse.Backend.Persistence.Postgres.Interfaces;
using Zynapse.Backend.Shared.Extensions;
using Zynapse.Backend.Shared.Utils;

namespace Zynapse.Backend.Api.Services.Cart;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductService _productService;

    public CartService(
        ICartRepository cartRepository,
        IProductService productService)
    {
        _cartRepository = cartRepository;
        _productService = productService;
    }

    public async Task<Result<CartOutputDto>> GetCartAsync(string userId)
    {
        var result = await _cartRepository.GetCartAsync(userId);
        return result.Match<Result<CartOutputDto>>(
            onSuccess: cart => Result<CartOutputDto>.Success(MapCartToDto(cart)),
            onFailure: Result<CartOutputDto>.Failure
        );
    }

    public async Task<Result<CartOutputDto>> AddItemToCartAsync(string userId, CartItemInputDto item)
    {
        // Validate product exists and has enough stock
        var productResult = await _productService.GetProductAsync(item.ProductId);
        if (productResult.IsFailure())
            return Result<CartOutputDto>.Failure("Product not found.");

        var product = productResult.Value();
        if (product.Stock < item.Quantity)
            return Result<CartOutputDto>.Failure($"Insufficient stock. Only {product.Stock} available.");

        var result = await _cartRepository.AddItemToCartAsync(userId, item.ProductId, item.Quantity);
        return result.Match<Result<CartOutputDto>>(
            onSuccess: cart => Result<CartOutputDto>.Success(MapCartToDto(cart)),
            onFailure: Result<CartOutputDto>.Failure
        );
    }

    public async Task<Result<CartOutputDto>> RemoveItemFromCartAsync(string userId, int cartItemId)
    {
        var result = await _cartRepository.RemoveItemFromCartAsync(userId, cartItemId);
        return result.Match<Result<CartOutputDto>>(
            onSuccess: cart => Result<CartOutputDto>.Success(MapCartToDto(cart)),
            onFailure: Result<CartOutputDto>.Failure
        );
    }

    public async Task<Result<CartOutputDto>> ClearCartAsync(string userId)
    {
        var result = await _cartRepository.ClearCartAsync(userId);
        return result.Match<Result<CartOutputDto>>(
            onSuccess: cart => Result<CartOutputDto>.Success(MapCartToDto(cart)),
            onFailure: Result<CartOutputDto>.Failure
        );
    }

    private CartOutputDto MapCartToDto(Persistence.Postgres.Entities.CartEntity cart)
    {
        var dto = new CartOutputDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = [],
            TotalPrice = 0,
            TotalItems = 0
        };

        foreach (var item in cart.Items)
        {
            var productDto = new ProductOutputDto
            {
                Id = item.Product.Id,
                Name = item.Product.Name,
                Description = item.Product.Description,
                Price = item.Product.Price,
                Stock = item.Product.Stock,
                SteamLink = item.Product.SteamLink
            };

            var itemDto = new CartItemOutputDto
            {
                Id = item.Id,
                Product = productDto,
                Quantity = item.Quantity,
                Subtotal = item.Quantity * item.Product.Price
            };

            dto.Items.Add(itemDto);
            dto.TotalPrice += itemDto.Subtotal;
            dto.TotalItems += item.Quantity;
        }

        return dto;
    }
}