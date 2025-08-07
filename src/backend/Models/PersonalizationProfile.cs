using System.ComponentModel.DataAnnotations;

namespace Prodigy.Backend.Models;

/// <summary>
/// User's writing style and communication preferences for AI-generated content throughout Prodigy.
/// This profile instructs AI agents on how to compose text in the user's distinct voice.
/// See /docs/PRODIGY_PERSONALIZATION_PROFILE.md for full implementation details.
/// </summary>
public class PersonalizationProfile
{
    /// <summary>
    /// Communication tone preference (e.g., "friendly", "formal", "concise", "enthusiastic")
    /// </summary>
    [Required]
    public string Tone { get; set; } = "professional";

    /// <summary>
    /// User's preferred greeting phrases for messages and emails
    /// </summary>
    public string[] PreferredGreetings { get; set; } = Array.Empty<string>();

    /// <summary>
    /// User's preferred closing phrases for messages and emails
    /// </summary>
    public string[] SignatureClosings { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Favorite phrases or expressions the user likes to use
    /// </summary>
    public string[] FavouritePhrases { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Words or phrases the user prefers to avoid in communications
    /// </summary>
    public string[] ProhibitedWords { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Sample texts written by the user to learn their writing style
    /// </summary>
    public string[] SampleTexts { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Brief description about the user for context (e.g., "I'm an engineering manager who values direct, helpful communication")
    /// </summary>
    public string AboutMe { get; set; } = string.Empty;

    /// <summary>
    /// Per-agent specific guidelines (e.g., email: "always start with project name")
    /// </summary>
    public Dictionary<string, string> CustomAgentHints { get; set; } = new();
}