# 健康检查脚本
# 用法: .\health-check.ps1

param(
    [Parameter(Mandatory=$false)]
    [switch]$Watch
)

$ErrorActionPreference = "Continue"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$ConfigFile = Join-Path $ScriptDir "config.ps1"

# 加载配置
. $ConfigFile

function Test-ServiceHealth {
    param($service)
    
    $result = @{
        Name = $service.displayName
        Status = "未知"
        ResponseTime = 0
        Ok = $false
    }
    
    # 检查 NSSM 状态
    try {
        $nssmStatus = nssm status $service.name 2>$null
        if ($nssmStatus -eq "SERVICE_RUNNING") {
            $result.Status = "运行中"
        }
        elseif ($nssmStatus -eq "SERVICE_STOPPED") {
            $result.Status = "已停止"
            return $result
        }
        else {
            $result.Status = $nssmStatus
        }
    }
    catch {
        $result.Status = "查询失败"
        return $result
    }
    
    # 健康检查
    if ($service.healthCheck) {
        $sw = [Diagnostics.Stopwatch]::StartNew()
        try {
            $response = Invoke-WebRequest -Uri $service.healthCheck -TimeoutSec 10 -UseBasicParsing -ErrorAction SilentlyContinue
            $sw.Stop()
            $result.ResponseTime = $sw.ElapsedMilliseconds
            
            if ($response.StatusCode -eq 200) {
                $result.Status = "正常"
                $result.Ok = $true
            }
            else {
                $result.Status = "HTTP $($response.StatusCode)"
            }
        }
        catch {
            $sw.Stop()
            $result.ResponseTime = $sw.ElapsedMilliseconds
            $result.Status = "请求失败"
        }
    }
    else {
        $result.Ok = $true
    }
    
    return $result
}

function Show-Status {
    Clear-Host
    Write-Host "`n===== 服务健康检查 =====" -ForegroundColor Cyan
    Write-Host "时间: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
    Write-Host ""
    
    $allOk = $true
    
    foreach ($service in $Services) {
        $result = Test-ServiceHealth -service $service
        
        $color = if ($result.Ok) { "Green" } else { "Red" }
        $status = if ($result.Ok) { "✓" } else { "✗" }
        
        Write-Host "$status $($result.Name)" -ForegroundColor $color -NoNewline
        Write-Host " - $($result.Status)" -ForegroundColor Gray -NoNewline
        if ($result.ResponseTime -gt 0) {
            Write-Host " ($($result.ResponseTime)ms)" -ForegroundColor DarkGray
        }
        else {
            Write-Host ""
        }
        
        if (-not $result.Ok) {
            $allOk = $false
        }
    }
    
    Write-Host ""
    if ($allOk) {
        Write-Host "所有服务运行正常 ✓" -ForegroundColor Green
    }
    else {
        Write-Host "存在异常服务 ✗" -ForegroundColor Red
    }
    
    return $allOk
}

# 主逻辑
if ($Watch) {
    Write-Host "监控模式 (Ctrl+C 退出)..." -ForegroundColor Yellow
    while ($true) {
        $result = Show-Status
        Start-Sleep -Seconds 10
    }
}
else {
    Show-Status
}
