#!/bin/bash

# Azure REST API-based deployment script for Prodigy
echo "🚀 Starting REST API-based deployment..."

# Environment variables validation
required_vars=("AZURE_TENANT_ID" "AZURE_CLIENT_ID" "AZURE_CLIENT_SECRET" "AZURE_SUBSCRIPTION_ID" "AZURE_RESOURCE_GROUP" "AZURE_LOCATION")
for var in "${required_vars[@]}"; do
  if [ -z "${!var}" ]; then
    echo "❌ Error: $var environment variable is not set"
    exit 1
  fi
done

# Get access token
echo "🔐 Getting Azure access token..."
ACCESS_TOKEN=$(curl -s -X POST \
  "https://login.microsoftonline.com/${AZURE_TENANT_ID}/oauth2/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "grant_type=client_credentials" \
  -d "client_id=${AZURE_CLIENT_ID}" \
  -d "client_secret=${AZURE_CLIENT_SECRET}" \
  -d "resource=https://management.azure.com/" | \
  jq -r '.access_token')

if [ "$ACCESS_TOKEN" == "null" ] || [ -z "$ACCESS_TOKEN" ]; then
  echo "❌ Failed to get access token"
  exit 1
fi

echo "✅ Access token obtained"

# Create resource group
echo "🏗️ Creating resource group..."
curl -s -X PUT \
  "https://management.azure.com/subscriptions/${AZURE_SUBSCRIPTION_ID}/resourcegroups/${AZURE_RESOURCE_GROUP}?api-version=2021-04-01" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"location\": \"${AZURE_LOCATION}\",
    \"tags\": {
      \"Environment\": \"${ENVIRONMENT:-prod}\",
      \"Project\": \"Prodigy\"
    }
  }" > /dev/null

echo "✅ Resource group created/updated"

# Compile Bicep to ARM
echo "📋 Compiling Bicep template..."
bicep build azure/main.bicep --outfile /tmp/main.json

if [ $? -ne 0 ]; then
  echo "❌ Failed to compile Bicep template"
  exit 1
fi

echo "✅ Bicep template compiled successfully"

# Create parameters for the deployment
echo "📄 Preparing deployment parameters..."
DEPLOYMENT_PARAMS=$(jq -n \
  --arg appName "${APP_NAME:-prodigy}" \
  --arg location "${AZURE_LOCATION}" \
  --arg environment "${ENVIRONMENT:-prod}" \
  --arg appServicePlanSku "${APP_SERVICE_PLAN_SKU:-B1}" \
  --arg azureTenantId "${AZURE_TENANT_ID}" \
  --arg azureClientId "${AZURE_CLIENT_ID}" \
  --arg azureClientSecret "${AZURE_CLIENT_SECRET}" \
  --arg jwtSecretKey "${JWT_SECRET_KEY:-}" \
  --arg githubToken "${GITHUB_TOKEN}" \
  --arg linkedinClientId "${LINKEDIN_CLIENT_ID:-}" \
  --arg linkedinClientSecret "${LINKEDIN_CLIENT_SECRET:-}" \
  --argjson template "$(cat /tmp/main.json)" \
  '{
    "properties": {
      "template": $template,
      "parameters": {
        "appName": {"value": $appName},
        "location": {"value": $location},
        "environment": {"value": $environment},
        "appServicePlanSku": {"value": $appServicePlanSku},
        "azureTenantId": {"value": $azureTenantId},
        "azureClientId": {"value": $azureClientId},
        "azureClientSecret": {"value": $azureClientSecret},
        "jwtSecretKey": {"value": $jwtSecretKey},
        "githubToken": {"value": $githubToken},
        "linkedinClientId": {"value": $linkedinClientId},
        "linkedinClientSecret": {"value": $linkedinClientSecret}
      },
      "mode": "Incremental"
    }
  }')

# Deploy the template
echo "🚀 Deploying infrastructure via REST API..."
DEPLOYMENT_NAME="prodigy-deployment-$(date +%s)"

echo "🔧 Debug: Deployment name: $DEPLOYMENT_NAME"
echo "🔧 Debug: Resource Group: $AZURE_RESOURCE_GROUP"
echo "🔧 Debug: Parameters size: $(echo "$DEPLOYMENT_PARAMS" | wc -c) bytes"

DEPLOYMENT_RESPONSE=$(curl -s -X PUT \
  "https://management.azure.com/subscriptions/${AZURE_SUBSCRIPTION_ID}/resourcegroups/${AZURE_RESOURCE_GROUP}/providers/Microsoft.Resources/deployments/${DEPLOYMENT_NAME}?api-version=2021-04-01" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d "$DEPLOYMENT_PARAMS")

echo "🔧 Debug: Deployment creation response:"
echo "$DEPLOYMENT_RESPONSE" | head -c 1000

# Check if deployment creation was successful
DEPLOYMENT_STATUS=$(echo "$DEPLOYMENT_RESPONSE" | jq -r '.properties.provisioningState // "Unknown"')
DEPLOYMENT_ERROR=$(echo "$DEPLOYMENT_RESPONSE" | jq -r '.error.code // "None"')

if [ "$DEPLOYMENT_ERROR" != "None" ]; then
  echo "❌ Failed to create deployment: $DEPLOYMENT_ERROR"
  echo "$DEPLOYMENT_RESPONSE" | jq '.error // empty'
  exit 1
