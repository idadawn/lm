#Requires -Version 5.1
<#
.SYNOPSIS
    Add or remove Windows Firewall inbound rules for the LM web gateway.
.DESCRIPTION
    Reads WEB_PORT from ops\.env by default and creates a TCP inbound allow rule.
    The deployment console calls this after the user saves port settings.
#>
param(
    [int[]]$Ports = @(),
    [string]$RuleGroup = "检测室数据分析系统",
    [switch]$Remove
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
. (Join-Path $ScriptDir "_dotenv.ps1")

$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) { throw "Please run as Administrator." }

$envMap = Get-DotEnv -Path (Join-Path $ScriptDir ".env")
function Val($k, $d = "") { Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key $k -Default $d }

if ($Ports.Count -eq 0) {
    $Ports = @([int](Val "WEB_PORT" "80"))
}

if ($Remove) {
    Get-NetFirewallRule -Group $RuleGroup -ErrorAction SilentlyContinue | Remove-NetFirewallRule
    Write-Host "Removed firewall rules in group: $RuleGroup" -ForegroundColor Green
    exit 0
}

foreach ($port in $Ports) {
    if ($port -lt 1 -or $port -gt 65535) { throw "Invalid TCP port: $port" }
}

foreach ($port in $Ports | Sort-Object -Unique) {
    $displayName = "LM Web Gateway TCP $port"
    Get-NetFirewallRule -DisplayName $displayName -ErrorAction SilentlyContinue | Remove-NetFirewallRule
    New-NetFirewallRule `
        -DisplayName $displayName `
        -Group $RuleGroup `
        -Direction Inbound `
        -Action Allow `
        -Protocol TCP `
        -LocalPort $port `
        -Profile Any | Out-Null
    Write-Host "Allowed inbound TCP port: $port" -ForegroundColor Green
}
