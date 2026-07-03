#Requires -Version 5.1
<#
.SYNOPSIS
    Build and publish a versioned deployment package to OSS without ossutil.
.DESCRIPTION
    Creates a zip from DEPLOY_ROOT, uploads it with OSS REST API, then uploads a
    small manifest. Servers run upgrade.ps1 to check the manifest and update.
#>
param(
    [string]$Version = "",
    [string]$DeployRoot = "",
    [string]$ManifestName = "",
    [ValidateSet("Full", "OpsOnly", "ToolPatch")]
    [string]$PackageMode = "Full",
    [switch]$SkipStage
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
. (Join-Path $ScriptDir "_dotenv.ps1")
. (Join-Path $ScriptDir "_ossrest.ps1")

$envMap = Get-DotEnv -Path (Join-Path $ScriptDir ".env")
function Val($k, $d = "") { Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key $k -Default $d }

if (-not $Version) { $Version = (Get-Date).ToString("yyyyMMddHHmmss") }
if (-not $DeployRoot) { $DeployRoot = Val "DEPLOY_ROOT" "D:\deploy\lab" }
$DeployRoot = (Resolve-Path $DeployRoot).Path

$Endpoint = Val "OSS_ENDPOINT" "oss-cn-hangzhou.aliyuncs.com"
$Bucket = Val "OSS_BUCKET" ""
$AccessKeyId = Val "OSS_ACCESS_KEY_ID" ""
$AccessKeySecret = Val "OSS_ACCESS_KEY_SECRET" ""
$Prefix = (Val "OSS_PREFIX" "lab-deploy").Trim("/")
$EnableHttps = ((Val "OSS_ENABLE_HTTPS" "true") -ne "false")
if (-not $ManifestName) { $ManifestName = Val "UPGRADE_MANIFEST" "release.json" }

if (-not $Bucket -or -not $AccessKeyId -or -not $AccessKeySecret) {
    throw "OSS config is incomplete. Fill OSS_BUCKET / OSS_ACCESS_KEY_ID / OSS_ACCESS_KEY_SECRET in ops/windows/.env."
}

function Join-OssKey {
    param([string[]]$Parts)
    return (($Parts | Where-Object { $_ } | ForEach-Object { $_.Trim("/") }) -join "/")
}

function Test-ReleaseExclude {
    param([string]$RelativePath)
    $p = ($RelativePath -replace "\\", "/").TrimStart("/")
    if ($p -eq "ops/.env") { return $true }
    if ($p -in @("ops/push-to-oss.ps1", "ops/pull-from-oss.ps1")) { return $true }
    if ($p -in @("nssm.exe", "otp_win64.exe", "rabbitmq.zip")) { return $true }
    if ($p -like "nginx/*" -or $p -like "redis/*" -or $p -like "erlang/*" -or $p -like "rabbitmq/*" -or $p -like "mysql/*") { return $true }
    if ($p -like "logs/*" -or $p -like "*/logs/*") { return $true }
    if ($p -like "ops/tools/*" -or $p -like "_downloads/*" -or $p -like "_upgrade/*" -or $p -like "_release/*" -or $p -like "backups/*") { return $true }
    if ($p -match "(^|/)__pycache__/") { return $true }
    if ($p -match "(^|/)\.venv/") { return $true }
    if ($p -match "(^|/)node_modules/") { return $true }
    if ($p -match "(^|/)\.pytest_cache/") { return $true }
    if ($p -match "(^|/)\.turbo/") { return $true }
    if ($p -match "(^|/)\.stash/") { return $true }
    if ($p -match "(^|/)\.git/") { return $true }
    return $false
}

function New-ReleaseZip {
    param(
        [Parameter(Mandatory = $true)][string]$Root,
        [Parameter(Mandatory = $true)][string]$ZipPath,
        [ValidateSet("Full", "OpsOnly", "ToolPatch")]
        [string]$Mode = "Full"
    )
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    if (Test-Path $ZipPath) { Remove-Item -LiteralPath $ZipPath -Force }
    $rootFull = (Resolve-Path $Root).Path.TrimEnd("\", "/")
    $zip = [IO.Compression.ZipFile]::Open($ZipPath, [IO.Compression.ZipArchiveMode]::Create)
    try {
        $files = Get-ChildItem -Path $rootFull -Recurse -File -Force
        $count = 0
        foreach ($file in $files) {
            $rel = $file.FullName.Substring($rootFull.Length).TrimStart("\", "/")
            $relForMatch = ($rel -replace "\\", "/").TrimStart("/")
            if ($Mode -eq "OpsOnly" -and $relForMatch -notlike "ops/*") { continue }
            if ($Mode -eq "ToolPatch" -and -not (
                $relForMatch -like "ops/*.ps1" -or
                $relForMatch -like "ops/*.cmd" -or
                $relForMatch -like "ops/LmDeployConsole/*"
            )) { continue }
            if (Test-ReleaseExclude $rel) { continue }
            $entry = ($rel -replace "\\", "/")
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, $entry, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
            $count++
        }
        return $count
    }
    finally {
        $zip.Dispose()
    }
}

if (-not $SkipStage) {
    Write-Host "==> Stage ops scripts into $DeployRoot\ops" -ForegroundColor Cyan
    $opsOut = Join-Path $DeployRoot "ops"
    New-Item -ItemType Directory -Force -Path $opsOut | Out-Null
    Get-ChildItem -Path $ScriptDir -Filter "*.ps1" -File |
        Where-Object { $_.Name -notin @("push-to-oss.ps1", "pull-from-oss.ps1") } |
        Copy-Item -Destination $opsOut -Force
    Copy-Item -Path (Join-Path $ScriptDir "*.cmd") -Destination $opsOut -Force -ErrorAction SilentlyContinue
    Copy-Item -Path (Join-Path $ScriptDir ".env.example") -Destination $opsOut -Force
    $tpl = Join-Path (Split-Path (Split-Path $ScriptDir -Parent) -Parent) "web\conf\nginx.windows.conf.template"
    if (Test-Path $tpl) { Copy-Item $tpl -Destination $opsOut -Force }
}

$workDir = Join-Path ([IO.Path]::GetTempPath()) "lm-release"
New-Item -ItemType Directory -Force -Path $workDir | Out-Null
$packageName = switch ($PackageMode) {
    "OpsOnly" { "lm-ops-$Version.zip" }
    "ToolPatch" { "lm-toolpatch-$Version.zip" }
    default { "lm-$Version.zip" }
}
$packagePath = Join-Path $workDir $packageName

Write-Host "==> Create package $packagePath" -ForegroundColor Cyan
$fileCount = New-ReleaseZip -Root $DeployRoot -ZipPath $packagePath -Mode $PackageMode
$hash = (Get-FileHash -Algorithm SHA256 -Path $packagePath).Hash.ToLowerInvariant()
$size = (Get-Item $packagePath).Length

$packageKey = Join-OssKey @($Prefix, "releases", $packageName)
$manifestKey = Join-OssKey @($Prefix, $ManifestName)
$manifestPath = Join-Path $workDir $ManifestName

$manifest = [ordered]@{
    app = "lm"
    version = $Version
    createdAt = [DateTime]::UtcNow.ToString("o")
    package = [ordered]@{
        mode = $PackageMode
        objectKey = $packageKey
        fileName = $packageName
        sha256 = $hash
        size = $size
        fileCount = $fileCount
    }
    deploy = [ordered]@{
        runDeployAll = ($PackageMode -eq "Full")
        preserveEnv = $true
    }
}
$manifest | ConvertTo-Json -Depth 8 | Set-Content -Path $manifestPath -Encoding UTF8

Write-Host "==> Upload package oss://$Bucket/$packageKey" -ForegroundColor Cyan
Put-OssObject -Endpoint $Endpoint -Bucket $Bucket -Key $packageKey -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -FilePath $packagePath -ContentType "application/zip" -EnableHttps $EnableHttps

Write-Host "==> Upload manifest oss://$Bucket/$manifestKey" -ForegroundColor Cyan
Put-OssObject -Endpoint $Endpoint -Bucket $Bucket -Key $manifestKey -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -FilePath $manifestPath -ContentType "application/json" -EnableHttps $EnableHttps

Write-Host ""
Write-Host "Release published." -ForegroundColor Green
Write-Host "  version: $Version"
Write-Host "  mode   : $PackageMode"
Write-Host "  package: oss://$Bucket/$packageKey"
Write-Host "  manifest: oss://$Bucket/$manifestKey"
Write-Host "  sha256 : $hash"
