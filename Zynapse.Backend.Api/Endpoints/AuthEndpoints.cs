namespace Zynapse.Backend.Api.Endpoints;

/// <summary>
/// Endpoint mapping for authentication-related API routes
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/v1/auth");

        // Authentication endpoints
        group.MapGet("validate", AuthHandlers.ValidateTokenAsync)
            .WithSummary("Validate the authentication token.")
            .RequireAuthorization();

        // Note: Token generation is handled by Supabase Auth directly.
        // The frontend should authenticate with Supabase, and this API 
        // only validates the tokens issued by Supabase.
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
            UserId = userId
        });
    }
}