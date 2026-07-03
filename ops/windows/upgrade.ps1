#Requires -Version 5.1
<#
.SYNOPSIS
    Server-side auto updater. Does not call ossutil.
.DESCRIPTION
    Reads local ops\.env, downloads a signed OSS manifest via REST, downloads the
    versioned zip, verifies SHA256, extracts into DEPLOY_ROOT, and runs deploy-all.
#>
param(
    [switch]$Force,
    [switch]$CheckOnly,
    [switch]$NoDeploy,
    [string]$ManifestName = ""
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
. (Join-Path $ScriptDir "_dotenv.ps1")
. (Join-Path $ScriptDir "_ossrest.ps1")

$envFile = Join-Path $ScriptDir ".env"
$envMap = Get-DotEnv -Path $envFile
function Val($k, $d = "") { Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key $k -Default $d }

$DeployRoot = (Val "DEPLOY_ROOT" "D:\deploy\lab")
$DeployRoot = (Resolve-Path $DeployRoot).Path
$Endpoint = Val "OSS_ENDPOINT" "oss-cn-hangzhou.aliyuncs.com"
$Bucket = Val "OSS_BUCKET" ""
$AccessKeyId = Val "OSS_ACCESS_KEY_ID" ""
$AccessKeySecret = Val "OSS_ACCESS_KEY_SECRET" ""
$Prefix = (Val "OSS_PREFIX" "lab-deploy").Trim("/")
$EnableHttps = ((Val "OSS_ENABLE_HTTPS" "true") -ne "false")
$ReleaseBackupKeep = [int](Val "RELEASE_BACKUP_KEEP" "5")
if (-not $ManifestName) { $ManifestName = Val "UPGRADE_MANIFEST" "release.json" }

if (-not $Bucket -or -not $AccessKeyId -or -not $AccessKeySecret) {
    throw "OSS config is incomplete. Fill OSS_BUCKET / OSS_ACCESS_KEY_ID / OSS_ACCESS_KEY_SECRET in $envFile."
}

function Join-OssKey {
    param([string[]]$Parts)
    return (($Parts | Where-Object { $_ } | ForEach-Object { $_.Trim("/") }) -join "/")
}

function Stop-LmServices {
    $services = @("lm-web", "lm-api", "lm-nlq-agent", "lm-redis", "lm-rabbitmq")
    foreach ($name in $services) {
        $svc = Get-Service -Name $name -ErrorAction SilentlyContinue
        if ($svc -and $svc.Status -ne "Stopped") {
            Write-Host "  stop $name" -ForegroundColor DarkGray
            Stop-Service -Name $name -Force -ErrorAction SilentlyContinue
            $svc.WaitForStatus("Stopped", [TimeSpan]::FromSeconds(45))
        }
    }
}

function Restore-EnvFile {
    param([string]$BackupPath)
    if (Test-Path $BackupPath) {
        New-Item -ItemType Directory -Force -Path (Join-Path $DeployRoot "ops") | Out-Null
        Copy-Item -Path $BackupPath -Destination (Join-Path $DeployRoot "ops\.env") -Force
    }
}

function Test-BackupExclude {
    param([string]$RelativePath)
    $p = ($RelativePath -replace "\\", "/").TrimStart("/")
    if ($p -like "logs/*" -or $p -like "*/logs/*") { return $true }
    if ($p -like "redis/data/*" -or $p -like "rabbitmq/data/*") { return $true }
    if ($p -like "_downloads/*" -or $p -like "_upgrade/*" -or $p -like "backups/*") { return $true }
    if ($p -match "(^|/)__pycache__/") { return $true }
    if ($p -match "(^|/)\.venv/") { return $true }
    if ($p -match "(^|/)node_modules/") { return $true }
    if ($p -match "(^|/)\.pytest_cache/") { return $true }
    if ($p -match "(^|/)\.turbo/") { return $true }
    return $false
}

function Add-FileToZip {
    param(
        [Parameter(Mandatory = $true)]$Zip,
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string]$EntryName
    )
    [IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $Zip,
        $FilePath,
        ($EntryName -replace "\\", "/"),
        [IO.Compression.CompressionLevel]::Optimal
    ) | Out-Null
}

