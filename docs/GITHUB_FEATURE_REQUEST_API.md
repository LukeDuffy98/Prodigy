# GitHub Feature Request Management API

## Overview

The Prodigy GitHub Agent provides comprehensive feature request management functionality, allowing users to create, view, update, and close GitHub issues directly from the application.

## Architecture

The implementation follows the established Prodigy architecture pattern:

```
Frontend (React) → Backend API (ASP.NET Core) → Azure Functions → GitHub REST API
```

### Components

1. **Frontend Component**: `GitHubAgent.tsx` - React component for user interface
2. **Backend Controller**: `GitHubAgentController.cs` - API endpoints for GitHub operations
3. **Azure Functions**: `GitHubFunctions.cs` - Business logic and GitHub API integration
4. **Models**: `FeatureRequest` and `FeatureRequestResponse` - Data transfer objects

## API Endpoints

### Create Feature Request
- **POST** `/api/agents/github/feature-request`
- Creates a new GitHub issue
- **Request Body**:
  ```json
  {
    "title": "Feature title",
    "description": "Detailed description",
    "labels": ["enhancement", "feature"],
    "assignedTo": "github-copilot",
    "priority": "Medium",
    "milestone": "v1.0.0"
  }
  ```
- **Response**: `FeatureRequestResponse` object with issue details

### Get Feature Request
- **GET** `/api/agents/github/feature-request/{issueNumber}`
- Retrieves a specific GitHub issue
- **Response**: `FeatureRequestResponse` object

### Update Feature Request
- **PUT** `/api/agents/github/feature-request/{issueNumber}`
- Updates an existing GitHub issue
- **Request Body**: Same as create request
- **Response**: Updated `FeatureRequestResponse` object

### List Feature Requests
- **GET** `/api/agents/github/feature-requests`
- Lists GitHub issues with optional filtering
- **Query Parameters**:
  - `state`: "open", "closed", or "all" (default: "open")
  - `assignee`: Filter by assigned user
  - `labels`: Comma-separated list of labels
- **Response**: Array of `FeatureRequestResponse` objects

### Close Feature Request
- **POST** `/api/agents/github/feature-request/{issueNumber}/close`
- Closes a GitHub issue
- **Response**: Closure confirmation object

## Configuration

### Required Environment Variables

#### Backend (appsettings.json or environment)
```json
{
  "AZURE_FUNCTIONS_URL": "http://localhost:7071/api/",
  "AZURE_FUNCTIONS_KEY": "your-functions-key"
}
```

#### Azure Functions (local.settings.json)
```json
{
  "Values": {
    "GITHUB_TOKEN": "your-github-personal-access-token",
    "GITHUB_REPO_OWNER": "LukeDuffy98",
    "GITHUB_REPO_NAME": "Prodigy"
  }
}
```

### GitHub Token Setup

1. Go to GitHub Settings → Developer settings → Personal access tokens
2. Generate a new token with the following scopes:
   - `repo` (for private repositories) or `public_repo` (for public repositories)
   - `issues` (to create and manage issues)
3. Add the token to your Azure Functions configuration

## Authentication

The system supports multiple authentication methods:

1. **Environment Variable**: Set `GITHUB_TOKEN` in Azure Functions
2. **Authorization Header**: Pass `Authorization: Bearer <token>` in requests
3. **Function Key**: Use Azure Functions key for additional security

## Error Handling

The API provides comprehensive error handling:

- **400 Bad Request**: Invalid request data
- **401 Unauthorized**: Missing or invalid GitHub token
- **403 Forbidden**: Insufficient GitHub permissions
- **404 Not Found**: Issue not found
- **500 Internal Server Error**: Unexpected errors

Error responses include descriptive messages to help with troubleshooting.

## Frontend Usage

The React component provides a complete interface for:

1. **Creating Feature Requests**: Form with all necessary fields
2. **Viewing Requests**: List of existing issues with filtering
3. **Managing Requests**: View on GitHub, close issues
4. **Real-time Feedback**: Success/error messages for all operations

### Component Features

- Form validation
- Loading states
- Error handling with user-friendly messages
- Responsive design
- Auto-refresh capability

## Testing

### Manual Testing Steps

1. **Setup**:
   - Configure GitHub token in Azure Functions
   - Start Azure Functions: `func start` in azure-functions directory
   - Start Backend API: `dotnet run` in src/backend directory
   - Start Frontend: `npm run dev` in src/frontend directory

2. **Test Create Feature Request**:
   - Fill out the form in the GitHub Agent interface
   - Submit and verify issue creation on GitHub
   - Check that response includes correct issue number and URL

3. **Test List Feature Requests**:
   - Use the refresh button to load existing issues
   - Verify filtering options work correctly

4. **Test Close Feature Request**:
   - Use the close button on an open issue
   - Verify issue state changes to "closed" on GitHub

### Error Scenarios to Test

1. **Invalid GitHub Token**: Should show authentication error
2. **Network Issues**: Should show appropriate error messages
3. **Invalid Repository**: Should handle repository not found
4. **Rate Limiting**: Should handle GitHub API rate limits gracefully

## Development Notes

### Code Organization

- **Minimal Changes**: Implementation reuses existing models and follows established patterns
- **Separation of Concerns**: Clear separation between UI, API, and business logic
- **Error Handling**: Comprehensive error handling at all layers
- **Documentation**: Extensive XML documentation for API discoverability

### Future Enhancements

1. **Comment Management**: Add/view comments on issues
2. **Label Management**: Create and manage repository labels
3. **Milestone Management**: Create and assign milestones
4. **Batch Operations**: Bulk operations on multiple issues
5. **Webhooks**: Real-time updates from GitHub
6. **Templates**: Issue templates for consistent feature requests

## Troubleshooting

### Common Issues

1. **"GitHub token is required"**:
   - Verify GITHUB_TOKEN is set in Azure Functions configuration
   - Ensure token has required permissions

2. **"Azure Function call failed"**:
   - Verify Azure Functions are running on correct port (7071)
   - Check Azure Functions URL in backend configuration

3. **"Feature request not found"**:
   - Verify issue number exists in the configured repository
   - Check repository owner/name configuration

4. **CORS issues**:
   - Ensure frontend URL is whitelisted in backend CORS policy
   - Verify API endpoints are accessible from frontend

### Debugging

1. Check browser console for frontend errors
2. Check backend logs for API errors
3. Check Azure Functions logs for GitHub API errors
4. Use Swagger UI for API testing: `http://localhost:5000/swagger`

## Security Considerations

1. **Token Security**: Never expose GitHub tokens in frontend code
2. **Function Keys**: Use Azure Functions keys in production
3. **CORS Policy**: Restrict CORS to trusted domains in production
4. **Rate Limiting**: Implement rate limiting to prevent abuse
5. **Input Validation**: All inputs are validated before processing