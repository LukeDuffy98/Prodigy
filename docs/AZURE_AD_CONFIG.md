# Azure AD Configuration Note

## Current Issue
The Azure AD app registration is configured as a "Web" application, but MSAL.js requires it to be configured as a "Single-Page Application" (SPA) to avoid CORS errors.

## Error Details
```
AADSTS9002326: Cross-origin token redemption is permitted only for the 'Single-Page Application' client-type.
```

## How to Fix (Optional)
If you want to enable the Microsoft popup authentication:

1. Go to Azure Portal → App registrations → GraphEmailReader
2. Navigate to Authentication
3. Change the application type from "Web" to "Single-Page Application"
4. Ensure redirect URIs include:
   - `http://localhost:5173`
   - `http://localhost:5174` 
   - `http://localhost:5175`
   - `http://localhost:3000`

## Current Solution
For now, we're using the **username/password authentication** which works perfectly without any Azure AD configuration changes. This provides the same functionality with a better user experience.

## Status
✅ **Working**: Username/Password authentication  
⚠️ **Disabled**: Microsoft popup (due to Azure AD config)

The email agent is fully functional using the credential-based authentication method.
