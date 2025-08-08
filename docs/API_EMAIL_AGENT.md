# Email Agent API Documentation

The Email Agent API provides endpoints for sending emails, drafting AI-powered emails, and replying to existing email threads.

## Base URL
```
http://localhost:5169/api/agents/email
```

## Authentication
All endpoints require valid JWT authentication. Include the token in the Authorization header:
```
Authorization: Bearer <your-jwt-token>
```

## Content Type
All POST requests must include the JSON content type header:
```
Content-Type: application/json
```

## Endpoints

### 1. Draft Email
Generate an AI-powered email draft using personalization.

**Endpoint:** `POST /draft`

**Request Body:**
```json
{
  "prompt": "string (required) - Description of what the email should accomplish",
  "context": "string (optional) - Additional context or information for the AI"
}
```

**Success Response (200):**
```json
{
  "recipients": [],
  "subject": "string - AI-generated subject line",
  "body": "string - AI-generated email body",
  "attachments": null
}
```

**Error Responses:**
- `400 Bad Request` - Invalid or missing prompt
- `500 Internal Server Error` - Server error during draft generation

**Example:**
```bash
curl -X POST http://localhost:5169/api/agents/email/draft \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "prompt": "Write a professional meeting request",
    "context": "For discussing the Q4 project proposal"
  }'
```

### 2. Send Email
Send an email to specified recipients.

**Endpoint:** `POST /send`

**Request Body:**
```json
{
  "recipients": ["string"] - Array of email addresses,
  "subject": "string (required) - Email subject line",
  "body": "string (required) - Email body content",
  "attachments": null - File attachments (future feature)
}
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Email sent successfully",
  "messageId": "string - Unique message identifier",
  "sentAt": "datetime - When the email was sent"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid request data or validation errors
- `401 Unauthorized` - Invalid or missing authentication
- `500 Internal Server Error` - Server error during email sending

**Example:**
```bash
curl -X POST http://localhost:5169/api/agents/email/send \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "recipients": ["user@example.com"],
    "subject": "Test Email",
    "body": "This is a test email."
  }'
```

### 3. Reply to Email
Reply to an existing email thread.

**Endpoint:** `POST /reply`

**Request Body:**
```json
{
  "originalMessageId": "string (required) - ID of the original message",
  "body": "string (required) - Reply content",
  "attachments": null - File attachments (future feature)
}
```

**Success Response (200):**
```json
{
  "success": true,
  "message": "Reply sent successfully",
  "replyId": "string - Unique reply identifier",
  "originalMessageId": "string - Original message ID",
  "sentAt": "datetime - When the reply was sent"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid request data or validation errors
- `401 Unauthorized` - Invalid or missing authentication
- `404 Not Found` - Original message not found
- `500 Internal Server Error` - Server error during reply sending

**Example:**
```bash
curl -X POST http://localhost:5169/api/agents/email/reply \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <token>" \
  -d '{
    "originalMessageId": "abc123",
    "body": "Thank you for your email. I will review and respond shortly."
  }'
```

## Error Handling

All endpoints return structured error responses with the following format:

```json
{
  "error": "string - Error message",
  "details": "string - Additional error details (in development)",
  "field": "string - Field name for validation errors (optional)"
}
```

### Common HTTP Status Codes
- `200 OK` - Request successful
- `400 Bad Request` - Invalid request data or validation error
- `401 Unauthorized` - Authentication required or invalid
- `404 Not Found` - Resource not found (for reply endpoint)
- `415 Unsupported Media Type` - Missing or incorrect Content-Type header
- `500 Internal Server Error` - Server error

## Frontend Integration

### Using Axios
```javascript
import axios from 'axios';

// Configure axios with default headers
const apiClient = axios.create({
  headers: {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${userToken}`
  }
});

// Draft email
const draftResponse = await apiClient.post('/api/agents/email/draft', {
  prompt: 'Write a meeting request',
  context: 'For project discussion'
});

// Send email
const sendResponse = await apiClient.post('/api/agents/email/send', {
  recipients: ['user@example.com'],
  subject: draftResponse.data.subject,
  body: draftResponse.data.body
});
```

## Notes

- All email operations use placeholder implementations for development
- Microsoft Graph API integration is planned for production deployment
- Personalization features will apply user profile preferences to generated content
- File attachments are planned for future releases
- CORS is configured for frontend integration on ports 3000 and 5173