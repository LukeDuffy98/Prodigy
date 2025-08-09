using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;
using Prodigy.Backend.Models;

namespace Prodigy.Backend.Controllers;

/// <summary>
/// Authentication controller for Microsoft Graph sign-in
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public AuthController(ILogger<AuthController> logger, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    /// <summary>
    /// Login with username and password using Resource Owner Password Credentials flow
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Access token and user information</returns>
    /// <response code="200">Login successful</response>
    /// <response code="400">Invalid credentials</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Email and password are required" });
            }

            var tenantId = _configuration["AZURE_TENANT_ID"] ?? Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var clientId = _configuration["AZURE_CLIENT_ID"] ?? Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = _configuration["AZURE_CLIENT_SECRET"] ?? Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                _logger.LogError("Azure AD configuration is missing. Ensure AZURE_TENANT_ID, AZURE_CLIENT_ID, and AZURE_CLIENT_SECRET are configured.");
                return StatusCode(500, new { error = "Authentication service is not properly configured." });
            }

            var tokenUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var formParams = new List<KeyValuePair<string, string>>
            {
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("scope", "https://graph.microsoft.com/User.Read https://graph.microsoft.com/Mail.ReadWrite https://graph.microsoft.com/Mail.Send"),
                new("username", request.Email),
                new("password", request.Password),
                new("grant_type", "password")
            };

            var formContent = new FormUrlEncodedContent(formParams);

            var response = await _httpClient.PostAsync(tokenUrl, formContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Authentication failed for user {Email}: {Response}", request.Email, responseContent);
                
                try
                {
                    var errorObj = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    var errorDescription = errorObj.TryGetProperty("error_description", out var desc) 
                        ? desc.GetString() 
                        : "Authentication failed";
                    
                    return BadRequest(new { error = errorDescription });
                }
                catch
                {
                    return BadRequest(new { error = "Authentication failed. Please check your credentials." });
                }
            }

            var tokenResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);
            var accessToken = tokenResponse.GetProperty("access_token").GetString();

            // Get user info from Microsoft Graph
            var userHttpClient = new HttpClient();
            userHttpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var userResponse = await userHttpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            var userContent = await userResponse.Content.ReadAsStringAsync();

            if (!userResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve user information for {Email}", request.Email);
                return BadRequest(new { error = "Failed to retrieve user information" });
            }

            var user = JsonSerializer.Deserialize<JsonElement>(userContent);

            _logger.LogInformation("User {Email} successfully authenticated", request.Email);

            return Ok(new
            {
                accessToken = accessToken,
                user = new
                {
                    email = user.TryGetProperty("mail", out var mail) ? mail.GetString() : 
                           user.TryGetProperty("userPrincipalName", out var upn) ? upn.GetString() : null,
                    name = user.TryGetProperty("displayName", out var name) ? name.GetString() : null,
                    id = user.TryGetProperty("id", out var id) ? id.GetString() : null
                }
            });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error during authentication for {Email}", request.Email);
            return StatusCode(500, new { error = "Network error during authentication. Please try again." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authentication error for {Email}", request.Email);
            return StatusCode(500, new { error = "An error occurred during authentication. Please try again." });
        }
    }

    /// <summary>
    /// Gets the current user's authentication status and information
    /// </summary>
    /// <returns>User authentication details</returns>
    /// <response code="200">User is authenticated</response>
    /// <response code="401">User is not authenticated</response>
    [HttpGet("me")]
    [Authorize(Policy = "RequireAzureAD")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? 
                          User.FindFirst("preferred_username")?.Value ?? 
                          User.FindFirst(ClaimTypes.Name)?.Value;
            
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? 
                          User.FindFirst("name")?.Value ?? 
                          userEmail;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                        User.FindFirst("sub")?.Value ?? 
                        User.FindFirst("oid")?.Value;

            if (string.IsNullOrEmpty(userEmail))
            {
                return Unauthorized(new { error = "Unable to retrieve user information from token" });
            }

            var result = new
            {
                IsAuthenticated = true,
                UserId = userId,
                UserEmail = userEmail,
                UserName = userName,
                AuthenticationScheme = "AzureAD",
                Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
            };

            _logger.LogInformation("User authenticated: {UserEmail}", userEmail);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user information");
            return StatusCode(500, new { error = "An error occurred while retrieving user information" });
        }
    }

    /// <summary>
    /// Validates the provided access token and returns user information
    /// </summary>
    /// <param name="accessToken">Microsoft Graph access token</param>
    /// <returns>Token validation result</returns>
    /// <response code="200">Token is valid</response>
    /// <response code="400">Invalid token</response>
    [HttpPost("validate-token")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidateToken([FromBody] TokenValidationRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.AccessToken))
            {
                return BadRequest(new { error = "Access token is required" });
            }

            // TODO: Implement actual token validation with Microsoft Graph
            // For now, we'll assume the token is valid if it's provided
            // In a real implementation, you would call Microsoft Graph to validate the token
            
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", request.AccessToken);

            try
            {
                var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
                
                if (response.IsSuccessStatusCode)
                {
                    var userInfo = await response.Content.ReadAsStringAsync();
                    var userJson = System.Text.Json.JsonSerializer.Deserialize<Microsoft.Graph.Models.User>(userInfo);
                    
                    return Ok(new
                    {
                        IsValid = true,
                        UserEmail = userJson?.Mail ?? userJson?.UserPrincipalName,
                        UserName = userJson?.DisplayName,
                        UserId = userJson?.Id
                    });
                }
                else
                {
                    return BadRequest(new { error = "Invalid access token", details = response.ReasonPhrase });
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error validating token with Microsoft Graph");
                return BadRequest(new { error = "Token validation failed", details = ex.Message });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating access token");
            return StatusCode(500, new { error = "An error occurred while validating the token" });
        }
    }

    /// <summary>
    /// Gets Microsoft Graph authentication configuration for the frontend
    /// </summary>
    /// <returns>Authentication configuration</returns>
    /// <response code="200">Configuration retrieved successfully</response>
    [HttpGet("config")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetAuthConfig()
    {
        try
        {
            var config = new
            {
                ClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"),
                TenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID"),
                Authority = $"https://login.microsoftonline.com/{Environment.GetEnvironmentVariable("AZURE_TENANT_ID")}",
                RedirectUri = $"{Request.Scheme}://{Request.Host}/auth/callback",
                Scopes = new[]
                {
                    "https://graph.microsoft.com/Mail.ReadWrite",
                    "https://graph.microsoft.com/Mail.Send",
                    "https://graph.microsoft.com/User.Read"
                }
            };

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auth configuration");
            return StatusCode(500, new { error = "An error occurred while retrieving configuration" });
        }
    }

    /// <summary>
    /// Logout endpoint (mainly for cleanup)
    /// </summary>
    /// <returns>Logout confirmation</returns>
    /// <response code="200">Logout successful</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        try
        {
            // In a real implementation, you might want to:
            // - Invalidate the token (if using a token store)
            // - Clear any server-side session data
            // - Log the logout event
            
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
            _logger.LogInformation("User logged out: {UserEmail}", userEmail);
            
            return Ok(new
            {
                Success = true,
                Message = "Logout successful",
                LoggedOutAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "An error occurred during logout" });
        }
    }
}

/// <summary>
/// Request model for token validation
/// </summary>
public class TokenValidationRequest
{
    /// <summary>
    /// Microsoft Graph access token to validate
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
}
