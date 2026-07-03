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
        [bool]$EnableHttps = $true,
        [string]$QueryString = ""
    )
    $hostName = Normalize-OssEndpoint $Endpoint
    $scheme = if ($EnableHttps) { "https" } else { Get-OssScheme -Endpoint $Endpoint -Default "http" }
    if ($Endpoint -match "^https://") { $scheme = "https" }
    if ($Endpoint -match "^http://") { $scheme = "http" }
    $escapedKey = ConvertTo-OssEscapedKey $Key
    $uri = "${scheme}://$Bucket.$hostName/$escapedKey"
    if ($QueryString) { $uri += "?$QueryString" }
    return $uri
}

function New-OssSignedHeaders {
    param(
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$Bucket,
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$AccessKeyId,
        [Parameter(Mandatory = $true)][string]$AccessKeySecret,
        [string]$ContentType = "",
        [string]$ResourceQuery = ""
    )
    $date = [DateTime]::UtcNow.ToString("r", [Globalization.CultureInfo]::InvariantCulture)
    $resource = "/$Bucket/$($Key.TrimStart('/'))"
    if ($ResourceQuery) { $resource += "?$ResourceQuery" }
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

function Invoke-OssHttp {
    param(
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$Endpoint,
        [Parameter(Mandatory = $true)][string]$Bucket,
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$AccessKeyId,
        [Parameter(Mandatory = $true)][string]$AccessKeySecret,
        [bool]$EnableHttps = $true,
        [string]$UriQuery = "",
        [string]$ResourceQuery = "",
        [string]$ContentType = "",
        [byte[]]$BodyBytes = $null
    )

    Add-Type -AssemblyName System.Net.Http
    $uri = New-OssObjectUri -Endpoint $Endpoint -Bucket $Bucket -Key $Key -EnableHttps $EnableHttps -QueryString $UriQuery
    $headers = New-OssSignedHeaders -Method $Method -Bucket $Bucket -Key $Key -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -ContentType $ContentType -ResourceQuery $ResourceQuery
    $client = $null
    $request = $null
    $content = $null
    try {
        $client = [System.Net.Http.HttpClient]::new()
        $client.Timeout = [TimeSpan]::FromHours(2)
        $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::new($Method.ToUpperInvariant()), $uri)
        foreach ($name in $headers.Keys) {
            $null = $request.Headers.TryAddWithoutValidation($name, [string]$headers[$name])
        }
        if ($BodyBytes -ne $null) {
            $content = [System.Net.Http.ByteArrayContent]::new($BodyBytes)
            if ($ContentType) {
                $content.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse($ContentType)
            }
            $request.Content = $content
        }

        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        if (-not $response.IsSuccessStatusCode) {
            throw "HTTP $([int]$response.StatusCode) $($response.ReasonPhrase)`n$body"
        }
        return [pscustomobject]@{ Response = $response; Body = $body }
    }
    catch {
        throw "OSS $Method failed: $Key`n$($_.Exception.Message)"
    }
    finally {
        if ($content) { $content.Dispose() }
        if ($request) { $request.Dispose() }
        if ($client) { $client.Dispose() }
    }
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
    $fileInfo = Get-Item -LiteralPath $FilePath
    if ($fileInfo.Length -ge 67108864) {
        Put-OssObjectMultipart -Endpoint $Endpoint -Bucket $Bucket -Key $Key -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -FilePath $FilePath -ContentType $ContentType -EnableHttps $EnableHttps | Out-Null
        return
    }

    $uri = New-OssObjectUri -Endpoint $Endpoint -Bucket $Bucket -Key $Key -EnableHttps $EnableHttps
    $headers = New-OssSignedHeaders -Method "PUT" -Bucket $Bucket -Key $Key -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -ContentType $ContentType
    $client = $null
    $request = $null
    $content = $null
    $stream = $null
    try {
        Add-Type -AssemblyName System.Net.Http
        $client = [System.Net.Http.HttpClient]::new()
        $client.Timeout = [TimeSpan]::FromHours(2)
        $stream = [System.IO.File]::OpenRead($FilePath)
        $content = [System.Net.Http.StreamContent]::new($stream)
        $content.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse($ContentType)
        $request = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::Put, $uri)
        $request.Content = $content
        foreach ($name in $headers.Keys) {
            $null = $request.Headers.TryAddWithoutValidation($name, [string]$headers[$name])
        }

        $response = $client.SendAsync($request).GetAwaiter().GetResult()
        if (-not $response.IsSuccessStatusCode) {
            $body = $response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
            throw "HTTP $([int]$response.StatusCode) $($response.ReasonPhrase)`n$body"
        }
    }
    catch {
        throw "OSS PUT failed: $Key`n$($_.Exception.Message)"
    }
    finally {
        if ($request) { $request.Dispose() }
        if ($content) { $content.Dispose() }
        if ($stream) { $stream.Dispose() }
        if ($client) { $client.Dispose() }
    }
}

