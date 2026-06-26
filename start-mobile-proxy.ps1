# start-mobile-proxy.ps1 — 拉起「移动端内网调试网关」(Nginx)
# 把后端(:10089) + NLQ 智能体(:8000) 聚合到单一来源，让 App 只配一个主服务器地址。
#
# 用法：仓库根目录执行   ./start-mobile-proxy.ps1
# 前置：1) 后端与 NLQ 已在运行（先跑 ./start-dev.ps1）  2) Docker Desktop 已开
#       3) 手机与本机在同一内网/WiFi
# 停止：docker compose --profile mobile down  （或 docker stop lm-nginx-mobile）

$ErrorActionPreference = 'SilentlyContinue'
$root = $PSScriptRoot
$port = if ($env:MOBILE_PROXY_PORT) { $env:MOBILE_PROXY_PORT } else { '8928' }

Write-Host "==> 启动 Nginx 移动端调试网关 (宿主端口 :$port) ..." -ForegroundColor Cyan
Push-Location $root
docker compose --profile mobile up -d nginx-mobile
Pop-Location

# 后端 / NLQ 必须已经在跑（网关只转发，不代管这两个进程）
function Test-Port($p) { try { $c = New-Object Net.Sockets.TcpClient; $c.Connect('127.0.0.1', $p); $c.Close(); $true } catch { $false } }
if (-not (Test-Port 10089)) { Write-Host "  [警告] 后端 :10089 未就绪 —— 请先运行 ./start-dev.ps1" -ForegroundColor Yellow }
if (-not (Test-Port 8000))  { Write-Host "  [警告] NLQ  :8000  未就绪 —— 请先运行 ./start-dev.ps1" -ForegroundColor Yellow }

# 取本机内网 IPv4（优先：带默认网关的活动网卡）
$cfg = Get-NetIPConfiguration | Where-Object { $_.IPv4DefaultGateway -ne $null -and $_.NetAdapter.Status -eq 'Up' } | Select-Object -First 1
$lan = @($cfg.IPv4Address.IPAddress)[0]
if (-not $lan) {
  $lan = (Get-NetIPAddress -AddressFamily IPv4 |
    Where-Object { $_.IPAddress -like '192.168.*' -or $_.IPAddress -like '10.*' -or $_.IPAddress -match '^172\.(1[6-9]|2[0-9]|3[01])\.' } |
    Select-Object -First 1).IPAddress
}
if (-not $lan) { $lan = '<本机内网IP>' }

# 尽力放行入站端口（需要管理员；非管理员会静默失败，仅在末尾提示检查防火墙）
netsh advfirewall firewall delete name="lm-mobile-proxy" | Out-Null
netsh advfirewall firewall add rule name="lm-mobile-proxy" dir=in action=allow protocol=TCP localport=$port | Out-Null

Write-Host ""
Write-Host "网关已就绪 ✓" -ForegroundColor Green
Write-Host "在 App「我的 → 服务器设置」把【主服务器地址】填为：" -ForegroundColor Green
Write-Host ("    http://{0}:{1}" -f $lan, $port) -ForegroundColor White
Write-Host ""
Write-Host "自检（本机）：" -ForegroundColor DarkGray
Write-Host ("    curl http://{0}:{1}/__gw_health   # 期望: gateway ok" -f $lan, $port) -ForegroundColor DarkGray
Write-Host "若手机连不上：确认同一 WiFi，且 Windows 防火墙已放行入站 TCP $port（本脚本已尝试添加规则，可能需管理员）。" -ForegroundColor DarkGray
