using System.ComponentModel.DataAnnotations;

namespace Prodigy.Backend.Models;

/// <summary>
/// Summary information for an email in the inbox list
/// </summary>
public class EmailSummary
{
    /// <summary>
    /// Unique message identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Email subject line
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Sender information
    /// </summary>
    public EmailSender From { get; set; } = new();

    /// <summary>
    /// Date and time the email was received
    /// </summary>
    public DateTimeOffset ReceivedDateTime { get; set; }

    /// <summary>
    /// Whether the email has been read
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Brief preview of the email body content
    /// </summary>
    public string? BodyPreview { get; set; }
}

/// <summary>
/// Detailed information for a specific email
/// </summary>
public class EmailDetail
{
    /// <summary>
    /// Unique message identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Email subject line
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Sender information
    /// </summary>
    public EmailSender From { get; set; } = new();

    /// <summary>
    /// List of recipients (To field)
    /// </summary>
    public EmailSender[] ToRecipients { get; set; } = Array.Empty<EmailSender>();

    /// <summary>
    /// List of CC recipients
    /// </summary>
    public EmailSender[] CcRecipients { get; set; } = Array.Empty<EmailSender>();

    /// <summary>
    /// Date and time the email was received
    /// </summary>
    public DateTimeOffset ReceivedDateTime { get; set; }

    /// <summary>
    /// Full email body content
    /// </summary>
    public EmailBody Body { get; set; } = new();

    /// <summary>
    /// Whether the email has been read
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// Email importance level
    /// </summary>
    public string Importance { get; set; } = "Normal";
}

/// <summary>
/// Email sender or recipient information
/// </summary>
public class EmailSender
{
    /// <summary>
    /// Display name of the person
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Address { get; set; } = string.Empty;
}

/// <summary>
/// Email body content with type information
/// </summary>
public class EmailBody
{
    /// <summary>
    /// Body content type (Text or HTML)
    /// </summary>
    public string ContentType { get; set; } = "Text";

    /// <summary>
    /// The actual email content
    /// </summary>
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// Input for sending a new email via the Email Agent
/// </summary>
public class SendEmailRequest
{
    /// <summary>
    /// Email addresses of the recipients
    /// </summary>
    [Required]
    public string[] Recipients { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Email subject line
    /// </summary>
    [Required]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body content
    /// </summary>
    [Required]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Optional file attachments
    /// </summary>
    public List<IFormFile>? Attachments { get; set; }
}

/// <summary>
/// Input for replying to an existing email
/// </summary>
public class ReplyEmailRequest
{
    /// <summary>
    /// Original message ID to reply to
    /// </summary>
    [Required]
    public string OriginalMessageId { get; set; } = string.Empty;

    /// <summary>
    /// Reply body content
    /// </summary>
    [Required]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Optional file attachments
    /// </summary>
    public List<IFormFile>? Attachments { get; set; }
}

/// <summary>
/// Input for drafting an email using AI
/// </summary>
public class DraftEmailRequest
{
    /// <summary>
    /// Description of what the email should accomplish
    /// </summary>
    [Required]
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Optional context or additional information for the AI
    /// </summary>
    public string? Context { get; set; }
}

/// <summary>
/// Input for generating AI-powered email drafts (replies)
/// </summary>
public class GenerateAIDraftRequest
{
    /// <summary>
    /// Original email content to reply to
    /// </summary>
    [Required]
    public string OriginalBody { get; set; } = string.Empty;

    /// <summary>
    /// User's intended reply or reaction
    /// </summary>
    [Required]
    public string UserInput { get; set; } = string.Empty;

    /// <summary>
    /// Original email subject for context
    /// </summary>
    public string? OriginalSubject { get; set; }

    /// <summary>
    /// Original sender information for context
    /// </summary>
    public string? OriginalSender { get; set; }
}

/// <summary>
/// Input for creating a task with an execution plan
/// </summary>
public class CreateTaskRequest
{
    /// <summary>
    /// Task title/name
    /// </summary>
    [Required]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed task description
    /// </summary>
    [Required]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Optional due date for the task
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Task priority level (Low, Medium, High, Critical)
    /// </summary>
    public string Priority { get; set; } = "Medium";

    /// <summary>
    /// Optional tags for categorizing the task
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Response containing a task with its execution plan
/// </summary>
public class TaskWithPlan
{
    /// <summary>
    /// Unique task identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Task title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Task description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Due date if specified
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Task priority
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Task tags
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();

    /// <summary>
    /// AI-generated step-by-step execution plan
    /// </summary>
    public ExecutionStep[] ExecutionPlan { get; set; } = Array.Empty<ExecutionStep>();

    /// <summary>
    /// Task creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Individual step in a task execution plan
/// </summary>
public class ExecutionStep
{
    /// <summary>
    /// Step number in the sequence
    /// </summary>
    public int StepNumber { get; set; }

    /// <summary>
    /// Step description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Estimated time to complete this step
    /// </summary>
    public string EstimatedTime { get; set; } = string.Empty;

    /// <summary>
    /// Dependencies or prerequisites for this step
    /// </summary>
    public string[] Dependencies { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether this step has been completed
    /// </summary>
    public bool IsCompleted { get; set; }
}