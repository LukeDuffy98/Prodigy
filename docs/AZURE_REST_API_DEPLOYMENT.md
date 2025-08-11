# Azure REST API Deployment Documentation

## Overview

This repository now uses Azure REST API for infrastructure deployment instead of Bicep CLI commands. This approach provides more control and reliability for CI/CD deployments.

## Changes Made

### 1. New Deployment Script: `deploy-with-rest-api.sh`

This script replaces the previous Azure CLI-based deployment with direct REST API calls to Azure Resource Manager.

**Key Features:**
- Direct REST API calls to Azure management endpoints
- Proper token management and authentication
- Comprehensive error handling and status monitoring
- Maintains same deployment outputs as previous approach

### 2. Updated Workflow: `.github/workflows/azure-deploy.yml`

The `deploy-infrastructure` job has been updated to:
- Remove dependency on `azure/login` action for infrastructure deployment
- Use the new REST API script instead of `az deployment group create`
- Maintain identical outputs for downstream jobs (backend, frontend, functions)

## Required Secrets

The following GitHub secrets must be configured for the deployment to work:

### Core Azure Authentication
- `AZURE_CLIENT_ID` - Service Principal Application ID
- `AZURE_CLIENT_SECRET` - Service Principal Secret
- `AZURE_TENANT_ID` - Azure AD Tenant ID
- `AZURE_SUBSCRIPTION_ID` - Azure Subscription ID

### Legacy Azure Credentials (for app deployments)
- `AZURE_CREDENTIALS` - JSON formatted credentials for `azure/login` action

### Application Secrets
- `JWT_SECRET_KEY` - Secret key for JWT token generation (optional)
- `GITHUB_TOKEN` - GitHub token for API access
- `LINKEDIN_CLIENT_ID` - LinkedIn API client ID (optional)
- `LINKEDIN_CLIENT_SECRET` - LinkedIn API client secret (optional)

## Benefits of REST API Approach

1. **Reduced Dependencies**: No need for Azure CLI installation and login
2. **Better Error Handling**: Direct access to Azure REST API error responses
3. **Improved Reliability**: Eliminates Azure CLI session and token management issues
4. **Enhanced Debugging**: Better visibility into deployment status and errors
5. **Consistency**: Follows the proven pattern from prodigy1 repository

## Deployment Flow

1. **Authentication**: Script obtains access token via OAuth2 client credentials flow
2. **Resource Group**: Creates or updates the target resource group
3. **Template Compilation**: Compiles Bicep template to ARM JSON
4. **Deployment**: Submits deployment via REST API with all required parameters
5. **Monitoring**: Polls deployment status until completion or timeout
6. **Outputs**: Extracts and sets deployment outputs for subsequent jobs

## Testing

To test the deployment locally (with appropriate environment variables set):

```bash
chmod +x deploy-with-rest-api.sh
./deploy-with-rest-api.sh
```

## Rollback Strategy

If issues arise, the deployment can be reverted by:
1. Restoring the previous workflow file
2. Removing the `deploy-with-rest-api.sh` script
3. Re-enabling Azure CLI-based deployment

The Bicep template remains unchanged, ensuring infrastructure compatibility.