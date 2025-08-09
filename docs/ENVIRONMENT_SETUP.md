# ⚙️ Environment Setup Guide

This guide provides detailed instructions for configuring environment variables, external services, and development settings for the Prodigy application.

## 📋 Table of Contents

- [Overview](#overview)
- [Environment Files](#environment-files)
- [Azure AD Configuration](#azure-ad-configuration)
- [Microsoft Graph Setup](#microsoft-graph-setup)
- [GitHub Integration](#github-integration)
- [LinkedIn Integration](#linkedin-integration)
- [JWT Configuration](#jwt-configuration)
- [Development vs Production](#development-vs-production)
- [Security Best Practices](#security-best-practices)
- [Troubleshooting](#troubleshooting)

## 🌐 Overview

Prodigy requires configuration for multiple external services and authentication providers. This guide walks through setting up each integration and managing environment-specific configurations.

### Required Integrations
- **Azure AD**: User authentication and authorization
- **Microsoft Graph**: Email, calendar, and user data access
- **GitHub API**: Repository and issue management
- **LinkedIn API**: Professional networking features (optional)

### Configuration Levels
1. **Development**: Local development with test accounts
2. **Staging**: Pre-production environment with limited access
3. **Production**: Live environment with full security

## 📄 Environment Files

### .env File Structure
Create a `.env` file in the project root directory:

```bash
# Copy from template
cp .env.example .env

# Edit with your specific values
nano .env
```

### Complete .env Template
```bash
# ===========================================
# PRODIGY ENVIRONMENT CONFIGURATION
# ===========================================

# Environment Settings
ASPNETCORE_ENVIRONMENT=Development
NODE_ENV=development

# ===========================================
# AZURE AD CONFIGURATION
# ===========================================
AZURE_TENANT_ID=your-azure-tenant-id
AZURE_CLIENT_ID=your-azure-client-id
AZURE_CLIENT_SECRET=your-azure-client-secret
AZURE_REDIRECT_URI=http://localhost:5173/auth/callback

# ===========================================
# JWT CONFIGURATION
# ===========================================
JWT_SECRET_KEY=your-development-secret-key-minimum-32-characters
JWT_ISSUER=Prodigy
JWT_AUDIENCE=ProdigyUsers
JWT_EXPIRATION_HOURS=24

# ===========================================
# GITHUB INTEGRATION
# ===========================================
GITHUB_TOKEN=your-github-personal-access-token
GITHUB_OWNER=your-github-username
GITHUB_REPO=your-repository-name

# ===========================================
# LINKEDIN INTEGRATION (Optional)
# ===========================================
LINKEDIN_CLIENT_ID=your-linkedin-client-id
LINKEDIN_CLIENT_SECRET=your-linkedin-client-secret
LINKEDIN_REDIRECT_URI=http://localhost:5173/auth/linkedin/callback

# ===========================================
# APPLICATION URLS
# ===========================================
API_BASE_URL=http://localhost:5169
FRONTEND_URL=http://localhost:5173
AZURE_FUNCTIONS_URL=http://localhost:7071/api/
AZURE_FUNCTIONS_KEY=your-functions-key

# ===========================================
# LOGGING AND MONITORING
# ===========================================
LOG_LEVEL=Information
APPLICATION_INSIGHTS_INSTRUMENTATION_KEY=your-app-insights-key

# ===========================================
# DATABASE (Future)
# ===========================================
DATABASE_CONNECTION_STRING=your-database-connection-string
```

## 🔐 Azure AD Configuration

### Step 1: Create Azure AD Application

1. **Navigate to Azure Portal**:
   - Go to [Azure Portal](https://portal.azure.com)
   - Sign in with your organizational account

2. **Register Application**:
   ```bash
   Azure Active Directory → App registrations → New registration
   ```
   
   - **Name**: "Prodigy Development" (or appropriate environment name)
   - **Supported account types**: "Accounts in this organizational directory only"
   - **Redirect URI**: 
     - Type: Web
     - URI: `http://localhost:5173/auth/callback` (development)

3. **Note Application Details**:
   ```bash
   Application (client) ID: [Copy this to AZURE_CLIENT_ID]
   Directory (tenant) ID: [Copy this to AZURE_TENANT_ID]
   ```

### Step 2: Configure Client Secret

1. **Create Client Secret**:
   ```bash
   App registration → Certificates & secrets → New client secret
   ```
   
   - **Description**: "Prodigy Development Secret"
   - **Expires**: 24 months (recommended)
   - **Copy Value**: [This goes to AZURE_CLIENT_SECRET]

2. **Security Note**: Store the secret value immediately - it won't be shown again.

### Step 3: Configure API Permissions

Add the following permissions for Microsoft Graph integration:

**Microsoft Graph Permissions**:
```bash
Application permissions:
- Mail.Send (Send mail as any user)
- Calendars.Read (Read calendars in all mailboxes)
- User.Read.All (Read all users' full profiles)

Delegated permissions:
- User.Read (Sign in and read user profile)
- Mail.ReadWrite (Read and write access to user mail)
- Calendars.ReadWrite (Have full access to user calendars)
```

**Grant Admin Consent**:
```bash
App registration → API permissions → Grant admin consent for [tenant]
```

### Step 4: Authentication Configuration

```bash
App registration → Authentication → Configure platforms
```

**Web Configuration**:
- **Redirect URIs**: 
  - Development: `http://localhost:5173/auth/callback`
  - Production: `https://yourdomain.com/auth/callback`
- **Front-channel logout URL**: `http://localhost:5173/logout`
- **Implicit grant**: Enable "ID tokens"

## 📧 Microsoft Graph Setup

### Required Scopes
Configure your application to request these Microsoft Graph scopes:

```javascript
// Frontend MSAL configuration
export const msalConfig = {
  auth: {
    clientId: process.env.REACT_APP_AZURE_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${process.env.REACT_APP_AZURE_TENANT_ID}`,
    redirectUri: process.env.REACT_APP_AZURE_REDIRECT_URI,
  },
  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: false,
  }
};

export const loginRequest = {
  scopes: [
    "User.Read",
    "Mail.ReadWrite", 
    "Calendars.ReadWrite"
  ]
};
```

### Graph API Endpoints Used
```bash
# User Profile
GET https://graph.microsoft.com/v1.0/me

# Send Email
POST https://graph.microsoft.com/v1.0/me/sendMail

# Calendar Events
GET https://graph.microsoft.com/v1.0/me/events
POST https://graph.microsoft.com/v1.0/me/events

# Calendar Free/Busy
POST https://graph.microsoft.com/v1.0/me/calendar/getSchedule
```

### Testing Graph Integration
```bash
# Test with Azure CLI
az login
az account get-access-token --resource https://graph.microsoft.com

# Test Graph API call
curl -H "Authorization: Bearer <token>" \
     https://graph.microsoft.com/v1.0/me
```

## 🐙 GitHub Integration

### Step 1: Create Personal Access Token

1. **GitHub Settings**:
   ```bash
   GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
   ```

2. **Generate New Token**:
   - **Note**: "Prodigy Development"
   - **Expiration**: 90 days (recommended for development)
   - **Scopes**:
     ```bash
     repo (Full control of private repositories)
     ├── repo:status
     ├── repo_deployment
     ├── public_repo
     └── repo:invite
     
     write:discussion (Write team discussions)
     read:user (Read user profile data)
     user:email (Access user email addresses)
     ```

3. **Copy Token**: [This goes to GITHUB_TOKEN]

### Step 2: Repository Configuration

```bash
# Set repository details in .env
GITHUB_OWNER=your-github-username
GITHUB_REPO=your-repository-name

# Example for organization repository:
GITHUB_OWNER=LukeDuffy98
GITHUB_REPO=Prodigy
```

### Step 3: Test GitHub Integration

```bash
# Test API access
curl -H "Authorization: token <your-token>" \
     https://api.github.com/user

# Test repository access
curl -H "Authorization: token <your-token>" \
     https://api.github.com/repos/<owner>/<repo>

# Test issue creation
curl -X POST \
     -H "Authorization: token <your-token>" \
     -H "Content-Type: application/json" \
     -d '{"title":"Test Issue","body":"Testing API access"}' \
     https://api.github.com/repos/<owner>/<repo>/issues
```

## 💼 LinkedIn Integration

### Step 1: Create LinkedIn Application

1. **LinkedIn Developer Portal**:
   - Go to [LinkedIn Developer Portal](https://developer.linkedin.com)
   - Sign in with your LinkedIn account

2. **Create Application**:
   ```bash
   My apps → Create app
   ```
   
   - **App name**: "Prodigy Development"
   - **LinkedIn Page**: Your company page or personal profile
   - **Privacy policy URL**: Your privacy policy URL
   - **App logo**: Upload appropriate logo

3. **Note Application Details**:
   ```bash
   Client ID: [Copy this to LINKEDIN_CLIENT_ID]
   Client Secret: [Copy this to LINKEDIN_CLIENT_SECRET]
   ```

### Step 2: Configure OAuth Settings

```bash
Application → Auth → OAuth 2.0 settings
```

**Authorized Redirect URLs**:
- Development: `http://localhost:5173/auth/linkedin/callback`
- Production: `https://yourdomain.com/auth/linkedin/callback`

### Step 3: Request API Access

**Default Permissions**:
- `r_liteprofile` (Read basic profile info)
- `r_emailaddress` (Read email address)

**Additional Permissions** (requires approval):
- `w_member_social` (Post on behalf of user)
- `r_organization_social` (Read organization posts)

### Step 4: Test LinkedIn Integration

```bash
# OAuth flow test URL
https://www.linkedin.com/oauth/v2/authorization?response_type=code&client_id=<client_id>&redirect_uri=<redirect_uri>&scope=r_liteprofile%20r_emailaddress
```

## 🔑 JWT Configuration

### Development JWT Settings

```bash
# Generate a secure secret key (minimum 32 characters)
JWT_SECRET_KEY=YourDevelopmentSecretKeyMinimum32Characters2024

# JWT configuration
JWT_ISSUER=Prodigy
JWT_AUDIENCE=ProdigyUsers
JWT_EXPIRATION_HOURS=24
```

### Generating Secure Keys

```bash
# Using OpenSSL
openssl rand -base64 32

# Using Node.js
node -e "console.log(require('crypto').randomBytes(32).toString('base64'))"

# Using .NET
dotnet run --project tools/KeyGenerator

# Using PowerShell
[System.Web.Security.Membership]::GeneratePassword(32, 0)
```

### Production JWT Settings

```bash
# Use a cryptographically secure key
JWT_SECRET_KEY=your-production-secret-key-from-secure-storage

# Production configuration
JWT_ISSUER=https://api.prodigy.company.com
JWT_AUDIENCE=https://prodigy.company.com
JWT_EXPIRATION_HOURS=8
```

## 🔄 Development vs Production

### Development Configuration

```bash
# Development .env
ASPNETCORE_ENVIRONMENT=Development
NODE_ENV=development

# Relaxed security settings
ASPNETCORE_URLS=http://localhost:5169
CORS_ORIGINS=http://localhost:5173,http://localhost:3000

# Debug settings
LOG_LEVEL=Debug
ENABLE_SWAGGER=true
```

### Staging Configuration

```bash
# Staging environment
ASPNETCORE_ENVIRONMENT=Staging
NODE_ENV=production

# Staging URLs
API_BASE_URL=https://staging-api.prodigy.company.com
FRONTEND_URL=https://staging.prodigy.company.com

# Production-like security
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
LOG_LEVEL=Information
```

### Production Configuration

```bash
# Production environment
ASPNETCORE_ENVIRONMENT=Production
NODE_ENV=production

# Production URLs
API_BASE_URL=https://api.prodigy.company.com
FRONTEND_URL=https://prodigy.company.com

# Enhanced security
ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
LOG_LEVEL=Warning
ENABLE_SWAGGER=false

# Production secrets (use secure storage)
JWT_SECRET_KEY=@Microsoft.KeyVault(SecretUri=https://vault.vault.azure.net/secrets/jwt-key/)
AZURE_CLIENT_SECRET=@Microsoft.KeyVault(SecretUri=https://vault.vault.azure.net/secrets/azure-secret/)
```

## 🛡️ Security Best Practices

### Secret Management

1. **Never Commit Secrets**:
   ```bash
   # Add to .gitignore
   .env
   .env.local
   .env.production
   appsettings.Production.json
   ```

2. **Use Secure Storage**:
   - **Development**: Local .env files
   - **Production**: Azure Key Vault, AWS Secrets Manager, or similar

3. **Secret Rotation**:
   ```bash
   # Regular rotation schedule
   Azure AD Client Secrets: Every 12 months
   GitHub Tokens: Every 90 days
   JWT Keys: Every 6 months
   ```

### Environment Isolation

```bash
# Separate environments
Development: dev-tenant, dev-apps, test-data
Staging: staging-tenant, staging-apps, staging-data  
Production: prod-tenant, prod-apps, live-data
```

### Access Control

```bash
# Principle of least privilege
Development: Read/write access to development resources
Staging: Read/write access to staging resources
Production: Minimal required permissions
```

## 🔧 Troubleshooting

### Common Configuration Issues

#### Azure AD Authentication Errors

```bash
# Check tenant ID
az account show --query tenantId

# Verify application registration
az ad app list --display-name "Prodigy Development"

# Test authentication
curl -X POST https://login.microsoftonline.com/<tenant>/oauth2/v2.0/token \
  -d "client_id=<client_id>&client_secret=<client_secret>&grant_type=client_credentials&scope=https://graph.microsoft.com/.default"
```

#### Microsoft Graph Permission Issues

```bash
# Check granted permissions
Azure Portal → App registration → API permissions

# Common issues:
- Admin consent not granted
- Incorrect permission type (Application vs Delegated)
- Insufficient permissions for operation
```

#### GitHub API Issues

```bash
# Test token validity
curl -H "Authorization: token <token>" https://api.github.com/rate_limit

# Check token scopes
curl -H "Authorization: token <token>" https://api.github.com/user

# Verify repository access
curl -H "Authorization: token <token>" https://api.github.com/repos/<owner>/<repo>
```

### Environment Variable Loading Issues

```bash
# Check if environment variables are loaded
# In Program.cs, add debug output:
Console.WriteLine($"AZURE_TENANT_ID: {Environment.GetEnvironmentVariable("AZURE_TENANT_ID")}");

# Verify .env file location and format
ls -la .env
cat .env | grep -v "^#" | grep -v "^$"
```

### CORS Configuration Issues

```bash
# Check CORS settings in Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProdigyFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

# Verify frontend URL matches CORS configuration
```

### JWT Token Issues

```bash
# Validate JWT token structure
echo "your-jwt-token" | base64 -d

# Check token expiration
# Use online JWT decoder: https://jwt.io

# Verify signing key consistency
```

### Getting Help

1. **Check Logs**: Review console output and error messages
2. **API Testing**: Use Postman or curl to test endpoints
3. **Azure Portal**: Verify Azure AD configuration
4. **GitHub Settings**: Check personal access token permissions
5. **Documentation**: Refer to official API documentation

---

## 📚 Related Documentation

- [Developer Guide](DEVELOPER_GUIDE.md) - Development setup and workflow
- [Azure AD Configuration](AZURE_AD_CONFIG.md) - Detailed Azure AD setup
- [Deployment Guide](DEPLOYMENT.md) - Production deployment instructions
- [API Reference](API_REFERENCE.md) - API endpoint documentation

---

*This environment setup guide provides the foundation for running Prodigy in any environment. Keep your secrets secure and follow security best practices for production deployments.*