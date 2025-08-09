# Email Agent Implementation Instructions

## Overview
The Email Agent in Prodigy allows users to send, reply to, and draft emails using the Microsoft Graph API. The email content is personalized using the user's `PersonalizationProfile` for consistent tone and style.

## Steps to Implement Email Sending Functionality

### 1. Fetch PersonalizationProfile
- Retrieve the user's `PersonalizationProfile` to apply their preferred tone, greetings, and closings to the email content.
- Example:
  ```csharp
  var personalizationProfile = new PersonalizationProfile
  {
      Greeting = "Hello",
      Closing = "Best regards",
      Tone = "Professional"
  };
  ```

### 2. Apply Personalization to Email Content
- Use the `PersonalizationProfile` to format the email body.
- Example:
  ```csharp
  var personalizedBody = $"{personalizationProfile.Greeting},\n\n{request.Body}\n\n{personalizationProfile.Closing}";
  ```

### 3. Integrate Microsoft Graph API
- Use the Microsoft Graph SDK to send the email.
- Install the SDK:
  ```pwsh
  dotnet add package Microsoft.Graph
  ```
- Example Code:
  ```csharp
  var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
  {
      requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "YOUR_ACCESS_TOKEN");
  }));

  var message = new Message
  {
      Subject = request.Subject,
      Body = new ItemBody
      {
          ContentType = BodyType.Text,
          Content = personalizedBody
      },
      ToRecipients = request.Recipients.Select(email => new Recipient
      {
          EmailAddress = new EmailAddress { Address = email }
      }).ToList()
  };

  await graphClient.Me.SendMail(message, null).Request().PostAsync();
  ```

### 4. Handle Errors and Logging
- Log the email sending process and handle exceptions.
- Example:
  ```csharp
  try
  {
      // Email sending logic
  }
  catch (ServiceException ex)
  {
      _logger.LogError(ex, "Error sending email via Microsoft Graph API");
      return StatusCode(500, new { error = "An error occurred while sending the email", details = ex.Message });
  }
  ```

### 5. Test the Endpoint
- Use tools like `curl` or Postman to test the `/api/agents/email/send` endpoint.
- Example `curl` command:
  ```pwsh
  curl -X POST http://localhost:5169/api/agents/email/send -H "Content-Type: application/json" -d @email_request.json
  ```
- Sample Request Body:
  ```json
  {
    "recipients": ["example@example.com"],
    "subject": "Test Email",
    "body": "This is a test email sent via the Prodigy Email Agent."
  }
  ```

## Additional Notes
- Replace `YOUR_ACCESS_TOKEN` with a valid Microsoft Graph API access token.
- Ensure the Azure AD app registration has the `Mail.Send` permission.
- Log all email operations for debugging and auditing purposes.

## Future Enhancements
- Implement retry logic for transient errors.
- Add support for attachments in emails.
- Use Azure Functions for offloading email sending logic if needed.
