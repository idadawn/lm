#Requires -Version 5.1
<#
.SYNOPSIS
    uni-app 鸿蒙(HarmonyOS NEXT) 本地打包 —— 调 HBuilderX CLI 走完整链：编译 Vue3→ArkTS、
    在 unpackage/dist/build/app-harmony 生成 DevEco 工程、由 hvigorw 构建签名出 .app/.hap。
.DESCRIPTION
    流程（对齐 build-android.ps1）：
      0. 加载 scripts/.env（命令行参数优先）
      1. 环境检查：HBuilderX cli.exe + DevEco/CLT 探测 + 路径合规性预警
      2. 清理旧 app-harmony 产物 → unpackage/.stash/
      3. 自动 bump versionCode（与 build-android 共用 manifest.json）
      4. 导入工程：cli project open --path（必须 project open，旧 open --project 不导入会报"项目不存在"）
      5. 触发鸿蒙打包：cli pack app-harmony --project（失败转 GUI 兜底；-UseHvigor 走纯 hvigorw）
      6. 产物校验：unpackage/dist/build/app-harmony 下最新 .app(优先)/.hap，时间戳须晚于构建开始
    打包成功后执行 upload-to-agc.ps1 上传到 AppGallery Connect。

    ⚠️ 前置（脚本无法替代，详见 PUBLISH-HARMONY.md）：
      - 安装 DevEco Studio(>=5.1.0.849) 或 HarmonyOS Command Line Tools，并在 HBuilderX 设置里
        配置 harmony.devTools.path 指向其安装目录（仅一次）
      - manifest.json 的 app-harmony.distribute.signingConfigs 配好签名证书
        （当前仓库仅有 debug 的 default 证书；正式发布需补 release 证书）
      - 鸿蒙工具链对路径含中文字符 / 超长（Windows 建议总路径 < 110 字符）敏感

    ⚠️ 诚实说明：cli 的鸿蒙 pack 精确子命令语法、本机 DevEco 是否就绪、产物落点，
    作者无法在本机验证；脚本用产物时间戳强校验 + 失败清晰提示 GUI 兜底，避免"假成功"。
.PARAMETER Description
    版本更新说明（透传给 upload-to-agc.ps1 时用）。空时读 .env 中 HARMONY_DESCRIPTION
.PARAMETER SkipBumpVersion
    跳过自动 bump versionCode
.PARAMETER KeepOldArtifacts
    保留旧产物，不移到 .stash/
.PARAMETER UseHvigor
    跳过 HBuilderX pack，直接对已生成的 DevEco 工程跑 hvigorw assembleApp。
    需先至少用 HBuilderX 成功 pack 过一次（生成 unpackage/dist/build/app-harmony 工程）。
.EXAMPLE
    # 走 HBuilderX CLI 全链（推荐）
    .\build-harmony.ps1
    # 跳过版本自增
    .\build-harmony.ps1 -SkipBumpVersion
    # 工程已生成后，仅用 hvigorw 重新构建
    .\build-harmony.ps1 -UseHvigor
#>
param(
    [string]$Description = "",
    [switch]$SkipBumpVersion,
    [switch]$KeepOldArtifacts,
    [switch]$UseHvigor
)

$ErrorActionPreference = "Stop"
$ProjectRoot   = Split-Path -Parent $PSScriptRoot
$BuildStartTime = Get-Date

# ── 加载 .env（命令行参数优先） ─────────────────────────
. (Join-Path $PSScriptRoot "_dotenv.ps1")
$envFile = Join-Path $PSScriptRoot ".env"
$envMap  = Get-DotEnv -Path $envFile
$envLoaded = (Test-Path $envFile)

$Description      = Coalesce-EnvValue -ParamValue $Description -EnvMap $envMap -Key "HARMONY_DESCRIPTION"
$HBuilderXFromEnv = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "HBUILDER_CLI"
$DevEcoHome       = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "DEVECO_HOME"
$autoBumpEnv      = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "AUTO_BUMP_VERSION" -Default "true"
if ($autoBumpEnv -eq "false" -or $autoBumpEnv -eq "0") { $SkipBumpVersion = $true }

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  检测室数据分析 - 鸿蒙(HarmonyOS) 本地打包" -ForegroundColor Cyan
Write-Host "  构建开始: $($BuildStartTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Cyan
if ($envLoaded) { Write-Host "  配置文件: $envFile" -ForegroundColor DarkGray }
else { Write-Host "  配置文件: (未找到 $envFile，使用默认值)" -ForegroundColor DarkGray }
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$harmonyBuildDir = Join-Path $ProjectRoot "unpackage\dist\build\app-harmony"

# ── 步骤 1：环境检查 ─────────────────────────────────────
Write-Host "【步骤 1/6】环境检查" -ForegroundColor Yellow