function Put-OssObjectMultipart {
    param(
        [Parameter(Mandatory = $true)][string]$Endpoint,
        [Parameter(Mandatory = $true)][string]$Bucket,
        [Parameter(Mandatory = $true)][string]$Key,
        [Parameter(Mandatory = $true)][string]$AccessKeyId,
        [Parameter(Mandatory = $true)][string]$AccessKeySecret,
        [Parameter(Mandatory = $true)][string]$FilePath,
        [string]$ContentType = "application/octet-stream",
        [bool]$EnableHttps = $true,
        [int]$PartSizeMb = 16
    )

    $init = Invoke-OssHttp -Method "POST" -Endpoint $Endpoint -Bucket $Bucket -Key $Key -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -EnableHttps $EnableHttps -UriQuery "uploads" -ResourceQuery "uploads"
    [xml]$initXml = $init.Body
    $uploadId = [string]$initXml.InitiateMultipartUploadResult.UploadId
    if (-not $uploadId) { throw "OSS multipart initiate failed: missing UploadId." }

    $parts = @()
    $partSize = [int64]$PartSizeMb * 1024 * 1024
    $buffer = New-Object byte[] ([int]$partSize)
    $stream = [System.IO.File]::OpenRead($FilePath)
    try {
        $partNumber = 1
        while ($true) {
            $offset = 0
            while ($offset -lt $partSize) {
                $read = $stream.Read($buffer, $offset, [int]($partSize - $offset))
                if ($read -le 0) { break }
                $offset += $read
            }
            if ($offset -le 0) { break }

            if ($offset -eq $buffer.Length) {
                $body = $buffer
            } else {
                $body = New-Object byte[] $offset
                [Array]::Copy($buffer, $body, $offset)
            }
            $rawQuery = "partNumber=$partNumber&uploadId=$uploadId"
            $uriQuery = "partNumber=$partNumber&uploadId=$([Uri]::EscapeDataString($uploadId))"
            Write-Host "  multipart part $partNumber ($offset bytes)" -ForegroundColor DarkGray
            $result = Invoke-OssHttp -Method "PUT" -Endpoint $Endpoint -Bucket $Bucket -Key $Key -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -EnableHttps $EnableHttps -UriQuery $uriQuery -ResourceQuery $rawQuery -ContentType $ContentType -BodyBytes $body
            $etag = $result.Response.Headers.ETag.Tag
            if (-not $etag) {
                $etagValues = $result.Response.Headers.GetValues("ETag")
                $etag = ($etagValues | Select-Object -First 1)
            }
            if (-not $etag) { throw "OSS multipart upload part $partNumber did not return ETag." }
            $parts += [pscustomobject]@{ PartNumber = $partNumber; ETag = $etag }
            $partNumber++
        }
    }
    finally {
        $stream.Dispose()
    }

    $xml = New-Object System.Text.StringBuilder
    [void]$xml.Append("<CompleteMultipartUpload>")
    foreach ($part in ($parts | Sort-Object PartNumber)) {
        [void]$xml.Append("<Part><PartNumber>")
        [void]$xml.Append($part.PartNumber)
        [void]$xml.Append("</PartNumber><ETag>")
        [void]$xml.Append([Security.SecurityElement]::Escape($part.ETag))
        [void]$xml.Append("</ETag></Part>")
    }
    [void]$xml.Append("</CompleteMultipartUpload>")
    $completeBytes = [Text.Encoding]::UTF8.GetBytes($xml.ToString())
    $completeRawQuery = "uploadId=$uploadId"
    $completeUriQuery = "uploadId=$([Uri]::EscapeDataString($uploadId))"
    Invoke-OssHttp -Method "POST" -Endpoint $Endpoint -Bucket $Bucket -Key $Key -AccessKeyId $AccessKeyId -AccessKeySecret $AccessKeySecret -EnableHttps $EnableHttps -UriQuery $completeUriQuery -ResourceQuery $completeRawQuery -ContentType "application/xml" -BodyBytes $completeBytes | Out-Null
}
