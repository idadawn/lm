#Requires -Version 5.1
<#
.SYNOPSIS
    Package and upload Windows prerequisite software to OSS without ossutil.
.DESCRIPTION
    Uploads the base software used by download-prereqs.ps1:
    nssm.zip, nginx.zip, redis.zip, otp_win64.exe, rabbitmq.zip.
    The packages are stored under <OSS_PREFIX>/<PREREQ_PREFIX>/.
#>
param(
    [string]$DeployRoot = "",
    [switch]$SkipExistingCheck
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
$PrereqPrefix = (Val "PREREQ_PREFIX" "prereqs").Trim("/")
$EnableHttps = ((Val "OSS_ENABLE_HTTPS" "true") -ne "false")

if (-not $Bucket -or -not $AccessKeyId -or -not $AccessKeySecret) {
    throw "OSS config is incomplete. Fill OSS_BUCKET / OSS_ACCESS_KEY_ID / OSS_ACCESS_KEY_SECRET in ops/windows/.env."
}

function Join-OssKey {
    param([string[]]$Parts)
    return (($Parts | Where-Object { $_ } | ForEach-Object { $_.Trim("/") }) -join "/")
}

function New-ZipFromDirectory {
    param(
        [Parameter(Mandatory = $true)][string]$SourceDir,
        [Parameter(Mandatory = $true)][string]$ZipPath,
        [string[]]$ExcludePatterns = @()
    )
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    if (Test-Path $ZipPath) { Remove-Item -LiteralPath $ZipPath -Force }
    $sourceFull = (Resolve-Path $SourceDir).Path.TrimEnd("\", "/")
    $zip = [IO.Compression.ZipFile]::Open($ZipPath, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($file in Get-ChildItem -Path $sourceFull -Recurse -File -Force) {
            $rel = ($file.FullName.Substring($sourceFull.Length).TrimStart("\", "/") -replace "\\", "/")
            $skip = $false
            foreach ($pattern in $ExcludePatterns) {
                if ($rel -like $pattern) { $skip = $true; break }
            }
            if ($skip) { continue }
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file.FullName, $rel, [IO.Compression.CompressionLevel]::Optimal) | Out-Null
        }
    }
    finally {
        $zip.Dispose()
    }
}

function Require-Path {
    param([string]$Path, [string]$Name)
    if (-not (Test-Path $Path)) { throw "Missing $Name`: $Path" }
    return (Resolve-Path $Path).Path
}

$workDir = Join-Path ([IO.Path]::GetTempPath()) "lm-prereqs"
if (Test-Path $workDir) { Remove-Item -LiteralPath $workDir -Recurse -Force }
New-Item -ItemType Directory -Force -Path $workDir | Out-Null

$nssmExe = Require-Path (Join-Path $DeployRoot "nssm.exe") "nssm.exe"
$nginxDir = Require-Path (Join-Path $DeployRoot "nginx") "nginx directory"
$redisDir = Require-Path (Join-Path $DeployRoot "redis") "redis directory"
$erlangInstaller = Require-Path (Join-Path $DeployRoot "otp_win64.exe") "Erlang installer"
$rabbitZip = Require-Path (Join-Path $DeployRoot "rabbitmq.zip") "RabbitMQ zip"

$nssmStage = Join-Path $workDir "nssm-stage\nssm-2.24\win64"
New-Item -ItemType Directory -Force -Path $nssmStage | Out-Null
Copy-Item -Path $nssmExe -Destination (Join-Path $nssmStage "nssm.exe") -Force
New-ZipFromDirectory -SourceDir (Join-Path $workDir "nssm-stage") -ZipPath (Join-Path $workDir "nssm.zip")
New-ZipFromDirectory -SourceDir $nginxDir -ZipPath (Join-Path $workDir "nginx.zip") -ExcludePatterns @("logs/*", "temp/*")
New-ZipFromDirectory -SourceDir $redisDir -ZipPath (Join-Path $workDir "redis.zip") -ExcludePatterns @("data/*", "redis.lm.conf")
Copy-Item -Path $erlangInstaller -Destination (Join-Path $workDir "otp_win64.exe") -Force
Copy-Item -Path $rabbitZip -Destination (Join-Path $workDir "rabbitmq.zip") -Force

$items = @(
    @{ Name = "nssm"; File = "nssm.zip"; ContentType = "application/zip" },
    @{ Name = "nginx"; File = "nginx.zip"; ContentType = "application/zip" },
    @{ Name = "redis"; File = "redis.zip"; ContentType = "application/zip" },
    @{ Name = "erlang"; File = "otp_win64.exe"; ContentType = "application/octet-stream" },
    @{ Name = "rabbitmq"; File = "rabbitmq.zip"; ContentType = "application/zip" }
)

$manifestItems = @()
foreach ($item in $items) {
    $path = Join-Path $workDir $item.File
    $key = Join-OssKey @($Prefix, $PrereqPrefix, $item.File)
    $hash = (Get-FileHash -Algorithm SHA256 -Path $path).Hash.ToLowerInvariant()
    $size = (Get-Item $path).Length
    Write-Host "==> Upload $($item.Name): oss://$Bucket/$key" -ForegroundColor Cyan
    Put-OssObject -Endpoint $Endpoint -Bucket $Bucket -Key $key -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -FilePath $path -ContentType $item.ContentType -EnableHttps $EnableHttps
    $manifestItems += [ordered]@{
        name = $item.Name
        fileName = $item.File
        objectKey = $key
        sha256 = $hash
        size = $size
    }
}

$manifestPath = Join-Path $workDir "manifest.json"
$manifest = [ordered]@{
    app = "lm"
    type = "windows-prerequisites"
    createdAt = [DateTime]::UtcNow.ToString("o")
    items = $manifestItems
}
$manifest | ConvertTo-Json -Depth 6 | Set-Content -Path $manifestPath -Encoding UTF8
$manifestKey = Join-OssKey @($Prefix, $PrereqPrefix, "manifest.json")
Write-Host "==> Upload manifest: oss://$Bucket/$manifestKey" -ForegroundColor Cyan
Put-OssObject -Endpoint $Endpoint -Bucket $Bucket -Key $manifestKey -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -FilePath $manifestPath -ContentType "application/json" -EnableHttps $EnableHttps

Write-Host ""
Write-Host "Prerequisites published." -ForegroundColor Green
Write-Host "  prefix: oss://$Bucket/$(Join-OssKey @($Prefix, $PrereqPrefix))"
