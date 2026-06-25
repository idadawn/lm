#Requires -Version 5.1
<#
.SYNOPSIS
    uni-app Android 自动云打包 + 蒲公英上传
.DESCRIPTION
    流程：
      0. 加载 scripts/.env 中的默认参数（命令行参数会覆盖 .env）
      1. 检查 HBuilderX 环境
      2. 清理旧产物（dist/build/app-plus、release/apk → 移到 .stash/）
      3. 自动 bump versionCode（避免设备识别不出新版本）
      4. 用 CLI 生成最新 App 资源，并校验产物时间戳
      5. 用 cli pack 触发自动化云打包（云端证书）
      6. 解析下载链接并下载 APK
      7. 上传到蒲公英（可选）
.PARAMETER PgyerApiKey
    蒲公英 API Key。空时读 .env 中 PGYER_API_KEY；都空则跳过上传
.PARAMETER Description
    蒲公英更新说明
.PARAMETER MaxWaitMinutes
    云打包最大等待分钟（cli pack 为阻塞命令，此参数仅作备用上限）
.PARAMETER SkipBumpVersion
    跳过自动 bump versionCode
.PARAMETER KeepOldArtifacts
    保留旧产物，不移到 .stash/
.PARAMETER PackType
    Android 打包证书类型：0=自定义证书, 1=公共测试证书(已弃用), 2=DCloud 旧证书, 3=云端证书。默认 3
.EXAMPLE
    # 全部从 .env 读
    .\build-android.ps1
    # 临时覆盖更新说明
    .\build-android.ps1 -Description "v1.2 修复班次统计 bug"
