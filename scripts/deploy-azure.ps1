# Prodigy Azure Deployment Script (PowerShell)
# This script deploys the Prodigy application to Azure

param(
    [string]$ResourceGroup = "rg-prodigy-prod",
    [string]$Location = "australiaeast",
    [string]$Environment = "prod",
    [string]$AppName = "prodigy",
    [switch]$Help
)

# Show help if requested
if ($Help) {
    Write-Host "Prodigy Azure Deployment Script" -ForegroundColor Blue
    Write-Host ""
    Write-Host "Usage: .\deploy-azure.ps1 [OPTIONS]" -ForegroundColor Green
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  -ResourceGroup NAME    Azure resource group name (default: rg-prodigy-prod)"
    Write-Host "  -Location LOCATION     Azure region (default: australiaeast)"
    Write-Host "  -Environment ENV       Environment name (default: prod)"
    Write-Host "  -AppName NAME          Application name (default: prodigy)"
    Write-Host "  -Help                  Show this help message"
    Write-Host ""
    Write-Host "Example:" -ForegroundColor Cyan
    Write-Host "  .\deploy-azure.ps1 -ResourceGroup 'my-rg' -Environment 'staging'"
    exit 0
}

# Functions
function Write-Header {
    param([string]$Message)
    Write-Host "================================" -ForegroundColor Blue
    Write-Host $Message -ForegroundColor Blue
    Write-Host "================================" -ForegroundColor Blue
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Test-Prerequisites {
    Write-Header "Checking Prerequisites"
    
    # Check if Azure CLI is installed
    try {
        $null = Get-Command az -ErrorAction Stop
    }
    catch {
        Write-Error "Azure CLI is not installed. Please install it first."
        Write-Host "Download from: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Cyan
        exit 1
    }
    
    # Check if .NET SDK is installed
    try {
        $null = Get-Command dotnet -ErrorAction Stop
    }
    catch {
        Write-Error ".NET SDK is not installed. Please install .NET 8 SDK."
        Write-Host "Download from: https://dotnet.microsoft.com/download" -ForegroundColor Cyan
        exit 1
    }
    
    # Check if Node.js is installed
    try {
        $null = Get-Command node -ErrorAction Stop
    }
    catch {
        Write-Error "Node.js is not installed. Please install Node.js 20+."
        Write-Host "Download from: https://nodejs.org/" -ForegroundColor Cyan
        exit 1
    }
    
    # Check if logged in to Azure
    try {
        $account = az account show --output json 2>$null | ConvertFrom-Json
        if (-not $account) {
            throw "Not logged in"
        }
    }
    catch {
        Write-Error "Not logged in to Azure. Please run 'az login' first."
        exit 1
    }
    
    Write-Success "All prerequisites met"
}

function Test-Environment {
    Write-Header "Validating Environment Variables"
    
    $requiredVars = @(
        "AZURE_TENANT_ID",
        "AZURE_CLIENT_ID", 
        "AZURE_CLIENT_SECRET",
        "JWT_SECRET_KEY",
        "GITHUB_TOKEN",
        "LINKEDIN_CLIENT_ID",
        "LINKEDIN_CLIENT_SECRET"
    )
    
    $missingVars = @()
    
    foreach ($var in $requiredVars) {
        if (-not (Get-ChildItem Env:$var -ErrorAction SilentlyContinue)) {
            $missingVars += $var
        }
    }
    
    if ($missingVars.Count -gt 0) {
        Write-Error "Missing required environment variables:"
        foreach ($var in $missingVars) {
            Write-Host "  - $var" -ForegroundColor Red
        }
        Write-Host ""
        Write-Host "Please set these variables in your environment or create a .env file." -ForegroundColor Yellow
        exit 1
    }
    
    Write-Success "All required environment variables are set"
}

function Build-Application {
    Write-Header "Building Application"
    
    # Build backend
    Write-Host "Building .NET backend..." -ForegroundColor Cyan
    dotnet restore
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    
    dotnet build --no-restore -c Release
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    
    # Build frontend
    Write-Host "Building React frontend..." -ForegroundColor Cyan
    Push-Location "src/frontend"
    npm ci
    if ($LASTEXITCODE -ne 0) { Pop-Location; exit $LASTEXITCODE }
    
    npm run build
    if ($LASTEXITCODE -ne 0) { Pop-Location; exit $LASTEXITCODE }
    
    npm run lint
    if ($LASTEXITCODE -ne 0) { Pop-Location; exit $LASTEXITCODE }
    
    Pop-Location
    
    Write-Success "Application built successfully"
}

function New-ResourceGroup {
    Write-Header "Creating Resource Group"
    
    $rgExists = az group exists --name $ResourceGroup
    if ($rgExists -eq "true") {
        Write-Warning "Resource group '$ResourceGroup' already exists"
    }
    else {
        Write-Host "Creating resource group '$ResourceGroup' in '$Location'..." -ForegroundColor Cyan
        az group create --name $ResourceGroup --location $Location
        if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
        Write-Success "Resource group created"
    }
}

function Deploy-Infrastructure {
    Write-Header "Deploying Infrastructure"
    
    Write-Host "Deploying Bicep template..." -ForegroundColor Cyan
    
    $deploymentOutput = az deployment group create `
        --resource-group $ResourceGroup `
        --template-file "azure/main.bicep" `
        --parameters `
            "appName=$AppName" `
            "environment=$Environment" `
            "location=$Location" `
            "azureTenantId=$env:AZURE_TENANT_ID" `
            "azureClientId=$env:AZURE_CLIENT_ID" `
            "azureClientSecret=$env:AZURE_CLIENT_SECRET" `
            "jwtSecretKey=$env:JWT_SECRET_KEY" `
            "githubToken=$env:GITHUB_TOKEN" `
            "linkedinClientId=$env:LINKEDIN_CLIENT_ID" `
            "linkedinClientSecret=$env:LINKEDIN_CLIENT_SECRET" `
        --query "properties.outputs" `
        --output json
    
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    
    # Parse deployment output
    $outputs = $deploymentOutput | ConvertFrom-Json
    $script:BackendUrl = $outputs.backendUrl.value
    $script:FrontendUrl = $outputs.frontendUrl.value
    $script:FunctionsUrl = $outputs.functionsUrl.value
    
    Write-Success "Infrastructure deployed successfully"
    Write-Host "Backend URL: $script:BackendUrl" -ForegroundColor Green
    Write-Host "Frontend URL: $script:FrontendUrl" -ForegroundColor Green
    Write-Host "Functions URL: $script:FunctionsUrl" -ForegroundColor Green
}

function Deploy-Backend {
    Write-Header "Deploying Backend"
    
    Write-Host "Publishing backend..." -ForegroundColor Cyan
    Push-Location "src/backend"
    dotnet publish -c Release -o "../../publish-backend"
    if ($LASTEXITCODE -ne 0) { Pop-Location; exit $LASTEXITCODE }
    Pop-Location
    
    Write-Host "Creating deployment package..." -ForegroundColor Cyan
    Push-Location "publish-backend"
    Compress-Archive -Path "*" -DestinationPath "../publish-backend.zip" -Force
    Pop-Location
    
    Write-Host "Deploying to App Service..." -ForegroundColor Cyan
    az webapp deployment source config-zip `
        --resource-group $ResourceGroup `
        --name "$AppName-$Environment-api" `
        --src "publish-backend.zip"
    
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Success "Backend deployed successfully"
}

function Deploy-Functions {
    Write-Header "Deploying Azure Functions"
    
    Write-Host "Publishing functions..." -ForegroundColor Cyan
    Push-Location "azure-functions"
    dotnet publish -c Release -o "../publish-functions"
    if ($LASTEXITCODE -ne 0) { Pop-Location; exit $LASTEXITCODE }
    Pop-Location
    
    Write-Host "Creating deployment package..." -ForegroundColor Cyan
    Push-Location "publish-functions"
    Compress-Archive -Path "*" -DestinationPath "../publish-functions.zip" -Force
    Pop-Location
    
    Write-Host "Deploying to Function App..." -ForegroundColor Cyan
    az functionapp deployment source config-zip `
        --resource-group $ResourceGroup `
        --name "$AppName-$Environment-functions" `
        --src "publish-functions.zip"
    
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Success "Azure Functions deployed successfully"
}

function Test-Deployment {
    Write-Header "Verifying Deployment"
    
    Write-Host "Checking backend health..." -ForegroundColor Cyan
    try {
        $response = Invoke-WebRequest -Uri "$script:BackendUrl/health" -UseBasicParsing -TimeoutSec 30
        if ($response.StatusCode -eq 200) {
            Write-Success "Backend is healthy"
        }
        else {
            Write-Warning "Backend health check returned status: $($response.StatusCode)"
        }
    }
    catch {
        Write-Warning "Backend health check failed: $($_.Exception.Message)"
    }
    
    Write-Host "Checking frontend..." -ForegroundColor Cyan
    try {
        $response = Invoke-WebRequest -Uri $script:FrontendUrl -UseBasicParsing -TimeoutSec 30
        if ($response.StatusCode -eq 200) {
            Write-Success "Frontend is accessible"
        }
        else {
            Write-Warning "Frontend accessibility check returned status: $($response.StatusCode)"
        }
    }
    catch {
        Write-Warning "Frontend accessibility check failed: $($_.Exception.Message)"
    }
}

function Remove-TempFiles {
    Write-Header "Cleaning Up"
    
    Write-Host "Removing temporary files..." -ForegroundColor Cyan
    
    $filesToRemove = @(
        "publish-backend.zip",
        "publish-functions.zip"
    )
    
    $foldersToRemove = @(
        "publish-backend",
        "publish-functions"
    )
    
    foreach ($file in $filesToRemove) {
        if (Test-Path $file) {
            Remove-Item $file -Force
        }
    }
    
    foreach ($folder in $foldersToRemove) {
        if (Test-Path $folder) {
            Remove-Item $folder -Recurse -Force
        }
    }
    
    Write-Success "Cleanup completed"
}

function Show-Summary {
    Write-Header "Deployment Summary"
    
    Write-Host "🚀 Prodigy has been successfully deployed to Azure!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Deployed URLs:" -ForegroundColor Yellow
    Write-Host "  📱 Frontend: $script:FrontendUrl" -ForegroundColor Cyan
    Write-Host "  🔧 Backend API: $script:BackendUrl" -ForegroundColor Cyan
    Write-Host "  ⚡ Functions: $script:FunctionsUrl" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1. Test all functionality" -ForegroundColor White
    Write-Host "  2. Configure custom domains if needed" -ForegroundColor White
    Write-Host "  3. Set up monitoring and alerts" -ForegroundColor White
    Write-Host "  4. Review security settings" -ForegroundColor White
}

# Main execution
function Main {
    Write-Host "🚀 Starting Prodigy Azure Deployment" -ForegroundColor Blue
    Write-Host ""
    
    # Load .env file if it exists
    if (Test-Path ".env") {
        Write-Warning "Loading environment variables from .env file"
        Get-Content ".env" | ForEach-Object {
            if ($_ -match "^([^#][^=]+)=(.*)$") {
                Set-Item -Path "Env:$($matches[1])" -Value $matches[2]
            }
        }
    }
    
    try {
        Test-Prerequisites
        Test-Environment
        Build-Application
        New-ResourceGroup
        Deploy-Infrastructure
        Deploy-Backend
        Deploy-Functions
        Test-Deployment
        Remove-TempFiles
        Show-Summary
    }
    catch {
        Write-Error "Deployment failed: $($_.Exception.Message)"
        exit 1
    }
}

# Run main function
Main