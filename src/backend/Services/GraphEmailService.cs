using Microsoft.Graph;
using Microsoft.Graph.Models;
using Azure.Identity;
using Prodigy.Backend.Models;

namespace Prodigy.Backend.Services;

/// <summary>
/// Implementation of Microsoft Graph email service
/// </summary>
public class GraphEmailService : IGraphEmailService
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly ILogger<GraphEmailService> _logger;

    public GraphEmailService(IConfiguration configuration, ILogger<GraphEmailService> logger)
    {
        _logger = logger;

        // Configure Azure AD authentication using client credentials flow
        var tenantId = configuration["AZURE_TENANT_ID"];
        var clientId = configuration["AZURE_CLIENT_ID"];
        var clientSecret = configuration["AZURE_CLIENT_SECRET"];

        if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("Azure AD configuration is missing. Please ensure AZURE_TENANT_ID, AZURE_CLIENT_ID, and AZURE_CLIENT_SECRET are configured.");
        }

        var options = new ClientSecretCredentialOptions
        {
            AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
        };

        var clientSecretCredential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);

        _graphServiceClient = new GraphServiceClient(clientSecretCredential);
    }

    /// <summary>
    /// Sends an email using Microsoft Graph API
    /// </summary>
    public async Task<string> SendEmailAsync(string[] recipients, string subject, string body)
    {
        try
        {
            var message = new Message
            {
                Subject = subject,
                Body = new ItemBody
                {
                    ContentType = BodyType.Text,
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

            // Send the email using the authenticated user (application permissions)
            // Note: This requires Mail.Send application permission in Azure AD
            await _graphServiceClient.Me.SendMail.PostAsync(new Microsoft.Graph.Me.SendMail.SendMailPostRequestBody
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