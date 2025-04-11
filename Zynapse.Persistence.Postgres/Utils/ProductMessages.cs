namespace Zynapse.Persistence.Postgres.Utils;

public static class ProductMessages
{
    public const string DbReadFailed = "Database read failed.";
    public const string DbUpdateFailed = "Database update failed.";
    public const string ProductNotFound = "Product not found.";
    public const string NoProducts = "No products available.";

    public static string NoProductsForCategory(string category) =>
        $"No products found for category: {category}.";
}