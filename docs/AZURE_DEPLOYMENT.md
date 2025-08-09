# 🚀 Azure Deployment Implementation Guide

This guide provides step-by-step instructions for deploying Prodigy to Azure using the included infrastructure and deployment files.

## 📋 Quick Start

### Prerequisites
- Azure subscription with appropriate permissions
- Azure CLI installed and logged in
- .NET 8 SDK installed
- Node.js 20+ installed
- Git repository access

### One-Click Deployment

```bash
# Clone the repository
git clone https://github.com/LukeDuffy98/Prodigy.git
cd Prodigy

# Set up environment variables
cp .env.prod.example .env
# Edit .env with your actual values

# Run the deployment script
./scripts/deploy-azure.sh
```

## 🏗️ Infrastructure Components

The Azure deployment creates the following resources:

### Core Services
- **App Service Plan** (B2): Hosts web applications
- **App Service** (Backend): ASP.NET Core Web API
- **Static Web App** (Frontend): React application
- **Azure Functions App**: Business logic functions
- **Key Vault**: Secure secret storage
- **Application Insights**: Monitoring and logging
- **Storage Account**: Azure Functions storage

### Networking & Security
- **HTTPS enforcement** on all services
- **Managed identity** for Key Vault access
- **RBAC permissions** for service access
- **CORS configuration** for API access

## 📁 Deployment Files Overview

```
azure/
├── main.bicep                    # Main infrastructure template
├── main.parameters.json          # Production parameters
└── main.parameters.template.json # Parameter template

.github/workflows/
└── azure-deploy.yml             # CI/CD workflow

scripts/
└── deploy-azure.sh              # Manual deployment script

Docker/
├── Dockerfile.backend           # Backend containerization
├── Dockerfile.frontend          # Frontend containerization
├── Dockerfile.functions         # Functions containerization
├── docker-compose.yml           # Local testing
└── nginx.conf                   # Frontend web server config

Environment/
├── .env.prod.example           # Production environment template
└── .env.staging.example        # Staging environment template
```

## 🔧 Manual Deployment Steps

### Step 1: Prepare Environment

1. **Set up Azure CLI**
   ```bash
   # Login to Azure
   az login
   
   # Set subscription (if multiple)
   az account set --subscription "your-subscription-id"
   ```

2. **Prepare environment variables**
   ```bash
   # Copy template and edit
   cp .env.prod.example .env
   
   # Required variables:
   # - AZURE_TENANT_ID
   # - AZURE_CLIENT_ID  
   # - AZURE_CLIENT_SECRET
   # - JWT_SECRET_KEY
   # - GITHUB_TOKEN
   # - LINKEDIN_CLIENT_ID
   # - LINKEDIN_CLIENT_SECRET
   ```

### Step 2: Deploy Infrastructure

1. **Create resource group**
   ```bash
   az group create \
     --name "rg-prodigy-prod" \
     --location "East US"
   ```

2. **Deploy Bicep template**
   ```bash
   az deployment group create \
     --resource-group "rg-prodigy-prod" \
     --template-file azure/main.bicep \
     --parameters @azure/main.parameters.template.json
   ```

### Step 3: Deploy Applications

1. **Build and deploy backend**
   ```bash
   cd src/backend
   dotnet publish -c Release -o ../../publish-backend
   
   # Create deployment package
   cd ../../publish-backend
   zip -r ../backend-deploy.zip .
   
   # Deploy to App Service
   az webapp deployment source config-zip \
     --resource-group "rg-prodigy-prod" \
     --name "prodigy-prod-api" \
     --src ../backend-deploy.zip
   ```

2. **Build and deploy frontend**
   ```bash
   cd src/frontend
   npm ci
   npm run build
   
   # Deploy to Static Web App (using GitHub integration)
   # Or manually upload dist/ folder via Azure portal
   ```

3. **Deploy Azure Functions**
   ```bash
   cd azure-functions
   dotnet publish -c Release -o ../publish-functions
   
   cd ../publish-functions
   zip -r ../functions-deploy.zip .
   
   az functionapp deployment source config-zip \
     --resource-group "rg-prodigy-prod" \
     --name "prodigy-prod-functions" \
     --src ../functions-deploy.zip
   ```

## 🔄 CI/CD Deployment

### GitHub Actions Setup

1. **Configure secrets in GitHub repository**
   ```
   AZURE_CREDENTIALS              # Service principal JSON
   AZURE_TENANT_ID                # Azure AD tenant ID
   AZURE_CLIENT_ID                # App registration client ID
   AZURE_CLIENT_SECRET            # App registration secret
   JWT_SECRET_KEY                 # Strong JWT signing key
   GITHUB_TOKEN                   # GitHub personal access token
   LINKEDIN_CLIENT_ID             # LinkedIn app client ID
   LINKEDIN_CLIENT_SECRET         # LinkedIn app secret
   AZURE_STATIC_WEB_APPS_API_TOKEN # Static Web Apps deployment token
   ```

