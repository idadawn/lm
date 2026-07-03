#Requires -Version 5.1

function Normalize-OssEndpoint {
    param([string]$Endpoint)
    if ([string]::IsNullOrWhiteSpace($Endpoint)) { return "" }
    $endpoint = $Endpoint.Trim().TrimEnd("/")
    return (($endpoint -replace "^https?://", "") -replace "/.*$", "")
}

function Get-OssScheme {
    param(
        [string]$Endpoint,
        [string]$Default = "https"
    )
    if ($Endpoint -match "^http://") { return "http" }
    if ($Endpoint -match "^https://") { return "https" }
    return $Default
}

function ConvertTo-OssEscapedKey {
    param([string]$Key)
    return (($Key.TrimStart("/") -split "/") | ForEach-Object { [Uri]::EscapeDataString($_) }) -join "/"
}

function New-OssObjectUri {
    param(
        [Parameter(Mandatory = $true)][string]$Endpoint,
        [Parameter(Mandatory = $true)][string]$Bucket,
        [Parameter(Mandatory = $true)][string]$Key,
        [bool]$EnableHttps = $true
    )
    $hostName = Normalize-OssEndpoint $Endpoint
    $scheme = if ($EnableHttps) { "https" } else { Get-OssScheme -Endpoint $Endpoint -Default "http" }
    if ($Endpoint -match "^https://") { $scheme = "https" }
    if ($Endpoint -match "^http://") { $scheme = "http" }
    $escapedKey = ConvertTo-OssEscapedKey $Key
    return "${scheme}://$Bucket.$hostName/$escapedKey"
}

function New-OssSignedHeaders {
    param(
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$Bucket,
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$AccessKeyId,
        [Parameter(Mandatory = $true)][string]$AccessKeySecret,
        [string]$ContentType = ""
    )
    $date = [DateTime]::UtcNow.ToString("r", [Globalization.CultureInfo]::InvariantCulture)
    $resource = "/$Bucket/$($Key.TrimStart('/'))"
    $stringToSign = "$($Method.ToUpperInvariant())`n`n$ContentType`n$date`n$resource"

    $hmac = New-Object System.Security.Cryptography.HMACSHA1
    $hmac.Key = [Text.Encoding]::UTF8.GetBytes($AccessKeySecret)
    $signature = [Convert]::ToBase64String($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($stringToSign)))

    $headers = @{
        Date = $date
        Authorization = "OSS $AccessKeyId`:$signature"
    }
    return $headers
}

function Read-OssErrorBody {
    param($ErrorRecord)
    try {
        $response = $ErrorRecord.Exception.Response
        if (-not $response) { return $ErrorRecord.Exception.Message }
        $stream = $response.GetResponseStream()
        if (-not $stream) { return $ErrorRecord.Exception.Message }
        $reader = New-Object IO.StreamReader($stream)
        return $reader.ReadToEnd()
    }
    catch {
        return $ErrorRecord.Exception.Message
    }
}

function Get-OssObject {
    param(
        [Parameter(Mandatory = $true)][string]$Endpoint,
        [Parameter(Mandatory = $true)][string]$Bucket,
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$AccessKeyId,
        [Parameter(Mandatory = $true)][string]$AccessKeySecret,
        [string]$OutFile = "",
        [bool]$EnableHttps = $true
    )
    $uri = New-OssObjectUri -Endpoint $Endpoint -Bucket $Bucket -Key $Key -EnableHttps $EnableHttps
    $headers = New-OssSignedHeaders -Method "GET" -Bucket $Bucket -Key $Key -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret
    try {
        if ($OutFile) {
            Invoke-WebRequest -Uri $uri -Headers $headers -OutFile $OutFile -UseBasicParsing -TimeoutSec 900 | Out-Null
            return $OutFile
        }
        return (Invoke-WebRequest -Uri $uri -Headers $headers -UseBasicParsing -TimeoutSec 120).Content
    }
    catch {
        $body = Read-OssErrorBody $_
        throw "OSS GET failed: $Key`n$body"
    }
}

function Put-OssObject {
    param(
        [Parameter(Mandatory = $true)][string]$Endpoint,
        [Parameter(Mandatory = $true)][string]$Bucket,
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$AccessKeyId,
        [Parameter(Mandatory = $true)][string]$AccessKeySecret,
        [Parameter(Mandatory = $true)][string]$FilePath,
        [string]$ContentType = "application/octet-stream",
        [bool]$EnableHttps = $true
    )
    $uri = New-OssObjectUri -Endpoint $Endpoint -Bucket $Bucket -Key $Key -EnableHttps $EnableHttps
    $headers = New-OssSignedHeaders -Method "PUT" -Bucket $Bucket -Key $Key -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -ContentType $ContentType
    try {
        Invoke-WebRequest -Uri $uri -Method Put -Headers $headers -ContentType $ContentType -InFile $FilePath -UseBasicParsing -TimeoutSec 1800 | Out-Null
    }
    catch {
        $body = Read-OssErrorBody $_
        throw "OSS PUT failed: $Key`n$body"
    }
}
