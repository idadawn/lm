#Requires -Version 5.1
<#
.SYNOPSIS
    简单的 .env 文件加载器（被打包脚本 dot-source 复用）
.DESCRIPTION
    解析格式：
      KEY=VALUE
      # 这是注释（行首 # 或纯空行会跳过）
      KEY="带空格的值"
      KEY='单引号值'
    返回 Hashtable，按 key 取值。
#>

function Get-DotEnv {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )
    $envMap = @{}
    if (-not (Test-Path $Path)) { return $envMap }

    Get-Content -Path $Path -Encoding UTF8 | ForEach-Object {
        $line = $_.Trim()
        if ($line -eq '' -or $line.StartsWith('#')) { return }
        $eqIdx = $line.IndexOf('=')
        if ($eqIdx -lt 1) { return }
        $key = $line.Substring(0, $eqIdx).Trim()
        $val = $line.Substring($eqIdx + 1).Trim()
        if ($val.Length -ge 2) {
            $first = $val[0]
            $last  = $val[$val.Length - 1]
            if (($first -eq '"' -and $last -eq '"') -or ($first -eq "'" -and $last -eq "'")) {
                $val = $val.Substring(1, $val.Length - 2)
            }
        }
        $envMap[$key] = $val
    }
    return $envMap
}

function Coalesce-EnvValue {
    param(
        [string]$ParamValue,
        [hashtable]$EnvMap,
        [string]$Key,
        [string]$Default = ""
    )
    if ($ParamValue) { return $ParamValue }
    if ($EnvMap.ContainsKey($Key) -and $EnvMap[$Key]) { return $EnvMap[$Key] }
    return $Default
}
