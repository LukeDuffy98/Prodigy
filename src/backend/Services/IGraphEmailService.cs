using Prodigy.Backend.Models;

namespace Prodigy.Backend.Services;

/// <summary>
/// Service interface for Microsoft Graph email operations
/// </summary>
public interface IGraphEmailService
{
    /// <summary>
    /// Sends an email using Microsoft Graph API
    /// </summary>
    /// <param name="recipients">Email recipients</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body content</param>
    /// <returns>Message ID of the sent email</returns>
    Task<string> SendEmailAsync(string[] recipients, string subject, string body);

    /// <summary>
    /// Applies personalization profile to email content
    /// </summary>
    /// <param name="body">Original email body</param>
    /// <param name="profile">User's personalization profile</param>
    /// <returns>Personalized email body</returns>
    string ApplyPersonalization(string body, PersonalizationProfile profile);
}