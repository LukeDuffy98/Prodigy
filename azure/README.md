# Azure Deployment Files

This directory contains all the necessary files for deploying Prodigy to Azure.

## 📁 Directory Structure

```
azure/
├── main.bicep                    # Main Bicep infrastructure template
├── main.parameters.json          # Production parameters (git-ignored)
└── main.parameters.template.json # Template for parameters

scripts/
├── deploy-azure.sh               # Bash deployment script (Linux/Mac)
└── deploy-azure.ps1              # PowerShell deployment script (Windows)

Environment files/
├── .env.prod.example             # Production environment template
├── .env.staging.example          # Staging environment template
└── docker-compose.yml            # Local testing with Docker

Docker files/
├── Dockerfile.backend            # Backend API containerization
├── Dockerfile.frontend           # Frontend containerization
├── Dockerfile.functions          # Azure Functions containerization
└── nginx.conf                    # Nginx configuration for frontend

GitHub Actions/
└── .github/workflows/azure-deploy.yml # CI/CD pipeline
```

## 🚀 Quick Deployment

### Option 1: Automated Script (Recommended)

```bash
# 1. Set up environment variables
cp .env.prod.example .env
# Edit .env with your actual values

# 2. Run deployment script
./scripts/deploy-azure.sh
```

### Option 2: Manual Azure CLI

```bash
# 1. Create resource group
az group create --name "rg-prodigy-prod" --location "East US"

# 2. Deploy infrastructure
az deployment group create \
  --resource-group "rg-prodigy-prod" \
  --template-file azure/main.bicep \
  --parameters @azure/main.parameters.template.json

# 3. Deploy applications (see full instructions in docs/AZURE_DEPLOYMENT.md)
```

### Option 3: GitHub Actions (CI/CD)

1. Configure GitHub secrets (see docs/AZURE_DEPLOYMENT.md)
2. Push to main branch or manually trigger workflow

## 🔧 Infrastructure Components

The Bicep template deploys:

- **App Service** - Backend API (.NET 8)
- **Static Web App** - Frontend (React)
- **Function App** - Business logic (Azure Functions)
- **Key Vault** - Secure secret storage
- **Application Insights** - Monitoring and logging
- **Storage Account** - Function app storage

## 🔒 Security Features

- HTTPS enforcement on all services
- Managed identities for secure access
- Key Vault integration for secrets
- RBAC permissions with least privilege
- Environment variable security

## 📊 Monitoring

- Application Insights integration
- Health check endpoints
- Structured logging
- Performance monitoring
- Error tracking

## 🛠️ Customization

### Environment Variables

Required variables (set in .env file):
- `AZURE_TENANT_ID` - Azure AD tenant
- `AZURE_CLIENT_ID` - App registration ID
- `AZURE_CLIENT_SECRET` - App registration secret
- `JWT_SECRET_KEY` - Strong JWT signing key
- `GITHUB_TOKEN` - GitHub personal access token
- `LINKEDIN_CLIENT_ID` - LinkedIn app ID
- `LINKEDIN_CLIENT_SECRET` - LinkedIn app secret

### Deployment Parameters

Customize in `azure/main.parameters.template.json`:
- Resource names and locations
- SKU sizes for App Service plans
- Environment-specific configurations

### Docker Deployment

For containerized deployment:
```bash
docker-compose up --build
```

## 📖 Documentation

- [Azure Deployment Guide](../docs/AZURE_DEPLOYMENT.md) - Complete deployment instructions
- [Architecture Guide](../docs/ARCHITECTURE.md) - System architecture overview
- [Developer Guide](../docs/DEVELOPER_GUIDE.md) - Development setup

## 🔧 Troubleshooting

Common issues and solutions:

1. **Permission Errors**: Ensure you have Contributor access to Azure subscription
2. **Resource Name Conflicts**: Modify resource names in Bicep parameters
3. **Key Vault Access**: Check managed identity role assignments
4. **Build Failures**: Verify .NET 8 and Node.js 20+ are installed

For detailed troubleshooting, see [docs/AZURE_DEPLOYMENT.md](../docs/AZURE_DEPLOYMENT.md).

## 📞 Support

- Check [GitHub Issues](https://github.com/LukeDuffy98/Prodigy/issues) for known problems
- Review [Azure documentation](https://docs.microsoft.com/en-us/azure/) for Azure-specific issues
- See [Application Insights](https://portal.azure.com) for runtime errors

---

*This Azure deployment setup provides a production-ready, secure, and scalable hosting solution for Prodigy with automated CI/CD and monitoring.*