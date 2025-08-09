
# Prodigy

**Prodigy** is your intelligent digital workspace: a single, responsive web application that empowers you with automated “agents” to streamline your daily workflow. Prodigy connects to Microsoft 365 (Outlook, Tasks, Calendar), LinkedIn, and GitHub to help you communicate, organize, plan, and track work far more efficiently.

---

## Vision

- **Prodigy** acts as a central desk staffed by digital agents: email, calendar, task, learning, and GitHub agents, all reporting to you.
- Automates repetitive tasks and integrates multiple services in one unified, mobile-friendly platform.
- Uses a modern front-end, a secure C# API backend, and Azure Functions for all business logic and third-party integrations.

---

## Features & Functional Requirements

### 1. Email Operations
- **Send Email** – Compose and send new emails.
- **Reply to Email** – Draft and send replies to existing threads.

### 2. Task/Execution Planning
- **Create Task** – Enter a work task.
- **Detailed Execution Plan** – Upon task creation, generate (automatically or via AI/Copilot prompt) a step-by-step plan for successful completion, visible in the app.

### 3. Learning Materials
- **Create Learning Material** – Describe a topic and have Prodigy generate structured learning content in text, outline, or slide formats.

### 4. Quote Generation
- **Create Quote** – Enter client/service info and terms; Prodigy generates a formatted quote (text or PDF).

### 5. Advanced Availability Lookup
- **Availability Finder** – Surface free blocks in your Outlook 365 calendar (e.g., “Find 3 consecutive days 9am–5pm where I can teach”), using Microsoft Graph.

### 6. Feature Requests Integration (GitHub)
- **Add Feature Request** – Submit new feature ideas directly from the app. These are written as issues to the GitHub repo and can be assigned to "github copilot user" for follow-up.

---

## Technical Architecture

### Front End
- Modern, responsive UI (React or Vue.js recommended)
- Provides user authentication via OAuth (Microsoft, LinkedIn, GitHub as needed)
- Displays dashboards for email, task, calendar, etc.
- Clean navigation between all agent modules
- Mobile-first design (using a framework like Tailwind CSS, Bootstrap, or Material-UI)

### Backend
- ASP.NET Core Web API (C#)
- Exposes REST endpoints under `/api/agents/*`
- Handles authentication, forwards calls securely to Azure Functions

### Azure Functions
- Core business logic for each major task and all external API calls
- C# durable functions as needed for asynchronous tasks (e.g., background plan generation)
- One function per feature:
    - Send/Reply Email Function
    - Task/Plan Generator Function
    - Learning Material Creator Function
    - Quote Generator Function
    - Availability Lookup Function
    - GitHub Issue Creator Function
- All functions use per-user OAuth tokens, passed securely from backend
- Errors and events logged to Application Insights

---

## Example API Contracts (Copilot-Friendly)

```csharp
/// <summary>Input for sending a new email</summary>
public class SendEmailRequest {
    public string[] Recipients { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public List<IFormFile> Attachments { get; set; }
}

/// <summary>Input for creating a task with an execution plan</summary>
public class CreateTaskRequest {
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime? DueDate { get; set; }
    // Copilot: Suggest adding field for task priority or tags
}

/// <summary>Input for learning material generation</summary>
public class LearningMaterialRequest {
    public string Topic { get; set; }
    public string Audience { get; set; }
    public string Format { get; set; } // e.g. "text", "outline", "slides"
}

/// <summary>Input for quote creation</summary>
public class QuoteRequest {
    public string Client { get; set; }
    public List<QuoteLineItem> Items { get; set; }
    public string Terms { get; set; }
}

/// <summary>Input for GitHub feature request</summary>
public class FeatureRequest {
    public string Title { get; set; }
    public string Description { get; set; }
    public string[] Labels { get; set; }
    public string AssignedTo { get; set; } // e.g. "github copilot user"
}
```

---

## Folder Structure

```
/prodigy
  /src
    /frontend            # React/Vue.js sources
    /backend             # ASP.NET Core Web API (C#)
      /Controllers
      /Models
      /Services
  /azure-functions       # C# Azure Functions project(s)
    /EmailFunctions
    /TaskFunctions
    /LearningFunctions
    /QuoteFunctions
    /CalendarFunctions
    /GitHubFunctions
  /docs                  # Architecture, API reference, user guides
  /.env.example          # Config template for required environment variables
```

---

## Security & Operations

- Store all secrets (API keys, client IDs/secrets) in environment variables or Azure Key Vault.
- Never commit secrets or tokens.
- Document authentication flow to provide Copilot/Contributors context.
- Backend and functions perform strong validation/sanitization.

---

## External Integrations

- **Microsoft Graph API** (mail, tasks, calendar)
- **LinkedIn API** (notifications, connections)
- **GitHub REST API** (issues)
- All via OAuth 2.0; tokens passed securely and never stored beyond session.

---

## Acceptance Criteria

- [ ] User can authenticate with Microsoft and LinkedIn.
- [ ] All major features work end-to-end via REST API and front-end UI.
- [ ] Azure Functions correctly execute complex/async tasks (plan, learning, quote, availability, GitHub issue).
- [ ] Fully responsive interface.
- [ ] All code documented with XML doc comments for DTOs and endpoints.
- [ ] Contributor guide and API reference included in `/docs`.

---

## References & Resources

- [Microsoft Graph API Docs](https://learn.microsoft.com/en-us/graph/)
- [LinkedIn API Docs](https://docs.microsoft.com/en-us/linkedin/)
- [GitHub REST API Docs](https://docs.github.com/en/rest/)
- [Azure Functions C# Guide](https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library)
- [Copilot Doc Best Practices](https://docs.github.com/en/copilot)

---

## Copilot Guidance

- Wherever possible, use XML or markdown doc comments on methods, classes, and endpoints explaining:
  - purpose
  - input/output (with examples)
  - links to relevant API endpoints
- Provide sample payloads and usage for API endpoints directly in comments.
- Favor explicit types, clear naming, and small focused functions, to help Copilot generate accurate completions and extensions.

---

> **Welcome to Prodigy! Digital agents at your command.**


