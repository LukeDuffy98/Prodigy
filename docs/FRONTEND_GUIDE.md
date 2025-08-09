# ⚛️ Frontend Guide

This guide provides comprehensive information about the Prodigy React frontend, including component architecture, development patterns, and best practices.

## 📋 Table of Contents

- [Overview](#overview)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Component Architecture](#component-architecture)
- [State Management](#state-management)
- [API Integration](#api-integration)
- [Styling and UI](#styling-and-ui)
- [Development Workflow](#development-workflow)
- [Testing](#testing)
- [Performance](#performance)
- [Deployment](#deployment)

## 🌐 Overview

The Prodigy frontend is a modern React application built with TypeScript and Vite. It provides an intelligent digital workspace interface that connects users with AI-powered agents for productivity enhancement.

### Key Features
- **Responsive Design**: Mobile-first approach with desktop optimization
- **Agent Interface**: Dedicated interfaces for each intelligent agent
- **Real-time Updates**: Dynamic UI updates based on agent interactions
- **Personalization**: User-customizable interface and agent behavior
- **Authentication**: Secure Azure AD integration with MSAL

### Architecture Philosophy
- **Component-Based**: Modular, reusable React components
- **Type Safety**: Full TypeScript implementation
- **Performance First**: Optimized builds and lazy loading
- **Accessibility**: WCAG 2.1 compliance for inclusive design

## 🔧 Technology Stack

### Core Technologies
```json
{
  "framework": "React 19.1.1",
  "language": "TypeScript 5.8.3",
  "buildTool": "Vite 7.1.0",
  "authentication": "@azure/msal-react 3.0.17",
  "httpClient": "axios 1.11.0",
  "styling": "CSS3 with CSS Modules",
  "linting": "ESLint 9.32.0"
}
```

### Development Dependencies
```json
{
  "@types/react": "19.1.9",
  "@types/react-dom": "19.1.7",
  "@vitejs/plugin-react": "4.7.0",
  "eslint-plugin-react-hooks": "5.2.0",
  "typescript-eslint": "8.39.0"
}
```

### Build Configuration
- **Vite**: Lightning-fast development server with HMR
- **TypeScript**: Strict mode enabled for enhanced type safety
- **ESLint**: Code quality and consistency enforcement
- **Tree Shaking**: Automatic dead code elimination

## 📁 Project Structure

```
src/frontend/
├── public/                     # Static assets
│   ├── index.html             # HTML template
│   ├── favicon.ico            # Application icon
│   └── manifest.json          # PWA manifest
├── src/
│   ├── components/            # React components
│   │   ├── common/            # Shared components
│   │   ├── agents/            # Agent-specific components
│   │   ├── auth/              # Authentication components
│   │   └── layout/            # Layout components
│   ├── hooks/                 # Custom React hooks
│   │   ├── useAuth.ts         # Authentication logic
│   │   ├── useApi.ts          # API communication
│   │   └── usePersonalization.ts # Profile management
│   ├── config/                # Configuration files
│   │   ├── apiConfig.ts       # API endpoints
│   │   ├── msalConfig.ts      # MSAL configuration
│   │   └── constants.ts       # Application constants
│   ├── types/                 # TypeScript type definitions
│   │   ├── api.ts             # API response types
│   │   ├── agents.ts          # Agent-specific types
│   │   └── auth.ts            # Authentication types
│   ├── utils/                 # Utility functions
│   │   ├── apiHelpers.ts      # API utility functions
│   │   ├── dateHelpers.ts     # Date manipulation
│   │   └── validators.ts      # Input validation
│   ├── assets/                # Static assets
│   │   ├── images/            # Image files
│   │   └── styles/            # Global styles
│   ├── App.tsx                # Main application component
│   ├── main.tsx               # Application entry point
│   └── vite-env.d.ts          # Vite type definitions
├── package.json               # Dependencies and scripts
├── tsconfig.json              # TypeScript configuration
├── vite.config.ts             # Vite configuration
└── eslint.config.js           # ESLint configuration
```

## 🧩 Component Architecture

### Component Hierarchy
```
App
├── AuthProvider (MSAL wrapper)
├── Layout
│   ├── Header
│   │   ├── Navigation
│   │   └── UserProfile
│   ├── MainContent
│   │   ├── Dashboard
│   │   │   ├── AgentCard (×6)
│   │   │   └── RecentActivity
│   │   ├── AgentPanel
│   │   │   ├── EmailAgent
│   │   │   ├── TaskAgent
│   │   │   ├── LearningAgent
│   │   │   ├── QuoteAgent
│   │   │   ├── CalendarAgent
│   │   │   └── GitHubAgent
│   │   └── PersonalizationSettings
│   └── Footer
└── ErrorBoundary
```

### Main Application Component
```typescript
// App.tsx
import React, { useState } from 'react';
import { AuthenticatedTemplate, UnauthenticatedTemplate } from '@azure/msal-react';
import './App.css';
import Dashboard from './components/Dashboard';
import AgentPanel from './components/AgentPanel';
import PersonalizationSettings from './components/PersonalizationSettings';
import Login from './components/auth/Login';

type ViewType = 'dashboard' | 'agents' | 'personalization';

function App() {
  const [activeView, setActiveView] = useState<ViewType>('dashboard');

  return (
    <div className="App">
      <AuthenticatedTemplate>
        <header className="App-header">
          <nav className="navigation">
            <div className="nav-brand">
              <h1>🤖 Prodigy</h1>
              <p>Your Intelligent Digital Workspace</p>
            </div>
            <div className="nav-links">
              <button 
                className={activeView === 'dashboard' ? 'active' : ''}
                onClick={() => setActiveView('dashboard')}
              >
                Dashboard
              </button>
              <button 
                className={activeView === 'agents' ? 'active' : ''}
                onClick={() => setActiveView('agents')}
              >
                Agents
              </button>
              <button 
                className={activeView === 'personalization' ? 'active' : ''}
                onClick={() => setActiveView('personalization')}
              >
                Personalization
              </button>
            </div>
          </nav>
        </header>

        <main className="main-content">
          {activeView === 'dashboard' && <Dashboard />}
          {activeView === 'agents' && <AgentPanel />}
          {activeView === 'personalization' && <PersonalizationSettings />}
        </main>

        <footer className="App-footer">
          <p>Prodigy - Digital agents at your command | Built with ❤️ for productivity</p>
        </footer>
      </AuthenticatedTemplate>
      
      <UnauthenticatedTemplate>
        <Login />
      </UnauthenticatedTemplate>
    </div>
  );
}

export default App;
```

### Component Design Patterns

#### Functional Components with Hooks
```typescript
interface EmailAgentProps {
  onEmailSent?: (result: EmailResult) => void;
}

const EmailAgent: React.FC<EmailAgentProps> = ({ onEmailSent }) => {
  const [formData, setFormData] = useState({
    recipients: '',
    subject: '',
    body: ''
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const { sendEmail } = useEmailApi();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const result = await sendEmail({
        recipients: formData.recipients.split(',').map(r => r.trim()),
        subject: formData.subject,
        body: formData.body
      });
      
      onEmailSent?.(result);
      setFormData({ recipients: '', subject: '', body: '' });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to send email');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="email-agent">
      <h2>📧 Email Agent</h2>
      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label htmlFor="recipients">Recipients:</label>
          <input
            id="recipients"
            type="email"
            value={formData.recipients}
            onChange={(e) => setFormData(prev => ({ ...prev, recipients: e.target.value }))}
            placeholder="user@example.com, another@example.com"
            required
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="subject">Subject:</label>
          <input
            id="subject"
            type="text"
            value={formData.subject}
            onChange={(e) => setFormData(prev => ({ ...prev, subject: e.target.value }))}
            placeholder="Email subject"
            required
          />
        </div>
        
        <div className="form-group">
          <label htmlFor="body">Message:</label>
          <textarea
            id="body"
            value={formData.body}
            onChange={(e) => setFormData(prev => ({ ...prev, body: e.target.value }))}
            placeholder="Your message here..."
            rows={6}
            required
          />
        </div>
        
        {error && <div className="error-message">{error}</div>}
        
        <button type="submit" disabled={loading}>
          {loading ? 'Sending...' : 'Send Email'}
        </button>
      </form>
    </div>
  );
};

export default EmailAgent;
```

#### Custom Hooks Pattern
```typescript
// hooks/useEmailApi.ts
import { useState } from 'react';
import axios from 'axios';
import { useAuth } from './useAuth';

interface SendEmailRequest {
  recipients: string[];
  subject: string;
  body: string;
}

interface EmailResult {
  success: boolean;
  messageId?: string;
  message: string;
}

export const useEmailApi = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { getAccessToken } = useAuth();

  const sendEmail = async (request: SendEmailRequest): Promise<EmailResult> => {
    setLoading(true);
    setError(null);

    try {
      const token = await getAccessToken();
      const response = await axios.post('/api/agents/email/send', request, {
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      });

      return response.data;
    } catch (err) {
      const errorMessage = axios.isAxiosError(err) 
        ? err.response?.data?.error || err.message
        : 'Unknown error occurred';
      
      setError(errorMessage);
      throw new Error(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  return {
    sendEmail,
    loading,
    error,
    clearError: () => setError(null)
  };
};
```

## 🔄 State Management

### Local State with useState
```typescript
// Simple component state
const [formData, setFormData] = useState({
  field1: '',
  field2: ''
});

// Complex state updates
const updateFormField = (field: string, value: string) => {
  setFormData(prev => ({
    ...prev,
    [field]: value
  }));
};
```

### Shared State Patterns
```typescript
// Context for app-wide state
interface AppContextType {
  user: User | null;
  theme: 'light' | 'dark';
  setTheme: (theme: 'light' | 'dark') => void;
}

const AppContext = React.createContext<AppContextType | undefined>(undefined);

export const AppProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<User | null>(null);
  const [theme, setTheme] = useState<'light' | 'dark'>('light');

  return (
    <AppContext.Provider value={{ user, theme, setTheme }}>
      {children}
    </AppContext.Provider>
  );
};

export const useApp = () => {
  const context = useContext(AppContext);
  if (!context) {
    throw new Error('useApp must be used within AppProvider');
  }
  return context;
};
```

## 🔌 API Integration

### API Configuration
```typescript
// config/apiConfig.ts
export const API_CONFIG = {
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5169',
  endpoints: {
    health: '/health',
    personalization: '/api/user/personalization-profile',
    emailAgent: '/api/agents/email',
    taskAgent: '/api/agents/tasks',
    learningAgent: '/api/agents/learning',
    quoteAgent: '/api/agents/quotes',
    calendarAgent: '/api/agents/calendar',
    githubAgent: '/api/agents/github'
  },
  timeout: 30000
};
```

### Axios Configuration
```typescript
// utils/apiHelpers.ts
import axios from 'axios';
import { API_CONFIG } from '../config/apiConfig';

const apiClient = axios.create({
  baseURL: API_CONFIG.baseURL,
  timeout: API_CONFIG.timeout,
  headers: {
    'Content-Type': 'application/json'
  }
});

// Request interceptor for auth token
apiClient.interceptors.request.use(
  async (config) => {
    const token = await getAccessToken(); // From MSAL
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Handle authentication error
      redirectToLogin();
    }
    return Promise.reject(error);
  }
);

export default apiClient;
```

### API Hook Pattern
```typescript
// Generic API hook
export const useApi = <T>(endpoint: string) => {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchData = async () => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await apiClient.get<T>(endpoint);
      setData(response.data);
    } catch (err) {
      const errorMessage = axios.isAxiosError(err) 
        ? err.response?.data?.error || err.message
        : 'Unknown error';
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, [endpoint]);

  return {
    data,
    loading,
    error,
    refetch: fetchData,
    clearError: () => setError(null)
  };
};
```

## 🎨 Styling and UI

### CSS Architecture
```css
/* Global styles */
:root {
  --primary-color: #007acc;
  --secondary-color: #f1f1f1;
  --text-color: #333;
  --background-color: #fff;
  --border-radius: 8px;
  --shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  --transition: all 0.3s ease;
}

/* Dark theme support */
[data-theme="dark"] {
  --text-color: #fff;
  --background-color: #1a1a1a;
  --secondary-color: #2d2d2d;
}

/* Component-specific styles */
.agent-card {
  background: var(--background-color);
  border-radius: var(--border-radius);
  box-shadow: var(--shadow);
  padding: 1.5rem;
  transition: var(--transition);
}

.agent-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(0, 0, 0, 0.15);
}
```

### Responsive Design
```css
/* Mobile-first responsive design */
.dashboard-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
  padding: 1rem;
}

/* Tablet */
@media (min-width: 768px) {
  .dashboard-grid {
    grid-template-columns: repeat(2, 1fr);
    padding: 1.5rem;
  }
}

/* Desktop */
@media (min-width: 1024px) {
  .dashboard-grid {
    grid-template-columns: repeat(3, 1fr);
    padding: 2rem;
  }
}

/* Large desktop */
@media (min-width: 1440px) {
  .dashboard-grid {
    grid-template-columns: repeat(4, 1fr);
    max-width: 1400px;
    margin: 0 auto;
  }
}
```

### Component Styling Pattern
```css
/* EmailAgent.module.css */
.emailAgent {
  background: var(--background-color);
  border-radius: var(--border-radius);
  padding: 2rem;
  max-width: 600px;
  margin: 0 auto;
}

.formGroup {
  margin-bottom: 1.5rem;
}

.formGroup label {
  display: block;
  margin-bottom: 0.5rem;
  font-weight: 600;
  color: var(--text-color);
}

.formGroup input,
.formGroup textarea {
  width: 100%;
  padding: 0.75rem;
  border: 1px solid var(--secondary-color);
  border-radius: var(--border-radius);
  font-size: 1rem;
  transition: var(--transition);
}

.formGroup input:focus,
.formGroup textarea:focus {
  outline: none;
  border-color: var(--primary-color);
  box-shadow: 0 0 0 2px rgba(0, 122, 204, 0.2);
}

.submitButton {
  background: var(--primary-color);
  color: white;
  border: none;
  padding: 0.75rem 1.5rem;
  border-radius: var(--border-radius);
  font-size: 1rem;
  cursor: pointer;
  transition: var(--transition);
}

.submitButton:hover:not(:disabled) {
  background: #0056a3;
  transform: translateY(-1px);
}

.submitButton:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.errorMessage {
  background: #fee;
  border: 1px solid #fcc;
  border-radius: var(--border-radius);
  padding: 0.75rem;
  margin: 1rem 0;
  color: #c33;
}
```

## 🛠️ Development Workflow

### Development Server
```bash
# Start development server with hot reload
npm run dev

# Server runs on http://localhost:5173
# Automatically opens browser
# Hot module replacement for instant updates
```

### Build Process
```bash
# Type checking
npm run type-check

# Linting
npm run lint

# Production build
npm run build

# Preview production build
npm run preview
```

### Environment Variables
```bash
# .env.local (for local development)
VITE_API_BASE_URL=http://localhost:5169
VITE_AZURE_CLIENT_ID=your-client-id
VITE_AZURE_TENANT_ID=your-tenant-id
VITE_AZURE_REDIRECT_URI=http://localhost:5173/auth/callback
```

### Development Best Practices

#### Code Organization
```typescript
// Group related imports
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

// Separate third-party imports
import axios from 'axios';

// Local imports last
import { useAuth } from '../hooks/useAuth';
import { EmailResult } from '../types/api';
import styles from './EmailAgent.module.css';
```

#### Error Boundaries
```typescript
class ErrorBoundary extends React.Component<
  { children: React.ReactNode },
  { hasError: boolean; error?: Error }
> {
  constructor(props: { children: React.ReactNode }) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: Error) {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error('Error caught by boundary:', error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="error-boundary">
          <h2>Something went wrong</h2>
          <p>Please refresh the page or contact support if the problem persists.</p>
        </div>
      );
    }

    return this.props.children;
  }
}
```

## 🧪 Testing

### Testing Strategy
- **Unit Tests**: Component logic and utility functions
- **Integration Tests**: Component interactions and API calls
- **Manual Testing**: User workflows and browser compatibility
- **Accessibility Testing**: WCAG compliance verification

### Manual Testing Checklist
```markdown
## Frontend Testing Checklist

### Navigation
- [ ] All navigation links work correctly
- [ ] Active state highlighting works
- [ ] Mobile navigation (if applicable) functions

### Dashboard
- [ ] Agent cards display correctly
- [ ] Card hover effects work
- [ ] Recent activity shows (when available)

### Email Agent
- [ ] Form validation works
- [ ] Email sending functions
- [ ] Error handling displays properly
- [ ] Success messages appear

### Personalization
- [ ] Profile settings save correctly
- [ ] Preview functionality works
- [ ] Changes apply to other agents

### Responsive Design
- [ ] Mobile layout (320px-768px)
- [ ] Tablet layout (768px-1024px)
- [ ] Desktop layout (1024px+)

### Browser Compatibility
- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Safari (latest)
- [ ] Edge (latest)
```

## ⚡ Performance

### Performance Optimization

#### Bundle Optimization
```typescript
// vite.config.ts
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom'],
          msal: ['@azure/msal-browser', '@azure/msal-react'],
          utils: ['axios']
        }
      }
    },
    chunkSizeWarningLimit: 1000
  }
});
```

#### Code Splitting
```typescript
// Lazy loading for route components
const Dashboard = React.lazy(() => import('./components/Dashboard'));
const AgentPanel = React.lazy(() => import('./components/AgentPanel'));

// Usage with Suspense
<Suspense fallback={<div>Loading...</div>}>
  <Dashboard />
</Suspense>
```

#### Performance Monitoring
```typescript
// Performance measurement
const measurePerformance = (name: string, fn: () => void) => {
  const start = performance.now();
  fn();
  const end = performance.now();
  console.log(`${name} took ${end - start} milliseconds`);
};

// Usage
measurePerformance('Dashboard render', () => {
  // Component rendering logic
});
```

## 🚀 Deployment

### Build for Production
```bash
# Clean build
rm -rf dist
npm run build

# Verify build output
ls -la dist/
```

### Static File Serving
```nginx
# Nginx configuration for React SPA
server {
    listen 80;
    server_name yourdomain.com;
    root /var/www/prodigy-frontend/dist;
    index index.html;

    # Handle client-side routing
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache static assets
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
        expires 1y;
        add_header Cache-Control "public, immutable";
    }

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header Referrer-Policy "strict-origin-when-cross-origin" always;
}
```

### Environment Configuration
```bash
# Production environment variables
VITE_API_BASE_URL=https://api.prodigy.company.com
VITE_AZURE_CLIENT_ID=production-client-id
VITE_AZURE_TENANT_ID=production-tenant-id
VITE_AZURE_REDIRECT_URI=https://prodigy.company.com/auth/callback
```

---

## 📚 Related Documentation

- [Developer Guide](DEVELOPER_GUIDE.md) - Setup and development workflow
- [API Reference](API_REFERENCE.md) - Backend API documentation
- [User Guide](USER_GUIDE.md) - End-user documentation
- [Contributing Guide](CONTRIBUTING.md) - Contribution guidelines

---

*This frontend guide provides comprehensive information for developing and maintaining the Prodigy React application. Keep it updated as the application evolves.*