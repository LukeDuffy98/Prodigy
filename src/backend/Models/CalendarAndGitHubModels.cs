using System.ComponentModel.DataAnnotations;

namespace Prodigy.Backend.Models;

/// <summary>
/// Input for advanced availability lookup in Outlook calendar
/// </summary>
public class AvailabilityRequest
{
    /// <summary>
    /// Start date for availability search
    /// </summary>
    [Required]
    public DateTime StartDate { get; set; }

    /// <summary>
    /// End date for availability search
    /// </summary>
    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Minimum duration required in minutes
    /// </summary>
    [Required]
    public int MinimumDurationMinutes { get; set; }

    /// <summary>
    /// Preferred start time (e.g., 9:00 AM)
    /// </summary>
    public TimeSpan? PreferredStartTime { get; set; }

    /// <summary>
    /// Preferred end time (e.g., 5:00 PM)
    /// </summary>
    public TimeSpan? PreferredEndTime { get; set; }

    /// <summary>
    /// Number of consecutive days required (for multi-day events)
    /// </summary>
    public int ConsecutiveDaysRequired { get; set; } = 1;

    /// <summary>
    /// Days of week to include (Monday=1, Sunday=7)
    /// </summary>
    public int[] DaysOfWeek { get; set; } = { 1, 2, 3, 4, 5 }; // Monday-Friday default
}

/// <summary>
/// Available time slot found in calendar
/// </summary>
public class AvailableTimeSlot
{
    /// <summary>
    /// Start date and time of availability
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// End date and time of availability
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Duration of the available slot in minutes
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Confidence score (0-100) based on surrounding calendar items
    /// </summary>
    public int ConfidenceScore { get; set; }

    /// <summary>
    /// Whether this spans multiple consecutive days
    /// </summary>
    public bool IsMultiDay { get; set; }
}

/// <summary>
/// Input for GitHub feature request creation
/// </summary>
public class FeatureRequest
{
    /// <summary>
    /// Feature request title
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the feature
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Labels to apply to the GitHub issue
    /// </summary>
    public string[] Labels { get; set; } = Array.Empty<string>();

    /// <summary>
    /// GitHub username to assign the issue to (e.g., "github-copilot")
    /// </summary>
    public string AssignedTo { get; set; } = string.Empty;

    /// <summary>
    /// Priority level (Low, Medium, High, Critical)
    /// </summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>
    /// Expected milestone or version
    /// </summary>
    public string Milestone { get; set; } = string.Empty;
}

/// <summary>
/// Response after creating a GitHub feature request
/// </summary>
public class FeatureRequestResponse
{
    /// <summary>
    /// GitHub issue number
    /// </summary>
    public int IssueNumber { get; set; }

    /// <summary>
    /// Direct URL to the created GitHub issue
    /// </summary>
    public string IssueUrl { get; set; } = string.Empty;

    /// <summary>
    /// Issue title as created
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Current issue state (usually "open")
    /// </summary>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Assigned user information
    /// </summary>
    public string AssignedTo { get; set; } = string.Empty;

    /// <summary>
    /// Applied labels
    /// </summary>
    public string[] Labels { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Generic request for AI content generation with personalization
/// Used by all agents that generate user-facing content
/// </summary>
public class AIGenerateContentRequest
{
    /// <summary>
    /// Type of content being generated (e.g., "email", "task", "quote", "learning")
    /// </summary>
    [Required]
    public string ContextType { get; set; } = string.Empty;

    /// <summary>
    /// Raw input data to be transformed into personalized content
    /// </summary>
    [Required]
    public string ContextData { get; set; } = string.Empty;

    /// <summary>
    /// User's personalization profile for consistent voice and style
    /// See /docs/PRODIGY_PERSONALIZATION_PROFILE.md for implementation details
    /// </summary>
    [Required]
    public PersonalizationProfile Profile { get; set; } = new();

    /// <summary>
    /// Additional context or instructions for content generation
    /// </summary>
    public string AdditionalInstructions { get; set; } = string.Empty;
}