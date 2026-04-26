# LM 系统发布入口脚本
# 快捷入口，调用 publish/publish.ps1
#
# 用法:
#   完整发布:   .\build-release.ps1 -Version "1.1.0"
#   增量发布:   .\build-release.ps1 -Version "1.1.0" -Incremental -BaseVersion "1.0.0"
#   仅 API:     .\build-release.ps1 -Target api -Version "1.1.0"
#   仅 Web:     .\build-release.ps1 -Target web -Version "1.1.0"

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("all", "api", "worker", "web")]
    [string]$Target = "all",
    
    [Parameter(Mandatory=$false)]
    [string]$Version = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$Incremental,
    
    [Parameter(Mandatory=$false)]
    [string]$BaseVersion = "",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipBuild,
    
    [Parameter(Mandatory=$false)]
    [switch]$Deploy,
    
    [Parameter(Mandatory=$false)]
    [switch]$Clean
)

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$PublishScript = Join-Path $ScriptDir "publish\publish.ps1"

if (-not (Test-Path $PublishScript)) {
    Write-Host "错误: 发布脚本不存在: $PublishScript" -ForegroundColor Red
    exit 1
}

# 构建参数
$publishParams = @{
    Target = $Target
}

if ($Version) {
    $publishParams.Version = $Version
}

if ($Incremental) {
    $publishParams.Incremental = $true
}

if ($BaseVersion) {
    $publishParams.BaseVersion = $BaseVersion
}

if ($SkipBuild) {
    $publishParams.SkipBuild = $true
}

if ($Deploy) {
    $publishParams.Deploy = $true
}

if ($Clean) {
    $publishParams.Clean = $true
}

# 执行发布脚本
& $PublishScript @publishParams
