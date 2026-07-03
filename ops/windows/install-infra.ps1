#Requires -Version 5.1
<#
.SYNOPSIS
    把基础环境（Redis / RabbitMQ）注册为 NSSM 托管的 Windows 服务。
.DESCRIPTION
    与 install-services.ps1 配套：先跑本脚本装基础环境，再跑 install-services.ps1 装应用。
    MySQL 不在本脚本范围内（本机已有 MySQL80 服务，:3306）。

    流程（按 -Only 选择的子集执行）：
      redis    : 使用 <DeployRoot>\redis\redis-server.exe，渲染 redis.lm.conf
                 （端口/密码来自 ops/windows/.env，默认对齐 Cache.YY.json），
                 注册服务 lm-redis
      rabbitmq : 如未安装 Erlang，则静默安装 <DeployRoot>\otp_win64.exe 到 <DeployRoot>\erlang；
                 如未解压 RabbitMQ，则解压 <DeployRoot>\rabbitmq.zip；
                 写 rabbitmq.conf（listeners.tcp.default = 端口），同步 Erlang cookie
                 （LocalSystem 与当前用户），启用 management 插件，注册服务 lm-rabbitmq，
                 启动后创建业务账号（默认 admin/admin123，对齐 EventBus.YY.json）

    幂等：服务已存在时先 stop + remove 再重装。必须以管理员身份运行。
.PARAMETER Only
    仅处理指定子集，取值 redis / rabbitmq 任意组合。默认全部。
.PARAMETER Uninstall
    停止并移除 -Only 指定的服务，不做安装。
.PARAMETER NoStart
    只注册服务，不启动（RabbitMQ 的业务账号创建也会跳过）。
.EXAMPLE
    .\install-infra.ps1
.EXAMPLE
    .\install-infra.ps1 -Only redis
#>
param(
    [ValidateSet('redis', 'rabbitmq')]
    [string[]]$Only = @('redis', 'rabbitmq'),
    [switch]$Uninstall,
    [switch]$NoStart
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot

# ── 0) 管理员权限检查 ─────────────────────────────────────────
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "❌ 需要管理员权限。请以「管理员身份」重新打开 PowerShell 再运行本脚本。" -ForegroundColor Red
    exit 1
}

# ── 1) 加载 .env ──────────────────────────────────────────────
. (Join-Path $ScriptDir "_dotenv.ps1")
$envFile = Join-Path $ScriptDir ".env"
$envMap  = Get-DotEnv -Path $envFile

$DeployRootRaw = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "DEPLOY_ROOT" -Default (Join-Path $env:USERPROFILE "lm-deploy")
$NssmExeOverride = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "NSSM_EXE" -Default ""

# 内部服务监听地址：默认仅本机（只有 nginx 对外）。Redis/RabbitMQ 都据此绑定。
$BindAddr      = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "BIND_ADDR" -Default "127.0.0.1"
$RedisHost     = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "REDIS_HOST" -Default $BindAddr
$RedisPort     = [int](Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "REDIS_PORT" -Default "6380")
$RedisPassword = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "REDIS_PASSWORD" -Default "Lm@Redis#2025Secure!"
$RedisMaxMem   = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "REDIS_MAXMEMORY" -Default "512mb"

$RmqHost     = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "RABBITMQ_HOST" -Default $BindAddr
$RmqPort     = [int](Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "RABBITMQ_PORT" -Default "8005")
$RmqMgmtPort = [int](Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "RABBITMQ_MANAGEMENT_PORT" -Default "15672")
$RmqUser     = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "RABBITMQ_USER" -Default "admin"
$RmqPassword = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "RABBITMQ_PASSWORD" -Default "admin123"

New-Item -ItemType Directory -Force -Path $DeployRootRaw | Out-Null
$DeployRoot = (Resolve-Path $DeployRootRaw).Path
$LogRoot = Join-Path $DeployRoot "logs"
New-Item -ItemType Directory -Force -Path $LogRoot | Out-Null

$ServiceNames = @{ "redis" = "lm-redis"; "rabbitmq" = "lm-rabbitmq" }

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  检测室数据分析系统 - 基础环境部署" -ForegroundColor Cyan
Write-Host "  目标: $($Only -join ', ')" -ForegroundColor Cyan
Write-Host "  部署根目录: $DeployRoot" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ── 辅助函数（与 install-services.ps1 保持一致的服务注册逻辑）──
function Get-Nssm {
    if ($NssmExeOverride -and (Test-Path $NssmExeOverride)) { return $NssmExeOverride }
    $local = Join-Path $DeployRoot "nssm.exe"
    if (Test-Path $local) { return $local }
    $cmd = Get-Command "nssm.exe" -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    throw "未找到 nssm.exe。请放置到 $DeployRoot\nssm.exe 或通过 ops/windows/.env 的 NSSM_EXE 指定。"
}

