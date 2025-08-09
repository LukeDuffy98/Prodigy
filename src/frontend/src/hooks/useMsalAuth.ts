import { useState, useEffect } from 'react';
import type { AccountInfo } from '@azure/msal-browser';
import { msalInstance, loginRequest } from '../config/msalConfig';

interface AuthState {
  isAuthenticated: boolean;
  account: AccountInfo | null;
  accessToken: string | null;
  isLoading: boolean;
  error: string | null;
}

export const useMsalAuth = () => {
  const [authState, setAuthState] = useState<AuthState>({
    isAuthenticated: false,
    account: null,
    accessToken: null,
    isLoading: true,
    error: null,
  });

  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const accounts = msalInstance.getAllAccounts();
        if (accounts.length > 0) {
          // User is already logged in
          const account = accounts[0];
          await acquireTokenSilent(account);
        } else {
          setAuthState(prev => ({ ...prev, isLoading: false }));
        }
      } catch (error) {
        console.error('Auth initialization error:', error);
        setAuthState(prev => ({ 
          ...prev, 
          isLoading: false, 
          error: 'Failed to initialize authentication' 
        }));
      }
    };

    initializeAuth();
  }, []);

  const acquireTokenSilent = async (account: AccountInfo) => {
    try {
      const response = await msalInstance.acquireTokenSilent({
        ...loginRequest,
        account: account,
      });

      setAuthState({
        isAuthenticated: true,
        account: account,
        accessToken: response.accessToken,
        isLoading: false,
        error: null,
      });

      return response.accessToken;
    } catch (error) {
      console.error('Silent token acquisition failed:', error);
      // If silent token acquisition fails, we'll need to login interactively
      setAuthState(prev => ({ 
        ...prev, 
        isLoading: false,
        error: 'Session expired. Please login again.' 
      }));
      return null;
    }
  };

  const login = async () => {
    try {
      setAuthState(prev => ({ ...prev, isLoading: true, error: null }));
      
      // Clear any potentially problematic cached state first
      try {
        localStorage.removeItem('msal.interaction.status');
        localStorage.removeItem('msal.request.state');
        sessionStorage.clear(); // Clear sessionStorage since we're using it now
        msalInstance.clearCache();
      } catch (clearError) {
        console.warn('Failed to clear cache:', clearError);
      }
      
      // Use redirect flow instead of popup to avoid cross-origin issues
      // Redirect flow is more reliable for SPAs and doesn't have origin header issues
      try {
        // Check if we're already in a redirect flow
        const accounts = msalInstance.getAllAccounts();
        if (accounts.length === 0) {
          // No accounts, initiate login redirect
          console.log('Initiating login redirect...');
          await msalInstance.loginRedirect({
            ...loginRequest,
            redirectUri: window.location.origin, // Use current origin
          });
          return null; // Redirect will handle the response
        } else {
          // We have accounts, try to get token silently
          const account = accounts[0];
          const response = await msalInstance.acquireTokenSilent({
            ...loginRequest,
            account: account,
          });
          
          setAuthState({
            isAuthenticated: true,
            account: account,
            accessToken: response.accessToken,
            isLoading: false,
            error: null,
          });
          
          return response.accessToken;
        }
      } catch (silentError) {
        console.warn('Silent token acquisition failed, trying redirect...', silentError);
        await msalInstance.loginRedirect({
          ...loginRequest,
          redirectUri: window.location.origin,
        });
        return null;
      }
    } catch (error: unknown) {
      console.error('Login error:', error);
      let errorMessage = 'Login failed. Please try again.';
      
      if (error && typeof error === 'object' && 'errorCode' in error) {
        const msalError = error as { errorCode: string };
        if (msalError.errorCode === 'popup_window_error') {
          errorMessage = 'Popup was blocked. Please allow popups and try again.';
        } else if (msalError.errorCode === 'user_cancelled') {
          errorMessage = 'Login was cancelled by user.';
        } else if (msalError.errorCode === 'invalid_request') {
          errorMessage = 'Authentication configuration issue. Please contact support.';
        }
      }
      
      setAuthState(prev => ({ 
        ...prev, 
        isLoading: false, 
        error: errorMessage 
      }));
      
      throw error;
    }
  };

  const logout = async () => {
    try {
      await msalInstance.logoutPopup({
        postLogoutRedirectUri: window.location.origin,
      });
      
      setAuthState({
        isAuthenticated: false,
        account: null,
        accessToken: null,
        isLoading: false,
        error: null,
      });
    } catch (error) {
      console.error('Logout error:', error);
      // Even if logout fails, clear the local state
      setAuthState({
        isAuthenticated: false,
        account: null,
        accessToken: null,
        isLoading: false,
        error: null,
      });
    }
  };

  const getAccessToken = async (): Promise<string | null> => {
    if (!authState.account) {
      return null;
    }

    try {
      const response = await msalInstance.acquireTokenSilent({
        ...loginRequest,
        account: authState.account,
      });
      
      // Update the stored token
      setAuthState(prev => ({ 
        ...prev, 
        accessToken: response.accessToken 
      }));
      
      return response.accessToken;
    } catch (error) {
      console.error('Token acquisition failed:', error);
      return null;
    }
  };

  return {
    ...authState,
    login,
    logout,
    getAccessToken,
  };
};
