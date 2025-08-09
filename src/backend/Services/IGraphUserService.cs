using Microsoft.Graph.Models;
using Prodigy.Backend.Models;

namespace Prodigy.Backend.Services;

/// <summary>
/// Service interface for Microsoft Graph user-authenticated operations
/// </summary>
public interface IGraphUserService
{
    /// <summary>
    /// Sends an email using Microsoft Graph API with user authentication
    /// </summary>
    /// <param name="recipients">Email recipients</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body content</param>
    /// <param name="accessToken">User's access token</param>
    /// <returns>Message ID of the sent email</returns>
    Task<string> SendEmailAsync(string[] recipients, string subject, string body, string accessToken);

    /// <summary>
    /// Retrieves emails from the user's inbox
    /// </summary>
    /// <param name="accessToken">User's access token</param>
    /// <param name="top">Number of emails to retrieve (default: 50)</param>
    /// <returns>Collection of inbox emails</returns>
    Task<IEnumerable<EmailSummary>> GetInboxEmailsAsync(string accessToken, int top = 50);

    /// <summary>
    /// Gets detailed information about a specific email
    /// </summary>
    /// <param name="messageId">The email message ID</param>
    /// <param name="accessToken">User's access token</param>
    /// <returns>Email details</returns>
    Task<EmailDetail> GetEmailDetailsAsync(string messageId, string accessToken);

    /// <summary>
    /// Creates a reply to an existing email
    /// </summary>
    /// <param name="messageId">Original message ID to reply to</param>
    /// <param name="body">Reply content</param>
    /// <param name="accessToken">User's access token</param>
    /// <param name="replyAll">Whether to reply to all recipients</param>
    /// <returns>Message ID of the sent reply</returns>
    Task<string> ReplyToEmailAsync(string messageId, string body, string accessToken, bool replyAll = false);

    /// <summary>
    /// Deletes an email
    /// </summary>
    /// <param name="messageId">The email message ID to delete</param>
    /// <param name="accessToken">User's access token</param>
    Task DeleteEmailAsync(string messageId, string accessToken);

    /// <summary>
    /// Generates an AI-powered email draft
    /// </summary>
    /// <param name="originalBody">The original email content (for replies)</param>
    /// <param name="userInput">User's intended response or instructions</param>
    /// <param name="profile">User's personalization profile</param>
    /// <returns>AI-generated email draft</returns>
    Task<string> GenerateAIDraftAsync(string originalBody, string userInput, PersonalizationProfile profile);

    /// <summary>
    /// Applies personalization profile to email content
    /// </summary>
    /// <param name="body">Original email body</param>
    /// <param name="profile">User's personalization profile</param>
    /// <returns>Personalized email body</returns>
    string ApplyPersonalization(string body, PersonalizationProfile profile);
}
