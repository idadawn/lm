# start-mobile-proxy.ps1 — 拉起「移动端内网调试网关」(原生 Windows Nginx)
# 把后端(:10089) + NLQ 智能体(:8000) 聚合到单一来源 http://<本机内网IP>:8928，供内网真机调试 App。
#
# 为什么用原生 nginx 而非 Docker：
#   Docker Desktop(WSL2) 只把发布端口转发到宿主机 127.0.0.1，不在内网网卡建真实套接字
#   → 手机不可达。原生 nginx 直接 listen 0.0.0.0:8928（真实套接字），内网真机可达。
#
# 用法：仓库根目录执行   ./start-mobile-proxy.ps1
# 前置：后端与 NLQ 已在运行（先跑 ./start-dev.ps1）。首次运行会自动下载 nginx-for-Windows。
# 停止：Get-Process nginx | Stop-Process

$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$port = if ($env:MOBILE_PROXY_PORT) { $env:MOBILE_PROXY_PORT } else { '8928' }
$conf = Join-Path $root 'web\conf\nginx.mobile.win.conf'
$installDir = Join-Path $env:USERPROFILE 'lm-nginx'

# 1) 确保 nginx 可用（缺失则从 nginx.org 下载最新版）
$exe = Get-ChildItem -Path $installDir -Recurse -Filter nginx.exe -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
if (-not $exe) {
  Write-Host "==> 未发现 nginx，正在下载 nginx-for-Windows ..." -ForegroundColor Cyan
  New-Item -ItemType Directory -Force -Path $installDir | Out-Null
  try {
    $page = Invoke-WebRequest 'http://nginx.org/download/' -UseBasicParsing -TimeoutSec 30
    $ver = [regex]::Matches($page.Content, 'nginx-(\d+\.\d+\.\d+)\.zip') | ForEach-Object { $_.Groups[1].Value } | Sort-Object { [version]$_ } -Unique | Select-Object -Last 1
  } catch { $ver = '1.27.4' }
  $zip = Join-Path $installDir "nginx-$ver.zip"
  Invoke-WebRequest "http://nginx.org/download/nginx-$ver.zip" -OutFile $zip -UseBasicParsing -TimeoutSec 120
  Expand-Archive -Path $zip -DestinationPath $installDir -Force
  $exe = Join-Path $installDir "nginx-$ver\nginx.exe"
}
$prefix = Split-Path $exe -Parent
Write-Host "==> nginx: $exe" -ForegroundColor DarkGray

# 2) 校验配置（nginx -t）
& $exe -p "$prefix/" -c $conf -t | Out-Null
if ($LASTEXITCODE -ne 0) { throw "nginx 配置校验失败，运行 `"$exe -p '$prefix/' -c '$conf' -t`" 查看详情" }

# 3) 清理历史遗留（best-effort）：停掉旧的 Docker 网关；管理员下顺带清 portproxy 并放行防火墙
try { docker stop lm-nginx-mobile | Out-Null } catch {}
$admin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
$lan = @((Get-NetIPConfiguration | Where-Object { $_.IPv4DefaultGateway -ne $null -and $_.NetAdapter.Status -eq 'Up' } | Select-Object -First 1).IPv4Address.IPAddress)[0]
if ($admin) {
  if ($lan) { $null = netsh interface portproxy delete v4tov4 listenaddress=$lan listenport=$port }
  Get-NetFirewallRule -DisplayName "lm-mobile-proxy-$port" -ErrorAction SilentlyContinue | Remove-NetFirewallRule -ErrorAction SilentlyContinue
  New-NetFirewallRule -DisplayName "lm-mobile-proxy-$port" -Direction Inbound -Action Allow -Protocol TCP -LocalPort $port -Profile Any | Out-Null
} else {
  Write-Host "  [提示] 非管理员：跳过防火墙放行。若手机连不上，请以管理员重跑本脚本，或手动放行入站 TCP $port。" -ForegroundColor Yellow
}

# 4) 重启 nginx
Get-Process nginx -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Milliseconds 400
Start-Process -FilePath $exe -ArgumentList '-p', "$prefix/", '-c', $conf -WorkingDirectory $prefix -WindowStyle Hidden
Start-Sleep -Seconds 1

# 5) 前置检查 + 打印 App 主服务器地址
function Test-Port($p) { try { $c = New-Object Net.Sockets.TcpClient; $c.Connect('127.0.0.1', $p); $c.Close(); $true } catch { $false } }
if (-not (Test-Port 10089)) { Write-Host "  [警告] 后端 :10089 未就绪 —— 请先运行 ./start-dev.ps1" -ForegroundColor Yellow }
if (-not (Test-Port 8000))  { Write-Host "  [警告] NLQ  :8000  未就绪 —— 请先运行 ./start-dev.ps1" -ForegroundColor Yellow }
if (-not $lan) { $lan = '<本机内网IP>' }

Write-Host ""
Write-Host "网关已就绪 ✓ (原生 nginx, 0.0.0.0:$port)" -ForegroundColor Green
Write-Host "在 App「我的 → 服务器设置」把【主服务器地址】填为：" -ForegroundColor Green
Write-Host ("    http://{0}:{1}" -f $lan, $port) -ForegroundColor White
Write-Host ""
Write-Host ("自检：curl http://{0}:{1}/__gw_health   # 期望 gateway ok" -f $lan, $port) -ForegroundColor DarkGray
Write-Host "停止网关：Get-Process nginx | Stop-Process" -ForegroundColor DarkGray
