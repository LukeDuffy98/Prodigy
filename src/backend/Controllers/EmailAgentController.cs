using Microsoft.AspNetCore.Mvc;
using Prodigy.Backend.Models;
using Prodigy.Backend.Services;

namespace Prodigy.Backend.Controllers;

/// <summary>
/// Email Agent controller for sending and replying to emails via Microsoft Graph API.
/// All email generation uses the user's PersonalizationProfile for consistent voice and style.
/// </summary>
[ApiController]
[Route("api/agents/email")]
[Produces("application/json")]
public class EmailAgentController : ControllerBase
{
    private readonly ILogger<EmailAgentController> _logger;
    private readonly IGraphEmailService _graphEmailService;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailAgentController(ILogger<EmailAgentController> logger, IGraphEmailService graphEmailService, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _graphEmailService = graphEmailService;
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

            // TODO: Implement Microsoft Graph API integration for replying to emails
            // TODO: Apply user's PersonalizationProfile to reply content
            // TODO: Forward request to Azure Function for actual email reply
            
            _logger.LogInformation("Email reply request processed for message ID: {MessageId}", request.OriginalMessageId);
            
            return Ok(new { 
                Success = true, 
                Message = "Reply sent successfully",
                ReplyId = Guid.NewGuid().ToString(),
                OriginalMessageId = request.OriginalMessageId,
                SentAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replying to email");
            return StatusCode(500, new { error = "An error occurred while replying to the email", details = ex.Message });
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
    public async Task<ActionResult<SendEmailRequest>> DraftEmail([FromBody] DraftEmailRequest request)
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