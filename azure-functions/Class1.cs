using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Octokit;
using System.Net;
using System.Text.Json;
using System.Web;

namespace Prodigy.Functions;

/// <summary>
/// Azure Functions for GitHub integration and feature request management
/// </summary>
public class GitHubFunctions
{
    private readonly ILogger<GitHubFunctions> _logger;

    public GitHubFunctions(ILogger<GitHubFunctions> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a new GitHub issue for feature request
    /// </summary>
    [Function("CreateFeatureRequest")]
    public async Task<HttpResponseData> CreateFeatureRequest(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<FeatureRequestModel>(requestBody);

            if (request == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid request body");
                return badResponse;
            }

            // Get GitHub token from environment or headers
            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") 
                ?? req.Headers.GetValues("Authorization").FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(githubToken))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("GitHub token is required");
                return unauthorizedResponse;
            }

            // Initialize GitHub client
            var github = new GitHubClient(new ProductHeaderValue("Prodigy-App"))
            {
                Credentials = new Credentials(githubToken)
            };

            var repoOwner = Environment.GetEnvironmentVariable("GITHUB_REPO_OWNER") ?? "LukeDuffy98";
            var repoName = Environment.GetEnvironmentVariable("GITHUB_REPO_NAME") ?? "Prodigy";

            // Create the issue
            var newIssue = new NewIssue(request.Title)
            {
                Body = request.Description
            };

            // Add labels if provided
            if (request.Labels != null)
            {
                foreach (var label in request.Labels)
                {
                    newIssue.Labels.Add(label);
                }
            }

            // Add assignee if provided and valid (skip "copilot" as it's not a real GitHub user)
            if (!string.IsNullOrEmpty(request.AssignedTo) && 
                !string.Equals(request.AssignedTo, "copilot", StringComparison.OrdinalIgnoreCase))
            {
                newIssue.Assignees.Add(request.AssignedTo);
            }

            var createdIssue = await github.Issue.Create(repoOwner, repoName, newIssue);

