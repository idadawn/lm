#!/usr/bin/env pwsh
# ============================================
# Only Build API Script for Windows
# Usage: .\build-api.ps1
# ============================================

$ErrorActionPreference = "Stop"

# Configuration
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$PROJECT_ROOT = $SCRIPT_DIR
$API_PROJECT_PATH = Join-Path $PROJECT_ROOT "api\src\application\Poxiao.API.Entry\Poxiao.API.Entry.csproj"
$OUTPUT_DIR = Join-Path $PROJECT_ROOT "apps\api"
$ZIP_OUTPUT_DIR = Join-Path $PROJECT_ROOT "apps"
$TIMESTAMP = Get-Date -Format "yyyyMMdd_HHmmss"
$ZIP_FILE = Join-Path $ZIP_OUTPUT_DIR "api_$TIMESTAMP.zip"

# Colors
function Write-Info($message) {
    Write-Host "[INFO] $message" -ForegroundColor Green
}

function Write-Error($message) {
    Write-Host "[ERROR] $message" -ForegroundColor Red
}

# Check dotnet
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Error "dotnet command not found. Please install .NET SDK."
    exit 1
}

# Check project file
if (-not (Test-Path $API_PROJECT_PATH)) {
    Write-Error "Project file not found: $API_PROJECT_PATH"
    exit 1
}

Write-Info "Building API..."
Write-Info "Project: $API_PROJECT_PATH"
Write-Info "Output: $OUTPUT_DIR"

# Clean output directory
if (Test-Path $OUTPUT_DIR) {
    Write-Info "Cleaning output directory..."
    Remove-Item $OUTPUT_DIR -Recurse -Force
}

# Ensure output directory exists
New-Item -ItemType Directory -Force -Path $OUTPUT_DIR | Out-Null

# Publish
Write-Info "Publishing..."
dotnet publish $API_PROJECT_PATH `
    -c Release `
    -o $OUTPUT_DIR `
    --nologo

if ($LASTEXITCODE -eq 0) {
    Write-Info "Build successful!"
    Write-Info "Artifacts are in: $OUTPUT_DIR"
    
    # Create ZIP archive
    Write-Info "Creating ZIP archive..."
    Compress-Archive -Path "$OUTPUT_DIR\*" -DestinationPath $ZIP_FILE -Force
    
    if (Test-Path $ZIP_FILE) {
        $zipSize = (Get-Item $ZIP_FILE).Length / 1MB
        Write-Info "ZIP archive created successfully!"
        Write-Info "Location: $ZIP_FILE"
        Write-Info "Size: $([math]::Round($zipSize, 2)) MB"
    } else {
        Write-Error "Failed to create ZIP archive"
        exit 1
    }
} else {
    Write-Error "Build failed."
    exit 1
}

Write-Info "Done!"
