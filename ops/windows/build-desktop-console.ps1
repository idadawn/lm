#Requires -Version 5.1
<#
.SYNOPSIS
    Build the native Windows deployment console and copy it into DEPLOY_ROOT\ops.
#>
param(
    [switch]$SelfContained
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
. (Join-Path $ScriptDir "_dotenv.ps1")
$envMap = Get-DotEnv -Path (Join-Path $ScriptDir ".env")
function Val($k, $d = "") { Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key $k -Default $d }

$repoRoot = Split-Path -Parent (Split-Path -Parent $ScriptDir)
$project = Join-Path $repoRoot "ops\desktop\LmDeployConsole\LmDeployConsole.csproj"
$deployRootRaw = Val "DEPLOY_ROOT" "D:\deploy\lab"
New-Item -ItemType Directory -Force -Path $deployRootRaw | Out-Null
$deployRoot = (Resolve-Path $deployRootRaw).Path
$opsOut = Join-Path $deployRoot "ops"
New-Item -ItemType Directory -Force -Path $opsOut | Out-Null

$publishDir = Join-Path $repoRoot "ops\desktop\LmDeployConsole\bin\publish\win-x64"
$args = @("publish", $project, "-c", "Release", "-r", "win-x64", "-o", $publishDir, "--nologo")
if ($SelfContained) {
    $args += "--self-contained"
    $args += "true"
} else {
    $args += "--self-contained"
    $args += "false"
}

Write-Host "==> dotnet $($args -join ' ')" -ForegroundColor Cyan
& dotnet @args
if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed (exit $LASTEXITCODE)" }

$exe = Join-Path $publishDir "LmDeployConsole.exe"
if (-not (Test-Path $exe)) { throw "Missing publish output: $exe" }

$consoleOut = Join-Path $opsOut "LmDeployConsole"
if (Test-Path $consoleOut) { Remove-Item -LiteralPath $consoleOut -Recurse -Force }
New-Item -ItemType Directory -Force -Path $consoleOut | Out-Null
Copy-Item -Path (Join-Path $publishDir "*") -Destination $consoleOut -Recurse -Force

$oldSingleExe = Join-Path $opsOut "LmDeployConsole.exe"
if (Test-Path $oldSingleExe) { Remove-Item -LiteralPath $oldSingleExe -Force }

$launcher = Join-Path $opsOut "LmDeployConsole.cmd"
@"
@echo off
setlocal
start "" "%~dp0LmDeployConsole\LmDeployConsole.exe"
"@ | Set-Content -Path $launcher -Encoding ASCII

Write-Host "Desktop console ready: $consoleOut\LmDeployConsole.exe" -ForegroundColor Green
Write-Host "Launcher ready       : $launcher" -ForegroundColor Green