function Backup-CurrentDeployment {
    param(
        [string]$CurrentVersion,
        [string]$NextVersion
    )

    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $backupRoot = Join-Path $DeployRoot "backups\releases"
    New-Item -ItemType Directory -Force -Path $backupRoot | Out-Null
    $fromVersion = if ($CurrentVersion) { $CurrentVersion } else { "unknown" }
    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $backupPath = Join-Path $backupRoot "lm-before-$NextVersion-from-$fromVersion-$stamp.zip"
    if (Test-Path $backupPath) { Remove-Item -LiteralPath $backupPath -Force }

    $serviceInfoPath = Join-Path $upgradeDir "service-info-before-upgrade.json"
    Get-CimInstance Win32_Service -Filter "Name='lm-web' OR Name='lm-api' OR Name='lm-nlq-agent' OR Name='lm-redis' OR Name='lm-rabbitmq'" |
        Select-Object Name,DisplayName,State,StartMode,PathName,StartName,ProcessId |
        ConvertTo-Json -Depth 4 |
        Set-Content -Path $serviceInfoPath -Encoding UTF8

    $zip = [IO.Compression.ZipFile]::Open($backupPath, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($relativeRoot in @("api", "web", "nlq-agent", "ops")) {
            $sourceRoot = Join-Path $DeployRoot $relativeRoot
            if (-not (Test-Path $sourceRoot)) { continue }
            $rootFull = (Resolve-Path $sourceRoot).Path.TrimEnd("\", "/")
            foreach ($file in Get-ChildItem -Path $rootFull -Recurse -File -Force) {
                $rel = Join-Path $relativeRoot ($file.FullName.Substring($rootFull.Length).TrimStart("\", "/"))
                if (Test-BackupExclude $rel) { continue }
                Add-FileToZip -Zip $zip -FilePath $file.FullName -EntryName $rel
            }
        }

        foreach ($fileName in @(".release-version", ".release-manifest.json")) {
            $path = Join-Path $DeployRoot $fileName
            if (Test-Path $path) {
                Add-FileToZip -Zip $zip -FilePath $path -EntryName $fileName
            }
        }

        Add-FileToZip -Zip $zip -FilePath $serviceInfoPath -EntryName "service-info-before-upgrade.json"
    }
    finally {
        $zip.Dispose()
    }

    if ($ReleaseBackupKeep -gt 0) {
        Get-ChildItem -Path $backupRoot -Filter "lm-before-*.zip" -File -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending |
            Select-Object -Skip $ReleaseBackupKeep |
            Remove-Item -Force
    }

    Write-Host "==> Previous deployment backup: $backupPath" -ForegroundColor Green
    return $backupPath
}

$manifestKey = Join-OssKey @($Prefix, $ManifestName)
$upgradeDir = Join-Path $DeployRoot "_upgrade"
$logDir = Join-Path $DeployRoot "logs"
New-Item -ItemType Directory -Force -Path $upgradeDir, $logDir | Out-Null
$logFile = Join-Path $logDir "upgrade.log"

Start-Transcript -Path $logFile -Append | Out-Null
try {
    $currentVersionPath = Join-Path $DeployRoot ".release-version"
    $currentVersion = ""
    if (Test-Path $currentVersionPath) { $currentVersion = (Get-Content $currentVersionPath -Raw).Trim() }

    $manifestPath = Join-Path $upgradeDir "release.json"
    Write-Host "==> Check manifest oss://$Bucket/$manifestKey" -ForegroundColor Cyan
    Get-OssObject -Endpoint $Endpoint -Bucket $Bucket -Key $manifestKey -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -OutFile $manifestPath -EnableHttps $EnableHttps | Out-Null
    $manifest = Get-Content -Path $manifestPath -Raw -Encoding UTF8 | ConvertFrom-Json

    if (-not $manifest.version -or -not $manifest.package.objectKey -or -not $manifest.package.sha256) {
        throw "Invalid release manifest: missing version/package.objectKey/package.sha256."
    }

    Write-Host "  current: $currentVersion"
    Write-Host "  latest : $($manifest.version)"
    if (-not $Force -and $currentVersion -eq $manifest.version) {
        Write-Host "Already up to date." -ForegroundColor Green
        return
    }
    if ($CheckOnly) {
        Write-Host "Update available." -ForegroundColor Yellow
        return
    }

    $runDeployAll = $true
    if ($manifest.deploy -and ($manifest.deploy.PSObject.Properties.Name -contains "runDeployAll")) {
        $runDeployAll = [bool]$manifest.deploy.runDeployAll
    }

    $packagePath = Join-Path $upgradeDir $manifest.package.fileName
    Write-Host "==> Download package oss://$Bucket/$($manifest.package.objectKey)" -ForegroundColor Cyan
    Get-OssObject -Endpoint $Endpoint -Bucket $Bucket -Key $manifest.package.objectKey -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -OutFile $packagePath -EnableHttps $EnableHttps | Out-Null

    $actualHash = (Get-FileHash -Algorithm SHA256 -Path $packagePath).Hash.ToLowerInvariant()
    if ($actualHash -ne $manifest.package.sha256.ToLowerInvariant()) {
        throw "SHA256 mismatch. expected=$($manifest.package.sha256) actual=$actualHash"
    }

    $envBackup = Join-Path $upgradeDir "server.env.bak"
    if (Test-Path $envFile) { Copy-Item -Path $envFile -Destination $envBackup -Force }

    Backup-CurrentDeployment -CurrentVersion $currentVersion -NextVersion $manifest.version | Out-Null

    if ($runDeployAll) {
        Write-Host "==> Stop services before extracting" -ForegroundColor Cyan
        Stop-LmServices
    } else {
        Write-Host "==> Ops-only package; services stay running" -ForegroundColor Cyan
    }

    Write-Host "==> Extract package into $DeployRoot" -ForegroundColor Cyan
    Expand-Archive -Path $packagePath -DestinationPath $DeployRoot -Force
    Restore-EnvFile -BackupPath $envBackup

    if (-not $NoDeploy -and $runDeployAll) {
        $deployAll = Join-Path $DeployRoot "ops\deploy-all.ps1"
        if (-not (Test-Path $deployAll)) { throw "Missing deploy script: $deployAll" }
        Write-Host "==> Run deploy-all.ps1" -ForegroundColor Cyan
        & $deployAll
        if ($LASTEXITCODE -ne 0) { throw "deploy-all failed with exit code $LASTEXITCODE" }
    } elseif (-not $runDeployAll) {
        Write-Host "==> Skip deploy-all for ops-only package" -ForegroundColor Cyan
    }

    $manifest.version | Set-Content -Path $currentVersionPath -Encoding ASCII
    Copy-Item -Path $manifestPath -Destination (Join-Path $DeployRoot ".release-manifest.json") -Force
    Write-Host "Upgrade complete: $($manifest.version)" -ForegroundColor Green
}
finally {
    Stop-Transcript | Out-Null
}
