using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Core;
using Prodigy.Backend.Models;

namespace Prodigy.Backend.Services;

/// <summary>
/// Implementation of Microsoft Graph user-authenticated email service
/// </summary>
public class GraphUserService : IGraphUserService
{
    private readonly ILogger<GraphUserService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public GraphUserService(ILogger<GraphUserService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Creates a GraphServiceClient with user access token
    /// </summary>
    private GraphServiceClient CreateUserGraphClient(string accessToken)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        
        var graphServiceClient = new GraphServiceClient(httpClient);
        return graphServiceClient;
    }

    /// <summary>
    /// Sends an email using Microsoft Graph API with user authentication
    /// </summary>
    public async Task<string> SendEmailAsync(string[] recipients, string subject, string body, string accessToken)
    {
        try
        {
            var graphServiceClient = CreateUserGraphClient(accessToken);
            
            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Html,
                    Content = body
                },
                ToRecipients = recipients.Select(email => new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = email
                    }
                }).ToList()
            };

            await graphServiceClient.Me.SendMail.PostAsync(new Microsoft.Graph.Me.SendMail.SendMailPostRequestBody
            {
                Message = message,
                SaveToSentItems = true
            });

            var messageId = Guid.NewGuid().ToString(); // Graph API doesn't return message ID immediately
            _logger.LogInformation("Email sent successfully to {Recipients}", string.Join(", ", recipients));
            
            return messageId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Microsoft Graph API");
            throw;
        }
    }

    /// <summary>
    /// Retrieves emails from the user's inbox
    /// </summary>
    public async Task<IEnumerable<EmailSummary>> GetInboxEmailsAsync(string accessToken, int top = 50)
    {
        try
        {
            var graphServiceClient = CreateUserGraphClient(accessToken);
            
            var messages = await graphServiceClient.Me.MailFolders["Inbox"].Messages
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Top = top;
                    requestConfiguration.QueryParameters.Select = new[] { "id", "subject", "from", "receivedDateTime", "isRead", "bodyPreview" };
                    requestConfiguration.QueryParameters.Orderby = new[] { "receivedDateTime desc" };
                });

            var emailSummaries = messages?.Value?.Select(msg => new EmailSummary
            {
                Id = msg.Id ?? string.Empty,
                Subject = msg.Subject ?? string.Empty,
                From = new EmailSender
                {
                    Name = msg.From?.EmailAddress?.Name ?? string.Empty,
                    Address = msg.From?.EmailAddress?.Address ?? string.Empty
                },
                ReceivedDateTime = msg.ReceivedDateTime ?? DateTimeOffset.Now,
                IsRead = msg.IsRead ?? false,
                BodyPreview = msg.BodyPreview
            }).ToList() ?? new List<EmailSummary>();

            _logger.LogInformation("Retrieved {Count} emails from inbox", emailSummaries.Count());
            return emailSummaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve inbox emails");
            throw;
        }
    }

    /// <summary>
    /// Gets detailed information about a specific email
    /// </summary>
    public async Task<EmailDetail> GetEmailDetailsAsync(string messageId, string accessToken)
    {
        try
        {
            var graphServiceClient = CreateUserGraphClient(accessToken);
            
            var message = await graphServiceClient.Me.Messages[messageId]
                .GetAsync(requestConfiguration =>
                {
                    requestConfiguration.QueryParameters.Select = new[] { "id", "subject", "body", "from", "toRecipients", "ccRecipients", "receivedDateTime", "isRead", "importance" };
                });

            if (message == null)
            {
                throw new InvalidOperationException($"Email with ID {messageId} not found");
            }

            var emailDetail = new EmailDetail
            {
                Id = message.Id ?? string.Empty,
                Subject = message.Subject ?? string.Empty,
                From = new EmailSender
                {
                    Name = message.From?.EmailAddress?.Name ?? string.Empty,
                    Address = message.From?.EmailAddress?.Address ?? string.Empty
                },
                ToRecipients = message.ToRecipients?.Select(r => new EmailSender
                {
                    Name = r.EmailAddress?.Name ?? string.Empty,
                    Address = r.EmailAddress?.Address ?? string.Empty
                }).ToArray() ?? Array.Empty<EmailSender>(),
                CcRecipients = message.CcRecipients?.Select(r => new EmailSender
                {
                    Name = r.EmailAddress?.Name ?? string.Empty,
                    Address = r.EmailAddress?.Address ?? string.Empty
                }).ToArray() ?? Array.Empty<EmailSender>(),
                ReceivedDateTime = message.ReceivedDateTime ?? DateTimeOffset.Now,
                Body = new EmailBody
                {
                    ContentType = message.Body?.ContentType?.ToString() ?? "Text",
                    Content = message.Body?.Content ?? string.Empty
                },
                IsRead = message.IsRead ?? false,
                Importance = message.Importance?.ToString() ?? "Normal"
            };

            _logger.LogInformation("Retrieved email details for message {MessageId}", messageId);
            return emailDetail;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve email details for message {MessageId}", messageId);
            throw;
        }
    }

    /// <summary>
    /// Creates a reply to an existing email
    /// </summary>
    public async Task<string> ReplyToEmailAsync(string messageId, string body, string accessToken, bool replyAll = false)
    {
        try
        {
            var graphServiceClient = CreateUserGraphClient(accessToken);
            
            // First create a reply draft
            var replyDraft = replyAll 
                ? await graphServiceClient.Me.Messages[messageId].CreateReplyAll.PostAsync(
                    new Microsoft.Graph.Me.Messages.Item.CreateReplyAll.CreateReplyAllPostRequestBody())
                : await graphServiceClient.Me.Messages[messageId].CreateReply.PostAsync(
                    new Microsoft.Graph.Me.Messages.Item.CreateReply.CreateReplyPostRequestBody());

            if (replyDraft?.Id == null)
            {
                throw new InvalidOperationException("Failed to create reply draft");
            }

            // Update the draft with our content
            await graphServiceClient.Me.Messages[replyDraft.Id]
                .PatchAsync(new Message
                {
                    Body = new ItemBody
                    {
                        ContentType = BodyType.Html,
                        Content = body
                    }
                });

            // Send the email
            await graphServiceClient.Me.Messages[replyDraft.Id].Send.PostAsync();

            _logger.LogInformation("Reply sent successfully for message {MessageId}, replyAll: {ReplyAll}", messageId, replyAll);
            return replyDraft.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reply to email {MessageId}", messageId);
            throw;
        }
    }

    /// <summary>
    /// Deletes an email
    /// </summary>
    public async Task DeleteEmailAsync(string messageId, string accessToken)
    {
        try
        {
            var graphServiceClient = CreateUserGraphClient(accessToken);
            
            await graphServiceClient.Me.Messages[messageId].DeleteAsync();
            _logger.LogInformation("Email {MessageId} deleted successfully", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete email {MessageId}", messageId);
            throw;
        }
    }

    /// <summary>
    /// Generates an AI-powered email draft
    /// </summary>
    public async Task<string> GenerateAIDraftAsync(string originalBody, string userInput, PersonalizationProfile profile)
    {
        try
        {
            // Create prompt following the Quill Learning pattern
            var prompt = $@"You are an assistant tasked with drafting professional email responses.
Below is the original email message: {originalBody}
Here is my intended reply or reaction: {userInput}

Instructions:
- Reference the main points from the original email (for context)
- Convey my intended reply or sentiment in a polished and business-appropriate way
- Use a {profile.Tone?.ToLower() ?? "professional"} tone
- Optionally, suggest next steps or ask a clarifying question if appropriate
- Format the reply as a full email, including a greeting and closing
- Do NOT include a subject line in your reply
- Do NOT include placeholders for name, position, or contact information; end with a simple closing such as 'Best regards' or 'Thank you' only.";

            // Here you would integrate with your AI service (Azure OpenAI, etc.)
            // For now, I'll create a placeholder implementation
            var aiDraft = await CallAIService(prompt, profile);
            
            _logger.LogInformation("AI draft generated successfully");
            return aiDraft;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI draft");
            throw;
        }
    }

    /// <summary>
    /// Placeholder for AI service integration
    /// </summary>
    private async Task<string> CallAIService(string prompt, PersonalizationProfile profile)
    {
        // TODO: Integrate with Azure OpenAI or your preferred AI service
        // This is a placeholder implementation
        
        await Task.Delay(100); // Simulate API call
        
        return $"Thank you for your email. I appreciate you taking the time to reach out.\n\n" +
               $"Based on your message, I understand your concerns and will look into this matter promptly.\n\n" +
               $"I'll get back to you with a detailed response within the next business day.\n\n" +
               $"{profile.SignatureClosings?.FirstOrDefault() ?? "Best regards"}";
    }

    /// <summary>
    /// Applies personalization profile to email content
    /// </summary>
    public string ApplyPersonalization(string body, PersonalizationProfile profile)
    {
        if (profile == null)
        {
            return body;
        }

        var personalizedBody = body;

        // Apply greeting if available
        if (profile.PreferredGreetings?.Length > 0)
        {
            var greeting = profile.PreferredGreetings[0]; // Use first preferred greeting
            personalizedBody = $"{greeting},\n\n{personalizedBody}";
        }

        // Apply closing if available
        if (profile.SignatureClosings?.Length > 0)
        {
            var closing = profile.SignatureClosings[0]; // Use first preferred closing
            personalizedBody = $"{personalizedBody}\n\n{closing}";
        }

        // Apply favorite phrases if email is too generic
        if (profile.FavouritePhrases?.Length > 0 && personalizedBody.Length < 100)
        {
            var phrase = profile.FavouritePhrases[0];
            personalizedBody = $"{personalizedBody}\n\n{phrase}";
        }

        _logger.LogDebug("Applied personalization profile with tone: {Tone}", profile.Tone);
        
        return personalizedBody;
    }
}
