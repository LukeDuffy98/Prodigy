#!/bin/bash

# Prodigy Azure Deployment Script
# This script deploys the Prodigy application to Azure

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Default values
RESOURCE_GROUP="rg-prodigy-prod"
LOCATION="australiaeast"
ENVIRONMENT="prod"
APP_NAME="prodigy"

# Functions
print_header() {
    echo -e "${BLUE}================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}================================${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

check_prerequisites() {
    print_header "Checking Prerequisites"
    
    # Check if Azure CLI is installed
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed. Please install it first."
        exit 1
    fi
    
    # Check if .NET SDK is installed
    if ! command -v dotnet &> /dev/null; then
        print_error ".NET SDK is not installed. Please install .NET 8 SDK."
        exit 1
    fi
    
    # Check if Node.js is installed
    if ! command -v node &> /dev/null; then
        print_error "Node.js is not installed. Please install Node.js 20+."
        exit 1
    fi
    
    # Check if logged in to Azure
    if ! az account show &> /dev/null; then
        print_error "Not logged in to Azure. Please run 'az login' first."
        exit 1
    fi
    
    print_success "All prerequisites met"
}

validate_environment() {
    print_header "Validating Environment Variables"
    
    required_vars=(
        "AZURE_TENANT_ID"
        "AZURE_CLIENT_ID" 
        "AZURE_CLIENT_SECRET"
        "JWT_SECRET_KEY"
        "GITHUB_TOKEN"
        "LINKEDIN_CLIENT_ID"
        "LINKEDIN_CLIENT_SECRET"
    )
    
    missing_vars=()
    
    for var in "${required_vars[@]}"; do
        if [[ -z "${!var}" ]]; then
            missing_vars+=("$var")
        fi
    done
    
    if [[ ${#missing_vars[@]} -gt 0 ]]; then
        print_error "Missing required environment variables:"
        for var in "${missing_vars[@]}"; do
            echo "  - $var"
        done
        echo ""
        echo "Please set these variables in your environment or create a .env file."
        exit 1
    fi
    
    print_success "All required environment variables are set"
}

build_application() {
    print_header "Building Application"
    
    # Build backend
    echo "Building .NET backend..."
    dotnet restore
    dotnet build --no-restore -c Release
    
    # Build frontend
    echo "Building React frontend..."
    cd src/frontend
    npm ci
    npm run build
    npm run lint
    cd ../..
    
    print_success "Application built successfully"
}

create_resource_group() {
    print_header "Creating Resource Group"
    
    if az group show --name "$RESOURCE_GROUP" &> /dev/null; then
        print_warning "Resource group '$RESOURCE_GROUP' already exists"
    else
        echo "Creating resource group '$RESOURCE_GROUP' in '$LOCATION'..."
        az group create --name "$RESOURCE_GROUP" --location "$LOCATION"
        print_success "Resource group created"
    fi
}

deploy_infrastructure() {
    print_header "Deploying Infrastructure"
    
    echo "Deploying Bicep template..."
    deployment_output=$(az deployment group create \
        --resource-group "$RESOURCE_GROUP" \
        --template-file azure/main.bicep \
        --parameters \
            appName="$APP_NAME" \
            environment="$ENVIRONMENT" \
            location="$LOCATION" \
            azureTenantId="$AZURE_TENANT_ID" \
            azureClientId="$AZURE_CLIENT_ID" \
            azureClientSecret="$AZURE_CLIENT_SECRET" \
            jwtSecretKey="$JWT_SECRET_KEY" \
            githubToken="$GITHUB_TOKEN" \
            linkedinClientId="$LINKEDIN_CLIENT_ID" \
            linkedinClientSecret="$LINKEDIN_CLIENT_SECRET" \
        --query properties.outputs \
        --output json)
    
    # Extract URLs from deployment output
    BACKEND_URL=$(echo "$deployment_output" | jq -r '.backendUrl.value')
    FRONTEND_URL=$(echo "$deployment_output" | jq -r '.frontendUrl.value')
    FUNCTIONS_URL=$(echo "$deployment_output" | jq -r '.functionsUrl.value')
    
    print_success "Infrastructure deployed successfully"
    echo "Backend URL: $BACKEND_URL"
    echo "Frontend URL: $FRONTEND_URL"
    echo "Functions URL: $FUNCTIONS_URL"
}

deploy_backend() {
    print_header "Deploying Backend"
    
    echo "Publishing backend..."
    cd src/backend
    dotnet publish -c Release -o ../../publish-backend
    cd ../..
    
    echo "Deploying to App Service..."
    az webapp deployment source config-zip \
        --resource-group "$RESOURCE_GROUP" \
        --name "${APP_NAME}-${ENVIRONMENT}-api" \
        --src publish-backend.zip
    
    # Create zip file
    cd publish-backend
    zip -r ../publish-backend.zip .
    cd ..
    
    print_success "Backend deployed successfully"
}

deploy_functions() {
    print_header "Deploying Azure Functions"
    
    echo "Publishing functions..."
    cd azure-functions
    dotnet publish -c Release -o ../publish-functions
    cd ..
    
    echo "Deploying to Function App..."
    cd publish-functions
    zip -r ../publish-functions.zip .
    cd ..
    
    az functionapp deployment source config-zip \
        --resource-group "$RESOURCE_GROUP" \
        --name "${APP_NAME}-${ENVIRONMENT}-functions" \
        --src publish-functions.zip
    
    print_success "Azure Functions deployed successfully"
}

verify_deployment() {
    print_header "Verifying Deployment"
    
    echo "Checking backend health..."
    if curl -f "$BACKEND_URL/health" &> /dev/null; then
        print_success "Backend is healthy"
    else
        print_warning "Backend health check failed"
    fi
    
    echo "Checking frontend..."
    if curl -f "$FRONTEND_URL" &> /dev/null; then
        print_success "Frontend is accessible"
    else
        print_warning "Frontend accessibility check failed"
    fi
}

cleanup() {
    print_header "Cleaning Up"
    
    echo "Removing temporary files..."
    rm -f publish-backend.zip
    rm -f publish-functions.zip
    rm -rf publish-backend
    rm -rf publish-functions
    
    print_success "Cleanup completed"
}

show_summary() {
    print_header "Deployment Summary"
    
    echo -e "${GREEN}🚀 Prodigy has been successfully deployed to Azure!${NC}"
    echo ""
    echo "Deployed URLs:"
    echo "  📱 Frontend: $FRONTEND_URL"
    echo "  🔧 Backend API: $BACKEND_URL"
    echo "  ⚡ Functions: $FUNCTIONS_URL"
    echo ""
    echo "Next steps:"
    echo "  1. Test all functionality"
    echo "  2. Configure custom domains if needed"
    echo "  3. Set up monitoring and alerts"
    echo "  4. Review security settings"
}

# Main execution
main() {
    echo -e "${BLUE}🚀 Starting Prodigy Azure Deployment${NC}"
    echo ""
    
    # Load .env file if it exists
    if [[ -f .env ]]; then
        print_warning "Loading environment variables from .env file"
        export $(grep -v '^#' .env | xargs)
    fi
    
    check_prerequisites
    validate_environment
    build_application
    create_resource_group
    deploy_infrastructure
    deploy_backend
    deploy_functions
    verify_deployment
    cleanup
    show_summary
}

# Handle script arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --resource-group)
            RESOURCE_GROUP="$2"
            shift 2
            ;;
        --location)
            LOCATION="$2"
            shift 2
            ;;
        --environment)
            ENVIRONMENT="$2"
            shift 2
            ;;
        --app-name)
            APP_NAME="$2"
            shift 2
            ;;
        --help)
            echo "Usage: $0 [OPTIONS]"
            echo ""
            echo "Options:"
            echo "  --resource-group NAME    Azure resource group name (default: rg-prodigy-prod)"
            echo "  --location LOCATION      Azure region (default: australiaeast)"
            echo "  --environment ENV        Environment name (default: prod)"
            echo "  --app-name NAME          Application name (default: prodigy)"
            echo "  --help                   Show this help message"
            exit 0
            ;;
        *)
            print_error "Unknown option: $1"
            echo "Use --help for usage information"
            exit 1
            ;;
    esac
done

# Run main function
main