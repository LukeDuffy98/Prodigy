import { PublicClientApplication } from '@azure/msal-browser';
import type { Configuration } from '@azure/msal-browser';

// MSAL configuration for Authorization Code Flow with PKCE
export const msalConfig: Configuration = {
  auth: {
    clientId: 'fd3955ab-6652-4536-934e-4284c39cc503', // Your NEW clean SPA-only Azure AD app registration client ID
    authority: 'https://login.microsoftonline.com/6a450216-13ff-486c-8de5-6bbef7b20ae2', // Your tenant ID
    redirectUri: window.location.origin, // Use dynamic origin to match exactly
    postLogoutRedirectUri: window.location.origin,
    navigateToLoginRequestUrl: false, // Important for SPAs
    supportsNestedAppAuth: false, // Disable nested app authentication
    clientCapabilities: ['CP1'], // Declare support for Conditional Access
  },
  cache: {
    cacheLocation: 'sessionStorage', // Use sessionStorage to avoid cross-origin issues
    storeAuthStateInCookie: true, // Enable cookie storage for better cross-origin support
    secureCookies: false, // Set to false for localhost (true for HTTPS in production)
  },
  system: {
    allowRedirectInIframe: false, // Prevent iframe redirects
    loggerOptions: {
      loggerCallback: (level, message, containsPii) => {
        if (containsPii) {
          return;
        }
        switch (level) {
          case 0: // LogLevel.Error
            console.error('[MSAL Error]', message);
            return;
          case 1: // LogLevel.Warning
            console.warn('[MSAL Warning]', message);
            return;
          case 2: // LogLevel.Info
            console.info('[MSAL Info]', message);
            return;
          case 3: // LogLevel.Verbose
            console.debug('[MSAL Debug]', message);
            return;
          default:
            return;
        }
      },
      piiLoggingEnabled: false, // Disable PII logging for security
      logLevel: 3, // Set to verbose for debugging
    },
  },
};

// Add scopes here for ID token to be used at Microsoft identity platform endpoints.
export const loginRequest = {
  scopes: [
    'User.Read',
    'Mail.ReadWrite', 
    'Mail.Send',
  ],
  redirectUri: window.location.origin, // Dynamic redirect to match current origin
  prompt: 'consent', // Force consent to ensure fresh tokens
  extraQueryParameters: {
    response_mode: 'fragment', // Use fragment for SPA
  },
};

// Add the endpoints here for Microsoft Graph API services you'd like to use.
export const graphConfig = {
  graphMeEndpoint: 'https://graph.microsoft.com/v1.0/me',
  graphMailEndpoint: 'https://graph.microsoft.com/v1.0/me/messages',
};

// Create the main instance of MSAL
export const msalInstance = new PublicClientApplication(msalConfig);

// Initialize MSAL - Using NEW clean SPA-only app registration
msalInstance.initialize().then(() => {
  console.log('[MSAL] Initialization successful with NEW clean SPA app');
  console.log('[MSAL] Configuration check:', {
    redirectUri: msalConfig.auth.redirectUri,
    currentOrigin: window.location.origin,
    currentUrl: window.location.href,
    clientId: msalConfig.auth.clientId, // New clean SPA-only client ID
    authority: msalConfig.auth.authority
  });
  
  // Handle redirect promise - this is crucial for redirect flow
  msalInstance.handleRedirectPromise().then((response) => {
    if (response) {
      console.log('[MSAL] Login successful via redirect with clean SPA app', {
        account: response.account?.username,
        scopes: response.scopes,
        // Don't log the actual token for security
        hasAccessToken: !!response.accessToken
      });
      // The useMsalAuth hook will pick up the account from getAllAccounts()
    } else {
      console.log('[MSAL] No redirect response detected - checking for existing accounts');
      const accounts = msalInstance.getAllAccounts();
      console.log('[MSAL] Existing accounts found:', accounts.length);
    }
  }).catch((error) => {
    console.error('[MSAL] Redirect login error with clean SPA app', {
      errorCode: error.errorCode,
      errorMessage: error.errorMessage,
      subError: error.subError,
      correlationId: error.correlationId,
      timestamp: error.timestamp
    });
    
    // Check if this is specifically the cross-origin error
    if (error.errorCode === 'invalid_request' && error.errorMessage?.includes('9002326')) {
      console.error('[MSAL] Cross-origin token redemption error STILL detected with clean SPA app!');
      console.error('[MSAL] This should NOT happen with a pure SPA app registration');
    } else {
      console.log('[MSAL] Different error - this is expected for a new clean app registration');
    }
  });
}).catch((error) => {
  console.error('[MSAL] Initialization error', error);
});
