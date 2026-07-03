#Requires -Version 5.1
<#
.SYNOPSIS
    在 Windows 上把 检测室数据分析系统 的后端(api) / 前端(web，经 nginx) / nlq-agent
    构建并注册为 NSSM 托管的常驻 Windows 服务。
.DESCRIPTION
    流程（按 -Only 选择的子集执行）：
      1. 加载 ops/windows/.env（命令行参数优先）
      2. 检查/下载 NSSM、检查/下载 nginx-for-Windows（web 需要）
      3. api      : dotnet publish Release -> <DeployRoot>/api，注册服务 lm-api
                     (dotnet.exe 运行 Poxiao.API.Entry.dll，ASPNETCORE_ENVIRONMENT/URLS 走服务环境变量)
      4. web      : pnpm build -> 复制 dist 到 <DeployRoot>/web/dist，
                     渲染 web/conf/nginx.windows.conf.template -> <DeployRoot>/web/nginx.conf，
                     注册服务 lm-web（nginx.exe -g "daemon off;"，反代 /api 与 /nlq-agent）
      5. nlq-agent: uv sync（生产依赖，不含 --extra dev），注册服务 lm-nlq-agent
                     (uv run uvicorn app.main:app，工作目录 nlq-agent/services/agent-api)
      6. 启动服务（除非 -NoStart），打印各服务状态与端口自检结果

    幂等：服务已存在时会先 stop + remove 再重装，可放心重复执行来更新配置/代码。
    必须以管理员身份运行 PowerShell（安装/卸载 Windows 服务的硬性要求）。
.PARAMETER Only
    仅处理指定的服务子集，取值 api / web / nlq-agent 任意组合。默认全部。
.PARAMETER Uninstall
    停止并移除 -Only 指定的服务（默认全部），不做构建。
.PARAMETER SkipBuild
    跳过 dotnet publish / pnpm build / uv sync，只重新生成 nginx 配置并重装服务
    （用于只改了端口/环境变量、产物已是最新的场景）。
.PARAMETER SelfContained
    api 使用自包含发布（-r win-x64 --self-contained true），目标机器不需要预装 .NET 运行时。
    默认框架依赖发布（体积小，要求目标机已装 .NET 10 运行时）。
.PARAMETER NoStart
    只安装/更新服务定义，不启动。
.PARAMETER Environment
    覆盖 .env 中的 ASPNETCORE_ENVIRONMENT。
.EXAMPLE
    # 全部安装并启动（以管理员身份运行）
    .\install-services.ps1
.EXAMPLE
    # 只重装前端网关（比如改了端口）
    .\install-services.ps1 -Only web -SkipBuild
.EXAMPLE
    # 卸载全部服务
    .\install-services.ps1 -Uninstall
#>
param(
    [ValidateSet('api', 'web', 'nlq-agent')]
    [string[]]$Only = @('api', 'web', 'nlq-agent'),
    [switch]$Uninstall,
    [switch]$SkipBuild,
    [switch]$SelfContained,
    [switch]$NoStart,
    [string]$Environment = ""
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$ScriptDir = $PSScriptRoot

# ── 0) 管理员权限检查（安装/卸载 Windows 服务的硬性要求）─────────
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "❌ 需要管理员权限。请以「管理员身份」重新打开 PowerShell 再运行本脚本。" -ForegroundColor Red
    exit 1
}

# ── 1) 加载 .env（命令行参数优先）─────────────────────────────
. (Join-Path $ScriptDir "_dotenv.ps1")
$envFile = Join-Path $ScriptDir ".env"
$envMap  = Get-DotEnv -Path $envFile
if (-not (Test-Path $envFile)) {
    Write-Host "ℹ️  未找到 $envFile，使用 .env.example 的默认值。建议复制一份并按需修改。" -ForegroundColor DarkGray
}

