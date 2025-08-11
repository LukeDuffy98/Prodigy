@description('Name of the application')
param appName string = 'prodigy'

@description('Environment name (dev, staging, prod)')
param environment string = 'prod'

@description('Azure region for resource deployment')
param location string = resourceGroup().location

@description('App Service Plan SKU')
param appServicePlanSku string = 'B1'

@description('Azure AD Tenant ID')
@secure()
param azureTenantId string

@description('Azure AD Client ID')
@secure()
param azureClientId string

@description('Azure AD Client Secret')
@secure()
param azureClientSecret string

@description('JWT Secret Key')
@secure()
param jwtSecretKey string = ''

@description('GitHub Token for API access')
@secure()
param githubToken string

@description('LinkedIn Client ID')
@secure()
param linkedinClientId string = ''

@description('LinkedIn Client Secret')
@secure()
param linkedinClientSecret string = ''

@description('Deployment timestamp for unique resource naming')
param deploymentTimestamp string = utcNow()

// Variables
var resourcePrefix = '${appName}-${environment}'
var keyVaultName = 'prodigykv${uniqueString(resourceGroup().id, deploymentTimestamp)}'
var appServicePlanName = '${resourcePrefix}-asp'
var backendAppName = '${resourcePrefix}-api'
var frontendAppName = '${resourcePrefix}-frontend'
var functionsAppName = '${resourcePrefix}-functions'
var storageAccountName = 'prodigyst${uniqueString(resourceGroup().id)}'
var applicationInsightsName = '${resourcePrefix}-ai'
var logAnalyticsWorkspaceName = '${resourcePrefix}-law'

// Log Analytics Workspace for Application Insights
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsWorkspaceName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Application Insights for monitoring
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}

// Storage Account for Azure Functions
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

// Key Vault for secrets management
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: backendApp.identity.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: functionsApp.identity.principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
    enabledForDeployment: false
    enabledForTemplateDeployment: true
    enabledForDiskEncryption: false
    enableRbacAuthorization: false
  }
}

// Helper function to map SKU name to tier
var appServicePlanTier = appServicePlanSku == 'F1' ? 'Free' : (startsWith(appServicePlanSku, 'B') ? 'Basic' : (startsWith(appServicePlanSku, 'S') ? 'Standard' : (startsWith(appServicePlanSku, 'P') ? 'Premium' : 'Basic')))

// App Service Plan
resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
    tier: appServicePlanTier
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

// Backend API App Service
resource backendApp 'Microsoft.Web/sites@2023-12-01' = {
  name: backendAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: concat([
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'AZURE_TENANT_ID'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=azure-tenant-id)'
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=azure-client-id)'
        }
        {
          name: 'AZURE_CLIENT_SECRET'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=azure-client-secret)'
        }
        {
          name: 'GITHUB_TOKEN'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=github-token)'
        }
        {
          name: 'JWT_ISSUER'
          value: 'Prodigy'
        }
        {
          name: 'JWT_AUDIENCE'
          value: 'ProdigyUsers'
        }
        {
          name: 'MICROSOFT_GRAPH_API_URL'
          value: 'https://graph.microsoft.com/v1.0'
        }
        {
          name: 'GITHUB_API_URL'
          value: 'https://api.github.com'
        }
        {
          name: 'GITHUB_REPO_OWNER'
          value: 'LukeDuffy98'
        }
        {
          name: 'GITHUB_REPO_NAME'
          value: 'Prodigy'
        }
      ], !empty(jwtSecretKey) ? [
        {
          name: 'JWT_SECRET_KEY'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=jwt-secret-key)'
        }
      ] : [], !empty(linkedinClientId) ? [
        {
          name: 'LINKEDIN_CLIENT_ID'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=linkedin-client-id)'
        }
      ] : [], !empty(linkedinClientSecret) ? [
        {
          name: 'LINKEDIN_CLIENT_SECRET'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=linkedin-client-secret)'
        }
      ] : [])
    }
  }
}

// Frontend App Service (serving built React app)
resource frontendApp 'Microsoft.Web/sites@2023-12-01' = {
  name: frontendAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'NODE|20-lts'
      alwaysOn: true
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'WEBSITE_NODE_DEFAULT_VERSION'
          value: '20.x'
        }
        {
          name: 'REACT_APP_API_BASE_URL'
          value: 'https://${backendAppName}.azurewebsites.net/api'
        }
      ]
    }
  }
}

// Azure Functions App
resource functionsApp 'Microsoft.Web/sites@2023-12-01' = {
  name: functionsAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNET-ISOLATED|8.0'
      alwaysOn: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'AzureWebJobsStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTAZUREFILECONNECTIONSTRING'
          value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};EndpointSuffix=${az.environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
        }
        {
          name: 'WEBSITE_CONTENTSHARE'
          value: toLower(functionsAppName)
        }
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsights.properties.ConnectionString
        }
        {
          name: 'AZURE_TENANT_ID'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=azure-tenant-id)'
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=azure-client-id)'
        }
        {
          name: 'AZURE_CLIENT_SECRET'
          value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=azure-client-secret)'
        }
      ]
    }
  }
}

// Key Vault Secrets
resource azureTenantIdSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'azure-tenant-id'
  properties: {
    value: azureTenantId
  }
}

resource azureClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'azure-client-id'
  properties: {
    value: azureClientId
  }
}

resource azureClientSecretSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'azure-client-secret'
  properties: {
    value: azureClientSecret
  }
}

resource jwtSecretKeySecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(jwtSecretKey)) {
  parent: keyVault
  name: 'jwt-secret-key'
  properties: {
    value: jwtSecretKey
  }
}

resource githubTokenSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'github-token'
  properties: {
    value: githubToken
  }
}

resource linkedinClientIdSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(linkedinClientId)) {
  parent: keyVault
  name: 'linkedin-client-id'
  properties: {
    value: linkedinClientId
  }
}

resource linkedinClientSecretSecret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = if (!empty(linkedinClientSecret)) {
  parent: keyVault
  name: 'linkedin-client-secret'
  properties: {
    value: linkedinClientSecret
  }
}

// Outputs
output backendUrl string = 'https://${backendApp.properties.defaultHostName}'
output frontendUrl string = 'https://${frontendApp.properties.defaultHostName}'
output functionsUrl string = 'https://${functionsApp.properties.defaultHostName}'
output keyVaultName string = keyVault.name
output applicationInsightsConnectionString string = applicationInsights.properties.ConnectionString
output resourceGroupName string = resourceGroup().name