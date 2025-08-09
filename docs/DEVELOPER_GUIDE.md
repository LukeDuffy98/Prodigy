# 🛠️ Developer Guide

This guide provides comprehensive instructions for setting up, developing, and contributing to the Prodigy project.

## 📋 Table of Contents

- [Prerequisites](#prerequisites)
- [Initial Setup](#initial-setup)
- [Development Environment](#development-environment)
- [Project Structure](#project-structure)
- [Development Workflow](#development-workflow)
- [Testing](#testing)
- [Debugging](#debugging)
- [Code Style](#code-style)
- [Contributing](#contributing)
- [Troubleshooting](#troubleshooting)

## 🔧 Prerequisites

### Required Software
- **.NET 8 SDK** - Download from [Microsoft .NET](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Node.js 20+** - Download from [Node.js](https://nodejs.org/)
- **Git** - Download from [Git SCM](https://git-scm.com/)

### Optional Tools
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **Postman** or similar API testing tool
- **Azure CLI** (for Azure deployments)

### Verify Installation
```bash
# Check .NET version
dotnet --version  # Should show 8.x.x

# Check Node.js version
node --version    # Should show 20.x.x or higher

# Check npm version
npm --version     # Should show 10.x.x or higher
```

## 🚀 Initial Setup

### 1. Clone the Repository
```bash
git clone https://github.com/LukeDuffy98/Prodigy.git
cd Prodigy
```

### 2. Backend Setup
```bash
# Navigate to repository root
cd /path/to/Prodigy

# Restore .NET dependencies (~20 seconds)
dotnet restore

# Build the solution (~8 seconds)
dotnet build --no-restore
```

**Expected Output:**
- `dotnet restore`: "Restored 2 projects"
- `dotnet build`: "Build succeeded" with potential async warnings (these are expected)

### 3. Frontend Setup
```bash
# Navigate to frontend directory
cd src/frontend

# Install dependencies (~24 seconds, can take up to 5 minutes on slower connections)
npm install

# Build frontend (~4 seconds)
npm run build

# Run linting to verify code quality
npm run lint
```

**Expected Output:**
- `npm install`: "added 214 packages" with 0 vulnerabilities
- `npm run build`: Successful Vite build with bundle information
- `npm run lint`: ESLint completes with no errors

### 4. Environment Configuration
```bash
# Copy environment template
cp .env.example .env

# Edit .env file with your configuration
# See Environment Setup guide for details
```

## 🏃‍♂️ Development Environment

### Starting the Development Servers

**Terminal 1 - Backend API:**
```bash
cd src/backend
dotnet run
```
- Backend runs on: http://localhost:5169
- Swagger UI available at: http://localhost:5169
- Health check: http://localhost:5169/health

**Terminal 2 - Frontend Development:**
```bash
cd src/frontend
npm run dev
```
- Frontend runs on: http://localhost:5173
- Hot module replacement enabled for instant updates

### Quick Validation
```bash
# Test backend health
curl http://localhost:5169/health

# Test API endpoint
curl http://localhost:5169/api/user/personalization-profile

# Visit frontend
open http://localhost:5173
```

## 📁 Project Structure

```
Prodigy/
├── .github/
│   └── workflows/              # CI/CD GitHub Actions
├── src/
│   ├── backend/                # ASP.NET Core Web API (.NET 8)
│   │   ├── Controllers/        # API controllers for each agent
│   │   │   ├── EmailAgentController.cs
│   │   │   ├── TaskAgentController.cs
│   │   │   ├── LearningAgentController.cs
│   │   │   ├── QuoteAgentController.cs
│   │   │   ├── CalendarAgentController.cs
│   │   │   ├── GitHubAgentController.cs
│   │   │   ├── PersonalizationController.cs
│   │   │   └── AuthController.cs
│   │   ├── Models/             # DTOs and data models
│   │   │   ├── EmailModels.cs
│   │   │   ├── PersonalizationProfile.cs
│   │   │   └── [Other model files]
│   │   ├── Services/           # Business logic services
│   │   │   ├── GraphEmailService.cs
│   │   │   ├── GraphUserService.cs
│   │   │   └── [Other services]
│   │   ├── Properties/         # Launch settings
│   │   ├── Program.cs          # Application startup
│   │   ├── appsettings.json    # Configuration
│   │   └── Prodigy.Backend.csproj
│   └── frontend/               # React TypeScript with Vite
│       ├── src/
│       │   ├── components/     # React components
│       │   │   ├── Dashboard.tsx
│       │   │   ├── AgentPanel.tsx
│       │   │   ├── PersonalizationSettings.tsx
│       │   │   └── [Other components]
│       │   ├── hooks/          # Custom React hooks
│       │   ├── config/         # Configuration files
│       │   ├── assets/         # Static assets
│       │   ├── App.tsx         # Main application component
│       │   └── main.tsx        # Application entry point
│       ├── public/             # Public static files
│       ├── package.json        # Frontend dependencies
│       ├── vite.config.ts      # Vite configuration
│       ├── tsconfig.json       # TypeScript configuration
│       └── eslint.config.js    # ESLint configuration
├── azure-functions/            # Azure Functions project (placeholder)
├── docs/                       # Documentation
├── .env.example                # Environment template
├── Prodigy.sln                 # Visual Studio solution
└── README.md                   # Project overview
```

## 💻 Development Workflow

### Daily Development Process

1. **Start Development Session:**
   ```bash
   # Pull latest changes
   git pull origin main
   
   # Start backend (Terminal 1)
   cd src/backend && dotnet run
   
   # Start frontend (Terminal 2)
   cd src/frontend && npm run dev
   ```

2. **Making Changes:**
   - Backend changes are automatically compiled by `dotnet run`
   - Frontend changes trigger hot reload automatically
   - API changes can be tested via Swagger UI at http://localhost:5169

3. **Before Committing:**
   ```bash
   # Lint frontend code
   cd src/frontend && npm run lint
   
   # Build to verify no errors
   dotnet build
   cd src/frontend && npm run build
   ```

### Adding New Features

#### Backend Development
1. **Create Controller:**
   ```csharp
   [ApiController]
   [Route("api/agents/[controller]")]
   public class NewAgentController : ControllerBase
   {
       // Implementation
   }
   ```

2. **Add Models:**
   ```csharp
   public class NewAgentRequest
   {
       // Properties with XML documentation
   }
   ```

3. **Register Services:**
   ```csharp
   // In Program.cs
   builder.Services.AddScoped<INewAgentService, NewAgentService>();
   ```

#### Frontend Development
1. **Create Component:**
   ```typescript
   import React from 'react';
   
   const NewComponent: React.FC = () => {
       return <div>New Component</div>;
   };
   
   export default NewComponent;
   ```

2. **Add to Navigation:**
   ```typescript
   // Update App.tsx or relevant parent component
   ```

3. **API Integration:**
   ```typescript
   // Use hooks for API calls
   import { useState, useEffect } from 'react';
   ```

### API Development Guidelines

1. **Controller Standards:**
   - Use `[ApiController]` attribute
   - Include XML documentation comments
   - Follow RESTful conventions
   - Return appropriate HTTP status codes

2. **Model Standards:**
   - Use `PersonalizationProfile` for AI-generated content
   - Include validation attributes
   - Provide example values in documentation

3. **Error Handling:**
   ```csharp
   try
   {
       // Implementation
   }
   catch (Exception ex)
   {
       return StatusCode(500, new { error = ex.Message });
   }
   ```

## 🧪 Testing

### Backend Testing
Currently, no test projects exist. Testing is done through:
- **Swagger UI**: Interactive API testing at http://localhost:5169
- **Health Check**: Verify API is running at http://localhost:5169/health
- **Manual Testing**: Use Postman or curl for endpoint testing

### Frontend Testing
- **ESLint**: `npm run lint` for code quality
- **Build Verification**: `npm run build` ensures no compilation errors
- **Manual Testing**: Browser testing at http://localhost:5173

### API Testing Examples
```bash
# Health check
curl http://localhost:5169/health

# Get personalization profile
curl http://localhost:5169/api/user/personalization-profile

# Test email agent (requires authentication)
curl -X POST http://localhost:5169/api/agents/email/send \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{"recipients":["test@example.com"],"subject":"Test","body":"Hello"}'
```

## 🐛 Debugging

### Backend Debugging
- **Visual Studio**: Set breakpoints and use F5 to debug
- **VS Code**: Use C# extension for debugging support
- **Console Logging**: Add `Console.WriteLine()` for quick debugging
- **Swagger UI**: Test API endpoints interactively

### Frontend Debugging
- **Browser DevTools**: Use React Developer Tools extension
- **Console Logging**: Use `console.log()` for debugging
- **Hot Reload**: Changes appear instantly during development
- **Network Tab**: Monitor API calls and responses

### Common Issues
1. **Port Conflicts**: Backend uses 5169, frontend uses 5173
2. **CORS Issues**: Check CORS configuration in Program.cs
3. **Build Errors**: Run `dotnet clean` then `dotnet build`
4. **npm Issues**: Delete `node_modules` and run `npm install`

## 📝 Code Style

### Backend (.NET)
- Follow standard C# conventions
- Use XML documentation comments
- Include `using` statements at the top
- Use async/await for I/O operations
- Handle exceptions appropriately

### Frontend (TypeScript/React)
- Use TypeScript strict mode
- Follow React hooks patterns
- Use functional components
- ESLint configuration enforces style
- Use meaningful component and variable names

### Git Commit Messages
```
type(scope): description

Examples:
feat(email): add email draft generation
fix(auth): resolve JWT token validation
docs(api): update endpoint documentation
style(frontend): fix linting issues
```

## 🤝 Contributing

### Pull Request Process
1. **Fork Repository**: Create your own fork
2. **Create Branch**: `git checkout -b feature/your-feature`
3. **Make Changes**: Follow development workflow
4. **Test Changes**: Ensure build and lint pass
5. **Commit**: Use conventional commit messages
6. **Push**: `git push origin feature/your-feature`
7. **Create PR**: Submit pull request with description

### Code Review Checklist
- [ ] Code builds successfully
- [ ] Frontend linting passes
- [ ] API endpoints documented
- [ ] Error handling implemented
- [ ] Security considerations addressed
- [ ] Performance impact considered

## 🔧 Troubleshooting

### Build Issues
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build

# Frontend issues
cd src/frontend
rm -rf node_modules package-lock.json
npm install
```

### Runtime Issues
```bash
# Check process running on ports
netstat -an | grep :5169  # Backend
netstat -an | grep :5173  # Frontend

# Kill processes if needed
lsof -ti:5169 | xargs kill -9
lsof -ti:5173 | xargs kill -9
```

### Environment Issues
1. Verify `.env` file configuration
2. Check environment variable loading in Program.cs
3. Ensure Azure AD credentials are correct
4. Verify external API access

### Getting Help
1. **Documentation**: Check `/docs` directory
2. **Issues**: Search GitHub issues for similar problems
3. **Debugging**: Use logging and breakpoints
4. **Community**: Ask questions in GitHub discussions

---

For more specific guidance, see:
- [API Reference](API_REFERENCE.md) - Complete API documentation
- [Environment Setup](ENVIRONMENT_SETUP.md) - Configuration details
- [Architecture Guide](ARCHITECTURE.md) - System design overview