#>
param(
    [string]$PgyerApiKey = "",
    [string]$Description = "",
    [int]$MaxWaitMinutes = 0,
    [switch]$SkipBumpVersion,
    [switch]$KeepOldArtifacts,
    [int]$PackType = -1
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent $PSScriptRoot
$BuildStartTime = Get-Date

# ── 加载 .env（命令行参数优先） ─────────────────────────
. (Join-Path $PSScriptRoot "_dotenv.ps1")
$envFile = Join-Path $PSScriptRoot ".env"
$envMap = Get-DotEnv -Path $envFile
$envLoaded = (Test-Path $envFile)

$PgyerApiKey       = Coalesce-EnvValue -ParamValue $PgyerApiKey -EnvMap $envMap -Key "PGYER_API_KEY"
$Description       = Coalesce-EnvValue -ParamValue $Description -EnvMap $envMap -Key "PGYER_DESCRIPTION"
$HBuilderXFromEnv  = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "HBUILDER_CLI"
if ($MaxWaitMinutes -le 0) {
    $waitFromEnv = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "MAX_WAIT_MINUTES" -Default "30"
    $MaxWaitMinutes = [int]$waitFromEnv
}
$autoBumpEnv = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "AUTO_BUMP_VERSION" -Default "true"
if ($autoBumpEnv -eq "false" -or $autoBumpEnv -eq "0") { $SkipBumpVersion = $true }

# Android 打包证书类型
if ($PackType -lt 0) {
    $ptEnv = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "ANDROID_PACK_TYPE" -Default "3"
    $PackType = [int]$ptEnv
}
if (@(0, 1, 2, 3) -notcontains $PackType) {
    Write-Host "❌ ANDROID_PACK_TYPE 必须为 0/1/2/3，当前: $PackType" -ForegroundColor Red
    exit 1
}

$certLabel = switch ($PackType) {
    0 { "自定义证书" }
    1 { "公共测试证书(已弃用)" }
    2 { "DCloud 旧证书" }
    3 { "云证书" }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  检测室数据分析 - Android 自动打包" -ForegroundColor Cyan
Write-Host "  构建开始: $($BuildStartTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor Cyan
Write-Host "  证书类型: $certLabel" -ForegroundColor Cyan
if ($envLoaded) {
    Write-Host "  配置文件: $envFile" -ForegroundColor DarkGray
}
else {
    Write-Host "  配置文件: (未找到 $envFile，使用默认值)" -ForegroundColor DarkGray
}
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# ── 步骤 1：查找 HBuilderX ───────────────────────────────
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
    Write-Host "   或下载 HBuilderX: https://www.dcloud.io/hbuilderx.html" -ForegroundColor Cyan
    exit 1
}
Write-Host ""

# ── 步骤 2：清理旧产物 ─────────────────────────────────
Write-Host "【步骤 2/6】清理旧构建产物" -ForegroundColor Yellow

$distBuildDir = Join-Path $ProjectRoot "unpackage\dist\build\app-plus"
$apkDir       = Join-Path $ProjectRoot "unpackage\release\apk"
$stashRoot    = Join-Path $ProjectRoot "unpackage\.stash"

if ($KeepOldArtifacts) {
    Write-Host "⏭  跳过清理（-KeepOldArtifacts）" -ForegroundColor DarkGray
}
else {
    $stashTag = $BuildStartTime.ToString('yyyyMMdd_HHmmss')
    $stashThis = Join-Path $stashRoot $stashTag

    foreach ($p in @($distBuildDir, $apkDir)) {
        if (Test-Path $p) {
            New-Item -ItemType Directory -Path $stashThis -Force | Out-Null
            $leaf = Split-Path -Leaf $p
            $dest = Join-Path $stashThis $leaf
            try {
                Move-Item -Path $p -Destination $dest -Force
                Write-Host "✅ 已归档旧产物: $leaf → .stash\$stashTag\" -ForegroundColor Green
            }
            catch {
                Write-Host "⚠️ 无法移动 $p (可能被占用)，尝试强删..." -ForegroundColor Yellow
                Remove-Item -Recurse -Force $p -ErrorAction SilentlyContinue
            }
        }
    }
    New-Item -ItemType Directory -Path $apkDir -Force | Out-Null
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
$pkgNameMatch = [regex]::Match($manifestRaw, '"packagename"\s*:\s*"([^"]+)"')

$packageName = if ($pkgNameMatch.Success) { $pkgNameMatch.Groups[1].Value } else { $null }

if ($verCodeMatch.Success) {
    $oldCode = [int]$verCodeMatch.Groups[1].Value
    $newCode = $oldCode + 1
    $verName = if ($verNameMatch.Success) { $verNameMatch.Groups[1].Value } else { "?" }

    if ($SkipBumpVersion) {
        Write-Host "⏭  跳过 bump (AUTO_BUMP_VERSION=false 或 -SkipBumpVersion)" -ForegroundColor DarkGray
        Write-Host "   当前 versionName=$verName, versionCode=$oldCode" -ForegroundColor White
    }
    else {
        $manifestNew = $manifestRaw -replace '("versionCode"\s*:\s*"?)\d+("?)', "`${1}$newCode`${2}"
        [System.IO.File]::WriteAllText($manifestPath, $manifestNew, (New-Object System.Text.UTF8Encoding $true))
        Write-Host "✅ versionCode: $oldCode → $newCode  (versionName=$verName)" -ForegroundColor Green
    }
}
else {
    Write-Host "⚠️ manifest.json 中无 versionCode，跳过自增" -ForegroundColor Yellow
}

if (-not $packageName) {
    Write-Host "⚠️ 未从 manifest.json 中读取到 android.packagename，cli pack 可能会报错。" -ForegroundColor Yellow
}
Write-Host ""

# ── 辅助函数：调用 HBuilderX CLI（显式指定 UTF-8 解码，避免乱码）──
function Invoke-HBuilderXCLI {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Arguments
    )
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $HBuilderXPath
    $psi.Arguments = $Arguments
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    # HBuilderX 5.07 输出 UTF-8，必须显式指定，否则 PowerShell 5.1 默认用 GBK 解码导致乱码
    $psi.StandardOutputEncoding = [System.Text.Encoding]::UTF8
    $psi.StandardErrorEncoding = [System.Text.Encoding]::UTF8
    $psi.UseShellExecute = $false
    $psi.CreateNoWindow = $true

    $proc = New-Object System.Diagnostics.Process
    $proc.StartInfo = $psi
    $allLines = New-Object System.Collections.Generic.List[string]

    [void]$proc.Start()
    # 先异步启动读取避免死锁，再等待退出
    $stdoutTask = $proc.StandardOutput.ReadToEndAsync()
    $stderrTask = $proc.StandardError.ReadToEndAsync()
    $proc.WaitForExit()

    $stdout = $stdoutTask.Result
    $stderr = $stderrTask.Result

    ($stdout + "`r`n" + $stderr) -split "`r?`n" | ForEach-Object {
        if ($_.Trim()) {
            [void]$allLines.Add($_)
            Write-Host $_
        }
    }
    return $allLines
}

# ── 步骤 4：生成最新 App 资源（强校验） ─────────────────
Write-Host "【步骤 4/6】生成最新 App 资源" -ForegroundColor Yellow
Write-Host "（如提示未登录，请先在 HBuilderX 登录 DCloud 账号）" -ForegroundColor DarkYellow

# 关键：cli publish/pack 只识别"已导入 HBuilderX 工作区"的项目。
# 必须用 `project open --path` 真正导入；旧写法 `open --project` 不会导入，
# 全新检出 / git worktree 会在 publish 阶段报"项目 ... 不存在，请先导入"。
& $HBuilderXPath project open --path "$ProjectRoot" 2>&1 | Out-Null

# manifest 引用的应用图标位于 unpackage/res/icons/*，这些资源通常不纳入 git，
# 全新检出 / worktree 可能缺失，会让云打包阶段报"文件不存在"。提前明确告警。
$iconProbe = Join-Path $ProjectRoot "unpackage\res\icons\72x72.png"
if (-not (Test-Path $iconProbe)) {
    Write-Host "⚠️ 未找到应用图标 $iconProbe —— 云打包会报「文件不存在」。" -ForegroundColor Yellow
    Write-Host "   请从已有检出复制 unpackage\res\icons\ 或在 HBuilderX 中重新生成图标后再打包。" -ForegroundColor Yellow
}

$publishResult = Invoke-HBuilderXCLI -Arguments "publish --platform APP --project `"$ProjectRoot`" --type appResource"

$distManifest = Join-Path $distBuildDir "manifest.json"
if (-not (Test-Path $distManifest)) {
    Write-Host ""
    Write-Host "❌ App 资源生成失败：$distManifest 不存在" -ForegroundColor Red
    Write-Host "   排查：HBuilderX 是否已登录？请手动在 HBuilderX 中执行" -ForegroundColor Yellow
    Write-Host "   [发行] -> [原生App-本地打包] -> [生成本地打包App资源]" -ForegroundColor Yellow
    exit 2
}
$distMTime = (Get-Item $distManifest).LastWriteTime
if ($distMTime -lt $BuildStartTime) {
    Write-Host ""
    Write-Host "❌ App 资源未重新生成！" -ForegroundColor Red
    Write-Host "   产物时间 $distMTime  <  构建开始 $BuildStartTime" -ForegroundColor Yellow
    Write-Host "   多半是 HBuilderX 未登录或 CLI 命令未真正执行。" -ForegroundColor Yellow
    exit 3
}
Write-Host "✅ 资源已重新生成（$distMTime）" -ForegroundColor Green
Write-Host ""

# ── 步骤 5：使用 cli pack 自动化云打包 ─────────────────
Write-Host "【步骤 5/6】云打包 APK（$certLabel）" -ForegroundColor Yellow
Write-Host "  cli pack 为阻塞命令，会实时输出云打包进度..." -ForegroundColor DarkYellow
Write-Host ""

if (-not $packageName) {
    Write-Host "❌ 缺少包名，无法执行 cli pack。请在 manifest.json 中配置 android.packagename" -ForegroundColor Red
    exit 1
}

$packOutputLines = Invoke-HBuilderXCLI -Arguments "pack --project `"$ProjectRoot`" --platform android --android.packagename $packageName --android.androidpacktype $PackType"

# 解析输出
$downloadUrl = $null
$packSuccess = $false
$packError = $false
$packOutputLines | ForEach-Object {
    $line = $_
    if ($line -match "Successfully packaged|打包成功") {
        $packSuccess = $true
    }
    if ($line -match "Error|error|失败|unsupported|请手动|参数.*不能为空|未登录") {
        $packError = $true
    }
    if ($line -match "(Download Link|下载地址):\s*(https?://\S+)") {
        $downloadUrl = $matches[2]
    }
}

# ── 步骤 6：下载 APK ──────────────────────────────────
Write-Host ""
Write-Host "【步骤 6/6】获取打包产物" -ForegroundColor Yellow

$foundApk = $null

if ($downloadUrl -and $packSuccess) {
    Write-Host "✅ 云打包成功，解析到下载链接" -ForegroundColor Green
    $apkFileName = "app_$($BuildStartTime.ToString('yyyyMMdd_HHmmss')).apk"
    $foundApk = Join-Path $apkDir $apkFileName

    Write-Host "→ 正在下载 APK..." -ForegroundColor Yellow
    try {
        # 先尝试 curl（通常更快更稳定）
        & curl.exe -s -L -o "$foundApk" "$downloadUrl" 2>&1 | ForEach-Object { Write-Host $_ }
        if ($LASTEXITCODE -eq 0 -and (Test-Path $foundApk) -and (Get-Item $foundApk).Length -gt 0) {
            Write-Host "✅ 下载完成" -ForegroundColor Green
        }
        else {
            # curl 失败时 fallback 到 Invoke-WebRequest
            if (Test-Path $foundApk) { Remove-Item $foundApk -Force -ErrorAction SilentlyContinue }
            Write-Host "⚠️ curl 下载异常，尝试 Invoke-WebRequest..." -ForegroundColor Yellow
            Invoke-WebRequest -Uri $downloadUrl -OutFile $foundApk -UseBasicParsing
        }
    }
    catch {
        Write-Host "❌ 下载 APK 失败: $_" -ForegroundColor Red
        $foundApk = $null
    }
}
elseif ($packError) {
    Write-Host "❌ 云打包过程报错，未能获取下载链接。" -ForegroundColor Red
}
else {
    Write-Host "⚠️ 云打包输出中未找到下载链接，可能打包未完成或输出格式有变化。" -ForegroundColor Yellow
}

# 如果自动化流程失败，提示手动兜底
if (-not $foundApk) {
    Write-Host ""
    Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host " ⚠️  自动云打包未能获取 APK" -ForegroundColor Red
    Write-Host "════════════════════════════════════════════════════════════" -ForegroundColor Red
    Write-Host ""
    Write-Host "  请按以下步骤手动在 HBuilderX GUI 中操作：" -ForegroundColor Yellow
    Write-Host "    1) 切到 HBuilderX 窗口（项目应已自动打开）" -ForegroundColor White
    Write-Host "    2) 顶部菜单 [发行] → [原生App-云打包]" -ForegroundColor White
    Write-Host "    3) 选 Android，勾选 [$certLabel]" -ForegroundColor White
    Write-Host "    4) 点击 [打包] 按钮，等待打包完成" -ForegroundColor White
    Write-Host "    5) 下载 APK 后放到: $apkDir" -ForegroundColor White
    Write-Host ""
    Write-Host "  手动完成后，可单独执行上传：" -ForegroundColor Cyan
    Write-Host "    .\upload-to-pgyer.ps1" -ForegroundColor Cyan
    Write-Host ""
    exit 5
}

# ── 上传 ───────────────────────────────────────────────
$apkInfo = Get-ItemProperty -Path $foundApk
$apkAgeSec = [math]::Round(((Get-Date) - $apkInfo.LastWriteTime).TotalSeconds)
Write-Host ""
Write-Host "APK 路径: $foundApk" -ForegroundColor Green
Write-Host "大小:     $([math]::Round($apkInfo.Length / 1MB, 2)) MB" -ForegroundColor White
Write-Host "生成于:   $($apkInfo.LastWriteTime)（${apkAgeSec} 秒前）" -ForegroundColor White
Write-Host ""

if ($PgyerApiKey) {
    Write-Host "→ 上传到蒲公英..." -ForegroundColor Yellow
    & "$PSScriptRoot\upload-to-pgyer.ps1" -ApkPath $foundApk -ApiKey $PgyerApiKey -Description $Description
}
else {
    Write-Host "未在 .env 或参数中找到 PGYER_API_KEY，跳过上传。" -ForegroundColor Yellow
    Write-Host "如需手动上传:" -ForegroundColor White
    Write-Host "  .\upload-to-pgyer.ps1 -ApkPath `"$foundApk`"" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "打包完成 ✅" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
