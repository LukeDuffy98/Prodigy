using System.ComponentModel.DataAnnotations;

namespace Prodigy.Backend.Models;

/// <summary>
/// Input for learning material generation
/// </summary>
public class LearningMaterialRequest
{
    /// <summary>
    /// Topic to create learning material for
    /// </summary>
    [Required]
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Target audience (e.g., "beginners", "intermediate", "advanced", "children")
    /// </summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Desired format (e.g., "text", "outline", "slides", "interactive")
    /// </summary>
    [Required]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Optional specific focus areas or subtopics
    /// </summary>
    public string[] FocusAreas { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Estimated learning time in minutes
    /// </summary>
    public int? EstimatedTimeMinutes { get; set; }
}

/// <summary>
/// Generated learning material response
/// </summary>
public class LearningMaterial
{
    /// <summary>
    /// Unique identifier for the learning material
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Learning material title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Topic covered
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Target audience
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Material format
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Generated content sections
    /// </summary>
    public LearningSection[] Sections { get; set; } = Array.Empty<LearningSection>();

    /// <summary>
    /// Estimated learning time
    /// </summary>
    public int EstimatedTimeMinutes { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Individual section within learning material
/// </summary>
public class LearningSection
{
    /// <summary>
    /// Section title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Section content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Section order/sequence
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Section type (e.g., "introduction", "concept", "example", "exercise", "summary")
    /// </summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Input for quote creation
/// </summary>
public class QuoteRequest
{
    /// <summary>
    /// Client name or company
    /// </summary>
    [Required]
    public string Client { get; set; } = string.Empty;

    /// <summary>
    /// Client contact information
    /// </summary>
    public string ClientContact { get; set; } = string.Empty;

    /// <summary>
    /// List of services or items being quoted
    /// </summary>
    [Required]
    public List<QuoteLineItem> Items { get; set; } = new();

    /// <summary>
    /// Terms and conditions for the quote
    /// </summary>
    public string Terms { get; set; } = string.Empty;

    /// <summary>
    /// Quote valid until date
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// Individual line item in a quote
/// </summary>
public class QuoteLineItem
{
    /// <summary>
    /// Service or product description
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of the item/service
    /// </summary>
    [Required]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit price
    /// </summary>
    [Required]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Unit of measurement (e.g., "hours", "items", "months")
    /// </summary>
    public string Unit { get; set; } = "items";

    /// <summary>
    /// Line total (Quantity * UnitPrice)
    /// </summary>
    public decimal LineTotal => Quantity * UnitPrice;
}

/// <summary>
/// Generated quote response
/// </summary>
public class Quote
{
    /// <summary>
    /// Unique quote identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Quote number for reference
    /// </summary>
    public string QuoteNumber { get; set; } = string.Empty;

    /// <summary>
    /// Client information
    /// </summary>
    public string Client { get; set; } = string.Empty;

    /// <summary>
    /// Client contact details
    /// </summary>
    public string ClientContact { get; set; } = string.Empty;

    /// <summary>
    /// Quote line items
    /// </summary>
    public List<QuoteLineItem> Items { get; set; } = new();

    /// <summary>
    /// Subtotal before taxes
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// Tax amount
    /// </summary>
    public decimal Tax { get; set; }

    /// <summary>
    /// Total quote amount
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Terms and conditions
    /// </summary>
    public string Terms { get; set; } = string.Empty;

    /// <summary>
    /// Quote valid until date
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Quote creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Generated quote content (formatted text or PDF path)
    /// </summary>
    public string FormattedContent { get; set; } = string.Empty;
}