$AspEnv    = Coalesce-EnvValue -ParamValue $Environment -EnvMap $envMap -Key "ASPNETCORE_ENVIRONMENT" -Default "YY"
$ApiPort   = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "API_PORT" -Default "10089"
$WebPort   = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "WEB_PORT" -Default "80"
$NlqPort   = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "NLQ_PORT" -Default "8000"
# 内部服务监听地址：默认仅 127.0.0.1（只有 nginx 对外）。web(nginx) 始终 0.0.0.0 监听 WEB_PORT。
$BindAddr  = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "BIND_ADDR" -Default "127.0.0.1"
$DeployRootRaw = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "DEPLOY_ROOT" -Default (Join-Path $env:USERPROFILE "lm-deploy")
$NlqDirRaw = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "NLQ_DIR" -Default (Join-Path $RepoRoot "nlq-agent")
$NssmExeOverride  = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "NSSM_EXE" -Default ""
$NginxExeOverride = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "NGINX_EXE" -Default ""

New-Item -ItemType Directory -Force -Path $DeployRootRaw | Out-Null
$DeployRoot = (Resolve-Path $DeployRootRaw).Path
$LogRoot = Join-Path $DeployRoot "logs"
New-Item -ItemType Directory -Force -Path $LogRoot | Out-Null

$ServiceNames = @{
    "api"        = "lm-api"
    "web"        = "lm-web"
    "nlq-agent"  = "lm-nlq-agent"
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  检测室数据分析系统 - Windows 服务部署" -ForegroundColor Cyan
Write-Host "  目标: $($Only -join ', ')" -ForegroundColor Cyan
Write-Host "  部署根目录: $DeployRoot" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ── 辅助函数 ───────────────────────────────────────────────────
function Get-Nssm {
    if ($NssmExeOverride -and (Test-Path $NssmExeOverride)) { return $NssmExeOverride }
    $found = Get-ChildItem -Path (Join-Path $env:USERPROFILE "lm-nssm") -Recurse -Filter "nssm.exe" -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match "win64" } | Select-Object -First 1 -ExpandProperty FullName
    if ($found) { return $found }

    $cmd = Get-Command "nssm.exe" -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    Write-Host "==> 未发现 NSSM，正在从 nssm.cc 下载 ..." -ForegroundColor Cyan
    $installDir = Join-Path $env:USERPROFILE "lm-nssm"
    New-Item -ItemType Directory -Force -Path $installDir | Out-Null
    $zip = Join-Path $installDir "nssm-2.24.zip"
    Invoke-WebRequest "https://nssm.cc/release/nssm-2.24.zip" -OutFile $zip -UseBasicParsing -TimeoutSec 120
    Expand-Archive -Path $zip -DestinationPath $installDir -Force
    $exe = Get-ChildItem -Path $installDir -Recurse -Filter "nssm.exe" -ErrorAction SilentlyContinue |
        Where-Object { $_.FullName -match "win64" } | Select-Object -First 1 -ExpandProperty FullName
    if (-not $exe) { throw "下载 NSSM 后仍未找到 nssm.exe（win64），请手动下载 https://nssm.cc/download 并通过 ops/windows/.env 的 NSSM_EXE 指定路径。" }
    return $exe
}

