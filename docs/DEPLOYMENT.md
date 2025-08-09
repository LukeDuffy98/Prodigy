# 🚀 Deployment Guide

This guide provides comprehensive instructions for deploying Prodigy to various environments, from development to production.

## 📋 Table of Contents

- [Overview](#overview)
- [Environment Requirements](#environment-requirements)
- [Local Development](#local-development)
- [Staging Deployment](#staging-deployment)
- [Production Deployment](#production-deployment)
- [Azure Deployment](#azure-deployment)
- [Docker Deployment](#docker-deployment)
- [CI/CD Pipeline](#cicd-pipeline)
- [Environment Configuration](#environment-configuration)
- [Monitoring & Maintenance](#monitoring--maintenance)
- [Troubleshooting](#troubleshooting)

## 🌐 Overview

Prodigy supports multiple deployment strategies:
- **Development**: Local development environment
- **Staging**: Pre-production testing environment
- **Production**: Live production environment
- **Cloud**: Azure App Service and Container deployments
- **Containerized**: Docker-based deployments

### Deployment Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Development   │    │     Staging     │    │   Production    │
│   (localhost)   │    │  (staging.url)  │    │ (production.url)│
├─────────────────┤    ├─────────────────┤    ├─────────────────┤
│ • Hot reload    │    │ • Production    │    │ • Load balanced │
│ • Local data    │    │   build         │    │ • SSL/HTTPS     │
│ • Debug mode    │    │ • Test data     │    │ • Monitoring    │
│ • No SSL       │    │ • SSL enabled   │    │ • Backup        │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## 🔧 Environment Requirements

### System Requirements
- **Operating System**: Windows 10+, macOS 11+, or Linux (Ubuntu 20.04+)
- **Memory**: Minimum 4GB RAM (8GB recommended)
- **Storage**: 2GB free space for development, 10GB for production
- **Network**: Stable internet connection for external API access

### Software Dependencies
- **.NET 8 SDK**: Latest stable version
- **Node.js**: Version 20.x or higher
- **Git**: Latest version for source control
- **Web Server** (Production): IIS, Nginx, or Apache

### External Services
- **Azure AD**: For authentication
- **Microsoft Graph**: For email and calendar integration
- **GitHub API**: For repository integration
- **LinkedIn API**: For professional networking (optional)

## 💻 Local Development

### Quick Setup
```bash
# Clone repository
git clone https://github.com/LukeDuffy98/Prodigy.git
cd Prodigy

# Backend setup
dotnet restore
dotnet build

# Frontend setup
cd src/frontend
npm install
npm run build
cd ../..

# Environment configuration
cp .env.example .env
# Edit .env with your configuration
```

### Running Development Servers

**Terminal 1 - Backend:**
```bash
cd src/backend
dotnet run
# Available at: http://localhost:5169
# Swagger UI: http://localhost:5169/swagger
```

**Terminal 2 - Frontend:**
```bash
cd src/frontend
npm run dev
# Available at: http://localhost:5173
```

### Development Environment Variables
Create `.env` file in project root:
```bash
# Azure AD Configuration
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret

# JWT Configuration
JWT_SECRET_KEY=your-development-secret-key
JWT_ISSUER=Prodigy
JWT_AUDIENCE=ProdigyUsers

# External APIs
GITHUB_TOKEN=your-github-token
LINKEDIN_CLIENT_ID=your-linkedin-client-id
LINKEDIN_CLIENT_SECRET=your-linkedin-client-secret

# Development Settings
ASPNETCORE_ENVIRONMENT=Development
NODE_ENV=development
```

## 🧪 Staging Deployment

### Staging Environment Setup
Staging should mirror production as closely as possible while allowing for testing.

#### Infrastructure Requirements
- **Web Server**: IIS, Nginx, or cloud equivalent
- **SSL Certificate**: Valid SSL certificate for HTTPS
- **Database**: Staging database (separate from production)
- **DNS**: Staging subdomain (e.g., staging.prodigy.company.com)

#### Deployment Steps
```bash
# 1. Build production assets
cd src/frontend
npm run build

# 2. Publish backend
cd ../backend
dotnet publish -c Release -o ./publish

# 3. Deploy to staging server
# Copy published files to staging server
# Configure environment variables
# Start services
```

#### Staging Configuration
```bash
# Staging Environment Variables
ASPNETCORE_ENVIRONMENT=Staging
NODE_ENV=production

# Use staging-specific Azure AD app
AZURE_CLIENT_ID=staging-client-id
AZURE_CLIENT_SECRET=staging-client-secret

# Staging-specific URLs
API_BASE_URL=https://staging-api.prodigy.company.com
FRONTEND_URL=https://staging.prodigy.company.com

# Enable detailed logging for debugging
ASPNETCORE_LOGGING__LOGLEVEL__DEFAULT=Information
```

## 🌍 Production Deployment

### Production Architecture
```
Internet → Load Balancer → Web Servers → Backend API → External APIs
                       → Static Assets    → Database
```

### Pre-deployment Checklist
- [ ] SSL certificates configured and valid
- [ ] Environment variables configured securely
- [ ] Database backups tested and verified
- [ ] Performance testing completed
- [ ] Security scanning passed
- [ ] Monitoring and alerting configured
- [ ] Rollback plan documented and tested

### Production Build Process

#### Backend Production Build
```bash
# Clean and restore
dotnet clean
dotnet restore

# Build in Release mode
dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o ./dist/backend \
  --self-contained false \
  --runtime linux-x64
```

#### Frontend Production Build
```bash
# Install dependencies
npm ci

# Build for production
npm run build

# Optional: Serve static files with optimized server
npm run preview
```

### Production Configuration

#### Web Server Configuration (Nginx)
```nginx
server {
    listen 443 ssl http2;
    server_name prodigy.company.com;
    
    ssl_certificate /path/to/certificate.crt;
    ssl_certificate_key /path/to/private.key;
    
    # Frontend static files
    location / {
        root /var/www/prodigy/frontend;
        try_files $uri $uri/ /index.html;
        
        # Cache static assets
        location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg)$ {
            expires 1y;
            add_header Cache-Control "public, immutable";
        }
    }
    
    # Backend API
    location /api/ {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}

# Redirect HTTP to HTTPS
server {
    listen 80;
    server_name prodigy.company.com;
    return 301 https://$server_name$request_uri;
}
```

#### Production Environment Variables
```bash
# Production Environment
ASPNETCORE_ENVIRONMENT=Production
NODE_ENV=production

# Security
ASPNETCORE_URLS=http://localhost:5000
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true

# Database
DATABASE_CONNECTION_STRING="your-production-db-connection"

# External APIs (production credentials)
AZURE_TENANT_ID=prod-tenant-id
AZURE_CLIENT_ID=prod-client-id
AZURE_CLIENT_SECRET=prod-client-secret

# JWT (strong production key)
JWT_SECRET_KEY=your-strong-production-secret-key

# Monitoring
APPLICATION_INSIGHTS_INSTRUMENTATION_KEY=your-ai-key
```

## ☁️ Azure Deployment

### Azure App Service Deployment

#### Prerequisites
- Azure subscription
- Azure CLI installed
- Resource group created

#### Deployment Steps
```bash
# 1. Login to Azure
az login

# 2. Create App Service Plan
az appservice plan create \
  --name prodigy-plan \
  --resource-group prodigy-rg \
  --sku B2 \
  --is-linux

# 3. Create Web App for Backend
az webapp create \
  --resource-group prodigy-rg \
  --plan prodigy-plan \
  --name prodigy-api \
  --runtime "DOTNETCORE|8.0"

# 4. Create Web App for Frontend  
az webapp create \
  --resource-group prodigy-rg \
  --plan prodigy-plan \
  --name prodigy-frontend \
  --runtime "NODE|20-lts"

# 5. Configure App Settings
az webapp config appsettings set \
  --resource-group prodigy-rg \
  --name prodigy-api \
  --settings @appsettings.json
```

#### Azure Configuration Files

**appsettings.json** for Azure App Service:
```json
[
  {
    "name": "ASPNETCORE_ENVIRONMENT",
    "value": "Production"
  },
  {
    "name": "AZURE_TENANT_ID",
    "value": "@Microsoft.KeyVault(SecretUri=https://vault.vault.azure.net/secrets/tenant-id/)"
  },
  {
    "name": "AZURE_CLIENT_SECRET",
    "value": "@Microsoft.KeyVault(SecretUri=https://vault.vault.azure.net/secrets/client-secret/)"
  }
]
```

### Azure Key Vault Integration
```bash
# Create Key Vault
az keyvault create \
  --name prodigy-keyvault \
  --resource-group prodigy-rg \
  --location eastus

# Add secrets
az keyvault secret set \
  --vault-name prodigy-keyvault \
  --name "TenantId" \
  --value "your-tenant-id"

# Grant App Service access
az webapp identity assign \
  --resource-group prodigy-rg \
  --name prodigy-api
```

## 🐳 Docker Deployment

### Dockerfile for Backend
```dockerfile
# Backend Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/backend/Prodigy.Backend.csproj", "src/backend/"]
RUN dotnet restore "src/backend/Prodigy.Backend.csproj"
COPY . .
WORKDIR "/src/src/backend"
RUN dotnet build "Prodigy.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Prodigy.Backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Prodigy.Backend.dll"]
```

### Dockerfile for Frontend
```dockerfile
# Frontend Dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY src/frontend/package*.json ./
RUN npm ci
COPY src/frontend ./
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Docker Compose
```yaml
version: '3.8'
services:
  prodigy-backend:
    build:
      context: .
      dockerfile: Dockerfile.backend
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - AZURE_TENANT_ID=${AZURE_TENANT_ID}
      - AZURE_CLIENT_ID=${AZURE_CLIENT_ID}
    depends_on:
      - database

  prodigy-frontend:
    build:
      context: .
      dockerfile: Dockerfile.frontend
    ports:
      - "80:80"
    depends_on:
      - prodigy-backend

  database:
    image: postgres:15
    environment:
      POSTGRES_DB: prodigy
      POSTGRES_USER: prodigy
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow
```yaml
name: Deploy Prodigy

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: 20
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build backend
      run: dotnet build --no-restore
      
    - name: Install frontend dependencies
      run: |
        cd src/frontend
        npm ci
        
    - name: Build frontend
      run: |
        cd src/frontend
        npm run build
        
    - name: Run linting
      run: |
        cd src/frontend
        npm run lint

  deploy:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'prodigy-api'
        slot-name: 'production'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: './src/backend'
```

## ⚙️ Environment Configuration

### Configuration Management Strategy
1. **Development**: Local `.env` files
2. **Staging**: Environment variables in hosting platform
3. **Production**: Secure secrets management (Azure Key Vault, etc.)

### Required Environment Variables

#### Core Application Settings
```bash
# Environment
ASPNETCORE_ENVIRONMENT=Production
NODE_ENV=production

# URLs
API_BASE_URL=https://api.prodigy.company.com
FRONTEND_URL=https://prodigy.company.com

# Security
JWT_SECRET_KEY=your-secure-secret-key
JWT_ISSUER=Prodigy
JWT_AUDIENCE=ProdigyUsers
```

#### Azure AD Configuration
```bash
AZURE_TENANT_ID=your-tenant-id
AZURE_CLIENT_ID=your-client-id
AZURE_CLIENT_SECRET=your-client-secret
```

#### External API Keys
```bash
GITHUB_TOKEN=your-github-token
LINKEDIN_CLIENT_ID=your-linkedin-client-id
LINKEDIN_CLIENT_SECRET=your-linkedin-client-secret
```

### Secrets Management Best Practices
- Never commit secrets to version control
- Use secure secret stores (Azure Key Vault, AWS Secrets Manager)
- Rotate secrets regularly
- Use least-privilege access for service accounts
- Monitor secret access and usage

## 📊 Monitoring & Maintenance

### Health Monitoring
```bash
# Health check endpoint
curl https://api.prodigy.company.com/health

# Expected response:
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "version": "1.0.0"
}
```

### Performance Monitoring
- **Response Times**: Monitor API response times
- **Error Rates**: Track error frequency and types
- **Resource Usage**: CPU, memory, and disk utilization
- **External API Calls**: Monitor third-party service performance

### Logging Strategy
```csharp
// Structured logging in production
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddApplicationInsights();
    logging.SetMinimumLevel(LogLevel.Information);
});
```

### Backup and Recovery
- **Database Backups**: Automated daily backups
- **Configuration Backups**: Version-controlled configurations
- **Disaster Recovery**: Documented recovery procedures
- **Testing**: Regular backup restoration testing

## 🔧 Troubleshooting

### Common Deployment Issues

#### Backend Deployment Issues
```bash
# Check application logs
dotnet Prodigy.Backend.dll --environment Production

# Verify configuration
curl https://api.prodigy.company.com/health

# Check SSL certificate
openssl s_client -connect api.prodigy.company.com:443
```

#### Frontend Deployment Issues
```bash
# Check build output
npm run build

# Verify static file serving
curl https://prodigy.company.com/

# Check for JavaScript errors in browser console
```

#### Environment Variable Issues
```bash
# List all environment variables
printenv | grep -E "(AZURE|JWT|GITHUB)"

# Test Azure AD connection
curl -X POST https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token
```

### Performance Issues
- **Slow API Responses**: Check external API latency
- **High Memory Usage**: Review object disposal and garbage collection
- **Database Performance**: Optimize queries and indexing
- **Frontend Loading**: Analyze bundle size and loading performance

### Security Issues
- **Authentication Failures**: Verify Azure AD configuration
- **CORS Errors**: Check origin whitelist configuration
- **SSL Certificate Issues**: Verify certificate validity and chain

### Recovery Procedures

#### Application Recovery
1. **Identify Issue**: Check health endpoint and logs
2. **Quick Fix**: Restart application services
3. **Rollback**: Deploy previous known-good version
4. **Investigation**: Analyze logs and error reports

#### Data Recovery
1. **Assess Impact**: Determine data loss scope
2. **Stop Services**: Prevent further data corruption
3. **Restore Backup**: Use most recent valid backup
4. **Verify Integrity**: Test restored data thoroughly

---

## 📚 Related Documentation

- [Developer Guide](DEVELOPER_GUIDE.md) - Development setup and workflow
- [Environment Setup](ENVIRONMENT_SETUP.md) - Configuration details
- [Architecture Guide](ARCHITECTURE.md) - System design overview
- [API Reference](API_REFERENCE.md) - API documentation

---

*This deployment guide covers standard deployment scenarios. For custom deployments or specific infrastructure requirements, consult with your DevOps team or system administrator.*