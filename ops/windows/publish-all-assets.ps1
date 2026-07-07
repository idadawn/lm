#Requires -Version 5.1
<#
.SYNOPSIS
    Publish every deployment asset required by the WPF deployment console.
.DESCRIPTION
    Uploads prerequisite software and then publishes a full release package.
    The full release package contains service artifacts and ops scripts from
    DEPLOY_ROOT, while prerequisites are stored separately under PREREQ_PREFIX.
#>
param(
    [string]$Version = "",
    [switch]$SkipPrereqs,
    [switch]$SkipDesktopBuild
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot

if (-not $Version) { $Version = (Get-Date).ToString("yyyyMMddHHmmss") }

if (-not $SkipDesktopBuild) {
    $desktopBuilder = Join-Path $ScriptDir "build-desktop-console.ps1"
    if (Test-Path $desktopBuilder) {
    Write-Host "### [1/4] Build desktop console ###" -ForegroundColor Cyan
        & $desktopBuilder -SelfContained
        if ($LASTEXITCODE -ne 0) { throw "build-desktop-console.ps1 failed with exit code $LASTEXITCODE" }
    }
}

if (-not $SkipPrereqs) {
    Write-Host ""
    Write-Host "### [2/4] Publish prerequisite software ###" -ForegroundColor Cyan
    & (Join-Path $ScriptDir "publish-prereqs.ps1")
    if ($LASTEXITCODE -ne 0) { throw "publish-prereqs.ps1 failed with exit code $LASTEXITCODE" }
}

Write-Host ""
Write-Host "### [3/4] Publish bootstrap console package ###" -ForegroundColor Cyan
& (Join-Path $ScriptDir "publish-console-package.ps1")
if ($LASTEXITCODE -ne 0) { throw "publish-console-package.ps1 failed with exit code $LASTEXITCODE" }

Write-Host ""
Write-Host "### [4/4] Publish full release package ###" -ForegroundColor Cyan
& (Join-Path $ScriptDir "publish-release.ps1") -PackageMode Full -Version $Version
if ($LASTEXITCODE -ne 0) { throw "publish-release.ps1 failed with exit code $LASTEXITCODE" }

Write-Host ""
Write-Host "All deployment assets published. Version: $Version" -ForegroundColor Green