# PS 5.1 陷阱：对原生命令用 2>&1 会把 stderr 包装成 ErrorRecord，
# 在 ErrorActionPreference=Stop 下即使 | Out-Null 也会变成终止错误
#（例如 nssm stop 一个未启动的服务）。统一经此函数调用原生命令。
function Invoke-Native {
    param([string]$Exe, [string[]]$Arguments)
    $eap = $ErrorActionPreference
    $ErrorActionPreference = "Continue"
    try { (& $Exe @Arguments 2>&1) | ForEach-Object { "$_" } } finally { $ErrorActionPreference = $eap }
}

function Remove-ServiceIfExists {
    param([string]$NssmExe, [string]$Name)
    if (Get-Service -Name $Name -ErrorAction SilentlyContinue) {
        Write-Host "  服务 $Name 已存在，先停止并移除 ..." -ForegroundColor DarkGray
        Invoke-Native $NssmExe @('stop', $Name) | Out-Null
        Start-Sleep -Seconds 1
        Invoke-Native $NssmExe @('remove', $Name, 'confirm') | Out-Null
        Start-Sleep -Milliseconds 500
    }
}

function Install-NssmService {
    param(
        [string]$NssmExe,
        [string]$Name,
        [string]$Exe,
        [string]$Params,
        [string]$WorkDir,
        [string]$Description,
        [hashtable]$EnvVars = @{}
    )
    Remove-ServiceIfExists -NssmExe $NssmExe -Name $Name
    $svcLogDir = Join-Path $LogRoot $Name
    New-Item -ItemType Directory -Force -Path $svcLogDir | Out-Null

    & $NssmExe install $Name $Exe | Out-Null
    & $NssmExe set $Name AppParameters $Params | Out-Null
    & $NssmExe set $Name AppDirectory $WorkDir | Out-Null
    & $NssmExe set $Name DisplayName $Name | Out-Null
    & $NssmExe set $Name Description $Description | Out-Null
    & $NssmExe set $Name Start SERVICE_AUTO_START | Out-Null
    & $NssmExe set $Name AppStdout (Join-Path $svcLogDir "stdout.log") | Out-Null
    & $NssmExe set $Name AppStderr (Join-Path $svcLogDir "stderr.log") | Out-Null
    & $NssmExe set $Name AppRotateFiles 1 | Out-Null
    & $NssmExe set $Name AppRotateOnline 1 | Out-Null
    & $NssmExe set $Name AppRotateBytes 10485760 | Out-Null
    & $NssmExe set $Name AppExit Default Restart | Out-Null
    & $NssmExe set $Name AppRestartDelay 5000 | Out-Null
    if ($EnvVars.Count -gt 0) {
        $envBlock = ($EnvVars.GetEnumerator() | ForEach-Object { "$($_.Key)=$($_.Value)" }) -join "`r`n"
        & $NssmExe set $Name AppEnvironmentExtra $envBlock | Out-Null
    }
    Write-Host "  ✅ 已注册服务 $Name" -ForegroundColor Green
}

function Test-Port {
    param([int]$Port)
    try {
        $c = New-Object System.Net.Sockets.TcpClient
        $c.Connect("127.0.0.1", $Port)
        $c.Close()
        return $true
    } catch { return $false }
}

function Wait-Port {
    param([int]$Port, [int]$TimeoutSec = 60)
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        if (Test-Port -Port $Port) { return $true }
        Start-Sleep -Seconds 2
    }
    return $false
}

# ── 卸载分支 ───────────────────────────────────────────────────
if ($Uninstall) {
    $nssmExe = Get-Nssm
    foreach ($key in $Only) {
        Remove-ServiceIfExists -NssmExe $nssmExe -Name $ServiceNames[$key]
    }
    Write-Host ""
    Write-Host ("已卸载: " + (($Only | ForEach-Object { $ServiceNames[$_] }) -join ', ')) -ForegroundColor Green
    exit 0
}

$nssmExe = Get-Nssm
Write-Host "NSSM: $nssmExe" -ForegroundColor DarkGray
Write-Host ""

