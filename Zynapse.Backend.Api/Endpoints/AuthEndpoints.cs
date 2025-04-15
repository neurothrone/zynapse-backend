using System.IdentityModel.Tokens.Jwt;
using Zynapse.Backend.Api.Services;

namespace Zynapse.Backend.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/v1/auth");

        // Test endpoint to verify if a user is authenticated
        group.MapGet("validate", AuthHandlers.ValidateTokenAsync)
            .WithSummary("Validate the authentication token.")
            .RequireAuthorization();
        
        // Public info endpoint
        group.MapGet("info", AuthHandlers.GetAuthInfoAsync)
            .WithSummary("Get authentication information.");
            
        // Token test endpoint (public for testing)
        group.MapGet("test-token", AuthHandlers.TestTokenAsync)
            .WithSummary("Test a JWT token without authentication requirements.");
            
        // Direct debug endpoint - will always work without auth
        group.MapGet("debug-direct", AuthHandlers.DebugDirectAsync)
            .WithSummary("Direct debug endpoint that always works.");
    }
}

public static class AuthHandlers
{
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
    
    public static IResult GetAuthInfoAsync()
    {
        return TypedResults.Ok(new 
        { 
            Message = "Authentication is required for modifying resources",
            PublicEndpoints = new[] { "GET /api/v1/products", "GET /api/v1/products/{id}", "GET /api/v1/products/random", "GET /api/v1/products/categories" },
            ProtectedEndpoints = new[] { "POST /api/v1/products", "PUT /api/v1/products/{id}", "DELETE /api/v1/products/{id}" }
        });
    }
    
    public static IResult TestTokenAsync(HttpContext httpContext, IConfiguration configuration)
    {
        string? authHeader = httpContext.Request.Headers.Authorization;
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            return TypedResults.BadRequest(new { Error = "Authorization header missing or not in Bearer format" });
        }
        
        string token = authHeader.Substring("Bearer ".Length).Trim();
        
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            
            if (jsonToken == null)
            {
                return TypedResults.BadRequest(new { Error = "Invalid token format" });
            }
            
            // Get JWT settings
            var jwtSettings = configuration.GetSection("JWT");
            var secret = jwtSettings["Secret"] ?? string.Empty;
            var maskedSecret = string.IsNullOrEmpty(secret) ? "Not configured" : 
                (secret.Length > 4 ? $"{secret.Substring(0, 2)}...{secret.Substring(secret.Length - 2)}" : "***");
            var issuer = jwtSettings["Issuer"] ?? "Not configured";
            var audience = jwtSettings["Audience"] ?? "Not configured";
            
            var claims = jsonToken.Claims.Select(c => new { c.Type, c.Value }).ToList();
            
            return TypedResults.Ok(new
            {
                TokenInfo = new {
                    Issuer = jsonToken.Issuer,
                    Audience = jsonToken.Audiences.FirstOrDefault(),
                    ValidFrom = jsonToken.ValidFrom,
                    ValidTo = jsonToken.ValidTo,
                    HasExpired = DateTime.UtcNow > jsonToken.ValidTo
                },
                Claims = claims,
                ConfigInfo = new {
                    ConfiguredIssuer = issuer,
                    ConfiguredAudience = audience,
                    SecretConfigured = !string.IsNullOrEmpty(secret),
                    MaskedSecret = maskedSecret,
                    DebugMode = configuration.GetValue<bool>("DebugAuth", false)
                }
            });
        }
        catch (Exception ex)
        {
            return TypedResults.BadRequest(new { Error = $"Failed to parse token: {ex.Message}" });
        }
    }
    
    public static IResult DebugDirectAsync(HttpContext httpContext, IConfiguration configuration)
    {
        // Direct debug info - always accessible
        bool isAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
        bool isDebugMode = configuration.GetValue<bool>("DebugAuth", false);
        
        var claims = httpContext.User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        
        return TypedResults.Ok(new
        {
            Message = "This is a direct debug endpoint that should always work",
            IsAuthenticated = isAuthenticated,
            HasAuthorizationHeader = !string.IsNullOrEmpty(httpContext.Request.Headers.Authorization),
            AuthorizationHeader = httpContext.Request.Headers.Authorization.ToString(),
            DebugModeEnabled = isDebugMode,
            AuthPipelineInfo = new
            {
                AuthScheme = httpContext.User.Identity?.AuthenticationType,
                Claims = claims,
                RequestPath = httpContext.Request.Path.ToString(),
                Method = httpContext.Request.Method
            },
            ServerTime = DateTime.UtcNow
        });
    }
} 