using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Zynapse.Backend.Api.Services;

public interface IJwtValidationService
{
    bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal);
    string? ExtractUserIdFromToken(string token);
    (bool isValid, string? message) AnalyzeToken(string token);
}

public class JwtValidationService : IJwtValidationService
{
    private readonly IConfiguration _configuration;
    private readonly TokenValidationParameters _tokenValidationParameters;
    private readonly ILogger<JwtValidationService> _logger;

    public JwtValidationService(IConfiguration configuration, ILogger<JwtValidationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var jwtSettings = configuration.GetSection("JWT");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
        
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,  // Relaxed validation
            ValidateAudience = false, // Relaxed validation
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            // Make more tolerant of clock skew
            ClockSkew = TimeSpan.FromMinutes(5)
        };
    }

    public bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal)
    {
        claimsPrincipal = null;
        
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Token validation failed: Empty token");
            return false;
        }
            
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            claimsPrincipal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out SecurityToken validatedToken);
            _logger.LogInformation("Token validation succeeded");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation failed with exception");
            return false;
        }
    }

    public string? ExtractUserIdFromToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("Cannot extract user ID: Token is empty");
            return null;
        }
            
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // For Supabase, check multiple possible ID claim types
            var userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value;
            if (userId == null)
            {
                userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "user_id")?.Value;
            }
            if (userId == null)
            {
                userId = jwtToken.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            }
            
            if (userId == null)
            {
                _logger.LogWarning("No user ID found in token claims");
            }
            else
            {
                _logger.LogInformation("Successfully extracted user ID: {UserId}", userId);
            }
            
            return userId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract user ID from token");
            return null;
        }
    }
    
    public (bool isValid, string? message) AnalyzeToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return (false, "Token is empty");
        }
            
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            
            // Check if token has expired
            var now = DateTime.UtcNow;
            if (jwtToken.ValidTo < now)
            {
                return (false, $"Token expired on {jwtToken.ValidTo}");
            }
            
            // Attempt manual signature validation
            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out _);
                return (true, "Token signature is valid");
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                return (false, "Token signature key not found. Check if the secret matches the one used by Supabase");
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return (false, "Token signature is invalid. The token may have been tampered with or the secret is incorrect");
            }
            catch (Exception validationEx)
            {
                return (false, $"Token validation error: {validationEx.Message}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"Token parsing error: {ex.Message}");
        }
    }
} 