# ── 2) Redis ──────────────────────────────────────────────────
if ($Only -contains "redis") {
    Write-Host "【redis】(:$RedisPort)" -ForegroundColor Yellow
    $redisExe = Get-ChildItem -Path (Join-Path $DeployRoot "redis") -Recurse -Filter "redis-server.exe" -ErrorAction SilentlyContinue |
        Select-Object -First 1 -ExpandProperty FullName
    if (-not $redisExe) { throw "未在 $DeployRoot\redis 下找到 redis-server.exe，请先下载解压 Redis for Windows。" }
    $redisHome = Split-Path $redisExe -Parent
    $redisData = Join-Path $DeployRoot "redis\data"
    New-Item -ItemType Directory -Force -Path $redisData | Out-Null

    $confPath = Join-Path $redisHome "redis.lm.conf"
    $confLines = @(
        "# 由 ops/windows/install-infra.ps1 生成（对齐 api Cache.YY.json）",
        "bind $RedisHost",
        "port $RedisPort",
        "requirepass `"$RedisPassword`"",
        ("dir `"" + ($redisData -replace '\\', '/') + "`""),
        "maxmemory $RedisMaxMem",
        "maxmemory-policy allkeys-lru",
        "appendonly yes",
        "save 900 1",
        "save 300 10"
    )
    [System.IO.File]::WriteAllLines($confPath, $confLines, (New-Object System.Text.UTF8Encoding $false))
    Write-Host "  配置: $confPath" -ForegroundColor DarkGray

    Install-NssmService -NssmExe $nssmExe -Name $ServiceNames["redis"] `
        -Exe $redisExe -Params "`"$confPath`"" -WorkDir $redisHome `
        -Description "检测室数据分析系统 - Redis 缓存 (:$RedisPort)"
    Write-Host ""
}

