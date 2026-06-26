#Requires -Version 5.1
<#
.SYNOPSIS
    上传鸿蒙 .app/.hap 到 AppGallery Connect (AGC) —— 华为 HarmonyOS 发布 API（v3）
.DESCRIPTION
    用 AGC「Connect API」(client_id + client_secret 鉴权) 上传 HarmonyOS 应用包：
      1. POST /api/oauth2/v1/token            换取 48h Bearer token
      2. GET  /api/publish/v2/upload-url/for-obs   取预签名 OBS 上传地址（5 分钟有效）
      3. PUT  到预签名地址                     上传二进制（curl -T 优先，IWR 兜底）
      4. PUT  /api/publish/v3/app-package-info 绑定 fileName+objectId，拿 packageId
      5.（可选 -Submit）POST /api/publish/v3/app-submit  提交审核 / 发布

    优先级：命令行参数 > scripts/.env > 自动探测

    ⚠️ 前置（脚本无法替代，详见 PUBLISH-HARMONY.md）：
      - 在 AGC 控制台手动创建 HarmonyOS 应用，拿到 appId；bundleName 必须与
        manifest.json app-harmony.distribute.bundleName(=com.emergen.lm) 完全一致
      - 在 AGC 控制台 [用户与访问]->[API 客户端] 创建 client_id/client_secret
        （创建时 Project 保持 N/A，否则 403；至少 运营人员(Operator) 角色；secret 仅一次性显示）
      - 首次正式发布需在控制台填齐元数据（截图/隐私政策/分类等），否则 app-submit 报信息不完整

    ⚠️ 诚实说明：作者无法在本机验证整条 AGC 上传链。其中 upload-url 响应里 objectId 的
    确切字段路径，华为各版本文档有出入——脚本会先打印整段响应再多路径回退取值；
    AGC 正式渠道只接收 .app（多 HAP 聚合包），单 .hap 仅适合测试通道，脚本会按后缀告警。
    默认只上传+绑定，不自动提审（避免误触发正式发布）；要提审请显式加 -Submit。
.PARAMETER HapPath
    .app(优先) 或 .hap 路径。空时自动取 unpackage/dist/build/app-harmony 下最新 .app/-signed.hap
.PARAMETER ClientId
    AGC API 客户端 ID。空时读 .env 中 AGC_CLIENT_ID
.PARAMETER ClientSecret
    AGC API 客户端 Secret。空时读 .env 中 AGC_CLIENT_SECRET
.PARAMETER AppId
    AGC 应用 ID。空时读 .env 中 AGC_APP_ID
.PARAMETER Region
    数据处理地，决定域名：cn / dre / dra / drru。空时读 .env 中 AGC_REGION，默认 cn
.PARAMETER ReleaseType
    1=全网正式发布，6=HarmonyOS 测试发布。空时读 .env 中 AGC_RELEASE_TYPE，默认 6（更快、适合内测自检）
.PARAMETER Submit
    上传绑定后提交审核 / 发布（POST app-submit）。默认不提交，仅上传到 AGC。
.PARAMETER Description
    更新说明。空时读 .env 中 HARMONY_DESCRIPTION
.EXAMPLE
    # 全部从 .env 读 + 自动找最新 .app，仅上传不提审
    .\upload-to-agc.ps1
    # 指定包并提交测试发布
    .\upload-to-agc.ps1 -HapPath "..\unpackage\dist\build\app-harmony\xxx.app" -ReleaseType 6 -Submit
#>
param(
    [string]$HapPath      = "",
    [string]$ClientId     = "",
    [string]$ClientSecret = "",
    [string]$AppId        = "",
    [string]$Region       = "",
    [int]$ReleaseType     = 0,
    [switch]$Submit,
    [string]$Description  = ""
)

$ErrorActionPreference = "Stop"

# ── 加载 .env（命令行参数优先） ─────────────────────
. (Join-Path $PSScriptRoot "_dotenv.ps1")
$envFile = Join-Path $PSScriptRoot ".env"
$envMap  = Get-DotEnv -Path $envFile

