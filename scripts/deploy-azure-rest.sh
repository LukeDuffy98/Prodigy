#!/bin/bash

# Prodigy Azure Deployment Script using REST API
# This script uses direct Azure REST API calls for enhanced error handling

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

get_access_token() {
    print_header "Getting Access Token"
    
    # Get access token using Azure CLI
    ACCESS_TOKEN=$(az account get-access-token --query accessToken --output tsv)
    if [[ -z "$ACCESS_TOKEN" ]]; then
        print_error "Failed to get access token. Please ensure you're logged in with 'az login'"
        exit 1
    fi
    
    # Get subscription ID
    SUBSCRIPTION_ID=$(az account show --query id --output tsv)
    if [[ -z "$SUBSCRIPTION_ID" ]]; then
        print_error "Failed to get subscription ID"
        exit 1
    fi
    
    print_success "Access token obtained for subscription: $SUBSCRIPTION_ID"
}

deploy_with_rest_api() {
    print_header "Deploying with Azure REST API"
    
    # Generate unique deployment name
    DEPLOYMENT_NAME="prodigy-deployment-$(date +%Y%m%d%H%M%S)"
    
    # Create deployment payload using --argjson for proper JSON structure
    DEPLOYMENT_PAYLOAD=$(jq -n \
        --arg templateUri "$(cat azure/main.bicep | base64 -w 0)" \
        --argjson parameters '{
            "appName": {"value": "'"$APP_NAME"'"},
            "environment": {"value": "'"$ENVIRONMENT"'"},
            "location": {"value": "'"$LOCATION"'"},
            "appServicePlanSku": {"value": "B1"},
            "azureTenantId": {"value": "'"$AZURE_TENANT_ID"'"},
            "azureClientId": {"value": "'"$AZURE_CLIENT_ID"'"},
            "azureClientSecret": {"value": "'"$AZURE_CLIENT_SECRET"'"},
            "jwtSecretKey": {"value": "'"$JWT_SECRET_KEY"'"},
            "githubToken": {"value": "'"$GITHUB_TOKEN"'"},
            "linkedinClientId": {"value": "'"$LINKEDIN_CLIENT_ID"'"},
            "linkedinClientSecret": {"value": "'"$LINKEDIN_CLIENT_SECRET"'"}
        }' \
        '{
            "properties": {
                "template": $templateUri,
                "parameters": $parameters,
                "mode": "Incremental"
            }
        }')
    
    # First, compile Bicep to ARM template
    echo "Compiling Bicep template to ARM..."
    ARM_TEMPLATE=$(az bicep build --file azure/main.bicep --stdout)
    
    # Create proper deployment payload
    DEPLOYMENT_PAYLOAD=$(echo "$ARM_TEMPLATE" | jq \
        --argjson parameters '{
            "appName": {"value": "'"$APP_NAME"'"},
            "environment": {"value": "'"$ENVIRONMENT"'"},
            "location": {"value": "'"$LOCATION"'"},
            "appServicePlanSku": {"value": "B1"},
            "azureTenantId": {"value": "'"$AZURE_TENANT_ID"'"},
            "azureClientId": {"value": "'"$AZURE_CLIENT_ID"'"},
            "azureClientSecret": {"value": "'"$AZURE_CLIENT_SECRET"'"},
            "jwtSecretKey": {"value": "'"$JWT_SECRET_KEY"'"},
            "githubToken": {"value": "'"$GITHUB_TOKEN"'"},
            "linkedinClientId": {"value": "'"$LINKEDIN_CLIENT_ID"'"},
            "linkedinClientSecret": {"value": "'"$LINKEDIN_CLIENT_SECRET"'"}
        }' \
        '{
            "properties": {
                "template": .,
                "parameters": $parameters,
                "mode": "Incremental"
            }
        }')
    
    echo "Deployment payload prepared"
    echo "Starting deployment: $DEPLOYMENT_NAME"
    echo "Parameters:"
    echo "  App Service Plan SKU: B1"
    echo "  Resource Group: $RESOURCE_GROUP"
    echo "  Location: $LOCATION"
    
    # Start deployment using REST API
    DEPLOYMENT_URL="https://management.azure.com/subscriptions/$SUBSCRIPTION_ID/resourcegroups/$RESOURCE_GROUP/providers/Microsoft.Resources/deployments/$DEPLOYMENT_NAME?api-version=2021-04-01"
    
    DEPLOYMENT_RESPONSE=$(curl -s \
        -X PUT \
        -H "Authorization: Bearer $ACCESS_TOKEN" \
        -H "Content-Type: application/json" \
        -d "$DEPLOYMENT_PAYLOAD" \
        "$DEPLOYMENT_URL")
    
    # Check for deployment start errors
    if echo "$DEPLOYMENT_RESPONSE" | jq -e '.error' > /dev/null; then
        print_error "Failed to start deployment"
        echo "$DEPLOYMENT_RESPONSE" | jq '.error'
        exit 1
    fi
    
    print_success "Deployment started successfully"
    
    # Poll for completion
    echo "Monitoring deployment progress..."
    max_wait_time=1800  # 30 minutes
    wait_interval=30    # 30 seconds
    elapsed_time=0
    
    while [ $elapsed_time -lt $max_wait_time ]; do
        # Get deployment status
        STATUS_RESPONSE=$(curl -s \
            -H "Authorization: Bearer $ACCESS_TOKEN" \
            "$DEPLOYMENT_URL")
        
        DEPLOYMENT_STATUS=$(echo "$STATUS_RESPONSE" | jq -r '.properties.provisioningState // "Unknown"')
        
        echo "Deployment status: $DEPLOYMENT_STATUS (${elapsed_time}s elapsed)"
        
        if [ "$DEPLOYMENT_STATUS" = "Succeeded" ]; then
            print_success "Deployment completed successfully!"
            
            # Extract outputs
            BACKEND_URL=$(echo "$STATUS_RESPONSE" | jq -r '.properties.outputs.backendUrl.value // ""')
            FRONTEND_URL=$(echo "$STATUS_RESPONSE" | jq -r '.properties.outputs.frontendUrl.value // ""')
            FUNCTIONS_URL=$(echo "$STATUS_RESPONSE" | jq -r '.properties.outputs.functionsUrl.value // ""')
            
            echo "Deployment outputs:"
            echo "  Backend URL: $BACKEND_URL"
            echo "  Frontend URL: $FRONTEND_URL"
            echo "  Functions URL: $FUNCTIONS_URL"
            
            return 0
        elif [ "$DEPLOYMENT_STATUS" = "Failed" ] || [ "$DEPLOYMENT_STATUS" = "Canceled" ]; then
            print_error "Deployment failed with status: $DEPLOYMENT_STATUS"
            echo "Error details:"
            echo "$STATUS_RESPONSE" | jq '.properties.error // "No error details available"'
            exit 1
        fi
        
        sleep $wait_interval
        elapsed_time=$((elapsed_time + wait_interval))
    done
    
    print_error "Deployment timed out after ${max_wait_time} seconds"
    exit 1
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
        exit 1
    fi
    
    print_success "All required environment variables are set"
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

# Main execution
main() {
    echo -e "${BLUE}🚀 Starting Prodigy Azure Deployment (REST API)${NC}"
    echo ""
    
    # Load .env file if it exists
    if [[ -f .env ]]; then
        print_warning "Loading environment variables from .env file"
        export $(grep -v '^#' .env | xargs)
    fi
    
    validate_environment
    get_access_token
    create_resource_group
    deploy_with_rest_api
    
    print_success "🎉 Deployment completed using REST API approach!"
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
            echo "This script uses Azure REST API for deployment with enhanced error handling"
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