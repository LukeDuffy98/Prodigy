# Prodigy Email Agent Enhancement

This document describes the enhanced email functionality that brings the capabilities of the Quill Learning Email Helper Application into the Prodigy platform.

## Overview

The enhanced email agent provides:
- **Microsoft Graph Integration**: Full read/write access to user's Outlook mailbox
- **AI-Powered Responses**: Automated professional email drafting using personalization profiles
- **User Authentication**: Microsoft Azure AD authentication for secure email access
- **Rich Email Management**: List, view, reply, delete emails with personalization

## Key Features

### 1. User Authentication
- **Microsoft Graph Authentication**: Users must sign in with their Microsoft account to access email functionality
- **Token-Based Security**: All email operations require a valid Microsoft Graph access token
- **Authorization Policies**: Different authentication schemes for different API endpoints

### 2. Email Management
- **Inbox Retrieval**: Get the latest emails from user's inbox with pagination support
- **Email Details**: View full email content, recipients, and metadata
- **Email Deletion**: Permanently delete emails from the mailbox
- **Reply & Reply All**: Respond to emails with personalized content

### 3. AI-Powered Email Drafting
- **Intelligent Drafting**: Generate professional email responses using AI
- **Personalization**: Apply user's personalization profile for consistent voice and tone
- **Context Awareness**: AI considers original email content and user intent
- **Professional Templates**: Following Quill Learning's proven email patterns

## API Endpoints

### Authentication
- `GET /api/auth/me` - Get current user information
- `POST /api/auth/validate-token` - Validate Microsoft Graph token
- `GET /api/auth/config` - Get authentication configuration
- `POST /api/auth/logout` - Logout user

### Email Operations
- `GET /api/agents/email/inbox?top=50` - Get inbox emails
- `GET /api/agents/email/details/{messageId}` - Get email details
- `DELETE /api/agents/email/delete/{messageId}` - Delete email
- `POST /api/agents/email/reply` - Reply to email
- `POST /api/agents/email/reply-all` - Reply to all recipients
- `POST /api/agents/email/generate-ai-draft` - Generate AI email draft

## Required Permissions

### Azure AD Application Registration
Your Azure AD application must be configured with the following Microsoft Graph permissions:

**Delegated Permissions:**
- `Mail.ReadWrite` - Read and write access to user's emails
- `Mail.Send` - Send emails on behalf of the user
- `User.Read` - Read user profile information

### Configuration
Update your environment variables or configuration:
```bash
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret
```

## Usage Examples

### 1. Authentication Flow
```javascript
// Frontend authentication (using MSAL.js)
const msalConfig = {
    auth: {
        clientId: "your-client-id",
        authority: "https://login.microsoftonline.com/your-tenant-id",
        redirectUri: window.location.origin + "/auth/callback"
    }
};

// Get access token
const tokenResponse = await msalInstance.acquireTokenSilent({
    scopes: ["https://graph.microsoft.com/Mail.ReadWrite"]
});
```

### 2. Getting Inbox Emails
```javascript
const response = await fetch('/api/agents/email/inbox?top=50', {
    headers: {
        'Authorization': `Bearer ${accessToken}`
    }
});
const emails = await response.json();
```