function Get-Nginx {
    if ($NginxExeOverride -and (Test-Path $NginxExeOverride)) { return $NginxExeOverride }
    $installDir = Join-Path $env:USERPROFILE "lm-nginx"
    $exe = Get-ChildItem -Path $installDir -Recurse -Filter "nginx.exe" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
    if ($exe) { return $exe }

    Write-Host "==> 未发现 nginx，正在下载 nginx-for-Windows ..." -ForegroundColor Cyan
    New-Item -ItemType Directory -Force -Path $installDir | Out-Null
    try {
        $page = Invoke-WebRequest "http://nginx.org/download/" -UseBasicParsing -TimeoutSec 30
        $ver = [regex]::Matches($page.Content, 'nginx-(\d+\.\d+\.\d+)\.zip') | ForEach-Object { $_.Groups[1].Value } | Sort-Object { [version]$_ } -Unique | Select-Object -Last 1
    } catch { $ver = "1.27.4" }
    $zip = Join-Path $installDir "nginx-$ver.zip"
    Invoke-WebRequest "http://nginx.org/download/nginx-$ver.zip" -OutFile $zip -UseBasicParsing -TimeoutSec 120
    Expand-Archive -Path $zip -DestinationPath $installDir -Force
    $exe = Join-Path $installDir "nginx-$ver\nginx.exe"
    if (-not (Test-Path $exe)) { throw "下载 nginx 失败，请手动下载 http://nginx.org/download/ 并通过 ops/windows/.env 的 NGINX_EXE 指定路径。" }
    return $exe
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

# ── 2) api ────────────────────────────────────────────────────
if ($Only -contains "api") {
    Write-Host "【api】.NET 后端" -ForegroundColor Yellow
    $apiProj = Join-Path $RepoRoot "api\src\application\Poxiao.API.Entry\Poxiao.API.Entry.csproj"
    $apiOut  = Join-Path $DeployRoot "api"

    $connOverride = Join-Path $RepoRoot "api\src\application\Poxiao.API.Entry\Configurations\ConnectionStrings.$AspEnv.json"
    if (-not (Test-Path $connOverride)) {
        Write-Host "  ⚠️ 未找到 Configurations\ConnectionStrings.$AspEnv.json —— 将使用 base ConnectionStrings.json（多半连不到你想要的库）。" -ForegroundColor Yellow
    }

    if (-not $SkipBuild) {
        Write-Host "  → dotnet publish (Release$(if ($SelfContained) { ', self-contained win-x64' }))..." -ForegroundColor DarkGray
        if (Test-Path $apiOut) { Remove-Item -Recurse -Force $apiOut }
        $publishArgs = @($apiProj, "-c", "Release", "-o", $apiOut, "--nologo")
        if ($SelfContained) { $publishArgs += @("-r", "win-x64", "--self-contained", "true") }
        & dotnet publish @publishArgs
        if ($LASTEXITCODE -ne 0) { throw "dotnet publish 失败 (exit $LASTEXITCODE)" }
    } else {
        Write-Host "  ⏭  跳过构建 (-SkipBuild)" -ForegroundColor DarkGray
        if (-not (Test-Path (Join-Path $apiOut "Poxiao.API.Entry.dll"))) {
            throw "$apiOut 下没有发布产物，请先不带 -SkipBuild 运行一次。"
        }
    }

    $dotnetExe = (Get-Command dotnet -ErrorAction Stop).Source
    Install-NssmService -NssmExe $nssmExe -Name $ServiceNames["api"] `
        -Exe $dotnetExe -Params "`"$apiOut\Poxiao.API.Entry.dll`"" -WorkDir $apiOut `
        -Description "检测室数据分析系统 - 后端 API ($AspEnv, :$ApiPort)" `
        -EnvVars @{
            "ASPNETCORE_ENVIRONMENT" = $AspEnv
            "ASPNETCORE_URLS"        = "http://${BindAddr}:$ApiPort"
        }
    Write-Host ""
}

# ── 3) web（nginx 提供静态资源 + 反代）───────────────────────────
if ($Only -contains "web") {
    Write-Host "【web】前端 + nginx 网关" -ForegroundColor Yellow
    $webDir = Join-Path $RepoRoot "web"
    $webDistOut = Join-Path $DeployRoot "web\dist"

    if (-not $SkipBuild) {
        Push-Location $webDir
        try {
            Write-Host "  → pnpm install --frozen-lockfile ..." -ForegroundColor DarkGray
            & pnpm install --frozen-lockfile
            if ($LASTEXITCODE -ne 0) { throw "pnpm install 失败 (exit $LASTEXITCODE)" }
            Write-Host "  → pnpm build (生产构建) ..." -ForegroundColor DarkGray
            & pnpm build
            if ($LASTEXITCODE -ne 0) { throw "pnpm build 失败 (exit $LASTEXITCODE)" }
        } finally {
            Pop-Location
        }
        if (Test-Path $webDistOut) { Remove-Item -Recurse -Force $webDistOut }
        New-Item -ItemType Directory -Force -Path $webDistOut | Out-Null
        Copy-Item -Path (Join-Path $webDir "dist\*") -Destination $webDistOut -Recurse -Force
    } else {
        Write-Host "  ⏭  跳过构建 (-SkipBuild)" -ForegroundColor DarkGray
        if (-not (Test-Path (Join-Path $webDistOut "index.html"))) {
            throw "$webDistOut 下没有构建产物，请先不带 -SkipBuild 运行一次。"
        }
    }

    $nginxExe = Get-Nginx
    Write-Host "  nginx: $nginxExe" -ForegroundColor DarkGray
    $nginxInstallDir = Split-Path $nginxExe -Parent
    $nginxWebLogDir = Join-Path $DeployRoot "logs\lm-web"
    New-Item -ItemType Directory -Force -Path $nginxWebLogDir | Out-Null

    # 模板查找：优先仓库(开发机构建)，回退到脚本同目录(服务器自包含包 ops\ 下的 stage 拷贝)
    $templatePath = Join-Path $RepoRoot "web\conf\nginx.windows.conf.template"
    if (-not (Test-Path $templatePath)) { $templatePath = Join-Path $ScriptDir "nginx.windows.conf.template" }
    if (-not (Test-Path $templatePath)) { throw "找不到 nginx 模板（仓库或 $ScriptDir 均无 nginx.windows.conf.template）" }
    $renderedConfPath = Join-Path $DeployRoot "web\nginx.conf"
    $toForwardSlash = { param($p) ($p -replace '\\', '/') }

    $content = Get-Content -Path $templatePath -Raw -Encoding UTF8
    $content = $content.Replace("__WEB_ROOT__", (& $toForwardSlash $webDistOut))
    $content = $content.Replace("__API_PORT__", $ApiPort)
    $content = $content.Replace("__NLQ_PORT__", $NlqPort)
    $content = $content.Replace("__WEB_PORT__", $WebPort)
    $content = $content.Replace("__LOG_DIR__", (& $toForwardSlash $nginxWebLogDir))
    $content = $content.Replace("__MIME_TYPES__", (& $toForwardSlash (Join-Path $nginxInstallDir "conf\mime.types")))
    New-Item -ItemType Directory -Force -Path (Split-Path $renderedConfPath -Parent) | Out-Null
    [System.IO.File]::WriteAllText($renderedConfPath, $content, (New-Object System.Text.UTF8Encoding $false))

    # 注意：nginx 前缀/配置路径必须用正斜杠。反斜杠结尾紧跟引号（如 -p "D:\x\nginx\"）
    # 会被 MSVCRT 命令行解析当成转义引号，导致后续所有参数黏进 -p 路径。
    $nginxPrefixFwd = ((& $toForwardSlash $nginxInstallDir)) + "/"
    $confFwd = (& $toForwardSlash $renderedConfPath)
    # nginx -t 的输出（含成功信息）走 stderr，必须经 Invoke-Native 调用
    Invoke-Native $nginxExe @('-p', $nginxPrefixFwd, '-c', $confFwd, '-t') | ForEach-Object { Write-Host "    $_" -ForegroundColor DarkGray }
    if ($LASTEXITCODE -ne 0) { throw "nginx 配置校验失败：nginx -p '$nginxPrefixFwd' -c '$confFwd' -t 查看详情" }

    # daemon off; 已写入渲染出的 nginx.conf（PS→nssm 传参会吞 -g "daemon off;" 的引号）
    Install-NssmService -NssmExe $nssmExe -Name $ServiceNames["web"] `
        -Exe $nginxExe -Params "-p $nginxPrefixFwd -c $confFwd" -WorkDir $nginxInstallDir `
        -Description "检测室数据分析系统 - 前端网关 (nginx, :$WebPort)"
    Write-Host ""
}

# ── 4) nlq-agent ──────────────────────────────────────────────
if ($Only -contains "nlq-agent") {
    Write-Host "【nlq-agent】NLQ 智能体 (FastAPI/uvicorn)" -ForegroundColor Yellow
    $nlqSrc  = Join-Path $RepoRoot "nlq-agent"
    $nlqRoot = $NlqDirRaw

    # NLQ_DIR 指向仓库外时（推荐：部署自包含，不依赖源码检出/worktree 的存续），
    # 先把源码同步过去（排除依赖与缓存目录，保留目标端已有 .venv）。
    New-Item -ItemType Directory -Force -Path $nlqRoot | Out-Null
    # 仅当仓库源码存在且与目标不同目录时才同步（服务器自包含包里没有仓库，源码已在 $nlqRoot）。
    if (Test-Path $nlqSrc) {
        $nlqSrcResolved = (Resolve-Path -LiteralPath $nlqSrc).Path.TrimEnd('\')
        $nlqDstResolved = (Resolve-Path -LiteralPath $nlqRoot).Path.TrimEnd('\')
        if ($nlqSrcResolved -ne $nlqDstResolved) {
            Write-Host "  → 同步源码 $nlqSrc -> $nlqRoot ..." -ForegroundColor DarkGray
            & robocopy $nlqSrc $nlqRoot /MIR /NFL /NDL /NJH /NJS /NP `
                /XD node_modules .venv __pycache__ .pytest_cache .turbo .git | Out-Null
            if ($LASTEXITCODE -ge 8) { throw "robocopy 同步 nlq-agent 源码失败 (exit $LASTEXITCODE)" }
            $global:LASTEXITCODE = 0
        }
    } else {
        Write-Host "  ℹ️ 无仓库源码($nlqSrc)，直接使用 $nlqRoot 下已部署的源码" -ForegroundColor DarkGray
        if (-not (Test-Path (Join-Path $nlqRoot "services\agent-api\pyproject.toml"))) {
            throw "$nlqRoot 下没有 nlq-agent 源码，且仓库源码也不存在 —— 请确认部署包完整。"
        }
    }
    $agentDir = Join-Path $nlqRoot "services\agent-api"

    $uvCmd = Get-Command uv -ErrorAction SilentlyContinue
    if (-not $uvCmd) { throw "未找到 uv，请先安装：https://docs.astral.sh/uv/getting-started/installation/" }
    $uvExe = $uvCmd.Source

    $envTarget = Join-Path $nlqRoot ".env"
    if (-not (Test-Path $envTarget)) {
        $envExample = Join-Path $agentDir ".env.example"
        Copy-Item $envExample $envTarget
        Write-Host "  ⚠️ 未找到 $envTarget，已从 .env.example 复制一份 —— 部署前请编辑填入真实的数据库/Redis/LLM 配置！" -ForegroundColor Yellow
    }

    if (-not $SkipBuild) {
        Push-Location $agentDir
        try {
            Write-Host "  → uv sync (生产依赖) ..." -ForegroundColor DarkGray
            & $uvExe sync
            if ($LASTEXITCODE -ne 0) { throw "uv sync 失败 (exit $LASTEXITCODE)" }
        } finally {
            Pop-Location
        }
    } else {
        Write-Host "  ⏭  跳过依赖安装 (-SkipBuild)" -ForegroundColor DarkGray
    }

    Install-NssmService -NssmExe $nssmExe -Name $ServiceNames["nlq-agent"] `
        -Exe $uvExe -Params "run uvicorn app.main:app --host $BindAddr --port $NlqPort" -WorkDir $agentDir `
        -Description "检测室数据分析系统 - NLQ 智能体 (uvicorn, :$NlqPort)" `
        -EnvVars @{
            "ALL_PROXY"   = ""
            "HTTP_PROXY"  = ""
            "HTTPS_PROXY" = ""
            "NO_PROXY"    = "*"
        }
    Write-Host ""
}

# ── 4.5) 从主配置渲染各服务配置文件（启动前，确保用的是最新集中配置）──
$renderScript = Join-Path $ScriptDir "render-config.ps1"
if (Test-Path $renderScript) {
    Write-Host "【渲染配置】从主配置 .env 生成 api/nlq 配置文件" -ForegroundColor Yellow
    & $renderScript -Environment $AspEnv
    Write-Host ""
}

# ── 5) 启动 + 自检 ────────────────────────────────────────────
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

    Write-Host ""
    Write-Host "【端口自检】(服务刚启动，若为 False 可能仍在初始化，稍后再看日志)" -ForegroundColor Yellow
    if ($Only -contains "api")       { Write-Host ("  api        :$ApiPort -> {0}" -f (Test-Port $ApiPort)) }
    if ($Only -contains "web")       { Write-Host ("  web        :$WebPort -> {0}" -f (Test-Port $WebPort)) }
    if ($Only -contains "nlq-agent") { Write-Host ("  nlq-agent  :$NlqPort -> {0}" -f (Test-Port $NlqPort)) }
} else {
    Write-Host "⏭  已安装但未启动 (-NoStart)。手动启动：nssm start <服务名>" -ForegroundColor DarkGray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "完成 ✅  日志目录: $LogRoot\<服务名>\{stdout,stderr}.log" -ForegroundColor Cyan
Write-Host "常用操作: nssm status <服务名> | nssm restart <服务名> | Get-Service lm-*" -ForegroundColor DarkGray
Write-Host "========================================" -ForegroundColor Cyan
