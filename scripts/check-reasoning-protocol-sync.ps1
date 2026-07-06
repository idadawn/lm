# check-reasoning-protocol-sync.ps1
# 同步检查脚本：保证 lm/web 与 lm/mobile 的共享类型与 nlq-agent 上游一致。
#
# 算法：
# 1. 遍历 $pairs manifest，每条为一个 upstream → downstreams 映射。
# 2. 计算 upstream 的 SHA256（标准化换行）。
# 3. 在每个下游 .d.ts 顶部期望注释行 `// upstream-sha: <hex>`。
# 4. 比对 pinned hash 与上游实际 hash；不一致 → exit 1。
# 5. 若下游 .d.ts 不存在（feature 阶段尚未生成），仅 warning，不 fail——主要为 CI 兜底。
#
# 用法：
#   pwsh scripts/check-reasoning-protocol-sync.ps1
#
# 退出码：
#   0 = in sync
#   1 = drift detected
#   2 = upstream file missing（fatal）

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot

$pairs = @(
    @{
        upstream = (Join-Path $repoRoot "nlq-agent\packages\shared-types\src\reasoning-protocol.ts")
        downstreams = @(
            (Join-Path $repoRoot "web\src\types\reasoning-protocol.d.ts"),
            (Join-Path $repoRoot "mobile\types\reasoning-protocol.d.ts")
        )
    },
    @{
        upstream = (Join-Path $repoRoot "nlq-agent\packages\shared-types\src\reasoning-step-presentation.ts")
        downstreams = @(
            (Join-Path $repoRoot "web\src\types\reasoning-step-presentation.d.ts"),
            (Join-Path $repoRoot "mobile\types\reasoning-step-presentation.d.ts")
        )
    }
)

function Get-NormalizedSha256 {
    param([string]$Path)
    $bytes = [System.IO.File]::ReadAllBytes($Path)
    # 标准化换行：CRLF -> LF，避免 Windows 编辑器误差
    $crlf = [string][char]13 + [string][char]10
    $lf   = [string][char]10
    $text = [System.Text.Encoding]::UTF8.GetString($bytes).Replace($crlf, $lf)
    $normalized = [System.Text.Encoding]::UTF8.GetBytes($text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha.ComputeHash($normalized)
        return ([System.BitConverter]::ToString($hash) -replace "-", "").ToLowerInvariant()
    } finally {
        $sha.Dispose()
    }
}

$drift = $false

foreach ($pair in $pairs) {
    $upstream = $pair.upstream
    $downstreams = $pair.downstreams

    if (-not (Test-Path $upstream)) {
        Write-Error "Upstream missing: $upstream"
        exit 2
    }

    $upstreamSha = Get-NormalizedSha256 -Path $upstream
    Write-Host "Upstream sha256: $upstreamSha"
    Write-Host "  Source: $upstream"

    $missing = @()

    foreach ($downstream in $downstreams) {
        if (-not (Test-Path $downstream)) {
            $missing += $downstream
            continue
        }

        $firstLines = Get-Content -Path $downstream -TotalCount 5
        $shaLine = $firstLines | Where-Object { $_ -match "//\s*upstream-sha:\s*([0-9a-fA-F]{64})" }

        if (-not $shaLine) {
            Write-Host "[FAIL] No `// upstream-sha: <hex>` comment in first 5 lines of $downstream" -ForegroundColor Red
            $drift = $true
            continue
        }

        $null = $shaLine -match "//\s*upstream-sha:\s*([0-9a-fA-F]{64})"
        $pinnedSha = $matches[1].ToLowerInvariant()

        if ($pinnedSha -ne $upstreamSha) {
            Write-Host "[FAIL] Drift detected in $downstream" -ForegroundColor Red
            Write-Host "       pinned: $pinnedSha"
            Write-Host "       upstream: $upstreamSha"
            Write-Host "       Action: copy upstream content to .d.ts and update pinned hash."
            $drift = $true
        } else {
            Write-Host "[OK]   $downstream in sync ($pinnedSha)" -ForegroundColor Green
        }
    }

    if ($missing.Count -gt 0) {
        Write-Host ""
        Write-Host "Note: the following downstream files do not exist yet:" -ForegroundColor Yellow
        foreach ($m in $missing) {
            Write-Host "  - $m" -ForegroundColor Yellow
        }
        Write-Host "This is expected during early phases when the lm/web or lm/mobile components have not been built." -ForegroundColor Yellow
    }
}

if ($drift) {
    exit 1
}

exit 0
