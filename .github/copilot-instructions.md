# Prodigy - Intelligent Digital Workspace

Prodigy is a comprehensive .NET 8 and React TypeScript application featuring AI-powered agents for email, tasks, learning, quotes, calendar management, and GitHub integration. The system consists of an ASP.NET Core Web API backend, React frontend with Vite, and Azure Functions for business logic.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites and Setup
- Install .NET 8 SDK (required for backend API and Azure Functions)
- Install Node.js 20+ (required for React frontend with Vite)
- No additional SDKs or tools required - standard .NET and Node.js toolchain works

### Initial Setup and Build Process
- Bootstrap and build the entire solution:
  - `cd /home/runner/work/Prodigy/Prodigy`
  - `dotnet restore` -- takes ~20 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
  - `dotnet build --no-restore` -- takes ~8 seconds with async warnings (expected). NEVER CANCEL. Set timeout to 180+ seconds.
  - `cd src/frontend && npm install` -- takes ~24 seconds. NEVER CANCEL. Set timeout to 600+ seconds.
  - `cd src/frontend && npm run build` -- takes ~4 seconds. Set timeout to 120+ seconds.
  - `cd src/frontend && npm run lint` -- takes <2 seconds. Set timeout to 60+ seconds.

### Exact Command Sequence for Fresh Clone
```bash
# Navigate to repository root
cd /home/runner/work/Prodigy/Prodigy

# Restore .NET dependencies
dotnet restore
# Expected output: "Restored 2 projects" in ~20 seconds

# Build .NET solution
dotnet build --no-restore
# Expected: Build succeeds with ~29 async warnings (EXPECTED, not errors)

# Install frontend dependencies
cd src/frontend
npm install
# Expected: "added 214 packages" in ~24 seconds

# Build frontend
npm run build
# Expected: Vite build completes successfully in ~4 seconds

# Validate frontend code quality
npm run lint
# Expected: ESLint completes with no errors

# Return to repository root
cd ../..
```

### Running the Applications
- ALWAYS run the setup commands above first before starting applications
- Start the backend API:
  - `cd src/backend && dotnet run`
  - API runs on http://localhost:5169 (or check console output for actual port)
  - Health check available at `/health` endpoint
  - Swagger UI available at root `/` endpoint
- Start the frontend:
  - `cd src/frontend && npm run dev`
  - Frontend runs on http://localhost:5173
  - Hot module replacement enabled for development

### Code Quality and Validation
- Frontend linting: `cd src/frontend && npm run lint` -- fast, no timeout needed
- ALWAYS run `npm run lint` before committing frontend changes or CI will fail
- No test projects exist currently - do not attempt to run `dotnet test` or `npm test`
- Build warnings about async methods are expected and do not prevent successful compilation

## Validation

### Manual Validation Requirements
- ALWAYS manually test the application after making changes:
  1. Start both backend (`dotnet run` in src/backend) and frontend (`npm run dev` in src/frontend)
  2. Navigate to http://localhost:5173 in browser
  3. Verify the Prodigy homepage loads with agent dashboard
  4. Test navigation between Dashboard, Agents, and Personalization tabs
  5. Test at least one API endpoint using curl or browser developer tools
  6. Example API test: `curl http://localhost:5169/health` should return healthy status
  7. Example API test: `curl http://localhost:5169/api/user/personalization-profile` should return profile data

### Key User Scenarios to Test
- **Navigation Flow:** Click Dashboard → Agents → Personalization tabs and verify each loads
- **API Connectivity:** Backend health endpoint responds correctly at `/health`
- **Agent Interface:** Agents page displays all 6 agent types (Email, Task, Learning, Quote, Calendar, GitHub)
- **Email Agent Interface:** Click on Agents tab, verify Email Agent form with To/Subject/Message fields displays
- **Personalization:** Personalization profile API returns default user style preferences with tone, greetings, closings
- **Swagger Documentation:** Navigate to backend root URL to access interactive API documentation
- **Complete User Journey:** Dashboard → Agents → click Email Agent → verify form loads → Dashboard (navigation working)

### Browser Testing Validation
- Homepage displays "Welcome to Prodigy" with 6 agent cards showing 0 counts
- Agent cards show: Email Agent, Task Agent, Learning Agent, Quote Agent, Calendar Agent, GitHub Agent
- Navigation buttons (Dashboard, Agents, Personalization) respond correctly
- Email Agent interface shows input fields for recipients, subject, message with Send Email button
- No console errors in browser developer tools
- UI is responsive and visually consistent

### Build Pipeline Validation
- The CI pipeline (`.github/workflows/copilot-setup-steps.yml`) runs:
  - .NET restore and build
  - TypeScript/React build and lint
- ALWAYS run equivalent commands locally before pushing changes
- Frontend path in CI is `./frontend` but local development uses `src/frontend`

## Common Tasks