            var response = req.CreateResponse(HttpStatusCode.Created);
            var result = new FeatureRequestResponseModel
            {
                IssueNumber = createdIssue.Number,
                IssueUrl = createdIssue.HtmlUrl,
                Title = createdIssue.Title,
                State = createdIssue.State.ToString().ToLowerInvariant(),
                AssignedTo = createdIssue.Assignee?.Login ?? "",
                Labels = createdIssue.Labels.Select(l => l.Name).ToArray(),
                CreatedAt = createdIssue.CreatedAt.DateTime
            };

            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(result));

            _logger.LogInformation("Created GitHub issue #{IssueNumber}: {Title}", createdIssue.Number, createdIssue.Title);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating GitHub feature request");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error creating feature request: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Retrieves a GitHub issue by number
    /// </summary>
    [Function("GetFeatureRequest")]
    public async Task<HttpResponseData> GetFeatureRequest(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "github/issue/{issueNumber}")] HttpRequestData req,
        int issueNumber)
    {
        try
        {
            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") 
                ?? req.Headers.GetValues("Authorization").FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(githubToken))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("GitHub token is required");
                return unauthorizedResponse;
            }

            var github = new GitHubClient(new ProductHeaderValue("Prodigy-App"))
            {
                Credentials = new Credentials(githubToken)
            };

            var repoOwner = Environment.GetEnvironmentVariable("GITHUB_REPO_OWNER") ?? "LukeDuffy98";
            var repoName = Environment.GetEnvironmentVariable("GITHUB_REPO_NAME") ?? "Prodigy";

            var issue = await github.Issue.Get(repoOwner, repoName, issueNumber);

            var response = req.CreateResponse(HttpStatusCode.OK);
            var result = new FeatureRequestResponseModel
            {
                IssueNumber = issue.Number,
                IssueUrl = issue.HtmlUrl,
                Title = issue.Title,
                State = issue.State.ToString().ToLowerInvariant(),
                AssignedTo = issue.Assignee?.Login ?? "",
                Labels = issue.Labels.Select(l => l.Name).ToArray(),
                CreatedAt = issue.CreatedAt.DateTime
            };

            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(result));

            _logger.LogInformation("Retrieved GitHub issue #{IssueNumber}", issueNumber);
            return response;
        }
        catch (NotFoundException)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Issue #{issueNumber} not found");
            return notFoundResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving GitHub issue #{IssueNumber}", issueNumber);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error retrieving issue: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Lists GitHub issues with optional filtering
    /// </summary>
    [Function("ListFeatureRequests")]
    public async Task<HttpResponseData> ListFeatureRequests(
        [HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
    {
        try
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query ?? "");
            var state = query["state"] ?? "open";
            var assignee = query["assignee"];
            var labels = query["labels"];

            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") 
                ?? req.Headers.GetValues("Authorization").FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(githubToken))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("GitHub token is required");
                return unauthorizedResponse;
            }

            var github = new GitHubClient(new ProductHeaderValue("Prodigy-App"))
            {
                Credentials = new Credentials(githubToken)
            };

            var repoOwner = Environment.GetEnvironmentVariable("GITHUB_REPO_OWNER") ?? "LukeDuffy98";
            var repoName = Environment.GetEnvironmentVariable("GITHUB_REPO_NAME") ?? "Prodigy";

            var issueRequest = new RepositoryIssueRequest
            {
                State = state.ToLowerInvariant() switch
                {
                    "closed" => ItemStateFilter.Closed,
                    "all" => ItemStateFilter.All,
                    _ => ItemStateFilter.Open
                }
            };

            if (!string.IsNullOrEmpty(assignee))
            {
                issueRequest.Assignee = assignee;
            }

            if (!string.IsNullOrEmpty(labels))
            {
                foreach (var label in labels.Split(','))
                {
                    issueRequest.Labels.Add(label.Trim());
                }
            }

            var issues = await github.Issue.GetAllForRepository(repoOwner, repoName, issueRequest);

            var response = req.CreateResponse(HttpStatusCode.OK);
            var results = issues.Select(issue => new FeatureRequestResponseModel
            {
                IssueNumber = issue.Number,
                IssueUrl = issue.HtmlUrl,
                Title = issue.Title,
                State = issue.State.ToString().ToLowerInvariant(),
                AssignedTo = issue.Assignee?.Login ?? "",
                Labels = issue.Labels.Select(l => l.Name).ToArray(),
                CreatedAt = issue.CreatedAt.DateTime
            }).ToArray();

            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(results));

            _logger.LogInformation("Retrieved {Count} GitHub issues", results.Length);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing GitHub issues");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error listing issues: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Updates a GitHub issue
    /// </summary>
    [Function("UpdateFeatureRequest")]
    public async Task<HttpResponseData> UpdateFeatureRequest(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "github/issue/{issueNumber}")] HttpRequestData req,
        int issueNumber)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<FeatureRequestModel>(requestBody);

            if (request == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid request body");
                return badResponse;
            }

            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") 
                ?? req.Headers.GetValues("Authorization").FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(githubToken))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("GitHub token is required");
                return unauthorizedResponse;
            }

            var github = new GitHubClient(new ProductHeaderValue("Prodigy-App"))
            {
                Credentials = new Credentials(githubToken)
            };

            var repoOwner = Environment.GetEnvironmentVariable("GITHUB_REPO_OWNER") ?? "LukeDuffy98";
            var repoName = Environment.GetEnvironmentVariable("GITHUB_REPO_NAME") ?? "Prodigy";

            var issueUpdate = new IssueUpdate
            {
                Title = request.Title,
                Body = request.Description
            };

            var updatedIssue = await github.Issue.Update(repoOwner, repoName, issueNumber, issueUpdate);

            var response = req.CreateResponse(HttpStatusCode.OK);
            var result = new FeatureRequestResponseModel
            {
                IssueNumber = updatedIssue.Number,
                IssueUrl = updatedIssue.HtmlUrl,
                Title = updatedIssue.Title,
                State = updatedIssue.State.ToString().ToLowerInvariant(),
                AssignedTo = updatedIssue.Assignee?.Login ?? "",
                Labels = updatedIssue.Labels.Select(l => l.Name).ToArray(),
                CreatedAt = updatedIssue.CreatedAt.DateTime
            };

            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(result));

            _logger.LogInformation("Updated GitHub issue #{IssueNumber}", issueNumber);
            return response;
        }
        catch (NotFoundException)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Issue #{issueNumber} not found");
            return notFoundResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating GitHub issue #{IssueNumber}", issueNumber);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error updating issue: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    /// Closes a GitHub issue
    /// </summary>
    [Function("CloseFeatureRequest")]
    public async Task<HttpResponseData> CloseFeatureRequest(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "github/issue/{issueNumber}/close")] HttpRequestData req,
        int issueNumber)
    {
        try
        {
            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") 
                ?? req.Headers.GetValues("Authorization").FirstOrDefault()?.Replace("Bearer ", "");

            if (string.IsNullOrEmpty(githubToken))
            {
                var unauthorizedResponse = req.CreateResponse(HttpStatusCode.Unauthorized);
                await unauthorizedResponse.WriteStringAsync("GitHub token is required");
                return unauthorizedResponse;
            }

            var github = new GitHubClient(new ProductHeaderValue("Prodigy-App"))
            {
                Credentials = new Credentials(githubToken)
            };

            var repoOwner = Environment.GetEnvironmentVariable("GITHUB_REPO_OWNER") ?? "LukeDuffy98";
            var repoName = Environment.GetEnvironmentVariable("GITHUB_REPO_NAME") ?? "Prodigy";

            var issueUpdate = new IssueUpdate
            {
                State = ItemState.Closed
            };

            var closedIssue = await github.Issue.Update(repoOwner, repoName, issueNumber, issueUpdate);

            var response = req.CreateResponse(HttpStatusCode.OK);
            var result = new
            {
                IssueNumber = closedIssue.Number,
                State = "closed",
                ClosedAt = DateTime.UtcNow,
                IssueUrl = closedIssue.HtmlUrl
            };

            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonSerializer.Serialize(result));

            _logger.LogInformation("Closed GitHub issue #{IssueNumber}", issueNumber);
            return response;
        }
        catch (NotFoundException)
        {
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Issue #{issueNumber} not found");
            return notFoundResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing GitHub issue #{IssueNumber}", issueNumber);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error closing issue: {ex.Message}");
            return errorResponse;
        }
    }
}

/// <summary>
/// Model for GitHub feature request input
/// </summary>
public class FeatureRequestModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[] Labels { get; set; } = Array.Empty<string>();
    public string AssignedTo { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Milestone { get; set; } = string.Empty;
}

/// <summary>
/// Model for GitHub feature request response
/// </summary>
public class FeatureRequestResponseModel
{
    public int IssueNumber { get; set; }
    public string IssueUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
    public string[] Labels { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
}