$HBuilderXPath = $null
if ($HBuilderXFromEnv -and (Test-Path $HBuilderXFromEnv)) {
    $HBuilderXPath = $HBuilderXFromEnv
}
else {
    $searchPaths = @(
        "C:\Program Files\HBuilderX\cli.exe",
        "C:\Program Files (x86)\HBuilderX\cli.exe",
        "C:\HBuilderX\cli.exe",
        "D:\app\HBuilderX\cli.exe",
        "D:\HBuilderX\cli.exe",
        "E:\HBuilderX\cli.exe",
        "$env:LOCALAPPDATA\HBuilderX\cli.exe",
        "$env:USERPROFILE\HBuilderX\cli.exe"
    )
    foreach ($path in $searchPaths) {
        if (Test-Path $path) { $HBuilderXPath = $path; break }
    }
    if (-not $HBuilderXPath) {
        $cliInPath = Get-Command "cli.exe" -ErrorAction SilentlyContinue
        if ($cliInPath) { $HBuilderXPath = $cliInPath.Source }
    }
}

if ($HBuilderXPath) {
    Write-Host "✅ HBuilderX: $HBuilderXPath" -ForegroundColor Green
}
else {
    Write-Host "❌ 未找到 HBuilderX cli.exe" -ForegroundColor Red
    Write-Host "   请在 scripts/.env 中设置 HBUILDER_CLI=完整路径" -ForegroundColor Yellow
    exit 1
}

# DevEco / Command Line Tools 探测：探测不到只告警不阻断（HBuilderX 可能已在设置里配好）
$devEcoFound = $false
$devEcoProbe = @()
if ($DevEcoHome) { $devEcoProbe += $DevEcoHome }
$devEcoProbe += @(
    "C:\Program Files\Huawei\DevEco Studio",
    "D:\app\DevEco Studio",
    "D:\DevEco Studio",
    "$env:LOCALAPPDATA\Huawei\DevEco Studio"
)
foreach ($p in $devEcoProbe) {
    if ($p -and (Test-Path $p)) { Write-Host "✅ DevEco/CLT: $p" -ForegroundColor Green; $devEcoFound = $true; break }
}
if (-not $devEcoFound) {
    Write-Host "⚠️ 未探测到 DevEco Studio / Command Line Tools（不阻断）" -ForegroundColor Yellow
    Write-Host "   鸿蒙打包依赖它编译签名；若未安装请先装好，并在 HBuilderX" -ForegroundColor Yellow
    Write-Host "   [工具]->[设置]->[运行配置] 里配置 鸿蒙 DevTools 路径。" -ForegroundColor Yellow
    Write-Host "   可在 scripts/.env 设 DEVECO_HOME=安装目录 以便本脚本探测。" -ForegroundColor DarkGray
}

# 路径合规性预警：鸿蒙工具链对中文/超长路径敏感
if ($ProjectRoot -match '[一-鿿]') {
    Write-Host "⚠️ 项目路径含中文字符，鸿蒙工具链可能构建失败：$ProjectRoot" -ForegroundColor Yellow
}
if ($harmonyBuildDir.Length -gt 110) {
    Write-Host "⚠️ DevEco 工程路径较长（$($harmonyBuildDir.Length) 字符），鸿蒙工具链建议 < 110：" -ForegroundColor Yellow
    Write-Host "   $harmonyBuildDir" -ForegroundColor DarkGray
}
Write-Host ""

# ── 辅助函数：调用 HBuilderX CLI（显式 UTF-8 解码，避免乱码）──
function Invoke-HBuilderXCLI {
    param([Parameter(Mandatory = $true)][string]$Arguments)
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $HBuilderXPath
    $psi.Arguments = $Arguments
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.StandardOutputEncoding = [System.Text.Encoding]::UTF8
    $psi.StandardErrorEncoding = [System.Text.Encoding]::UTF8
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true
    $proc = New-Object System.Diagnostics.Process
    $proc.StartInfo = $psi
    $allLines = New-Object System.Collections.Generic.List[string]
    [void]$proc.Start()
    $stdoutTask = $proc.StandardOutput.ReadToEndAsync()
    $stderrTask = $proc.StandardError.ReadToEndAsync()
    $proc.WaitForExit()
    ($stdoutTask.Result + "`r`n" + $stderrTask.Result) -split "`r?`n" | ForEach-Object {
        if ($_.Trim()) { [void]$allLines.Add($_); Write-Host $_ }
    }
    return $allLines
}

# 在 app-harmony 工程下递归找最新 .app(优先) / *-signed.hap，返回 FileInfo 或 $null
function Find-LatestHarmonyArtifact {
    if (-not (Test-Path $harmonyBuildDir)) { return $null }
    $app = Get-ChildItem -Path $harmonyBuildDir -Recurse -Filter "*.app" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($app) { return $app }
    $hap = Get-ChildItem -Path $harmonyBuildDir -Recurse -Filter "*-signed.hap" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    return $hap
}