### Repository Structure
```
Prodigy/
├── .github/workflows/           # GitHub Actions CI/CD
├── src/
│   ├── backend/                # ASP.NET Core Web API (.NET 8)
│   │   ├── Controllers/        # API controllers for each agent
│   │   ├── Models/            # DTOs and data models
│   │   └── Program.cs         # API startup and configuration
│   └── frontend/              # React TypeScript with Vite
│       ├── src/               # React components and logic
│       ├── package.json       # Frontend dependencies
│       └── vite.config.ts     # Vite build configuration
├── azure-functions/            # Azure Functions project (placeholder)
├── docs/                      # API reference and documentation
├── Prodigy.sln               # Visual Studio solution file
└── .env.example              # Environment configuration template
```

### Key Files and Configurations
- **Backend:** `src/backend/Prodigy.Backend.csproj` - .NET 8 project with JWT auth, Swagger, Microsoft Graph
- **Frontend:** `src/frontend/package.json` - React 19, TypeScript, Vite, ESLint
- **Solution:** `Prodigy.sln` - Contains backend and Azure Functions projects
- **Documentation:** `docs/API_REFERENCE.md` - Complete API documentation
- **Personalization:** `docs/PRODIGY_PERSONALIZATION_PROFILE.md` - User style configuration system

### API Endpoints and Agent Controllers
- Health: `/health` - System status check
- Personalization: `/api/user/personalization-profile` - User style preferences
- Email Agent: `/api/agents/email/*` - Email operations with AI assistance
- Task Agent: `/api/agents/tasks/*` - Task creation with execution plans
- Learning Agent: `/api/agents/learning/*` - Learning material generation
- Quote Agent: `/api/agents/quotes/*` - Professional quote creation
- Calendar Agent: `/api/agents/calendar/*` - Availability and scheduling
- GitHub Agent: `/api/agents/github/*` - Feature request management

### Development Workflow
- Always test changes against both frontend and backend
- Use personalization profile system for all AI-generated content
- Follow the comprehensive API contracts in `docs/API_REFERENCE.md`
- Reference `docs/PRODIGY_PERSONALIZATION_PROFILE.md` for user customization features
- Backend serves Swagger UI at root for interactive API exploration
- Frontend development server includes hot reload for rapid iteration

## Architecture Notes

### Technology Stack
- **Backend:** ASP.NET Core 8, JWT authentication, Swagger/OpenAPI, Microsoft Graph integration
- **Frontend:** React 19, TypeScript, Vite build system, ESLint for code quality
- **External APIs:** Microsoft Graph (email/calendar), LinkedIn API, GitHub REST API
- **Future:** Azure Functions for async business logic (project structure exists)

### Authentication and Security
- JWT-based authentication with configurable secrets
- CORS configured for frontend origin (localhost:3000 in config, localhost:5173 in dev)
- Environment variables for all external API credentials (.env.example provided)
- Swagger UI includes JWT bearer token authentication

### Personalization System
- All AI agents use PersonalizationProfile for user-specific content generation
- Configurable tone, greetings, phrases, and style preferences
- Agent-specific customization hints supported
- Full specification in `docs/PRODIGY_PERSONALIZATION_PROFILE.md`

## Critical Warnings

### Build and Timing Expectations
- **NEVER CANCEL** npm install - can take up to 5 minutes on slower connections, typically 24 seconds
- **NEVER CANCEL** dotnet restore - includes multiple projects and large dependencies, typically 20 seconds  
- **NEVER CANCEL** dotnet build - may take longer on first build due to compilation, typically 8 seconds
- Build warnings about async methods are EXPECTED and do not indicate errors (29 warnings typical)
- Total initial setup time: ~1 minute for fast connections, up to 6 minutes for slower connections
- **Always set timeouts to 120+ seconds minimum for .NET commands, 600+ seconds for npm install**

### Expected Build Output
- `dotnet restore`: Should show "Restored 2 projects" with no errors
- `dotnet build`: Should show "Build succeeded" with 29 async warnings (CS1998) - these are EXPECTED
- `npm install`: Should show "added 214 packages" with 0 vulnerabilities  
- `npm run build`: Should show successful Vite build with bundle sizes
- `npm run lint`: Should complete with no ESLint errors

### Known Limitations and Expected Behavior
- No test projects exist - do not attempt to run `dotnet test` or `npm test`
- Azure Functions project is placeholder - builds but contains minimal implementation
- External API integration requires environment variables (see .env.example for configuration)
- Frontend CI path differs from local development structure (CI uses `./frontend`, local uses `src/frontend`)
- Backend CORS configured for port 3000 but frontend dev server uses 5173 (works correctly)
- Swagger UI serves at root `/` in development, includes full API documentation

### Production Considerations
- Configure HTTPS and update CORS origins for production deployment
- Set environment variables for JWT secrets and external API credentials
- Enable XML documentation generation for enhanced Swagger documentation
- Review and update JWT validation parameters for production security