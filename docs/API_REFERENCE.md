# Prodigy API Reference

## Overview

The Prodigy API provides access to all intelligent digital workspace agents through a RESTful interface. All endpoints support AI-powered personalization using the user's PersonalizationProfile.

**Base URL:** `https://api.prodigy.com` (or `http://localhost:5000` for development)

**Authentication:** Bearer token (JWT) required for all endpoints except health check.

---

## API Endpoints

### Health Check

#### GET /health
Check API status and version.

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "1.0.0"
}
```

---

### Personalization Profile

#### GET /api/user/personalization-profile
Retrieve the current user's personalization profile.

**Response:**
```json
{
  "tone": "professional",
  "preferredGreetings": ["Hi", "Hello"],
  "signatureClosings": ["Best regards", "Thanks"],
  "favouritePhrases": ["Let's collaborate"],
  "prohibitedWords": ["ASAP"],
  "sampleTexts": [],
  "aboutMe": "I'm a professional who values clear communication.",
  "customAgentHints": {}
}
```

#### POST /api/user/personalization-profile
Create or update the user's personalization profile.

**Request Body:** PersonalizationProfile object

**Response:** Updated PersonalizationProfile

#### POST /api/user/personalization-profile/test
Test how content would sound with the current profile.

**Request Body:** String (test text)

**Response:** Personalized version of the input text

---

### Email Agent

#### POST /api/agents/email/send
Send a new email with AI personalization.

**Request:**
```json
{
  "recipients": ["user@example.com"],
  "subject": "Meeting Follow-up",
  "body": "Thanks for the great meeting today..."
}
```

**Response:**
```json
{
  "success": true,
  "message": "Email sent successfully",
  "messageId": "msg_123456",
  "sentAt": "2024-01-15T10:30:00Z"
}
```

#### POST /api/agents/email/reply
Reply to an existing email thread.

**Request:**
```json
{
  "originalMessageId": "msg_123456",
  "body": "Thanks for the update..."
}
```

#### POST /api/agents/email/draft
Generate an AI-powered email draft.

**Request Body:** String (description of email purpose)

**Response:** SendEmailRequest with generated content

---

### Task Agent

#### POST /api/agents/tasks
Create a new task with AI-generated execution plan.

**Request:**
```json
{
  "title": "Launch new product feature",
  "description": "Complete the rollout of the new dashboard feature",
  "dueDate": "2024-02-01T00:00:00Z",
  "priority": "High",
  "tags": ["product", "launch"]
}
```

**Response:**
```json
{
  "id": "task_123",
  "title": "Launch new product feature",
  "description": "Complete the rollout of the new dashboard feature",
  "dueDate": "2024-02-01T00:00:00Z",
  "priority": "High",
  "tags": ["product", "launch"],
  "executionPlan": [
    {
      "stepNumber": 1,
      "description": "Finalize feature requirements",
      "estimatedTime": "2 hours",
      "dependencies": [],
      "isCompleted": false
    }
  ],
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### GET /api/agents/tasks/{id}
Retrieve a specific task by ID.

#### PATCH /api/agents/tasks/{taskId}/steps/{stepNumber}
Update the completion status of an execution step.

**Request Body:** Boolean (isCompleted)

#### POST /api/agents/tasks/{taskId}/regenerate-plan
Regenerate the execution plan for an existing task.

---

### Learning Agent

#### POST /api/agents/learning
Generate structured learning material.

**Request:**
```json
{
  "topic": "React Hooks",
  "audience": "intermediate",
  "format": "text",
  "focusAreas": ["useState", "useEffect"],
  "estimatedTimeMinutes": 60
}
```

**Response:**
```json
{
  "id": "learning_123",
  "title": "Learning Guide: React Hooks",
  "topic": "React Hooks",
  "audience": "intermediate",
  "format": "text",
  "sections": [
    {
      "title": "Introduction",
      "content": "React Hooks are...",
      "order": 1,
      "type": "introduction"
    }
  ],
  "estimatedTimeMinutes": 60,
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### GET /api/agents/learning/{id}
Retrieve specific learning material.

#### POST /api/agents/learning/{id}/convert
Convert learning material to different format.

#### POST /api/agents/learning/{id}/quiz
Generate a quiz based on the learning material.

---

### Quote Agent

#### POST /api/agents/quotes
Create a professional quote.

**Request:**
```json
{
  "client": "Acme Corp",
  "clientContact": "john@acme.com",
  "items": [
    {
      "description": "Web Development Services",
      "quantity": 40,
      "unitPrice": 125.00,
      "unit": "hours"
    }
  ],
  "terms": "Net 30 days",
  "validUntil": "2024-02-15T00:00:00Z",
  "notes": "Includes responsive design"
}
```

**Response:**
```json
{
  "id": "quote_123",
  "quoteNumber": "Q20240115-1234",
  "client": "Acme Corp",
  "clientContact": "john@acme.com",
  "items": [...],
  "subtotal": 5000.00,
  "tax": 500.00,
  "total": 5500.00,
  "terms": "Net 30 days",
  "validUntil": "2024-02-15T00:00:00Z",
  "notes": "Includes responsive design",
  "createdAt": "2024-01-15T10:30:00Z",
  "formattedContent": "QUOTE #Q20240115-1234..."
}
```

#### GET /api/agents/quotes/{id}
Retrieve a specific quote.

#### GET /api/agents/quotes/{id}/pdf
Generate PDF version of the quote.

#### POST /api/agents/quotes/{id}/email
Email the quote to the client.

#### PUT /api/agents/quotes/{id}
Update an existing quote.

---

### Calendar Agent

#### POST /api/agents/calendar/availability
Find available time slots in calendar.

**Request:**
```json
{
  "startDate": "2024-01-20T00:00:00Z",
  "endDate": "2024-01-25T00:00:00Z",
  "minimumDurationMinutes": 120,
  "preferredStartTime": "09:00:00",
  "preferredEndTime": "17:00:00",
  "consecutiveDaysRequired": 1,
  "daysOfWeek": [1, 2, 3, 4, 5]
}
```

**Response:**
```json
[
  {
    "startTime": "2024-01-20T09:00:00Z",
    "endTime": "2024-01-20T17:00:00Z",
    "durationMinutes": 480,
    "confidenceScore": 90,
    "isMultiDay": false
  }
]
```

#### POST /api/agents/calendar/book
Book a calendar appointment.

#### GET /api/agents/calendar/events/{id}
Retrieve calendar event details.

#### POST /api/agents/calendar/suggest-meeting
Suggest optimal meeting times for multiple participants.

---

### GitHub Agent

#### POST /api/agents/github/feature-request
Create a new GitHub feature request.

**Request:**
```json
{
  "title": "Add dark mode support",
  "description": "Implement dark mode theme for better user experience",
  "labels": ["enhancement", "ui"],
  "assignedTo": "github-copilot",
  "priority": "Medium",
  "milestone": "v2.0"
}
```

**Response:**
```json
{
  "issueNumber": 1234,
  "issueUrl": "https://github.com/owner/repo/issues/1234",
  "title": "Add dark mode support",
  "state": "open",
  "assignedTo": "github-copilot",
  "labels": ["enhancement", "ui"],
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### GET /api/agents/github/feature-request/{issueNumber}
Retrieve specific feature request.

#### PUT /api/agents/github/feature-request/{issueNumber}
Update existing feature request.

#### GET /api/agents/github/feature-requests
List all feature requests with filtering.

#### POST /api/agents/github/feature-request/{issueNumber}/comment
Add comment to feature request.

#### POST /api/agents/github/feature-request/{issueNumber}/close
Close a feature request.

---

## Error Responses

All endpoints return consistent error responses:

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid request data",
    "details": ["Field 'title' is required"]
  },
  "timestamp": "2024-01-15T10:30:00Z"
}
```

**Common Error Codes:**
- `VALIDATION_ERROR` (400) - Invalid request data
- `UNAUTHORIZED` (401) - Invalid or missing authentication
- `FORBIDDEN` (403) - Insufficient permissions
- `NOT_FOUND` (404) - Resource not found
- `INTERNAL_ERROR` (500) - Server error

---

## Authentication

Include the JWT token in the Authorization header:

```
Authorization: Bearer your_jwt_token_here
```

Tokens can be obtained through the authentication flow (implementation pending).

---

## Rate Limiting

- 1000 requests per hour per user
- 10 requests per minute for AI generation endpoints
- Rate limit headers included in responses

---

## SDKs and Libraries

Official SDKs available for:
- JavaScript/TypeScript (NPM: `@prodigy/api-client`)
- Python (PyPI: `prodigy-api`)
- C# (NuGet: `Prodigy.ApiClient`)

---

For more information, see the [Prodigy Documentation](https://docs.prodigy.com).