# ── 步骤 2：清理旧产物 ─────────────────────────────────
Write-Host "【步骤 2/6】清理旧构建产物" -ForegroundColor Yellow
if ($KeepOldArtifacts -or $UseHvigor) {
    Write-Host "⏭  跳过清理（-KeepOldArtifacts 或 -UseHvigor）" -ForegroundColor DarkGray
}
elseif (Test-Path $harmonyBuildDir) {
    $stashTag  = $BuildStartTime.ToString('yyyyMMdd_HHmmss')
    $stashThis = Join-Path $ProjectRoot "unpackage\.stash\$stashTag"
    try {
        New-Item -ItemType Directory -Path $stashThis -Force | Out-Null
        Move-Item -Path $harmonyBuildDir -Destination (Join-Path $stashThis "app-harmony") -Force
        Write-Host "✅ 已归档旧产物: app-harmony → .stash\$stashTag\" -ForegroundColor Green
    }
    catch {
        Write-Host "⚠️ 无法移动 app-harmony（可能被 DevEco/守护进程占用），尝试强删..." -ForegroundColor Yellow
        Remove-Item -Recurse -Force $harmonyBuildDir -ErrorAction SilentlyContinue
    }
}
else {
    Write-Host "ℹ️ 无旧产物" -ForegroundColor DarkGray
}
Write-Host ""

# ── 步骤 3：bump versionCode ───────────────────────────
Write-Host "【步骤 3/6】更新版本号" -ForegroundColor Yellow
$manifestPath = Join-Path $ProjectRoot "manifest.json"
if (-not (Test-Path $manifestPath)) {
    Write-Host "❌ 找不到 manifest.json: $manifestPath" -ForegroundColor Red
    exit 1
}
$manifestRaw  = Get-Content $manifestPath -Raw -Encoding UTF8
$verCodeMatch = [regex]::Match($manifestRaw, '"versionCode"\s*:\s*"?(\d+)"?')
$verNameMatch = [regex]::Match($manifestRaw, '"versionName"\s*:\s*"([^"]+)"')
if ($verCodeMatch.Success) {
    $oldCode = [int]$verCodeMatch.Groups[1].Value
    $verName = if ($verNameMatch.Success) { $verNameMatch.Groups[1].Value } else { "?" }
    if ($SkipBumpVersion) {
        Write-Host "⏭  跳过 bump  (versionName=$verName, versionCode=$oldCode)" -ForegroundColor DarkGray
    }
    else {
        $newCode = $oldCode + 1
        $manifestNew = $manifestRaw -replace '("versionCode"\s*:\s*"?)\d+("?)', "`${1}$newCode`${2}"
        [System.IO.File]::WriteAllText($manifestPath, $manifestNew, (New-Object System.Text.UTF8Encoding $true))
        Write-Host "✅ versionCode: $oldCode → $newCode  (versionName=$verName)" -ForegroundColor Green
    }
}
else {
    Write-Host "⚠️ manifest.json 中无 versionCode，跳过自增" -ForegroundColor Yellow
}
Write-Host ""