$ClientId     = Coalesce-EnvValue -ParamValue $ClientId     -EnvMap $envMap -Key "AGC_CLIENT_ID"
$ClientSecret = Coalesce-EnvValue -ParamValue $ClientSecret -EnvMap $envMap -Key "AGC_CLIENT_SECRET"
$AppId        = Coalesce-EnvValue -ParamValue $AppId        -EnvMap $envMap -Key "AGC_APP_ID"
$Region       = Coalesce-EnvValue -ParamValue $Region       -EnvMap $envMap -Key "AGC_REGION" -Default "cn"
$Description  = Coalesce-EnvValue -ParamValue $Description   -EnvMap $envMap -Key "HARMONY_DESCRIPTION"
if ($ReleaseType -le 0) {
    $rtEnv = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "AGC_RELEASE_TYPE" -Default "6"
    $ReleaseType = [int]$rtEnv
}
if (-not $Submit) {
    $subEnv = Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key "AGC_SUBMIT" -Default "false"
    if ($subEnv -eq "true" -or $subEnv -eq "1") { $Submit = $true }
}

# ── 必填校验 ─────────────────────────────────────────
$missing = @()
if (-not $ClientId)     { $missing += "AGC_CLIENT_ID" }
if (-not $ClientSecret) { $missing += "AGC_CLIENT_SECRET" }
if (-not $AppId)        { $missing += "AGC_APP_ID" }
if ($missing.Count -gt 0) {
    Write-Host "❌ 缺少 AGC 配置: $($missing -join ', ')" -ForegroundColor Red
    Write-Host "   请在 $envFile 中填写，或用命令行参数传入。" -ForegroundColor Yellow
    Write-Host "   client_id/secret 来自 AGC 控制台 [用户与访问]->[API 客户端]（Project 保持 N/A，至少 Operator 角色）" -ForegroundColor Yellow
    Write-Host "   appId 来自 AGC 控制台 应用信息页（需先手动创建 HarmonyOS 应用）" -ForegroundColor Yellow
    exit 1
}
if (@(1, 6) -notcontains $ReleaseType) {
    Write-Host "❌ ReleaseType 必须为 1(正式) 或 6(测试)，当前: $ReleaseType" -ForegroundColor Red
    exit 1
}

# ── 域名映射 ─────────────────────────────────────────
$domainMap = @{ "cn" = "connect-api.cloud.huawei.com"; "dre" = "connect-api-dre.cloud.huawei.com";
                "dra" = "connect-api-dra.cloud.huawei.com"; "drru" = "connect-api-drru.cloud.huawei.com" }
$regionKey = $Region.ToLower()
if (-not $domainMap.ContainsKey($regionKey)) {
    Write-Host "❌ Region 必须为 cn/dre/dra/drru，当前: $Region" -ForegroundColor Red
    exit 1
}
$base = "https://" + $domainMap[$regionKey]

