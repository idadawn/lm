# 部署日志查看脚本
# 用法: .\logs.ps1 [-Days 7] [-Service lab-api]

param(
    [Parameter(Mandatory=$false)]
    [int]$Days = 7,
    
    [Parameter(Mandatory=$false)]
    [string]$Service = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$Follow
)

$ErrorActionPreference = "Continue"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$LogDir = Join-Path $ScriptDir "logs"

function Show-Logs {
    $logFiles = Get-ChildItem -Path $LogDir -Filter "deploy-*.log" -ErrorAction SilentlyContinue | 
                Where-Object { $_.LastWriteTime -gt (Get-Date).AddDays(-$Days) } |
                Sort-Object LastWriteTime -Descending
    
    if ($logFiles.Count -eq 0) {
        Write-Host "暂无部署日志" -ForegroundColor Yellow
        return
    }
    
    foreach ($file in $logFiles) {
        Write-Host "`n===== $($file.Name) =====" -ForegroundColor Cyan
        $content = Get-Content $file.FullName -Tail 50
        
        if ($Service) {
            $content = $content | Where-Object { $_ -match $Service }
        }
        
        $content | ForEach-Object {
            if ($_ -match "ERROR") {
                Write-Host $_ -ForegroundColor Red
            }
            elseif ($_ -match "WARN") {
                Write-Host $_ -ForegroundColor Yellow
            }
            elseif ($_ -match "SUCCESS") {
                Write-Host $_ -ForegroundColor Green
            }
            else {
                Write-Host $_
            }
        }
    }
}

# 主逻辑
if ($Follow) {
    Write-Host "跟踪日志模式 (Ctrl+C 退出)..." -ForegroundColor Yellow
    Get-ChildItem -Path $LogDir -Filter "deploy-*.log" -ErrorAction SilentlyContinue | 
        Sort-Object LastWriteTime -Descending | 
        Select-Object -First 1 | 
        ForEach-Object { Get-Content $_.FullName -Tail 50 -Wait }
}
else {
    Show-Logs
}