# ── 3) RabbitMQ ───────────────────────────────────────────────
if ($Only -contains "rabbitmq") {
    Write-Host "【rabbitmq】(AMQP :$RmqPort, management :$RmqMgmtPort)" -ForegroundColor Yellow

    # 3a. Erlang
    $erlangDir = Join-Path $DeployRoot "erlang"
    $erlExe = Get-ChildItem -Path $erlangDir -Recurse -Filter "erl.exe" -ErrorAction SilentlyContinue |
        Select-Object -First 1 -ExpandProperty FullName
    if (-not $erlExe) {
        $otpInstaller = Join-Path $DeployRoot "otp_win64.exe"
        if (-not (Test-Path $otpInstaller)) { throw "未找到 Erlang（$erlangDir）也没有安装包（$otpInstaller）。" }
        Write-Host "  → 静默安装 Erlang OTP 到 $erlangDir（约 1-2 分钟）..." -ForegroundColor DarkGray
        $p = Start-Process -FilePath $otpInstaller -ArgumentList "/S", "/D=$erlangDir" -Wait -PassThru
        if ($p.ExitCode -ne 0) { throw "Erlang 安装失败 (exit $($p.ExitCode))" }
        $erlExe = Get-ChildItem -Path $erlangDir -Recurse -Filter "erl.exe" -ErrorAction SilentlyContinue |
            Select-Object -First 1 -ExpandProperty FullName
        if (-not $erlExe) { throw "Erlang 安装后仍未找到 erl.exe" }
    }
    # ERLANG_HOME 是包含 bin\erl.exe 的目录
    $erlangHome = Split-Path (Split-Path $erlExe -Parent) -Parent
    Write-Host "  ERLANG_HOME: $erlangHome" -ForegroundColor DarkGray

    # 3b. RabbitMQ 本体
    $rmqRoot = Join-Path $DeployRoot "rabbitmq"
    $rmqServerBat = Get-ChildItem -Path $rmqRoot -Recurse -Filter "rabbitmq-server.bat" -ErrorAction SilentlyContinue |
        Select-Object -First 1 -ExpandProperty FullName
    if (-not $rmqServerBat) {
        $rmqZip = Join-Path $DeployRoot "rabbitmq.zip"
        if (-not (Test-Path $rmqZip)) { throw "未找到 RabbitMQ（$rmqRoot 下无 rabbitmq-server.bat）也没有压缩包（$rmqZip）。" }
        Write-Host "  → 解压 RabbitMQ ..." -ForegroundColor DarkGray
        Expand-Archive -Path $rmqZip -DestinationPath $rmqRoot -Force
        $rmqServerBat = Get-ChildItem -Path $rmqRoot -Recurse -Filter "rabbitmq-server.bat" -ErrorAction SilentlyContinue |
            Select-Object -First 1 -ExpandProperty FullName
        if (-not $rmqServerBat) { throw "解压后仍未找到 rabbitmq-server.bat" }
    }
    $rmqSbin = Split-Path $rmqServerBat -Parent
    Write-Host "  sbin: $rmqSbin" -ForegroundColor DarkGray

    # 3c. 数据目录 + rabbitmq.conf
    $rmqBase = Join-Path $rmqRoot "data"
    New-Item -ItemType Directory -Force -Path $rmqBase | Out-Null
    $rmqConf = Join-Path $rmqBase "rabbitmq.conf"
    $rmqConfLines = @(
        "# 由 ops/windows/install-infra.ps1 生成（对齐 api EventBus.YY.json）",
        "listeners.tcp.default = ${RmqHost}:$RmqPort",
        "management.tcp.ip = $RmqHost",
        "management.tcp.port = $RmqMgmtPort",
        "log.file.level = info"
    )
    [System.IO.File]::WriteAllLines($rmqConf, $rmqConfLines, (New-Object System.Text.UTF8Encoding $false))

    # 3d. Erlang cookie：LocalSystem（服务）与当前用户（rabbitmqctl）必须一致
    $systemCookie = Join-Path $env:SystemRoot "System32\config\systemprofile\.erlang.cookie"
    $userCookie   = Join-Path $env:USERPROFILE ".erlang.cookie"
    if (Test-Path $systemCookie) {
        $cookieVal = (Get-Content $systemCookie -Raw).Trim()
    } elseif (Test-Path $userCookie) {
        $cookieVal = (Get-Content $userCookie -Raw).Trim()
    } else {
        $cookieVal = -join ((65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object { [char]$_ })
    }
    foreach ($ck in @($systemCookie, $userCookie)) {
        if (Test-Path $ck) { Set-ItemProperty -Path $ck -Name IsReadOnly -Value $false -ErrorAction SilentlyContinue }
        [System.IO.File]::WriteAllText($ck, $cookieVal, (New-Object System.Text.ASCIIEncoding))
    }
    Write-Host "  Erlang cookie 已同步（LocalSystem + 当前用户）" -ForegroundColor DarkGray

    $rmqEnv = @{
        "ERLANG_HOME"          = $erlangHome
        "RABBITMQ_BASE"        = $rmqBase
        "RABBITMQ_CONFIG_FILE" = $rmqConf
        "RABBITMQ_LOG_BASE"    = (Join-Path $LogRoot "lm-rabbitmq")
    }

    # 3e. 启用 management 插件（离线模式，不需要节点在跑）
    Write-Host "  → 启用 management 插件 ..." -ForegroundColor DarkGray
    $pluginsBat = Join-Path $rmqSbin "rabbitmq-plugins.bat"
    $oldEnv = @{}
    foreach ($k in $rmqEnv.Keys) { $oldEnv[$k] = [Environment]::GetEnvironmentVariable($k); [Environment]::SetEnvironmentVariable($k, $rmqEnv[$k]) }
    try {
        Invoke-Native $pluginsBat @('enable', '--offline', 'rabbitmq_management') | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
    } finally {
        foreach ($k in $oldEnv.Keys) { [Environment]::SetEnvironmentVariable($k, $oldEnv[$k]) }
    }

    # 3f. 注册服务（cmd /c 运行 rabbitmq-server.bat）
    $cmdExe = $env:ComSpec
    Install-NssmService -NssmExe $nssmExe -Name $ServiceNames["rabbitmq"] `
        -Exe $cmdExe -Params "/c `"`"$rmqServerBat`"`"" -WorkDir $rmqSbin `
        -Description "检测室数据分析系统 - RabbitMQ 消息队列 (AMQP :$RmqPort)" `
        -EnvVars $rmqEnv
    Write-Host ""
}

# ── 4) 启动 + 自检 ────────────────────────────────────────────
if (-not $NoStart) {
    Write-Host "【启动服务】" -ForegroundColor Yellow
    foreach ($key in $Only) {
        $name = $ServiceNames[$key]
        Invoke-Native $nssmExe @('start', $name) | Out-Null
        Start-Sleep -Seconds 1
        $svc = Get-Service -Name $name -ErrorAction SilentlyContinue
        $status = if ($svc) { $svc.Status } else { "未知" }
        Write-Host "  $name : $status" -ForegroundColor $(if ($status -eq "Running") { "Green" } else { "Yellow" })
    }

    if ($Only -contains "redis") {
        $ok = Wait-Port -Port $RedisPort -TimeoutSec 30
        Write-Host ("  redis    :$RedisPort -> {0}" -f $ok) -ForegroundColor $(if ($ok) { "Green" } else { "Red" })
    }

    if ($Only -contains "rabbitmq") {
        Write-Host "  等待 RabbitMQ 启动（首次启动需初始化数据库，最多 120 秒）..." -ForegroundColor DarkGray
        $ok = Wait-Port -Port $RmqPort -TimeoutSec 120
        Write-Host ("  rabbitmq :$RmqPort -> {0}" -f $ok) -ForegroundColor $(if ($ok) { "Green" } else { "Red" })

        if ($ok) {
            # 创建业务账号（幂等：已存在则改密码）
            Write-Host "  → 配置业务账号 $RmqUser ..." -ForegroundColor DarkGray
            $ctlBat = Get-ChildItem -Path (Join-Path $DeployRoot "rabbitmq") -Recurse -Filter "rabbitmqctl.bat" | Select-Object -First 1 -ExpandProperty FullName
            $rmqSbinDir = Split-Path $ctlBat -Parent
            $rmqBaseDir = Join-Path (Join-Path $DeployRoot "rabbitmq") "data"
            $oldEnv2 = @{}
            $ctlEnv = @{
                "ERLANG_HOME"          = (Get-ChildItem -Path (Join-Path $DeployRoot "erlang") -Recurse -Filter "erl.exe" | Select-Object -First 1 | ForEach-Object { Split-Path (Split-Path $_.FullName -Parent) -Parent })
                "RABBITMQ_BASE"        = $rmqBaseDir
                "RABBITMQ_CONFIG_FILE" = (Join-Path $rmqBaseDir "rabbitmq.conf")
            }
            foreach ($k in $ctlEnv.Keys) { $oldEnv2[$k] = [Environment]::GetEnvironmentVariable($k); [Environment]::SetEnvironmentVariable($k, $ctlEnv[$k]) }
            try {
                # 注意：必须按行取第一列做精确匹配。子串/正则匹配会被 guest 的
                # [administrator] 标签误命中（"administrator" 包含 "admin"）。
                $userExists = $false
                foreach ($line in (Invoke-Native $ctlBat @('list_users', '--silent'))) {
                    $name = ($line -split "\s+")[0]
                    if ($name -eq $RmqUser) { $userExists = $true; break }
                }
                if ($userExists) {
                    Invoke-Native $ctlBat @('change_password', $RmqUser, $RmqPassword) | Out-Null
                    Write-Host "  账号 $RmqUser 已存在，密码已同步" -ForegroundColor DarkGray
                } else {
                    Invoke-Native $ctlBat @('add_user', $RmqUser, $RmqPassword) | Out-Null
                    Invoke-Native $ctlBat @('set_user_tags', $RmqUser, 'administrator') | Out-Null
                    Invoke-Native $ctlBat @('set_permissions', '-p', '/', $RmqUser, '.*', '.*', '.*') | Out-Null
                    Write-Host "  账号 $RmqUser 已创建（administrator，vhost /）" -ForegroundColor DarkGray
                }
                # 用 authenticate_user 实测账号可用，不依赖上面命令的静默结果
                $authOut = (Invoke-Native $ctlBat @('authenticate_user', $RmqUser, $RmqPassword)) -join ' '
                if ($authOut -match 'Success') {
                    Write-Host "  ✅ 账号 $RmqUser 认证验证通过" -ForegroundColor Green
                } else {
                    Write-Host "  ❌ 账号 $RmqUser 认证失败：$authOut" -ForegroundColor Red
                }
            } finally {
                foreach ($k in $oldEnv2.Keys) { [Environment]::SetEnvironmentVariable($k, $oldEnv2[$k]) }
            }
        } else {
            Write-Host "  ⚠️ RabbitMQ 端口未就绪，跳过账号创建。查看日志: $LogRoot\lm-rabbitmq\stderr.log" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "⏭  已注册但未启动 (-NoStart)。" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "基础环境完成 ✅  （MySQL 使用已有服务 MySQL80 :3306，未改动）" -ForegroundColor Cyan
Write-Host "下一步: .\install-services.ps1 部署应用（api/web/nlq-agent）" -ForegroundColor DarkGray
Write-Host "========================================" -ForegroundColor Cyan