# ── HapPath 自动探测 ───────────────────────────────
if (-not $HapPath) {
    $harmonyDir = Join-Path (Split-Path -Parent $PSScriptRoot) "unpackage\dist\build\app-harmony"
    if (-not (Test-Path $harmonyDir)) {
        Write-Error "未指定 -HapPath 且 $harmonyDir 不存在。请先执行 build-harmony.ps1。"
        exit 1
    }
    $latest = Get-ChildItem -Path $harmonyDir -Recurse -Include "*.app" -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $latest) {
        $latest = Get-ChildItem -Path $harmonyDir -Recurse -Filter "*-signed.hap" -ErrorAction SilentlyContinue |
            Sort-Object LastWriteTime -Descending | Select-Object -First 1
    }
    if (-not $latest) {
        Write-Error "$harmonyDir 下没有 .app / *-signed.hap 文件。请先执行 build-harmony.ps1。"
        exit 1
    }
    $HapPath = $latest.FullName
    Write-Host "ℹ️  自动选取最新产物: $HapPath" -ForegroundColor DarkGray
}
if (-not (Test-Path $HapPath)) { Write-Error "文件不存在: $HapPath"; exit 1 }
$HapPath = (Resolve-Path $HapPath).Path
$fileInfo = Get-Item $HapPath
$fileName = $fileInfo.Name
$fileSize = $fileInfo.Length
$ext = $fileInfo.Extension.ToLower()
if ($ext -ne ".app" -and $ReleaseType -eq 1) {
    Write-Host "⚠️ 正式发布(ReleaseType=1) 只接收 .app（多 HAP 聚合包），当前是 $ext。" -ForegroundColor Yellow
    Write-Host "   单 .hap 可能解析失败。建议用 .app，或改 ReleaseType=6 走测试通道。" -ForegroundColor Yellow
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  AppGallery Connect 鸿蒙包上传" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "文件    : $HapPath" -ForegroundColor White
Write-Host "大小    : $([math]::Round($fileSize / 1MB, 2)) MB" -ForegroundColor White
Write-Host "域名    : $base ($regionKey)" -ForegroundColor White
Write-Host "发布类型: $(if ($ReleaseType -eq 1) {'1=全网正式（需华为审核）'} else {'6=HarmonyOS 测试'})" -ForegroundColor White
Write-Host "提交审核: $(if ($Submit) {'是（-Submit）'} else {'否（仅上传，不提审）'})" -ForegroundColor White
Write-Host ""

# 统一判定 AGC 返回是否成功（兼容 code=0 与 "00000000" 两种码型）
function Test-AgcOk($resp) {
    if ($null -eq $resp) { return $false }
    $code = $null
    if ($resp.PSObject.Properties.Name -contains 'ret') { $code = $resp.ret.code }
    elseif ($resp.PSObject.Properties.Name -contains 'code') { $code = $resp.code }
    return ($code -eq 0 -or "$code" -eq "0" -or "$code" -eq "00000000")
}

try {
    # ── Step 1: 获取 access token ─────────────────────
    Write-Host "→ [1/5] 获取 access token..." -ForegroundColor Yellow
    $tokenBody = @{ grant_type = "client_credentials"; client_id = $ClientId; client_secret = $ClientSecret } | ConvertTo-Json
    $tokenResp = Invoke-RestMethod -Uri "$base/api/oauth2/v1/token" -Method POST -ContentType "application/json" -Body $tokenBody
    $token = $tokenResp.access_token
    if (-not $token) { Write-Error "未取得 access_token，返回: $($tokenResp | ConvertTo-Json -Depth 5)"; exit 1 }
    $authHeaders = @{ "client_id" = $ClientId; "Authorization" = "Bearer $token" }
    Write-Host "  ✅ token 获取成功（有效期 $($tokenResp.expires_in) 秒）" -ForegroundColor Green

    # ── Step 2: 获取上传地址（5 分钟有效，拿到后立即上传）──
    Write-Host "→ [2/5] 获取上传地址..." -ForegroundColor Yellow
    $sha256 = (Get-FileHash $HapPath -Algorithm SHA256).Hash.ToLower()
    $q = "appId=$AppId&fileName=$([uri]::EscapeDataString($fileName))&contentLength=$fileSize&sha256=$sha256&releaseType=$ReleaseType"
    $urlResp = Invoke-RestMethod -Uri "$base/api/publish/v2/upload-url/for-obs?$q" -Method GET -Headers $authHeaders
    Write-Host "  ↳ upload-url 原始响应（用于核对 objectId 字段路径）：" -ForegroundColor DarkGray
    Write-Host "    $($urlResp | ConvertTo-Json -Depth 6 -Compress)" -ForegroundColor DarkGray
    if (-not (Test-AgcOk $urlResp)) { Write-Error "获取上传地址失败: $($urlResp.ret.msg)"; exit 1 }
    $uploadUrl = $urlResp.urlInfo.url
    # objectId 字段路径各版本文档有出入：多路径回退
    $objectId = $urlResp.urlInfo.objectId
    if (-not $objectId) { $objectId = $urlResp.objectId }
    if (-not $objectId -and $uploadUrl) {
        # 兜底：从 OBS URL 路径里截取（去掉查询串后的 path 即 objectId）
        try { $objectId = ([uri]$uploadUrl).AbsolutePath.TrimStart('/') } catch {}
    }
    if (-not $uploadUrl -or -not $objectId) {
        Write-Error "未能从响应解析 url/objectId，请按上面打印的响应结构调整脚本取值路径。"
        exit 1
    }
    Write-Host "  ✅ 上传地址已获取（5 分钟内有效）" -ForegroundColor Green

    # ── Step 3: PUT 上传文件到 OBS（curl -T 优先，更稳）──
    Write-Host "→ [3/5] 上传文件到 OBS..." -ForegroundColor Yellow
    $putOk = $false
    $curl = Get-Command curl.exe -ErrorAction SilentlyContinue
    if ($curl) {
        & curl.exe -s -S -X PUT -H "Content-Type: application/octet-stream" -T "$HapPath" "$uploadUrl"
        if ($LASTEXITCODE -eq 0) { $putOk = $true }
    }
    if (-not $putOk) {
        Write-Host "  （curl 不可用或失败，回退 Invoke-RestMethod）" -ForegroundColor DarkGray
        $bytes = [System.IO.File]::ReadAllBytes($HapPath)
        Invoke-RestMethod -Uri $uploadUrl -Method PUT -ContentType "application/octet-stream" -Body $bytes | Out-Null
        $putOk = $true
    }
    Write-Host "  ✅ 文件上传完成" -ForegroundColor Green

    # ── Step 4: 绑定包信息 ────────────────────────────
    Write-Host "→ [4/5] 绑定包信息 (app-package-info)..." -ForegroundColor Yellow
    $pkgBody = @{ fileName = $fileName; objectId = $objectId } | ConvertTo-Json
    $pkgResp = Invoke-RestMethod -Uri "$base/api/publish/v3/app-package-info?appId=$AppId&releaseType=$ReleaseType" `
        -Method PUT -ContentType "application/json" -Headers $authHeaders -Body $pkgBody
    if (-not (Test-AgcOk $pkgResp)) { Write-Error "绑定包信息失败: $($pkgResp.ret.msg) ($($pkgResp | ConvertTo-Json -Depth 5 -Compress))"; exit 1 }
    $packageId = $pkgResp.packageId
    Write-Host "  ✅ 绑定成功，packageId=$packageId" -ForegroundColor Green

    if (-not $Submit) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "✅ 已上传并绑定到 AGC（未提审）" -ForegroundColor Green
        Write-Host "   去 AGC 控制台 → 应用 → 版本管理 查看/手动提交，" -ForegroundColor White
        Write-Host "   或重跑加 -Submit 自动提交（releaseType=$ReleaseType）。" -ForegroundColor White
        Write-Host "========================================" -ForegroundColor Green
        exit 0
    }

    # ── Step 5: 提交审核 / 发布 ───────────────────────
    Write-Host "→ [5/5] 包异步解析中，等待 120 秒后提交..." -ForegroundColor Yellow
    Start-Sleep -Seconds 120
    $submitBody = @{ releaseType = $ReleaseType; releasePhase = 0 }
    if ($Description) { $submitBody["remark"] = $Description }
    $submitJson = $submitBody | ConvertTo-Json
    $submitResp = Invoke-RestMethod -Uri "$base/api/publish/v3/app-submit?appId=$AppId" `
        -Method POST -ContentType "application/json" -Headers $authHeaders -Body $submitJson
    if (Test-AgcOk $submitResp) {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "✅ 已提交（releaseType=$ReleaseType）" -ForegroundColor Green
        if ($ReleaseType -eq 1) { Write-Host "   进入华为审核队列（约 1-7 工作日）。" -ForegroundColor White }
        else { Write-Host "   已提交测试发布，去 AGC 控制台查看测试分发状态。" -ForegroundColor White }
        Write-Host "========================================" -ForegroundColor Green
    }
    else {
        Write-Error "提交失败: $($submitResp.ret.msg) ($($submitResp | ConvertTo-Json -Depth 5 -Compress))"
        exit 1
    }
}
catch {
    Write-Error "AGC 上传过程发生错误: $_"
    exit 1
}
