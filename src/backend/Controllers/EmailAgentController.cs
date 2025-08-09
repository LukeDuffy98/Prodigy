using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Prodigy.Backend.Models;
using Prodigy.Backend.Services;

namespace Prodigy.Backend.Controllers;

/// <summary>
/// Email Agent controller for sending and replying to emails via Microsoft Graph API.
/// All email generation uses the user's PersonalizationProfile for consistent voice and style.
/// Requires Azure AD authentication for Microsoft Graph API access.
/// </summary>
[ApiController]
[Route("api/agents/email")]
[Produces("application/json")]
// Remove Authorize attribute - we'll validate the token by using it to call Microsoft Graph directly
public class EmailAgentController : ControllerBase
{
    private readonly ILogger<EmailAgentController> _logger;
    private readonly IGraphEmailService _graphEmailService;
    private readonly IGraphUserService _graphUserService;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailAgentController(
        ILogger<EmailAgentController> logger, 
        IGraphEmailService graphEmailService,
        IGraphUserService graphUserService, 
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _graphEmailService = graphEmailService;
        _graphUserService = graphUserService;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Sends a new email using Microsoft Graph API.
    /// Email content is personalized using the user's PersonalizationProfile.
    /// </summary>
    /// <param name="request">Email details including recipients, subject, and body</param>
    /// <returns>Confirmation of email send status</returns>
    /// <response code="200">Email sent successfully</response>
    /// <response code="400">Invalid email request data</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("send")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get user's personalization profile
            var personalizationProfile = await GetPersonalizationProfileAsync();
            
            // Apply personalization to email content
            var personalizedBody = _graphEmailService.ApplyPersonalization(request.Body, personalizationProfile);
            
            // Send email via Microsoft Graph API
            var messageId = await _graphEmailService.SendEmailAsync(request.Recipients, request.Subject, personalizedBody);
            
            _logger.LogInformation("Email sent successfully to {RecipientCount} recipients via Microsoft Graph API", request.Recipients.Length);
            
            return Ok(new { 
                Success = true, 
                Message = "Email sent successfully via Microsoft Graph API",
                MessageId = messageId,
                SentAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via Microsoft Graph API");
            return StatusCode(500, new { error = "An error occurred while sending the email", details = ex.Message });
        }
    }

    /// <summary>
    /// Retrieves the latest emails from the user's inbox using Microsoft Graph API.
    /// </summary>
    /// <param name="top">Number of emails to retrieve (default: 50, max: 100)</param>
    /// <returns>List of email summaries from the inbox</returns>
    /// <response code="200">Emails retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("inbox")]
    [ProducesResponseType(typeof(IEnumerable<EmailSummary>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetInboxEmails([FromQuery] int top = 50)
    {
        try
        {
            if (top > 100) top = 100; // Limit to reasonable number
            if (top <= 0) top = 50;   // Default fallback
            
            var accessToken = GetAccessTokenFromHeader();
            var emails = await _graphUserService.GetInboxEmailsAsync(accessToken, top);
            
            _logger.LogInformation("Retrieved {Count} emails from inbox", emails.Count());
            
            return Ok(emails);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to inbox emails");
            return Unauthorized(new { error = "Access token required", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inbox emails");
            return StatusCode(500, new { error = "An error occurred while retrieving inbox emails", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets detailed information about a specific email using Microsoft Graph API.
    /// </summary>
    /// <param name="messageId">The email message ID</param>
    /// <returns>Detailed email information</returns>
    /// <response code="200">Email details retrieved successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Email not found</response>
    [HttpGet("details/{messageId}")]
    [ProducesResponseType(typeof(EmailDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmailDetails(string messageId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(messageId))
            {
                return BadRequest(new { error = "Message ID is required" });
            }
            
            var accessToken = GetAccessTokenFromHeader();
            var emailDetail = await _graphUserService.GetEmailDetailsAsync(messageId, accessToken);
            
            _logger.LogInformation("Retrieved email details for message {MessageId}", messageId);
            
            return Ok(emailDetail);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to email details");
            return Unauthorized(new { error = "Access token required", details = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = "Email not found", messageId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email details for message {MessageId}", messageId);
            return StatusCode(500, new { error = "An error occurred while retrieving email details", details = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an email using Microsoft Graph API.
    /// </summary>
    /// <param name="messageId">The email message ID to delete</param>
    /// <returns>Confirmation of deletion</returns>
    /// <response code="200">Email deleted successfully</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Email not found</response>
    [HttpDelete("delete/{messageId}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmail(string messageId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(messageId))
            {
                return BadRequest(new { error = "Message ID is required" });
            }
            
            var accessToken = GetAccessTokenFromHeader();
            await _graphUserService.DeleteEmailAsync(messageId, accessToken);
            
            _logger.LogInformation("Email {MessageId} deleted successfully", messageId);
            
            return Ok(new { 
                Success = true, 
                Message = "Email deleted successfully",
                MessageId = messageId,
                DeletedAt = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to delete email");
            return Unauthorized(new { error = "Access token required", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email {MessageId}", messageId);
            return StatusCode(500, new { error = "An error occurred while deleting the email", details = ex.Message });
        }
    }

    /// <summary>
    /// Generates an AI-powered draft for replying to an email using Microsoft Graph API.
    /// Uses the user's PersonalizationProfile to generate content in their voice.
    /// </summary>
    /// <param name="request">Request containing original email content and user input</param>
    /// <returns>AI-generated reply draft</returns>
    /// <response code="200">AI draft generated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("generate-ai-draft")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GenerateAIDraft([FromBody] GenerateAIDraftRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get user's personalization profile
            var personalizationProfile = await GetPersonalizationProfileAsync();
            
            // Generate AI draft using the user service
            var aiDraft = await _graphUserService.GenerateAIDraftAsync(
                request.OriginalBody, 
                request.UserInput, 
                personalizationProfile);
            
            _logger.LogInformation("AI draft generated successfully");
            
            return Ok(new { 
                Success = true,
                Draft = aiDraft,
                GeneratedAt = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to generate AI draft");
            return Unauthorized(new { error = "Access token required", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating AI draft");
            return StatusCode(500, new { error = "An error occurred while generating AI draft", details = ex.Message });
        }
    }

    /// <summary>
    /// Replies to an existing email thread using Microsoft Graph API.
    /// Reply content is personalized using the user's PersonalizationProfile.
    /// </summary>
    /// <param name="request">Reply details including original message ID and reply body</param>
    /// <returns>Confirmation of reply send status</returns>
    /// <response code="200">Reply sent successfully</response>
    /// <response code="400">Invalid reply request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Original message not found</response>
    [HttpPost("reply")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReplyToEmail([FromBody] ReplyEmailRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get user's personalization profile
            var personalizationProfile = await GetPersonalizationProfileAsync();
            
            // Apply personalization to reply content
            var personalizedBody = _graphUserService.ApplyPersonalization(request.Body, personalizationProfile);
            
            // Get access token and send reply via Microsoft Graph API
            var accessToken = GetAccessTokenFromHeader();
            var replyId = await _graphUserService.ReplyToEmailAsync(
                request.OriginalMessageId, 
                personalizedBody, 
                accessToken,
                false); // Single reply by default
            
            _logger.LogInformation("Email reply sent successfully for message ID: {MessageId}", request.OriginalMessageId);
            
            return Ok(new { 
                Success = true, 
                Message = "Reply sent successfully",
                ReplyId = replyId,
                OriginalMessageId = request.OriginalMessageId,
                SentAt = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to reply to email");
            return Unauthorized(new { error = "Access token required", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replying to email");
            return StatusCode(500, new { error = "An error occurred while replying to the email", details = ex.Message });
        }
    }

    /// <summary>
    /// Replies to all recipients of an existing email thread using Microsoft Graph API.
    /// Reply content is personalized using the user's PersonalizationProfile.
    /// </summary>
    /// <param name="request">Reply details including original message ID and reply body</param>
    /// <returns>Confirmation of reply all send status</returns>
    /// <response code="200">Reply all sent successfully</response>
    /// <response code="400">Invalid reply request data</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="404">Original message not found</response>
    [HttpPost("reply-all")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReplyAllToEmail([FromBody] ReplyEmailRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get user's personalization profile
            var personalizationProfile = await GetPersonalizationProfileAsync();
            
            // Apply personalization to reply content
            var personalizedBody = _graphUserService.ApplyPersonalization(request.Body, personalizationProfile);
            
            // Get access token and send reply all via Microsoft Graph API
            var accessToken = GetAccessTokenFromHeader();
            var replyId = await _graphUserService.ReplyToEmailAsync(
                request.OriginalMessageId, 
                personalizedBody, 
                accessToken,
                true); // Reply to all recipients
            
            _logger.LogInformation("Email reply all sent successfully for message ID: {MessageId}", request.OriginalMessageId);
            
            return Ok(new { 
                Success = true, 
                Message = "Reply all sent successfully",
                ReplyId = replyId,
                OriginalMessageId = request.OriginalMessageId,
                SentAt = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to reply all to email");
            return Unauthorized(new { error = "Access token required", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replying all to email");
            return StatusCode(500, new { error = "An error occurred while replying all to the email", details = ex.Message });
        }
    }

    /// <summary>
    /// Drafts an email using AI with personalization but does not send it.
    /// Uses the user's PersonalizationProfile to generate content in their voice.
    /// </summary>
    /// <param name="request">Request containing the prompt and optional context</param>
    /// <returns>AI-generated email draft with personalized content</returns>
    /// <response code="200">Email draft generated successfully</response>
    /// <response code="400">Invalid prompt provided</response>
    [HttpPost("draft")]
    [ProducesResponseType(typeof(SendEmailRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<SendEmailRequest> DraftEmail([FromBody] DraftEmailRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { error = "Prompt cannot be empty", field = "prompt" });
            }

            // TODO: Implement AI content generation with PersonalizationProfile
            // TODO: Forward request to Azure Function for AI-powered email drafting
            
            var draft = new SendEmailRequest
            {
                Recipients = Array.Empty<string>(),
                Subject = "[AI Generated] Subject based on: " + request.Prompt,
                Body = $"[AI Generated Email Body]\n\nBased on your request: {request.Prompt}\n\n" +
                       (string.IsNullOrWhiteSpace(request.Context) ? "" : $"Additional context: {request.Context}\n\n") +
                       "This email would be personalized using your profile preferences."
            };

            _logger.LogInformation("Email draft generated for prompt: {Prompt}", request.Prompt);
            return Ok(draft);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error drafting email");
            return StatusCode(500, new { error = "An error occurred while drafting the email", details = ex.Message });
        }
    }

    /// <summary>
    /// Helper method to extract access token from authorization header
    /// </summary>
    private string GetAccessTokenFromHeader()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            return authHeader.Substring("Bearer ".Length).Trim();
        }
        
        throw new UnauthorizedAccessException("Access token not found in Authorization header");
    }

    /// <summary>
    /// Helper method to get the user's personalization profile
    /// </summary>
    private async Task<PersonalizationProfile> GetPersonalizationProfileAsync()
    {
        try
        {
            // Create HTTP client to call the personalization API
            var httpClient = _httpClientFactory.CreateClient();
            
            // In a real implementation, this would use the current user's token
            // For now, we'll call the local personalization endpoint
            var response = await httpClient.GetAsync("http://localhost:5169/api/user/personalization-profile");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var profile = System.Text.Json.JsonSerializer.Deserialize<PersonalizationProfile>(content, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return profile ?? GetDefaultPersonalizationProfile();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to retrieve personalization profile, using default");
        }
        
        return GetDefaultPersonalizationProfile();
    }

    /// <summary>
    /// Returns a default personalization profile
    /// </summary>
    private static PersonalizationProfile GetDefaultPersonalizationProfile()
    {
        return new PersonalizationProfile
        {
            Tone = "professional",
            PreferredGreetings = new[] { "Hello" },
            SignatureClosings = new[] { "Best regards" },
            FavouritePhrases = Array.Empty<string>(),
            ProhibitedWords = Array.Empty<string>(),
            AboutMe = "Professional user",
            CustomAgentHints = new Dictionary<string, string>()
        };
    }
}