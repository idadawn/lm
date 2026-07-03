#Requires -Version 5.1
<#
.SYNOPSIS
    Back up the configured MySQL database now or register a daily backup task.
.DESCRIPTION
    Reads database settings from ops\.env. Backups are written to
    <DEPLOY_ROOT>\backups\db as zipped SQL dumps. The scheduled task runs as
    SYSTEM with highest privileges for unattended servers.
#>
param(
    [switch]$RunNow,
    [switch]$InstallSchedule,
    [switch]$RemoveSchedule,
    [string]$At = "",
    [int]$KeepDays = 0,
    [string]$TaskName = "lm-db-backup"
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
. (Join-Path $ScriptDir "_dotenv.ps1")

$envFile = Join-Path $ScriptDir ".env"
$envMap = Get-DotEnv -Path $envFile
function Val($k, $d = "") { Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key $k -Default $d }

$DeployRoot = Val "DEPLOY_ROOT" "D:\deploy\lab"
$DeployRoot = (Resolve-Path $DeployRoot).Path
$DbType = Val "DB_TYPE" "MySql"
$DbHost = Val "DB_HOST" "127.0.0.1"
$DbPort = [int](Val "DB_PORT" "3306")
$DbName = Val "DB_NAME" "lumei"
$DbUser = Val "DB_USER" "root"
$DbPassword = Val "DB_PASSWORD" ""
if (-not $At) { $At = Val "DB_BACKUP_TIME" "02:30" }
if ($KeepDays -le 0) { $KeepDays = [int](Val "DB_BACKUP_KEEP_DAYS" "14") }

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (($InstallSchedule -or $RemoveSchedule) -and -not $isAdmin) { throw "Please run as Administrator." }

function Find-MySqlDump {
    $override = Val "MYSQLDUMP_EXE" ""
    if ($override -and (Test-Path $override)) { return $override }

    $cmd = Get-Command "mysqldump.exe" -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    $roots = @()
    if ($env:ProgramFiles) { $roots += (Join-Path $env:ProgramFiles "MySQL") }
    if (${env:ProgramFiles(x86)}) { $roots += (Join-Path ${env:ProgramFiles(x86)} "MySQL") }
    $roots += "C:\MySQL"
    $roots = $roots | Where-Object { $_ -and (Test-Path $_) }

    foreach ($root in $roots) {
        $found = Get-ChildItem -Path $root -Recurse -Filter "mysqldump.exe" -ErrorAction SilentlyContinue |
            Sort-Object FullName -Descending |
            Select-Object -First 1 -ExpandProperty FullName
        if ($found) { return $found }
    }

    throw "Cannot find mysqldump.exe. Install MySQL client tools or set MYSQLDUMP_EXE in $envFile."
}

function Invoke-DatabaseBackup {
    if ($DbType -notin @("MySql", "MySQL")) {
        throw "Database backup currently supports MySQL only. DB_TYPE=$DbType"
    }
    if (-not $DbName) { throw "DB_NAME is empty in $envFile." }
    if (-not $DbUser) { throw "DB_USER is empty in $envFile." }

    $dumpExe = Find-MySqlDump
    $backupDir = Join-Path $DeployRoot "backups\db"
    New-Item -ItemType Directory -Force -Path $backupDir | Out-Null

    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $safeName = ($DbName -replace '[^\w.-]', '_')
    $sqlFile = Join-Path $backupDir "$safeName-$stamp.sql"
    $zipFile = Join-Path $backupDir "$safeName-$stamp.zip"

    Write-Host "Using mysqldump: $dumpExe" -ForegroundColor DarkGray
    Write-Host "Backup target: $zipFile" -ForegroundColor Cyan

    $args = @(
        "--host=$DbHost",
        "--port=$DbPort",
        "--user=$DbUser",
        "--default-character-set=utf8mb4",
        "--single-transaction",
        "--routines",
        "--events",
        "--databases",
        $DbName,
        "--result-file=$sqlFile"
    )

    $oldPwd = $env:MYSQL_PWD
    if ($DbPassword) { $env:MYSQL_PWD = $DbPassword }
    try {
        & $dumpExe @args
        if ($LASTEXITCODE -ne 0) { throw "mysqldump failed with exit code $LASTEXITCODE" }
    }
    finally {
        $env:MYSQL_PWD = $oldPwd
    }

    Compress-Archive -Path $sqlFile -DestinationPath $zipFile -Force
    Remove-Item -LiteralPath $sqlFile -Force

    $cutoff = (Get-Date).AddDays(-$KeepDays)
    Get-ChildItem -Path $backupDir -Filter "*.zip" -File -ErrorAction SilentlyContinue |
        Where-Object { $_.LastWriteTime -lt $cutoff } |
        Remove-Item -Force

    Write-Host "Database backup complete: $zipFile" -ForegroundColor Green
}

function Install-BackupSchedule {
    $backupTime = [TimeSpan]::Zero
    if (-not [TimeSpan]::TryParse($At, [ref]$backupTime)) {
        throw "Invalid backup time: $At. Expected HH:mm, for example 02:30."
    }

    $scriptPath = Join-Path $DeployRoot "ops\backup-database.ps1"
    if (-not (Test-Path $scriptPath)) { $scriptPath = $PSCommandPath }
    $action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`" -RunNow"
    $trigger = New-ScheduledTaskTrigger -Daily -At ([DateTime]::Today.Add($backupTime))
    $principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -RunLevel Highest
    $settings = New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew -StartWhenAvailable -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries

    Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
    Write-Host "Registered scheduled task: $TaskName (daily $At)" -ForegroundColor Green
}

if ($RemoveSchedule) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false -ErrorAction SilentlyContinue
    Write-Host "Removed scheduled task: $TaskName" -ForegroundColor Green
    exit 0
}

if ($InstallSchedule) {
    Install-BackupSchedule
}

if ($RunNow -or (-not $InstallSchedule)) {
    Invoke-DatabaseBackup
}