2. **Push to main branch**
   ```bash
   git push origin main
   ```

The GitHub Actions workflow will automatically:
- Build and test the application
- Deploy infrastructure
- Deploy backend, frontend, and functions
- Verify deployment health

### Manual Workflow Trigger

```bash
# Trigger deployment via GitHub CLI
gh workflow run azure-deploy.yml

# Or via web interface
# Go to Actions tab → Deploy to Azure → Run workflow
```

## 🐳 Docker Deployment

### Local Testing

```bash
# Copy environment file
cp .env.prod.example .env
# Edit with your values

# Build and run all services
docker-compose up --build

# Access applications
# Frontend: http://localhost
# Backend: http://localhost:5000
# Functions: http://localhost:7071
```

### Production Container Deployment

```bash
# Build images
docker build -f Dockerfile.backend -t prodigy-backend .
docker build -f Dockerfile.frontend -t prodigy-frontend .
docker build -f Dockerfile.functions -t prodigy-functions .

# Push to Azure Container Registry
az acr login --name your-registry
docker tag prodigy-backend your-registry.azurecr.io/prodigy-backend:latest
docker push your-registry.azurecr.io/prodigy-backend:latest
```

## 🔍 Verification & Testing

### Health Checks

1. **Backend API**
   ```bash
   curl https://prodigy-prod-api.azurewebsites.net/health
   # Expected: {"status":"Healthy","timestamp":"..."}
   ```

2. **Frontend**
   ```bash
   curl https://prodigy-prod-frontend.azurestaticapps.net/
   # Expected: HTML response with Prodigy app
   ```

3. **Azure Functions**
   ```bash
   curl https://prodigy-prod-functions.azurewebsites.net/api/health
   # Expected: Function response
   ```

### Application Testing

1. **Navigate to frontend URL**
2. **Test authentication flow**
3. **Verify API connectivity**
4. **Test each agent functionality**
5. **Check Application Insights logs**

## 🔧 Troubleshooting

### Common Issues

1. **Key Vault Access Denied**
   ```bash
   # Check managed identity permissions
   az role assignment list \
     --assignee $(az webapp identity show \
       --resource-group rg-prodigy-prod \
       --name prodigy-prod-api \
       --query principalId -o tsv)
   ```

2. **App Service Won't Start**
   ```bash
   # Check logs
   az webapp log tail \
     --resource-group rg-prodigy-prod \
     --name prodigy-prod-api
   ```

3. **Static Web App Build Fails**
   ```bash
   # Check build logs in Azure portal
   # Verify build configuration in Bicep template
   ```

### Debugging Commands

```bash
# View deployment history
az deployment group list \
  --resource-group rg-prodigy-prod \
  --output table

# Check resource status
az resource list \
  --resource-group rg-prodigy-prod \
  --output table

# View Key Vault secrets
az keyvault secret list \
  --vault-name prodigy-prod-kv-xxxxx
```

## 📊 Monitoring & Maintenance

### Application Insights

1. **View metrics in Azure portal**
2. **Set up alerts for failures**
3. **Monitor performance**
4. **Track usage patterns**

### Regular Maintenance

1. **Update dependencies monthly**
2. **Rotate secrets quarterly**
3. **Review security settings**
4. **Monitor costs and optimize**
5. **Backup configurations**

## 🔒 Security Considerations

### Best Practices Implemented

1. **HTTPS enforcement** on all endpoints
2. **Managed identities** for Azure resource access
3. **Key Vault** for secret storage
4. **RBAC permissions** with least privilege
5. **Environment isolation** between staging/production
6. **Secret rotation** capability
7. **Security headers** in Nginx configuration

### Additional Security Steps

1. **Configure custom domains with SSL certificates**
2. **Set up Azure Front Door for global performance**
3. **Enable Azure Security Center recommendations**
4. **Configure backup and disaster recovery**
5. **Set up monitoring and alerting**

## 📚 Additional Resources

- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure Static Web Apps Documentation](https://docs.microsoft.com/en-us/azure/static-web-apps/)
- [Azure Functions Documentation](https://docs.microsoft.com/en-us/azure/azure-functions/)
- [Azure Key Vault Documentation](https://docs.microsoft.com/en-us/azure/key-vault/)
- [Bicep Documentation](https://docs.microsoft.com/en-us/azure/azure-resource-manager/bicep/)

---

*This implementation provides a complete, production-ready Azure deployment for the Prodigy application with security best practices, monitoring, and automated CI/CD.*