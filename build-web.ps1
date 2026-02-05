#!/usr/bin/env pwsh
# ============================================
# Only Build Web Script for Windows
# Usage: .\build-web.ps1
# ============================================

$ErrorActionPreference = "Stop"

# Configuration
$SCRIPT_DIR = Split-Path -Parent $MyInvocation.MyCommand.Path
$PROJECT_ROOT = $SCRIPT_DIR
$WEB_PROJECT_PATH = Join-Path $PROJECT_ROOT "web"
$OUTPUT_DIR = Join-Path $PROJECT_ROOT "apps\dist"
$ZIP_OUTPUT_DIR = Join-Path $PROJECT_ROOT "apps"
$TIMESTAMP = Get-Date -Format "yyyyMMdd_HHmmss"
$ZIP_FILE = Join-Path $ZIP_OUTPUT_DIR "web_$TIMESTAMP.zip"

# Colors
function Write-Info($message) {
    Write-Host "[INFO] $message" -ForegroundColor Green
}

function Write-Error($message) {
    Write-Host "[ERROR] $message" -ForegroundColor Red
}

function Write-Warn($message) {
    Write-Host "[WARN] $message" -ForegroundColor Yellow
}

# Check Node.js
$node = Get-Command node -ErrorAction SilentlyContinue
if (-not $node) {
    Write-Error "node command not found. Please install Node.js >= 16.15.0."
    exit 1
}

$NODE_VERSION = (node -v) -replace '^v' -replace '\..*$'
if ([int]$NODE_VERSION -lt 16) {
    Write-Error "Node.js version must be >= 16.15.0. Current version: $(node -v)"
    exit 1
}

Write-Info "Node.js version: $(node -v)"

# Check pnpm
$pnpm = Get-Command pnpm -ErrorAction SilentlyContinue
if (-not $pnpm) {
    Write-Error "pnpm command not found. Please install pnpm >= 8.1.0."
    Write-Info "Install pnpm: npm install -g pnpm"
    exit 1
}

Write-Info "pnpm version: $(pnpm -v)"

# Check project directory
if (-not (Test-Path $WEB_PROJECT_PATH)) {
    Write-Error "Web project directory not found: $WEB_PROJECT_PATH"
    exit 1
}

# Check package.json
$PACKAGE_JSON = Join-Path $WEB_PROJECT_PATH "package.json"
if (-not (Test-Path $PACKAGE_JSON)) {
    Write-Error "package.json not found in ${WEB_PROJECT_PATH}"
    exit 1
}

Write-Info "Building Web..."
Write-Info "Project: $WEB_PROJECT_PATH"
Write-Info "Output: $OUTPUT_DIR"

# Clean output directory
if (Test-Path $OUTPUT_DIR) {
    Write-Info "Cleaning output directory..."
    Remove-Item $OUTPUT_DIR -Recurse -Force
}

New-Item -ItemType Directory -Force -Path $OUTPUT_DIR | Out-Null

# Install dependencies
Set-Location $WEB_PROJECT_PATH

Write-Info "Installing dependencies with parallel download..."
pnpm install --frozen-lockfile --prefer-offline --reporter=silent

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to install dependencies"
    exit 1
}

# Build
Set-Location $WEB_PROJECT_PATH

# Clean cache before build
Write-Info "Cleaning build cache..."
$cachePaths = @(
    "node_modules\.vite",
    "node_modules\.cache",
    "dist",
    ".turbo"
)
foreach ($path in $cachePaths) {
    $fullPath = Join-Path $WEB_PROJECT_PATH $path
    if (Test-Path $fullPath) {
        Remove-Item $fullPath -Recurse -Force -ErrorAction SilentlyContinue
    }
}

# Set environment variables for optimization
$env:NODE_OPTIONS = "--max-old-space-size=8192"
$env:VITE_TSC = "false"
$env:VITE_SOURCE_MAP = "false"

# Build with fast mode
Write-Info "Building with high-performance settings..."

# Try build:fast first, fallback to build
try {
    pnpm build:fast
} catch {
    Write-Warn "build:fast failed, trying standard build..."
    pnpm build
}

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed."
    exit 1
}

# Check if dist directory exists
$WEB_DIST = Join-Path $WEB_PROJECT_PATH "dist"
if (-not (Test-Path $WEB_DIST)) {
    Write-Error "Build failed. dist directory not found."
    exit 1
}

# Copy dist to output directory
Write-Info "Copying build artifacts to ${OUTPUT_DIR}..."
Copy-Item -Path "$WEB_DIST\*" -Destination $OUTPUT_DIR -Recurse -Force

if ($LASTEXITCODE -eq 0) {
    Write-Info "Build successful!"
    Write-Info "Artifacts are in: $OUTPUT_DIR"
    
    # List built files
    Write-Info "Built files:"
    Get-ChildItem $OUTPUT_DIR | ForEach-Object { Write-Info "  $($_.Name)" }
    
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