### 3. Generating AI Draft
```javascript
const response = await fetch('/api/agents/email/generate-ai-draft', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${accessToken}`
    },
    body: JSON.stringify({
        originalBody: "Original email content...",
        userInput: "I want to politely decline this meeting",
        originalSubject: "Meeting Request",
        originalSender: "sender@example.com"
    })
});
const draft = await response.json();
```

### 4. Sending Reply
```javascript
const response = await fetch('/api/agents/email/reply', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${accessToken}`
    },
    body: JSON.stringify({
        originalMessageId: "message-id",
        body: "Thank you for your email. I'll review this and get back to you soon."
    })
});
```

## AI Prompt Engineering

The AI drafting follows the Quill Learning pattern with this template:

```
You are an assistant tasked with drafting professional email responses.
Below is the original email message: {originalBody}
Here is my intended reply or reaction: {userInput}

Instructions:
- Reference the main points from the original email (for context)
- Convey my intended reply or sentiment in a polished and business-appropriate way
- Use a {tone} tone
- Optionally, suggest next steps or ask a clarifying question if appropriate
- Format the reply as a full email, including a greeting and closing
- Do NOT include a subject line in your reply
- Do NOT include placeholders for name, position, or contact information
- End with a simple closing such as 'Best regards' or 'Thank you' only
```

## Personalization Integration

The email agent integrates with the user's personalization profile to ensure consistent voice:

### Profile Elements Applied:
- **Tone**: Professional, friendly, formal, etc.
- **Preferred Greetings**: "Hello", "Hi", "Good morning", etc.
- **Signature Closings**: "Best regards", "Thank you", "Sincerely", etc.
- **Favorite Phrases**: Common expressions the user prefers
- **Prohibited Words**: Words to avoid in communications

### Example Personalization:
```json
{
  "tone": "professional",
  "preferredGreetings": ["Hello", "Good morning"],
  "signatureClosings": ["Best regards", "Thank you"],
  "favouritePhrases": ["I appreciate your time"],
  "prohibitedWords": ["urgent", "ASAP"]
}
```

## Security Considerations

### Token Management
- **Access Tokens**: Stored in memory only, never persisted locally
- **Token Validation**: Regular validation against Microsoft Graph
- **Expiration Handling**: Automatic token refresh when possible

### Data Protection
- **No Local Storage**: Email content never persisted locally
- **HTTPS Enforcement**: All communications over secure channels
- **Minimal Permissions**: Only request necessary Graph API permissions

### Error Handling
- **Authentication Errors**: Clear messaging for token issues
- **API Rate Limits**: Respectful request patterns
- **Network Failures**: Graceful degradation and retry logic

## Frontend Integration

A sample HTML interface (`email-helper.html`) is provided that demonstrates:
- Microsoft Graph authentication flow
- Email list display with read/unread status
- Email detail view with sender, date, and content
- AI draft generation interface
- Reply composition and sending
- Email deletion functionality

## Testing

### Manual Testing Steps:
1. **Authentication**: Verify login flow and token handling
2. **Email Retrieval**: Test inbox loading and email details
3. **AI Drafting**: Generate drafts with various user intents
4. **Reply Functionality**: Send replies and reply-all messages
5. **Error Handling**: Test with invalid tokens and network issues

### Integration Testing:
- Verify personalization profile application
- Test with different email formats (HTML/text)
- Validate proper error responses for unauthorized access
- Confirm email threading and reply attribution

## Deployment Considerations

### Azure AD Setup:
1. Register application in Azure AD
2. Configure redirect URIs for your domain
3. Grant admin consent for Graph API permissions
4. Configure application secrets

### Backend Configuration:
1. Update authentication middleware
2. Configure Graph API client settings
3. Set up environment variables
4. Test token validation endpoints

### Frontend Deployment:
1. Configure MSAL.js with your client ID
2. Set correct redirect URIs
3. Test authentication flow
4. Verify API endpoint connectivity

## Known Limitations

1. **Token Management**: Frontend must handle token refresh
2. **Attachment Support**: File attachments not yet implemented
3. **Email Formatting**: Limited rich text formatting options
4. **Calendar Integration**: Meeting responses require additional permissions
5. **Shared Mailboxes**: Only supports user's primary mailbox

## Future Enhancements

1. **File Attachments**: Support for adding/viewing email attachments
2. **Rich Text Editor**: Integration with TinyMCE or similar editor
3. **Email Templates**: Pre-defined email templates for common scenarios
4. **Scheduling**: Integration with calendar for meeting responses
5. **Email Filtering**: Advanced search and filtering capabilities
6. **Offline Support**: Basic functionality when network is unavailable

---

This enhancement brings enterprise-grade email management capabilities to the Prodigy platform while maintaining security, personalization, and user experience standards established by the Quill Learning Email Helper Application.
