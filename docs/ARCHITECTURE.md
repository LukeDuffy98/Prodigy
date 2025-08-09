# 🏗️ Architecture Guide

This document provides a comprehensive technical overview of the Prodigy system architecture, design decisions, and implementation details.

## 📋 Table of Contents

- [System Overview](#system-overview)
- [Technology Stack](#technology-stack)
- [Application Architecture](#application-architecture)
- [Security Architecture](#security-architecture)
- [Data Flow](#data-flow)
- [API Design](#api-design)
- [Frontend Architecture](#frontend-architecture)
- [Integration Patterns](#integration-patterns)
- [Scalability Considerations](#scalability-considerations)
- [Future Architecture](#future-architecture)

## 🌐 System Overview

Prodigy is a modern, cloud-ready intelligent digital workspace built with a microservices-oriented architecture. The system consists of three primary layers:

1. **Presentation Layer**: React-based frontend with TypeScript
2. **API Layer**: ASP.NET Core Web API backend
3. **Integration Layer**: External service connectors and future Azure Functions

### High-Level Architecture
```
┌─────────────────────────────────────────────────────────────────┐
│                        External Services                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐   │
│  │ Microsoft    │  │   LinkedIn   │  │      GitHub          │   │
│  │ Graph API    │  │     API      │  │    REST API          │   │
│  └──────────────┘  └──────────────┘  └──────────────────────┘   │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Prodigy Backend API                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │   Agent     │  │    Auth     │  │      Services           │  │
│  │ Controllers │  │   System    │  │   (Graph, User, etc.)   │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Prodigy Frontend                            │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │ Dashboard   │  │ Agent Panel │  │   Personalization       │  │
│  │ Component   │  │ Components  │  │     Settings            │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## 🔧 Technology Stack

### Backend (.NET 8)
- **Framework**: ASP.NET Core 8.0
- **Language**: C# with nullable reference types
- **Authentication**: JWT Bearer + Azure AD
- **API Documentation**: Swagger/OpenAPI 3.0
- **HTTP Client**: Built-in HttpClient with dependency injection
- **Configuration**: Environment variables + appsettings.json

### Frontend (React 19)
- **Framework**: React 19 with functional components
- **Language**: TypeScript with strict mode
- **Build System**: Vite 7.x
- **State Management**: React hooks (useState, useEffect)
- **HTTP Client**: Axios
- **Authentication**: Azure MSAL
- **Code Quality**: ESLint + TypeScript compiler

### External Integrations
- **Microsoft Graph**: Email, calendar, and user data
- **LinkedIn API**: Professional networking integration
- **GitHub REST API**: Repository and issue management
- **Azure Functions**: Future business logic implementation

### Development Tools
- **Version Control**: Git with GitHub
- **CI/CD**: GitHub Actions
- **Documentation**: Markdown with cross-references
- **API Testing**: Swagger UI + Postman

## 🏛️ Application Architecture

### Layered Architecture Pattern
The application follows a clean layered architecture:

```
┌─────────────────────────────────────────┐
│              Presentation Layer          │  ← React Components, UI Logic
├─────────────────────────────────────────┤
│                API Layer                 │  ← Controllers, DTOs, Validation
├─────────────────────────────────────────┤
│              Business Layer              │  ← Services, Domain Logic
├─────────────────────────────────────────┤
│             Integration Layer            │  ← External API Clients
└─────────────────────────────────────────┘
```

### Backend Components

#### Controllers (API Layer)
- **Purpose**: Handle HTTP requests and responses
- **Location**: `src/backend/Controllers/`
- **Pattern**: One controller per agent type
- **Responsibilities**:
  - Request validation
  - Authentication/authorization
  - Response formatting
  - Error handling

```csharp
[ApiController]
[Route("api/agents/[controller]")]
public class EmailAgentController : ControllerBase
{
    private readonly IGraphEmailService _emailService;
    
    public EmailAgentController(IGraphEmailService emailService)
    {
        _emailService = emailService;
    }
    
    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
    {
        // Implementation with proper error handling
    }
}
```

#### Models (Data Transfer Objects)
- **Purpose**: Define API contracts and data structures
- **Location**: `src/backend/Models/`
- **Pattern**: Separate models per domain area
- **Features**:
  - Data annotations for validation
  - XML documentation for Swagger
  - Nullable reference types

#### Services (Business Layer)
- **Purpose**: Implement business logic and external integrations
- **Location**: `src/backend/Services/`
- **Pattern**: Interface-based with dependency injection
- **Examples**:
  - `IGraphEmailService`: Microsoft Graph email operations
  - `IGraphUserService`: User data management

### Frontend Components

#### Component Structure
```
src/frontend/src/
├── components/
│   ├── Dashboard.tsx              # Main dashboard view
│   ├── AgentPanel.tsx             # Agent selection and interaction
│   ├── PersonalizationSettings.tsx # User preference management
│   └── [AgentSpecific]/           # Individual agent components
├── hooks/
│   ├── useAuth.ts                 # Authentication logic
│   ├── useApi.ts                  # API communication
│   └── usePersonalization.ts     # Profile management
├── config/
│   └── apiConfig.ts               # API endpoints and configuration
└── assets/
    └── [images, styles, etc.]
```

#### Component Patterns
- **Functional Components**: All components use React hooks
- **TypeScript Interfaces**: Strongly typed props and state
- **Custom Hooks**: Reusable logic extraction
- **Error Boundaries**: Graceful error handling

## 🔐 Security Architecture

### Authentication Flow
```
┌─────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Browser   │    │  Prodigy API    │    │   Azure AD      │
└─────────────┘    └─────────────────┘    └─────────────────┘
       │                    │                       │
       │ 1. Login Request   │                       │
       ├───────────────────►│                       │
       │                    │ 2. Redirect to Azure  │
       │                    ├──────────────────────►│
       │                    │                       │ 3. User Auth
       │◄───────────────────┼───────────────────────┤
       │ 4. Auth Code       │                       │
       ├───────────────────►│                       │
       │                    │ 5. Exchange for Token │
       │                    ├──────────────────────►│
       │                    │ 6. JWT Token          │
       │                    │◄──────────────────────┤
       │ 7. JWT to Client   │                       │
       │◄───────────────────┤                       │
       │                    │                       │
       │ 8. API Calls with JWT                      │
       ├───────────────────►│                       │
```

### Security Features
- **JWT Authentication**: Stateless token-based authentication
- **Azure AD Integration**: Enterprise-grade identity management
- **CORS Configuration**: Restricted origin access
- **HTTPS Enforcement**: Secure communication in production
- **Environment Secrets**: Sensitive data in environment variables

### Authorization Patterns
```csharp
[Authorize(Policy = "RequireAzureAD")]
[HttpGet("profile")]
public async Task<IActionResult> GetProfile()
{
    // Protected endpoint implementation
}
```

## 🔄 Data Flow

### Typical Request Flow
1. **User Interaction**: User interacts with React component
2. **API Call**: Frontend makes HTTP request to backend
3. **Authentication**: JWT token validation
4. **Controller Action**: Request routing to appropriate controller
5. **Service Call**: Controller delegates to service layer
6. **External API**: Service calls Microsoft Graph/LinkedIn/GitHub
7. **Response Chain**: Data flows back through layers
8. **UI Update**: React component updates with new data

### Personalization Flow
```
User Input → PersonalizationProfile → AI Agent → Personalized Output
```

The PersonalizationProfile is injected into all AI-powered operations to ensure consistent user experience.

## 🔌 API Design

### RESTful Principles
- **Resource-Based URLs**: `/api/agents/{agent-type}/{action}`
- **HTTP Verbs**: GET, POST, PUT, DELETE for appropriate actions
- **Status Codes**: Consistent HTTP status code usage
- **Content Negotiation**: JSON for all data exchange

### API Versioning Strategy
- **URL Versioning**: `/api/v1/agents/...` (future implementation)
- **Header Versioning**: `Accept: application/vnd.prodigy.v1+json` (alternative)

### Error Handling Pattern
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Invalid request data",
    "details": ["Field 'title' is required"],
    "timestamp": "2024-01-15T10:30:00Z"
  }
}
```

### Pagination Pattern
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalItems": 100,
    "totalPages": 10
  }
}
```

## ⚛️ Frontend Architecture

### State Management Strategy
- **Local State**: React useState for component-specific data
- **Shared State**: Props drilling for small applications
- **Future**: Context API or Redux for complex state management

### Component Hierarchy
```
App
├── Navigation
├── Dashboard
│   ├── AgentCard (×6)
│   └── RecentActivity
├── AgentPanel
│   ├── EmailAgent
│   ├── TaskAgent
│   ├── LearningAgent
│   ├── QuoteAgent
│   ├── CalendarAgent
│   └── GitHubAgent
└── PersonalizationSettings
    ├── ProfileEditor
    └── PreviewPane
```

### API Integration Pattern
```typescript
const useAgentApi = () => {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  
  const callAgent = async (endpoint: string, data: any) => {
    setLoading(true);
    try {
      const response = await axios.post(endpoint, data);
      return response.data;
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };
  
  return { callAgent, loading, error };
};
```

## 🔗 Integration Patterns

### Microsoft Graph Integration
- **Authentication**: OAuth 2.0 with Azure AD
- **Endpoints**: Mail, Calendar, User profile
- **Error Handling**: Graph-specific error codes
- **Rate Limiting**: Respect Graph API limits

### GitHub Integration
- **Authentication**: Personal Access Token or GitHub App
- **Operations**: Issues, repositories, pull requests
- **Webhooks**: Future implementation for real-time updates

### LinkedIn Integration
- **Authentication**: OAuth 2.0
- **Scope**: Profile data, connections
- **Use Cases**: Professional networking features

## 📈 Scalability Considerations

### Current Architecture Scalability
- **Stateless API**: Enables horizontal scaling
- **Dependency Injection**: Supports service replacement
- **Configuration-Based**: Environment-specific settings
- **External API Abstraction**: Service interfaces for mocking/testing

### Performance Optimization
- **Async/Await**: Non-blocking I/O operations
- **HttpClient Reuse**: Singleton HttpClient instances
- **Response Caching**: Browser-level caching for static data
- **Bundle Optimization**: Vite build optimization

### Monitoring and Observability
- **Health Checks**: `/health` endpoint for system status
- **Logging**: Console logging (future: structured logging)
- **Error Tracking**: Console errors (future: Application Insights)

## 🚀 Future Architecture

### Planned Enhancements

#### Azure Functions Integration
```
Current:  Frontend ↔ API ↔ External Services
Future:   Frontend ↔ API ↔ Azure Functions ↔ External Services
```

#### Microservices Evolution
- **Agent Services**: Separate services per agent type
- **Authentication Service**: Dedicated auth microservice
- **Notification Service**: Real-time updates via SignalR

#### Database Integration
- **User Preferences**: Persistent storage for personalization profiles
- **Activity History**: Task and communication history
- **Caching Layer**: Redis for frequently accessed data

#### Advanced Features
- **Real-time Collaboration**: Multi-user workspaces
- **Advanced Analytics**: Usage patterns and optimization
- **Mobile Applications**: Native iOS/Android apps
- **Enterprise Features**: Multi-tenant architecture

### Migration Strategy
1. **Maintain Compatibility**: Existing API contracts remain stable
2. **Gradual Enhancement**: Feature-by-feature migration
3. **Service Abstraction**: Interface-based service replacement
4. **Configuration-Driven**: Feature flags for new capabilities

---

## 📚 Related Documentation

- [Developer Guide](DEVELOPER_GUIDE.md) - Setup and development workflow
- [API Reference](API_REFERENCE.md) - Complete API documentation
- [Deployment Guide](DEPLOYMENT.md) - Production deployment instructions
- [Environment Setup](ENVIRONMENT_SETUP.md) - Configuration management

---

*This architecture guide reflects the current system design and planned evolution. For the latest updates, refer to the main documentation and development discussions.*