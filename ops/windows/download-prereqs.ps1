#Requires -Version 5.1
<#
.SYNOPSIS
    Download portable prerequisites into DEPLOY_ROOT for Windows deployment.
.DESCRIPTION
    Downloads and places the software expected by install-infra.ps1 and
    install-services.ps1: NSSM, nginx, Redis, Erlang installer, RabbitMQ zip.
    URLs can be overridden from ops\.env.
#>
param(
    [ValidateSet("all", "nssm", "nginx", "redis", "erlang", "rabbitmq")]
    [string[]]$Only = @("all"),
    [switch]$Force
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
. (Join-Path $ScriptDir "_dotenv.ps1")

$envMap = Get-DotEnv -Path (Join-Path $ScriptDir ".env")
function Val($k, $d = "") { Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key $k -Default $d }
function Wants($name) { return ($Only -contains "all" -or $Only -contains $name) }

$DeployRootRaw = Val "DEPLOY_ROOT" "D:\deploy\lab"
New-Item -ItemType Directory -Force -Path $DeployRootRaw | Out-Null
$DeployRoot = (Resolve-Path $DeployRootRaw).Path
$DownloadDir = Join-Path $DeployRoot "_downloads"
New-Item -ItemType Directory -Force -Path $DownloadDir | Out-Null

$NssmZipUrl = Val "NSSM_ZIP_URL" "https://nssm.cc/release/nssm-2.24.zip"
$NginxZipUrl = Val "NGINX_ZIP_URL" "http://nginx.org/download/nginx-1.27.4.zip"
$RedisZipUrl = Val "REDIS_ZIP_URL" "https://github.com/tporadowski/redis/releases/download/v5.0.14.1/Redis-x64-5.0.14.1.zip"
$ErlangOtpUrl = Val "ERLANG_OTP_URL" "https://github.com/erlang/otp/releases/download/OTP-26.2.5/otp_win64_26.2.5.exe"
$RabbitMqZipUrl = Val "RABBITMQ_ZIP_URL" "https://github.com/rabbitmq/rabbitmq-server/releases/download/v3.13.7/rabbitmq-server-windows-3.13.7.zip"

function Download-File {
    param(
        [Parameter(Mandatory = $true)][string]$Url,
        [Parameter(Mandatory = $true)][string]$OutFile
    )
    if ((Test-Path $OutFile) -and -not $Force) {
        Write-Host "  skip: $OutFile" -ForegroundColor DarkGray
        return
    }
    Write-Host "  download: $Url" -ForegroundColor DarkGray
    Invoke-WebRequest -Uri $Url -OutFile $OutFile -UseBasicParsing -TimeoutSec 900
}

function Expand-ZipContent {
    param(
        [Parameter(Mandatory = $true)][string]$ZipPath,
        [Parameter(Mandatory = $true)][string]$Destination
    )
    $tmp = Join-Path $DownloadDir ([IO.Path]::GetFileNameWithoutExtension($ZipPath) + "-extract")
    if (Test-Path $tmp) { Remove-Item -LiteralPath $tmp -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $tmp | Out-Null
    Expand-Archive -Path $ZipPath -DestinationPath $tmp -Force

    $roots = Get-ChildItem -Path $tmp -Force
    $source = $tmp
    if ($roots.Count -eq 1 -and $roots[0].PSIsContainer) { $source = $roots[0].FullName }
    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    Copy-Item -Path (Join-Path $source "*") -Destination $Destination -Recurse -Force
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  LM Windows prerequisites downloader" -ForegroundColor Cyan
Write-Host "  DEPLOY_ROOT: $DeployRoot" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

if (Wants "nssm") {
    Write-Host "[nssm]" -ForegroundColor Yellow
    $target = Join-Path $DeployRoot "nssm.exe"
    if ((Test-Path $target) -and -not $Force) {
        Write-Host "  exists: $target" -ForegroundColor DarkGray
    } else {
        $zip = Join-Path $DownloadDir "nssm.zip"
        Download-File -Url $NssmZipUrl -OutFile $zip
        $tmp = Join-Path $DownloadDir "nssm-extract"
        if (Test-Path $tmp) { Remove-Item -LiteralPath $tmp -Recurse -Force }
        Expand-Archive -Path $zip -DestinationPath $tmp -Force
        $exe = Get-ChildItem -Path $tmp -Recurse -Filter "nssm.exe" |
            Where-Object { $_.FullName -match "win64" } |
            Select-Object -First 1
        if (-not $exe) { throw "nssm.exe not found in $zip" }
        Copy-Item -Path $exe.FullName -Destination $target -Force
        Write-Host "  ready: $target" -ForegroundColor Green
    }
}

if (Wants "nginx") {
    Write-Host "[nginx]" -ForegroundColor Yellow
    $targetDir = Join-Path $DeployRoot "nginx"
    $target = Join-Path $targetDir "nginx.exe"
    if ((Test-Path $target) -and -not $Force) {
        Write-Host "  exists: $target" -ForegroundColor DarkGray
    } else {
        $zip = Join-Path $DownloadDir "nginx.zip"
        Download-File -Url $NginxZipUrl -OutFile $zip
        Expand-ZipContent -ZipPath $zip -Destination $targetDir
        if (-not (Test-Path $target)) { throw "nginx.exe not found after extracting $zip" }
        Write-Host "  ready: $target" -ForegroundColor Green
    }
}

if (Wants "redis") {
    Write-Host "[redis]" -ForegroundColor Yellow
    $targetDir = Join-Path $DeployRoot "redis"
    $target = Join-Path $targetDir "redis-server.exe"
    if ((Test-Path $target) -and -not $Force) {
        Write-Host "  exists: $target" -ForegroundColor DarkGray
    } else {
        $zip = Join-Path $DownloadDir "redis.zip"
        Download-File -Url $RedisZipUrl -OutFile $zip
        Expand-ZipContent -ZipPath $zip -Destination $targetDir
        if (-not (Test-Path $target)) { throw "redis-server.exe not found after extracting $zip" }
        Write-Host "  ready: $target" -ForegroundColor Green
    }
}

if (Wants "erlang") {
    Write-Host "[erlang]" -ForegroundColor Yellow
    $target = Join-Path $DeployRoot "otp_win64.exe"
    if ((Test-Path $target) -and -not $Force) {
        Write-Host "  exists: $target" -ForegroundColor DarkGray
    } else {
        Download-File -Url $ErlangOtpUrl -OutFile $target
        Write-Host "  ready: $target" -ForegroundColor Green
    }
}

if (Wants "rabbitmq") {
    Write-Host "[rabbitmq]" -ForegroundColor Yellow
    $target = Join-Path $DeployRoot "rabbitmq.zip"
    if ((Test-Path $target) -and -not $Force) {
        Write-Host "  exists: $target" -ForegroundColor DarkGray
    } else {
        Download-File -Url $RabbitMqZipUrl -OutFile $target
        Write-Host "  ready: $target" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Prerequisites are ready." -ForegroundColor Green
