#Requires -Version 5.1
<#
.SYNOPSIS
    Register a Windows scheduled task for automatic server upgrades.
#>
param(
    [string]$DeployRoot = "",
    [int]$EveryMinutes = 10,
    [string]$TaskName = "lm-auto-upgrade",
    [switch]$Uninstall
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
. (Join-Path $ScriptDir "_dotenv.ps1")
$envMap = Get-DotEnv -Path (Join-Path $ScriptDir ".env")
function Val($k, $d = "") { Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key $k -Default $d }

if (-not $DeployRoot) { $DeployRoot = Val "DEPLOY_ROOT" "D:\deploy\lab" }
$DeployRoot = (Resolve-Path $DeployRoot).Path

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) { throw "Please run as Administrator." }

if ($Uninstall) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false -ErrorAction SilentlyContinue
    Write-Host "Removed scheduled task: $TaskName" -ForegroundColor Green
    exit 0
}

$upgradeScript = Join-Path $DeployRoot "ops\upgrade.ps1"
if (-not (Test-Path $upgradeScript)) { throw "Missing updater: $upgradeScript" }

$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$upgradeScript`""
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date).AddMinutes(1) -RepetitionInterval (New-TimeSpan -Minutes $EveryMinutes) -RepetitionDuration ([TimeSpan]::MaxValue)
$principal = New-ScheduledTaskPrincipal -UserId "SYSTEM" -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -MultipleInstances IgnoreNew -StartWhenAvailable -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries

Register-ScheduledTask -TaskName $TaskName -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
Write-Host "Registered scheduled task: $TaskName (every $EveryMinutes minutes)" -ForegroundColor Green
