# 🤖 Prodigy - Intelligent Digital Workspace

**Prodigy** is your AI-powered digital workspace that streamlines daily productivity through intelligent agents. Built with .NET 8 and React, Prodigy integrates with Microsoft 365, LinkedIn, and GitHub to automate communication, task management, learning, and collaboration workflows.

## 🚀 Quick Start

```bash
# Clone and setup
git clone https://github.com/LukeDuffy98/Prodigy.git
cd Prodigy

# Backend setup
dotnet restore
dotnet build

# Frontend setup  
cd src/frontend
npm install
npm run build

# Run the application
# Terminal 1 - Backend API
cd src/backend && dotnet run

# Terminal 2 - Frontend Dev Server
cd src/frontend && npm run dev
```

Visit http://localhost:5173 to access the Prodigy workspace.

## 📋 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Quick Start](#-quick-start)
- [Documentation](#-documentation)
- [Development](#-development)
- [Deployment](#-deployment)
- [Contributing](#-contributing)
- [License](#-license)

## ✨ Features

### Intelligent Agents
- **📧 Email Agent** - AI-powered email composition and replies with Microsoft Graph integration
- **📝 Task Agent** - Smart task creation with automated execution planning
- **📚 Learning Agent** - Dynamic learning material generation and knowledge management
- **💰 Quote Agent** - Professional quote creation and client communication
- **📅 Calendar Agent** - Intelligent availability lookup and meeting optimization
- **🐙 GitHub Agent** - Feature request management and GitHub integration

### Core Capabilities
- **🎨 Personalization** - AI agents adapt to your writing style and preferences
- **🔐 Enterprise Security** - JWT + Azure AD authentication with secure API access
- **📱 Mobile-First** - Responsive design for desktop and mobile productivity
- **🔌 API-First** - RESTful API with comprehensive Swagger documentation
- **⚡ Real-Time** - Fast, modern React frontend with hot reload development

## 🏗️ Architecture

### Technology Stack
- **Backend**: ASP.NET Core 8 Web API with JWT authentication
- **Frontend**: React 19 + TypeScript with Vite build system
- **Cloud**: Azure Functions for business logic (future enhancement)
- **Integrations**: Microsoft Graph, LinkedIn API, GitHub REST API
- **Database**: Configuration-based (environment dependent)

### System Design
```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────────┐
│   React Frontend │────│  ASP.NET Core    │────│ External APIs       │
│   (Port 5173)    │    │  Backend API     │    │ • Microsoft Graph   │
│                 │    │  (Port 5169)     │    │ • LinkedIn          │
│                 │    │                  │    │ • GitHub            │
└─────────────────┘    └──────────────────┘    └─────────────────────┘
         │                       │                        │
         │                       │                        │
         ▼                       ▼                        ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────────┐
│   User Interface │    │  Agent Services  │    │ Azure Functions     │
│   • Dashboard    │    │  • Email         │    │ (Future)            │
│   • Agent Panel  │    │  • Tasks         │    │                     │
│   • Settings     │    │  • Learning      │    │                     │
└─────────────────┘    └──────────────────┘    └─────────────────────┘
```

## 📚 Documentation

### User Documentation
- [User Guide](docs/USER_GUIDE.md) - Complete guide to using Prodigy's features
- [Agent Overview](docs/AGENT_OVERVIEW.md) - Detailed information about each agent

### Developer Documentation
- [Developer Guide](docs/DEVELOPER_GUIDE.md) - Setup, development, and contribution guide
- [API Reference](docs/API_REFERENCE.md) - Complete REST API documentation
- [Architecture Guide](docs/ARCHITECTURE.md) - Technical system overview
- [Frontend Guide](docs/FRONTEND_GUIDE.md) - React component and development guide

### Deployment & Operations
- [Deployment Guide](docs/DEPLOYMENT.md) - Production deployment instructions
- [Environment Setup](docs/ENVIRONMENT_SETUP.md) - Configuration and secrets management
- [Azure AD Configuration](docs/AZURE_AD_CONFIG.md) - Authentication setup guide

### API-Specific Documentation
- [Email Agent API](docs/API_EMAIL_AGENT.md) - Email operations and Microsoft Graph integration
- [GitHub Feature Requests](docs/GITHUB_FEATURE_REQUEST_API.md) - GitHub integration details
- [Personalization Profile](docs/PRODIGY_PERSONALIZATION_PROFILE.md) - AI personalization system

## 🛠️ Development

### Prerequisites
- .NET 8 SDK
- Node.js 20+
- Git

### Development Workflow
```bash
# Setup development environment
dotnet restore
cd src/frontend && npm install

# Run in development mode
cd src/backend && dotnet run  # API on http://localhost:5169
cd src/frontend && npm run dev  # Frontend on http://localhost:5173

# Build for production
dotnet build --configuration Release
cd src/frontend && npm run build

# Run tests and linting
cd src/frontend && npm run lint
```

### Project Structure
```
Prodigy/
├── src/
│   ├── backend/                # ASP.NET Core Web API
│   │   ├── Controllers/        # API controllers for each agent
│   │   ├── Models/            # DTOs and data models
│   │   ├── Services/          # Business logic services
│   │   └── Program.cs         # API startup and configuration
│   └── frontend/              # React TypeScript application
│       ├── src/               # React components and logic
│       ├── public/            # Static assets
│       └── package.json       # Frontend dependencies
├── azure-functions/            # Azure Functions project (future)
├── docs/                      # Comprehensive documentation
├── .github/workflows/         # CI/CD pipeline configuration
└── Prodigy.sln               # Visual Studio solution
```

### Key Development Resources
- **API Documentation**: Swagger UI available at http://localhost:5169 when running backend
- **Health Check**: http://localhost:5169/health
- **Frontend Hot Reload**: Automatic refresh during development
- **ESLint Configuration**: Consistent code style enforcement

## 🚀 Deployment

### Local Development
The application runs locally with:
- Backend API: http://localhost:5169
- Frontend: http://localhost:5173
- Swagger Documentation: http://localhost:5169/swagger

### Azure Production Deployment 🌥️

**One-Click Azure Deployment:**
```bash
# Set up environment variables
cp .env.prod.example .env
# Edit .env with your Azure credentials

# Deploy to Azure
./scripts/deploy-azure.sh
```

**What gets deployed:**
- 🔧 **App Service** - Backend API (.NET 8)
- 📱 **Static Web App** - Frontend (React)
- ⚡ **Azure Functions** - Business logic
- 🔐 **Key Vault** - Secure secret storage
- 📊 **Application Insights** - Monitoring

**Alternative deployment methods:**
- **GitHub Actions**: Automated CI/CD on push to main
- **Docker**: `docker-compose up --build` for containerized deployment
- **Manual**: Step-by-step Azure CLI commands

### Documentation
- 📖 [Azure Deployment Guide](docs/AZURE_DEPLOYMENT.md) - Complete Azure setup
- 🏗️ [General Deployment Guide](docs/DEPLOYMENT.md) - All deployment options
- 🔧 [Azure Files Reference](azure/README.md) - Infrastructure files overview

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](docs/CONTRIBUTING.md) for:
- Development setup
- Coding standards
- Pull request process
- Issue reporting guidelines

### Quick Contribution Steps
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 🛡️ Security

- JWT-based authentication with Azure AD integration
- Secure API endpoints with proper authorization
- Environment-based configuration management
- No secrets in source code
- CORS configuration for frontend integration

## 📧 Support

- **Documentation**: Check our comprehensive [docs](docs/) directory
- **Issues**: Report bugs and feature requests via GitHub Issues
- **API Help**: Explore the interactive Swagger documentation
- **Development**: See the [Developer Guide](docs/DEVELOPER_GUIDE.md)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🎯 Vision

**Prodigy** acts as your digital command center, staffed by intelligent agents that understand your work style and preferences. By automating repetitive tasks and integrating multiple services into one unified platform, Prodigy empowers you to focus on what matters most - your core work and creativity.

> **Welcome to Prodigy! Digital agents at your command.** 🚀

---

*Built with ❤️ for productivity and powered by AI for intelligence.*