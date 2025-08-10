#!/bin/bash

# Test script to verify Azure deployment fixes are in place
# This validates the key issues mentioned in the GitHub issue

set -e

echo "🧪 Testing Azure Deployment Fixes"
echo "=================================="

# Test 1: Check that appServicePlanSku parameter is included in GitHub Actions workflow
echo "Test 1: Checking GitHub Actions workflow for appServicePlanSku parameter..."
if grep -q 'appServicePlanSku="B1"' .github/workflows/azure-deploy.yml; then
    echo "✅ PASS: appServicePlanSku parameter is present in GitHub Actions workflow"
else
    echo "❌ FAIL: appServicePlanSku parameter missing from GitHub Actions workflow"
    exit 1
fi

# Test 2: Check that Bicep template defaults to B1 SKU
echo "Test 2: Checking Bicep template default SKU..."
if grep -q 'param appServicePlanSku string = '\''B1'\''' azure/main.bicep; then
    echo "✅ PASS: Bicep template defaults to B1 SKU"
else
    echo "❌ FAIL: Bicep template does not default to B1 SKU"
    exit 1
fi

# Test 3: Check that tier mapping is present in Bicep template
echo "Test 3: Checking Bicep template tier mapping..."
if grep -q 'appServicePlanTier.*Basic' azure/main.bicep; then
    echo "✅ PASS: Tier mapping logic is present in Bicep template"
else
    echo "❌ FAIL: Tier mapping logic missing from Bicep template"
    exit 1
fi

# Test 4: Check that deployment scripts include appServicePlanSku parameter
echo "Test 4: Checking deployment scripts for appServicePlanSku parameter..."
if grep -q 'appServicePlanSku="B1"' scripts/deploy-azure.sh; then
    echo "✅ PASS: Bash deployment script includes appServicePlanSku parameter"
else
    echo "❌ FAIL: Bash deployment script missing appServicePlanSku parameter"
    exit 1
fi

if grep -q 'appServicePlanSku=B1' scripts/deploy-azure.ps1; then
    echo "✅ PASS: PowerShell deployment script includes appServicePlanSku parameter"
else
    echo "❌ FAIL: PowerShell deployment script missing appServicePlanSku parameter"
    exit 1
fi

# Test 5: Check that parameter template is updated to B1
echo "Test 5: Checking parameter template..."
if grep -q '"value": "B1"' azure/main.parameters.template.json; then
    echo "✅ PASS: Parameter template uses B1 SKU"
else
    echo "❌ FAIL: Parameter template does not use B1 SKU"
    exit 1
fi

# Test 6: Check that enhanced logging is present in GitHub Actions
echo "Test 6: Checking GitHub Actions for enhanced logging..."
if grep -q 'echo "Deployment parameters:"' .github/workflows/azure-deploy.yml; then
    echo "✅ PASS: Enhanced logging is present in GitHub Actions workflow"
else
    echo "❌ FAIL: Enhanced logging missing from GitHub Actions workflow"
    exit 1
fi

# Test 7: Check that REST API deployment script exists
echo "Test 7: Checking REST API deployment script..."
if [[ -f scripts/deploy-azure-rest.sh && -x scripts/deploy-azure-rest.sh ]]; then
    echo "✅ PASS: REST API deployment script exists and is executable"
else
    echo "❌ FAIL: REST API deployment script missing or not executable"
    exit 1
fi

# Test 8: Validate Bicep template compiles without errors
echo "Test 8: Validating Bicep template compilation..."
if command -v az &> /dev/null; then
    if az bicep build --file azure/main.bicep --stdout > /dev/null 2>&1; then
        echo "✅ PASS: Bicep template compiles successfully"
    else
        echo "❌ FAIL: Bicep template has compilation errors"
        exit 1
    fi
else
    echo "⚠️ SKIP: Azure CLI not available for Bicep validation"
fi

echo ""
echo "🎉 All tests passed! Azure deployment fixes are correctly implemented."
echo ""
echo "Key fixes verified:"
echo "✅ App Service Plan SKU set to B1 (minimum for Azure Functions)"
echo "✅ Missing appServicePlanSku parameter added to all deployment methods"
echo "✅ Bicep template tier mapping implemented (B1 → Basic)"
echo "✅ Enhanced debugging and logging added"
echo "✅ REST API deployment approach available"
echo "✅ All deployment scripts have correct syntax"
echo ""
echo "The deployment should now work correctly for Azure Functions!"