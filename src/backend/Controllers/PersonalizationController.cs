using Microsoft.AspNetCore.Mvc;
using Prodigy.Backend.Models;
using System.ComponentModel.DataAnnotations;

namespace Prodigy.Backend.Controllers;

/// <summary>
/// Personalization Profile controller for managing user's writing style and AI personalization preferences.
/// See /docs/PRODIGY_PERSONALIZATION_PROFILE.md for complete feature specification.
/// </summary>
[ApiController]
[Route("api/user/personalization-profile")]
[Produces("application/json")]
public class PersonalizationController : ControllerBase
{
    private readonly ILogger<PersonalizationController> _logger;

    public PersonalizationController(ILogger<PersonalizationController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the current user's personalization profile
    /// </summary>
    /// <returns>User's personalization profile with all style preferences</returns>
    /// <response code="200">Returns the user's personalization profile</response>
    /// <response code="404">User profile not found</response>
    [HttpGet]
    [ProducesResponseType(typeof(PersonalizationProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PersonalizationProfile>> GetPersonalizationProfile()
    {
        try
        {
            // TODO: Implement user authentication and profile retrieval from database
            // For now, return a sample profile
            var profile = new PersonalizationProfile
            {
                Tone = "professional",
                PreferredGreetings = new[] { "Hi", "Hello" },
                SignatureClosings = new[] { "Best regards", "Thanks" },
                FavouritePhrases = new[] { "Let's collaborate", "I appreciate your input" },
                ProhibitedWords = new[] { "ASAP", "Urgently" },
                AboutMe = "I'm a professional who values clear, direct communication.",
                CustomAgentHints = new Dictionary<string, string>
                {
                    { "email", "Keep messages concise and action-oriented" },
                    { "quote", "Emphasize value and transparency" }
                }
            };

            _logger.LogInformation("Retrieved personalization profile for user");
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving personalization profile");
            return StatusCode(500, "An error occurred while retrieving the profile");
        }
    }

    /// <summary>
    /// Creates or updates the user's personalization profile
    /// </summary>
    /// <param name="profile">Complete personalization profile with user preferences</param>
    /// <returns>Updated personalization profile</returns>
    /// <response code="200">Profile updated successfully</response>
    /// <response code="400">Invalid profile data provided</response>
    [HttpPost]
    [ProducesResponseType(typeof(PersonalizationProfile), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PersonalizationProfile>> SavePersonalizationProfile([FromBody] PersonalizationProfile profile)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Implement user authentication and profile saving to database
            _logger.LogInformation("Saved personalization profile for user with tone: {Tone}", profile.Tone);
            
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving personalization profile");
            return StatusCode(500, "An error occurred while saving the profile");
        }
    }

    /// <summary>
    /// Tests how content would sound with the current personalization profile
    /// </summary>
    /// <param name="testText">Sample text to personalize</param>
    /// <returns>Personalized version of the input text</returns>
    /// <response code="200">Returns personalized text sample</response>
    /// <response code="400">Invalid test text provided</response>
    [HttpPost("test")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<string>> TestPersonalization([FromBody] [Required] string testText)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(testText))
            {
                return BadRequest("Test text cannot be empty");
            }

            // TODO: Implement actual AI personalization logic
            // For now, return a simple transformation
            var personalizedText = $"[Personalized with professional tone]: {testText}";
            
            _logger.LogInformation("Generated personalization test for user");
            return Ok(personalizedText);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing personalization");
            return StatusCode(500, "An error occurred while testing personalization");
        }
    }
}