# ── -UseHvigor 分支：跳过 HBuilderX，直接 hvigorw ───────
if ($UseHvigor) {
    Write-Host "【步骤 4-5/6】使用 hvigorw 直接构建（-UseHvigor）" -ForegroundColor Yellow
    if (-not (Test-Path $harmonyBuildDir)) {
        Write-Host "❌ 未找到 DevEco 工程 $harmonyBuildDir" -ForegroundColor Red
        Write-Host "   -UseHvigor 需要先用 HBuilderX 成功 pack 过一次以生成工程目录。" -ForegroundColor Yellow
        exit 2
    }
    $hvigorw = Join-Path $harmonyBuildDir "hvigorw.bat"
    if (-not (Test-Path $hvigorw)) { $hvigorw = Join-Path $harmonyBuildDir "hvigorw" }
    if (-not (Test-Path $hvigorw)) {
        Write-Host "❌ 未找到 hvigorw（$harmonyBuildDir 下）。工程可能不完整。" -ForegroundColor Red
        exit 2
    }
    Push-Location $harmonyBuildDir
    try {
        Write-Host "→ ohpm install --all" -ForegroundColor DarkGray
        & cmd /c "ohpm install --all" 2>&1 | ForEach-Object { Write-Host $_ }
        Write-Host "→ hvigorw assembleApp -p buildMode=release --no-daemon" -ForegroundColor DarkGray
        & cmd /c "`"$hvigorw`" assembleApp -p buildMode=release --no-daemon" 2>&1 | ForEach-Object { Write-Host $_ }
    }
    finally { Pop-Location }
}
else {
    # ── 步骤 4：导入工程 ───────────────────────────────────
    Write-Host "【步骤 4/6】导入工程到 HBuilderX 工作区" -ForegroundColor Yellow
    Write-Host "（如提示未登录，请先在 HBuilderX 登录 DCloud 账号；鸿蒙打包还需 DevEco 工具链就绪）" -ForegroundColor DarkYellow
    & $HBuilderXPath project open --path "$ProjectRoot" 2>&1 | Out-Null
    Write-Host "✅ 已触发 project open" -ForegroundColor Green
    Write-Host ""

    # ── 步骤 5：触发鸿蒙打包 ───────────────────────────────
    Write-Host "【步骤 5/6】鸿蒙本地打包（HBuilderX 调 hvigorw）" -ForegroundColor Yellow
    Write-Host "  ⚠️ cli 鸿蒙 pack 子命令语法存在不确定性，脚本会按已知形式尝试并以产物时间戳判定成功。" -ForegroundColor DarkYellow
    Write-Host ""
    # 已知/可能的命令形式（按可能性排序）；任一让产物刷新即视为成功
    $packAttempts = @(
        "pack app-harmony --project `"$ProjectRoot`"",
        "pack --platform harmony --project `"$ProjectRoot`"",
        "pack --project `"$ProjectRoot`" --platform harmony"
    )
    foreach ($args in $packAttempts) {
        Write-Host "→ cli $args" -ForegroundColor DarkGray
        Invoke-HBuilderXCLI -Arguments $args | Out-Null
        $art = Find-LatestHarmonyArtifact
        if ($art -and $art.LastWriteTime -ge $BuildStartTime) {
            Write-Host "✅ 命令生效，产物已刷新" -ForegroundColor Green
            break
        }
        Write-Host "  （该形式未产出新包，尝试下一种…）" -ForegroundColor DarkGray
    }
}
Write-Host ""

# ── 步骤 6：产物校验 ──────────────────────────────────
Write-Host "【步骤 6/6】校验打包产物" -ForegroundColor Yellow
$artifact = Find-LatestHarmonyArtifact

if (-not $artifact -or $artifact.LastWriteTime -lt $BuildStartTime) {
    Write-Host ""
    Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host " ⚠️  未获取到本次新生成的鸿蒙产物（.app/.hap）" -ForegroundColor Red
    Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host ""
    Write-Host "  多半原因：DevEco/CLT 未就绪、HBuilderX 未配 鸿蒙 DevTools 路径、" -ForegroundColor Yellow
    Write-Host "  签名证书未配，或 cli 鸿蒙 pack 命令形式与本机 HBuilderX 版本不符。" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  请改用 GUI 兜底：" -ForegroundColor Yellow
    Write-Host "    1) 打开 HBuilderX（项目应已导入）" -ForegroundColor White
    Write-Host "    2) 顶部菜单 [发行] → [原生App-本地打包] → 选择 鸿蒙(HarmonyOS)" -ForegroundColor White
    Write-Host "       或直接用 DevEco Studio 打开 $harmonyBuildDir，Build → Build App(s)" -ForegroundColor White
    Write-Host "    3) 产物 .app 默认在 $harmonyBuildDir 下" -ForegroundColor White
    Write-Host "    4) 出包后单独执行上传： .\upload-to-agc.ps1" -ForegroundColor Cyan
    Write-Host ""
    exit 5
}

$ageSec = [math]::Round(((Get-Date) - $artifact.LastWriteTime).TotalSeconds)
$isApp  = $artifact.Extension -ieq ".app"
Write-Host "✅ 产物: $($artifact.FullName)" -ForegroundColor Green
Write-Host "   类型: $(if ($isApp) {'.app (App Pack，AGC 正式渠道需要此格式)'} else {'.hap (单 HAP，仅可走测试通道/安装调试)'})" -ForegroundColor White
Write-Host "   大小: $([math]::Round($artifact.Length / 1MB, 2)) MB  生成于: $($artifact.LastWriteTime)（${ageSec} 秒前）" -ForegroundColor White
if (-not $isApp) {
    Write-Host "⚠️ 当前产物是 .hap 而非 .app。AGC 正式发布渠道只接收 .app（多 HAP 聚合包）。" -ForegroundColor Yellow
    Write-Host "   如需上架请用 hvigorw assembleApp 或 HBuilderX 出 .app。" -ForegroundColor Yellow
}
Write-Host ""
Write-Host "下一步上传到 AppGallery Connect：" -ForegroundColor Cyan
Write-Host "  .\upload-to-agc.ps1 -HapPath `"$($artifact.FullName)`"" -ForegroundColor Cyan
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "鸿蒙打包完成 ✅" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
