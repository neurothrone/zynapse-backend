namespace Zynapse.Backend.Api.Endpoints;

/// <summary>
/// Endpoint mapping for authentication-related API routes
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/v1/auth");

        group.MapGet("validate", AuthHandlers.ValidateTokenAsync)
            .WithSummary("Validate the authentication token.")
            .WithDescription("Returns the user's authentication status and ID if authenticated.")
            .Produces<object>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        // Public info endpoint
        group.MapGet("info", AuthHandlers.GetAuthInfoAsync)
            .WithSummary("Get authentication information.")
            .WithDescription("Returns information about which endpoints require authentication.")
            .Produces<object>(StatusCodes.Status200OK);
    }
}

/// <summary>
/// Handlers for authentication-related endpoints
/// </summary>
public static class AuthHandlers
{
    /// <summary>
    /// Validates the user's authentication token
    /// </summary>
    public static IResult ValidateTokenAsync(HttpContext httpContext)
    {
        // This endpoint is protected, so if we get here, the user is authenticated
        var userId = httpContext.User.FindFirst("sub")?.Value;

        return TypedResults.Ok(new
        {
            IsAuthenticated = true,
            UserId = userId,
            Message = "Token is valid"
        });
    }

    /// <summary>
    /// Returns information about authentication requirements
    /// </summary>
    public static IResult GetAuthInfoAsync()
    {
        return TypedResults.Ok(new
        {
            Message = "Authentication is required for modifying resources",
            PublicEndpoints = new[]
            {
                "GET /api/v1/products",
                "GET /api/v1/products/{id}",
                "GET /api/v1/products/random",
                "GET /api/v1/products/categories"
            },
            ProtectedEndpoints = new[]
            {
                "POST /api/v1/products",
                "PUT /api/v1/products/{id}",
                "DELETE /api/v1/products/{id}"
            }
        });
    }
}