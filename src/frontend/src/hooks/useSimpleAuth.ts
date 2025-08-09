import { useState } from 'react';

interface SimpleAuthState {
  isAuthenticated: boolean;
  userEmail: string | null;
  userName: string | null;
  accessToken: string | null;
  isLoading: boolean;
  error: string | null;
}

export const useSimpleAuth = () => {
  const [authState, setAuthState] = useState<SimpleAuthState>({
    isAuthenticated: false,
    userEmail: null,
    userName: null,
    accessToken: null,
    isLoading: false,
    error: null,
  });

  const loginWithCredentials = async (email: string, password: string) => {
    try {
      setAuthState(prev => ({ ...prev, isLoading: true, error: null }));
      
      // Use backend proxy endpoint to avoid CORS issues
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, password }),
      });

      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.error || 'Authentication failed');
      }

      const data = await response.json();
      const { accessToken, user } = data;

      setAuthState({
        isAuthenticated: true,
        userEmail: user.email,
        userName: user.name,
        accessToken: accessToken,
        isLoading: false,
        error: null,
      });

      // Store token for later use
      localStorage.setItem('simple_auth_token', accessToken);
      localStorage.setItem('simple_auth_user', JSON.stringify({
        email: user.email,
        name: user.name,
      }));

      return accessToken;
    } catch (error: unknown) {
      console.error('Simple auth login error:', error);
      let errorMessage = 'Login failed. Please check your credentials.';
      
      if (error instanceof Error) {
        errorMessage = error.message;
      }
      
      setAuthState(prev => ({ 
        ...prev, 
        isLoading: false, 
        error: errorMessage 
      }));
      
      throw error;
    }
  };

  const logout = () => {
    localStorage.removeItem('simple_auth_token');
    localStorage.removeItem('simple_auth_user');
    setAuthState({
      isAuthenticated: false,
      userEmail: null,
      userName: null,
      accessToken: null,
      isLoading: false,
      error: null,
    });
  };

  const checkStoredAuth = () => {
    const token = localStorage.getItem('simple_auth_token');
    const userStr = localStorage.getItem('simple_auth_user');
    
    if (token && userStr) {
      try {
        const user = JSON.parse(userStr);
        setAuthState({
          isAuthenticated: true,
          userEmail: user.email,
          userName: user.name,
          accessToken: token,
          isLoading: false,
          error: null,
        });
        return token;
      } catch (error) {
        console.error('Error parsing stored user:', error);
        logout();
      }
    }
    return null;
  };

  const getAccessToken = () => {
    return authState.accessToken || localStorage.getItem('simple_auth_token');
  };

  return {
    ...authState,
    loginWithCredentials,
    logout,
    checkStoredAuth,
    getAccessToken,
  };
};