fi

echo "📊 Deployment initiated with status: $DEPLOYMENT_STATUS"

# Wait for deployment to complete
echo "⏳ Waiting for deployment to complete..."
max_wait_time=1800  # 30 minutes
wait_interval=30    # 30 seconds
elapsed_time=0

while [ $elapsed_time -lt $max_wait_time ]; do
  sleep $wait_interval
  elapsed_time=$((elapsed_time + wait_interval))
  
  RESPONSE=$(curl -s -X GET \
    "https://management.azure.com/subscriptions/${AZURE_SUBSCRIPTION_ID}/resourcegroups/${AZURE_RESOURCE_GROUP}/providers/Microsoft.Resources/deployments/${DEPLOYMENT_NAME}?api-version=2021-04-01" \
    -H "Authorization: Bearer $ACCESS_TOKEN")
  
  # Check if we got an error response
  ERROR_CODE=$(echo "$RESPONSE" | jq -r '.error.code // "None"')
  if [ "$ERROR_CODE" == "DeploymentNotFound" ]; then
    echo "⚠️ Deployment not found yet (elapsed: ${elapsed_time}s) - deployment might still be starting..."
    continue
  elif [ "$ERROR_CODE" != "None" ]; then
    echo "❌ Error checking deployment status: $ERROR_CODE"
    echo "$RESPONSE" | jq '.error // empty'
    exit 1
  fi
  
  STATUS=$(echo "$RESPONSE" | jq -r '.properties.provisioningState // "Unknown"')
  
  echo "Status: $STATUS (${elapsed_time}s elapsed)"
  
  # Debug: Print full response if status is Unknown for the first few attempts
  if [ "$STATUS" == "Unknown" ] && [ $elapsed_time -le 90 ]; then
    echo "Debug response: $RESPONSE" | head -c 500
  fi
  
  if [ "$STATUS" == "Succeeded" ]; then
    echo "✅ Infrastructure deployment completed successfully"
    break
  elif [ "$STATUS" == "Failed" ]; then
    echo "❌ Infrastructure deployment failed"
    # Get error details
    echo "$RESPONSE" | jq '.properties.error // empty'
    exit 1
  fi
done

if [ $elapsed_time -ge $max_wait_time ]; then
  echo "⏰ Deployment timeout - checking final status..."
  FINAL_RESPONSE=$(curl -s -X GET \
    "https://management.azure.com/subscriptions/${AZURE_SUBSCRIPTION_ID}/resourcegroups/${AZURE_RESOURCE_GROUP}/providers/Microsoft.Resources/deployments/${DEPLOYMENT_NAME}?api-version=2021-04-01" \
    -H "Authorization: Bearer $ACCESS_TOKEN")

  FINAL_STATUS=$(echo "$FINAL_RESPONSE" | jq -r '.properties.provisioningState // "Unknown"')

  if [ "$FINAL_STATUS" != "Succeeded" ]; then
    echo "❌ Infrastructure deployment failed or timed out: $FINAL_STATUS"
    if [ "$FINAL_STATUS" == "Failed" ]; then
      echo "$FINAL_RESPONSE" | jq '.properties.error // empty'
    fi
    exit 1
  fi
fi

echo "🎯 Retrieving deployment outputs..."

# Retrieve deployment outputs after successful completion
DEPLOYMENT_OUTPUT=$(curl -s -X GET \
  "https://management.azure.com/subscriptions/${AZURE_SUBSCRIPTION_ID}/resourcegroups/${AZURE_RESOURCE_GROUP}/providers/Microsoft.Resources/deployments/${DEPLOYMENT_NAME}?api-version=2021-04-01" \
  -H "Authorization: Bearer $ACCESS_TOKEN" | jq '.properties.outputs // {}')

if [ "$DEPLOYMENT_OUTPUT" == "{}" ] || [ "$DEPLOYMENT_OUTPUT" == "null" ]; then
  echo "❌ Error: Could not retrieve deployment outputs"
  exit 1
fi

echo "✅ Deployment outputs retrieved successfully"

# Extract individual output values and set them as GitHub outputs
backend_url=$(echo $DEPLOYMENT_OUTPUT | jq -r '.backendUrl.value // empty')
frontend_url=$(echo $DEPLOYMENT_OUTPUT | jq -r '.frontendUrl.value // empty')
functions_url=$(echo $DEPLOYMENT_OUTPUT | jq -r '.functionsUrl.value // empty')

if [ -z "$backend_url" ] || [ -z "$frontend_url" ] || [ -z "$functions_url" ]; then
  echo "❌ Error: One or more required outputs are missing"
  echo "Backend URL: $backend_url"
  echo "Frontend URL: $frontend_url"
  echo "Functions URL: $functions_url"
  exit 1
fi

# Set GitHub outputs if running in GitHub Actions
if [ -n "$GITHUB_OUTPUT" ]; then
  echo "backendUrl=$backend_url" >> $GITHUB_OUTPUT
  echo "frontendUrl=$frontend_url" >> $GITHUB_OUTPUT
  echo "functionsUrl=$functions_url" >> $GITHUB_OUTPUT
fi

echo "🎉 Successfully completed deployment via REST API:"
echo "  🌐 Backend URL: $backend_url"
echo "  🌐 Frontend URL: $frontend_url"
echo "  🌐 Functions URL: $functions_url"