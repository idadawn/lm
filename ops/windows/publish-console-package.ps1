#Requires -Version 5.1
<#
.SYNOPSIS
    Publish a single bootstrap console package to OSS.
.DESCRIPTION
    The package contains the WPF console, launcher, ops scripts, templates, and
    .env.example. It excludes the real .env and legacy ossutil push/pull scripts.
#>
param(
    [string]$DeployRoot = "",
    [string]$PackageName = "lm-deploy-console.zip",
    [string]$BootstrapPrefix = ""
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
. (Join-Path $ScriptDir "_dotenv.ps1")
. (Join-Path $ScriptDir "_ossrest.ps1")

$envMap = Get-DotEnv -Path (Join-Path $ScriptDir ".env")
function Val($k, $d = "") { Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key $k -Default $d }

if (-not $DeployRoot) { $DeployRoot = Val "DEPLOY_ROOT" "D:\deploy\lab" }
$DeployRoot = (Resolve-Path $DeployRoot).Path
$Endpoint = Val "OSS_ENDPOINT" "oss-cn-hangzhou.aliyuncs.com"
$Bucket = Val "OSS_BUCKET" ""
$AccessKeyId = Val "OSS_ACCESS_KEY_ID" ""
$AccessKeySecret = Val "OSS_ACCESS_KEY_SECRET" ""
$Prefix = (Val "OSS_PREFIX" "lab-deploy").Trim("/")
if (-not $BootstrapPrefix) { $BootstrapPrefix = (Val "BOOTSTRAP_PREFIX" "bootstrap").Trim("/") }
$EnableHttps = ((Val "OSS_ENABLE_HTTPS" "true") -ne "false")

if (-not $Bucket -or -not $AccessKeyId -or -not $AccessKeySecret) {
    throw "OSS config is incomplete. Fill OSS_BUCKET / OSS_ACCESS_KEY_ID / OSS_ACCESS_KEY_SECRET in ops/windows/.env."
}

function Join-OssKey {
    param([string[]]$Parts)
    return (($Parts | Where-Object { $_ } | ForEach-Object { $_.Trim("/") }) -join "/")
}

function Test-BootstrapExclude {
    param([string]$RelativePath)
    $p = ($RelativePath -replace "\\", "/").TrimStart("/")
    if ($p -eq ".env") { return $true }
    if ($p -in @("push-to-oss.ps1", "pull-from-oss.ps1")) { return $true }
    if ($p -like "tools/*" -or $p -like "logs/*") { return $true }
    if ($p -like "*.log") { return $true }
    return $false
}

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$opsRoot = Join-Path $DeployRoot "ops"
if (-not (Test-Path (Join-Path $opsRoot "LmDeployConsole\LmDeployConsole.exe"))) {
    throw "Missing WPF console under $opsRoot. Run build-desktop-console.ps1 first."
}
New-Item -ItemType Directory -Force -Path $opsRoot | Out-Null
Get-ChildItem -Path $ScriptDir -Filter "*.ps1" -File |
    Where-Object { $_.Name -notin @("push-to-oss.ps1", "pull-from-oss.ps1") } |
    Copy-Item -Destination $opsRoot -Force
Copy-Item -Path (Join-Path $ScriptDir "*.cmd") -Destination $opsRoot -Force -ErrorAction SilentlyContinue
Copy-Item -Path (Join-Path $ScriptDir ".env.example") -Destination $opsRoot -Force
$tpl = Join-Path (Split-Path (Split-Path $ScriptDir -Parent) -Parent) "web\conf\nginx.windows.conf.template"
if (Test-Path $tpl) { Copy-Item $tpl -Destination $opsRoot -Force }

$workDir = Join-Path ([IO.Path]::GetTempPath()) "lm-bootstrap"
New-Item -ItemType Directory -Force -Path $workDir | Out-Null
$zipPath = Join-Path $workDir $PackageName
if (Test-Path $zipPath) { Remove-Item -LiteralPath $zipPath -Force }

$zip = [IO.Compression.ZipFile]::Open($zipPath, [IO.Compression.ZipArchiveMode]::Create)
try {
    $rootFull = (Resolve-Path $opsRoot).Path.TrimEnd("\", "/")
    foreach ($file in Get-ChildItem -Path $rootFull -Recurse -File -Force) {
        $rel = $file.FullName.Substring($rootFull.Length).TrimStart("\", "/")
        if (Test-BootstrapExclude $rel) { continue }
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, ($rel -replace "\\", "/"), [IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
}
finally {
    $zip.Dispose()
}

$hash = (Get-FileHash -Algorithm SHA256 -Path $zipPath).Hash.ToLowerInvariant()
$size = (Get-Item $zipPath).Length
$packageKey = Join-OssKey @($Prefix, $BootstrapPrefix, $PackageName)
Write-Host "==> Upload bootstrap console: oss://$Bucket/$packageKey" -ForegroundColor Cyan
Put-OssObject -Endpoint $Endpoint -Bucket $Bucket -Key $packageKey -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -FilePath $zipPath -ContentType "application/zip" -EnableHttps $EnableHttps

$manifestPath = Join-Path $workDir "console-manifest.json"
$manifest = [ordered]@{
    app = "lm"
    type = "deploy-console"
    createdAt = [DateTime]::UtcNow.ToString("o")
    package = [ordered]@{
        objectKey = $packageKey
        fileName = $PackageName
        sha256 = $hash
        size = $size
    }
}
$manifest | ConvertTo-Json -Depth 6 | Set-Content -Path $manifestPath -Encoding UTF8
$manifestKey = Join-OssKey @($Prefix, $BootstrapPrefix, "console-manifest.json")
Write-Host "==> Upload bootstrap manifest: oss://$Bucket/$manifestKey" -ForegroundColor Cyan
Put-OssObject -Endpoint $Endpoint -Bucket $Bucket -Key $manifestKey -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -FilePath $manifestPath -ContentType "application/json" -EnableHttps $EnableHttps

Write-Host ""
Write-Host "Bootstrap console published." -ForegroundColor Green
Write-Host "  package : oss://$Bucket/$packageKey"
Write-Host "  sha256  : $hash"
