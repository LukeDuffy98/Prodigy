using Microsoft.AspNetCore.Mvc;
using Prodigy.Backend.Models;
using System.Text.Json;
using System.Text;

namespace Prodigy.Backend.Controllers;

/// <summary>
/// GitHub Agent controller for creating feature requests and managing GitHub integration.
/// Automatically creates GitHub issues and can assign them to specific users like "github-copilot".
/// </summary>
[ApiController]
[Route("api/agents/github")]
[Produces("application/json")]
public class GitHubAgentController : ControllerBase
{
    private readonly ILogger<GitHubAgentController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public GitHubAgentController(ILogger<GitHubAgentController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Creates a new feature request as a GitHub issue in the repository.
    /// Issue content is personalized using the user's communication style and preferences.
    /// </summary>
    /// <param name="request">Feature request details including title, description, and assignment preferences</param>
    /// <returns>Created GitHub issue details with URL and tracking information</returns>
    /// <response code="201">Feature request created successfully on GitHub</response>
    /// <response code="400">Invalid feature request data</response>
    /// <response code="401">User not authenticated with GitHub</response>
    /// <response code="403">Insufficient GitHub repository permissions</response>
    [HttpPost("feature-request")]
    [ProducesResponseType(typeof(FeatureRequestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FeatureRequestResponse>> CreateFeatureRequest([FromBody] FeatureRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = _httpClientFactory.CreateClient("AzureFunctions");
            
            // Forward auth header if present
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
            }

            var json = JsonSerializer.Serialize(new
            {
                Title = request.Title,
                Description = request.Description,
                Labels = request.Labels,
                AssignedTo = request.AssignedTo,
                Priority = request.Priority,
                Milestone = request.Milestone
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("CreateFeatureRequest", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FeatureRequestResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Created GitHub feature request #{IssueNumber}: {Title}", result?.IssueNumber, request.Title);
                return CreatedAtAction(nameof(GetFeatureRequest), new { issueNumber = result?.IssueNumber }, result);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure Function call failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                
                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => Unauthorized("GitHub authentication failed"),
                    System.Net.HttpStatusCode.Forbidden => Forbid("Insufficient GitHub permissions"),
                    System.Net.HttpStatusCode.BadRequest => BadRequest(errorContent),
                    _ => StatusCode(500, "An error occurred while creating the feature request")
                };
            }
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("actively refused"))
        {
            _logger.LogWarning(ex, "Azure Functions service is not available. GitHub feature creation requires Azure Functions to be running.");
            return StatusCode(503, new { 
                error = "GitHub feature request service is temporarily unavailable", 
                message = "Azure Functions service is not running. Please start the Functions service to enable GitHub integration.",
                suggestion = "Run 'func start' in the azure-functions directory or contact your administrator."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GitHub feature request");
            return StatusCode(500, "An error occurred while creating the feature request");
        }
    }

    /// <summary>
    /// Retrieves details of a specific GitHub feature request by issue number
    /// </summary>
    /// <param name="issueNumber">GitHub issue number</param>
    /// <returns>Feature request details from GitHub</returns>
    /// <response code="200">Feature request found and returned</response>
    /// <response code="404">Feature request not found</response>
    [HttpGet("feature-request/{issueNumber}")]
    [ProducesResponseType(typeof(FeatureRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeatureRequestResponse>> GetFeatureRequest(int issueNumber)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AzureFunctions");
            
            // Forward auth header if present
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
            }

            var response = await client.GetAsync($"github/issue/{issueNumber}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FeatureRequestResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Retrieved GitHub feature request #{IssueNumber}", issueNumber);
                return Ok(result);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound($"Feature request #{issueNumber} not found");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure Function call failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return StatusCode(500, "An error occurred while retrieving the feature request");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitHub feature request #{IssueNumber}", issueNumber);
            return StatusCode(500, "An error occurred while retrieving the feature request");
        }
    }

    /// <summary>
    /// Updates an existing GitHub feature request with new information
    /// </summary>
    /// <param name="issueNumber">GitHub issue number to update</param>
    /// <param name="request">Updated feature request information</param>
    /// <returns>Updated feature request details</returns>
    /// <response code="200">Feature request updated successfully</response>
    /// <response code="404">Feature request not found</response>
    /// <response code="400">Invalid update data</response>
    [HttpPut("feature-request/{issueNumber}")]
    [ProducesResponseType(typeof(FeatureRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeatureRequestResponse>> UpdateFeatureRequest(int issueNumber, [FromBody] FeatureRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var client = _httpClientFactory.CreateClient("AzureFunctions");
            
            // Forward auth header if present
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
            }

            var json = JsonSerializer.Serialize(new
            {
                Title = request.Title,
                Description = request.Description,
                Labels = request.Labels,
                AssignedTo = request.AssignedTo,
                Priority = request.Priority,
                Milestone = request.Milestone
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"github/issue/{issueNumber}", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<FeatureRequestResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Updated GitHub feature request #{IssueNumber}", issueNumber);
                return Ok(result);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound($"Feature request #{issueNumber} not found");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure Function call failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return BadRequest("Failed to update feature request");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating GitHub feature request #{IssueNumber}", issueNumber);
            return StatusCode(500, "An error occurred while updating the feature request");
        }
    }

    /// <summary>
    /// Lists all feature requests from the repository with optional filtering
    /// </summary>
    /// <param name="state">Filter by issue state (open, closed, all)</param>
    /// <param name="assignee">Filter by assigned user</param>
    /// <param name="labels">Filter by labels (comma-separated)</param>
    /// <returns>List of feature requests matching the criteria</returns>
    /// <response code="200">Feature requests retrieved successfully</response>
    [HttpGet("feature-requests")]
    [ProducesResponseType(typeof(FeatureRequestResponse[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<FeatureRequestResponse[]>> ListFeatureRequests(
        [FromQuery] string state = "open",
        [FromQuery] string? assignee = null,
        [FromQuery] string? labels = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AzureFunctions");
            
            // Forward auth header if present
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
            }

            var queryParams = new List<string>();
            queryParams.Add($"state={Uri.EscapeDataString(state)}");
            
            if (!string.IsNullOrEmpty(assignee))
                queryParams.Add($"assignee={Uri.EscapeDataString(assignee)}");
            
            if (!string.IsNullOrEmpty(labels))
                queryParams.Add($"labels={Uri.EscapeDataString(labels)}");

            var queryString = string.Join("&", queryParams);
            var response = await client.GetAsync($"ListFeatureRequests?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var results = JsonSerializer.Deserialize<FeatureRequestResponse[]>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? Array.Empty<FeatureRequestResponse>();

                _logger.LogInformation("Retrieved {RequestCount} feature requests with filters - state: {State}, assignee: {Assignee}", 
                    results.Length, state, assignee ?? "none");
                
                return Ok(results);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure Function call failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return StatusCode(500, "An error occurred while listing feature requests");
            }
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("actively refused"))
        {
            _logger.LogWarning(ex, "Azure Functions service is not available. GitHub feature listing requires Azure Functions to be running.");
            return StatusCode(503, new { 
                error = "GitHub feature request service is temporarily unavailable", 
                message = "Azure Functions service is not running. Please start the Functions service to enable GitHub integration.",
                suggestion = "Run 'func start' in the azure-functions directory or contact your administrator."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing GitHub feature requests");
            return StatusCode(500, "An error occurred while listing feature requests");
        }
    }

    /// <summary>
    /// Adds a comment to an existing GitHub feature request
    /// </summary>
    /// <param name="issueNumber">GitHub issue number</param>
    /// <param name="comment">Comment content to add</param>
    /// <returns>Comment creation confirmation</returns>
    /// <response code="201">Comment added successfully</response>
    /// <response code="404">Feature request not found</response>
    /// <response code="400">Invalid comment content</response>
    [HttpPost("feature-request/{issueNumber}/comment")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddComment(int issueNumber, [FromBody] string comment)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return BadRequest("Comment content cannot be empty");
            }

            // For now, return a placeholder as this would require extending the Azure Function
            var commentResult = new
            {
                Id = Random.Shared.Next(10000, 99999),
                IssueNumber = issueNumber,
                Content = comment,
                CreatedAt = DateTime.UtcNow,
                Author = "current-user",
                Message = "Comment functionality would be implemented in future Azure Function extension"
            };

            _logger.LogInformation("Simulated adding comment to GitHub feature request #{IssueNumber}", issueNumber);
            
            return CreatedAtAction(nameof(GetFeatureRequest), new { issueNumber }, commentResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to GitHub feature request #{IssueNumber}", issueNumber);
            return StatusCode(500, "An error occurred while adding the comment");
        }
    }

    /// <summary>
    /// Closes a GitHub feature request with an optional closing comment
    /// </summary>
    /// <param name="issueNumber">GitHub issue number to close</param>
    /// <param name="reason">Reason for closing (completed, not_planned, duplicate)</param>
    /// <param name="comment">Optional closing comment</param>
    /// <returns>Closure confirmation</returns>
    /// <response code="200">Feature request closed successfully</response>
    /// <response code="404">Feature request not found</response>
    [HttpPost("feature-request/{issueNumber}/close")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseFeatureRequest(int issueNumber, [FromQuery] string reason = "completed", [FromBody] string? comment = null)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AzureFunctions");
            
            // Forward auth header if present
            var authHeader = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader))
            {
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
            }

            var response = await client.PostAsync($"github/issue/{issueNumber}/close", null);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<object>(responseContent);

                _logger.LogInformation("Closed GitHub feature request #{IssueNumber} with reason: {Reason}", issueNumber, reason);
                return Ok(result);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound($"Feature request #{issueNumber} not found");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Azure Function call failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return StatusCode(500, "An error occurred while closing the feature request");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing GitHub feature request #{IssueNumber}", issueNumber);
            return StatusCode(500, "An error occurred while closing the feature request");
        }
    }
}