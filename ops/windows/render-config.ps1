#Requires -Version 5.1
<#
.SYNOPSIS
    从单一主配置 ops/windows/.env 渲染出各服务的配置文件。
.DESCRIPTION
    这是"配置集中化"的核心：唯一需要人工编辑的是 ops/windows/.env，
    本脚本据此生成/覆盖以下文件（写进已部署的 <DeployRoot> 树，不动仓库源码）：
      1. <DeployRoot>\api\Configurations\ConnectionStrings.<Env>.json  (MySQL)
      2. <DeployRoot>\api\Configurations\Cache.<Env>.json               (Redis)
      3. <DeployRoot>\api\Configurations\EventBus.<Env>.json            (RabbitMQ)
      4. <DeployRoot>\api\Configurations\AI.json                       (.NET AI Chat/Embedding)
      5. <DeployRoot>\api\Configurations\OSS.json                      (.NET 文件服务 OSS)
      6. 就地修补 <DeployRoot>\api\Configurations\AppSetting.json 的 NlqAgent.BaseUrl
      7. <NlqDir>\.env                                                  (nlq DATABASE_URL/REDIS_URL/LLM/JWT)

    可独立运行：改完 .env 后 `.\render-config.ps1` 再 `nssm restart lm-api lm-nlq-agent`
    即可让新配置生效，无需完整重新部署。install-services.ps1 也会在启动服务前自动调用它。
.PARAMETER Environment
    覆盖 .env 中 ASPNETCORE_ENVIRONMENT（决定生成 *.<Env>.json 的后缀）。
#>
param(
    [string]$Environment = ""
)

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot

. (Join-Path $ScriptDir "_dotenv.ps1")
$envMap = Get-DotEnv -Path (Join-Path $ScriptDir ".env")

function Val($key, $default = "") { Coalesce-EnvValue -ParamValue "" -EnvMap $envMap -Key $key -Default $default }
function ValBool($key, [bool]$default = $false) {
    $raw = Val $key ""
    if (-not $raw) { return $default }
    return @("1", "true", "yes", "y", "on").Contains($raw.ToLowerInvariant())
}

$Env        = Coalesce-EnvValue -ParamValue $Environment -EnvMap $envMap -Key "ASPNETCORE_ENVIRONMENT" -Default "YY"
$DeployRoot = Val "DEPLOY_ROOT" (Join-Path $env:USERPROFILE "lm-deploy")
$NlqDir     = Val "NLQ_DIR" (Join-Path $DeployRoot "nlq-agent")

$DbHost = Val "DB_HOST" "127.0.0.1"; $DbPort = Val "DB_PORT" "3306"
$DbName = Val "DB_NAME" "lumei";     $DbType = Val "DB_TYPE" "MySql"
$DbUser = Val "DB_USER" "root";      $DbPass = Val "DB_PASSWORD" ""

$RedisHost = Val "REDIS_HOST" "127.0.0.1"; $RedisPort = Val "REDIS_PORT" "6380"
$RedisPass = Val "REDIS_PASSWORD" "";      $RedisDbApi = Val "REDIS_DB_API" "2"; $RedisDbNlq = Val "REDIS_DB_NLQ" "0"

$RmqHost = Val "RABBITMQ_HOST" "127.0.0.1"; $RmqPort = Val "RABBITMQ_PORT" "8005"
$RmqUser = Val "RABBITMQ_USER" "admin";     $RmqPass = Val "RABBITMQ_PASSWORD" ""

$NlqPort  = Val "NLQ_PORT" "8000"
$BindAddr = Val "BIND_ADDR" "127.0.0.1"
$LlmBase  = Val "LITELLM_BASE_URL" "http://localhost:4000"
$LlmKey   = Val "LITELLM_API_KEY" ""
$LlmModel = Val "NLQ_LLM_MODEL" "deepseek-chat"
$JwtSecret = Val "NLQ_JWT_SECRET" ""

$AiChatEndpoint = Val "AI_CHAT_ENDPOINT" $LlmBase
$AiChatModelId  = Val "AI_CHAT_MODEL_ID" $LlmModel
$AiChatKey      = Val "AI_CHAT_API_KEY" $LlmKey
if (-not $AiChatKey) { $AiChatKey = "dummy-key" }
$AiEmbeddingEndpoint = Val "AI_EMBEDDING_ENDPOINT" "http://localhost:8081"
$AiEmbeddingModel    = Val "AI_EMBEDDING_MODEL" "bge-m3"

$OssProvider    = Val "OSS_PROVIDER" "Invalid"
$OssEndpoint    = Val "OSS_ENDPOINT" ""
$OssBucket      = Val "OSS_BUCKET" ""
$OssAccessKey   = Val "OSS_ACCESS_KEY_ID" ""
$OssSecretKey   = Val "OSS_ACCESS_KEY_SECRET" ""
$OssRegion      = Val "OSS_REGION" ""
$OssEnableHttps = ValBool "OSS_ENABLE_HTTPS" $false
$OssEnableCache = ValBool "OSS_ENABLE_CACHE" $true

$apiCfgDir = Join-Path $DeployRoot "api\Configurations"
$utf8NoBom = New-Object System.Text.UTF8Encoding $false

function Write-Text($path, $text) {
    New-Item -ItemType Directory -Force -Path (Split-Path $path -Parent) | Out-Null
    [System.IO.File]::WriteAllText($path, $text, $utf8NoBom)
    Write-Host "  ✅ 渲染 $path" -ForegroundColor Green
}

Write-Host "【render-config】环境=$Env  部署根=$DeployRoot" -ForegroundColor Cyan

# ── 1) ConnectionStrings.<Env>.json ──────────────────────────
if (Test-Path $apiCfgDir) {
    $defConn = "server=$DbHost;Port=$DbPort;Database={0};Uid=$DbUser;Pwd=$DbPass;AllowLoadLocalInfile=true;Charset=utf8mb4;"
    $connObj = [ordered]@{
        ConnectionStrings = [ordered]@{
            DBName = $DbName
            DBType = $DbType
            Host = $DbHost
            Port = $DbPort
            UserName = $DbUser
            Password = $DbPass
            DefaultConnection = $defConn
            ConnectionConfigs = @(
                [ordered]@{ ConfigId = "default";    DBName = $DbName; DBType = $DbType; Host = $DbHost; Port = $DbPort; UserName = $DbUser; Password = $DbPass; DefaultConnection = $defConn },
                [ordered]@{ ConfigId = "Poxiao-Job"; DBName = $DbName; DBType = $DbType; Host = $DbHost; Port = $DbPort; UserName = $DbUser; Password = $DbPass; DefaultConnection = $defConn }
            )
        }
    }
    Write-Text (Join-Path $apiCfgDir "ConnectionStrings.$Env.json") ($connObj | ConvertTo-Json -Depth 6)

    # ── 2) Cache.<Env>.json (Redis) ──────────────────────────
    $cacheObj = [ordered]@{
        Cache = [ordered]@{
            CacheType = "RedisCache"
            ip = $RedisHost
            port = [int]$RedisPort
            password = $RedisPass
            RedisConnectionString = "{0}:{1},password={2}, poolsize=500,ssl=false,defaultDatabase=$RedisDbApi"
        }
    }
    Write-Text (Join-Path $apiCfgDir "Cache.$Env.json") ($cacheObj | ConvertTo-Json -Depth 6)

    # ── 3) EventBus.<Env>.json (RabbitMQ) ────────────────────
    $ebObj = [ordered]@{
        EventBus = [ordered]@{
            EventBusType = "RabbitMQ"
            HostName = "${RmqHost}:$RmqPort"
            UserName = $RmqUser
            Password = $RmqPass
        }
    }
    Write-Text (Join-Path $apiCfgDir "EventBus.$Env.json") ($ebObj | ConvertTo-Json -Depth 6)

    # ── 4) AI.json (.NET AI Chat/Embedding) ──────────────────
    $aiObj = [ordered]@{
        AI = [ordered]@{
            Chat = [ordered]@{
                Endpoint = $AiChatEndpoint
                ModelId = $AiChatModelId
                Key = $AiChatKey
            }
            Embedding = [ordered]@{
                Endpoint = $AiEmbeddingEndpoint
                Model = $AiEmbeddingModel
            }
        }
    }
    Write-Text (Join-Path $apiCfgDir "AI.json") ($aiObj | ConvertTo-Json -Depth 6)

    # ── 5) OSS.json (.NET 文件服务) ──────────────────────────
    $ossObj = [ordered]@{
        OSS = [ordered]@{
            BucketName = $OssBucket
            Provider = $OssProvider
            Endpoint = $OssEndpoint
            AccessKey = $OssAccessKey
            SecretKey = $OssSecretKey
            Region = if ($OssRegion) { $OssRegion } else { $null }
            IsEnableHttps = $OssEnableHttps
            IsEnableCache = $OssEnableCache
        }
    }
    Write-Text (Join-Path $apiCfgDir "OSS.json") ($ossObj | ConvertTo-Json -Depth 6)

    # ── 6) 就地修补 AppSetting.json 的 NlqAgent.BaseUrl ──────
    $appSettingPath = Join-Path $apiCfgDir "AppSetting.json"
    if (Test-Path $appSettingPath) {
        $newBase = "http://${BindAddr}:$NlqPort"
        try {
            $appSetting = Get-Content $appSettingPath -Raw -Encoding UTF8 | ConvertFrom-Json
            if (-not ($appSetting.PSObject.Properties.Name -contains "NlqAgent") -or $null -eq $appSetting.NlqAgent) {
                $appSetting | Add-Member -NotePropertyName "NlqAgent" -NotePropertyValue ([pscustomobject]@{}) -Force
            }
            if (-not ($appSetting.NlqAgent.PSObject.Properties.Name -contains "BaseUrl")) {
                $appSetting.NlqAgent | Add-Member -NotePropertyName "BaseUrl" -NotePropertyValue $newBase -Force
            } else {
                $appSetting.NlqAgent.BaseUrl = $newBase
            }
            [System.IO.File]::WriteAllText($appSettingPath, ($appSetting | ConvertTo-Json -Depth 12), $utf8NoBom)
            Write-Host "  ✅ 修补 AppSetting.json NlqAgent.BaseUrl -> $newBase" -ForegroundColor Green
        } catch {
            Write-Host "  ⚠️ AppSetting.json 不是可解析 JSON，未能自动修补 NlqAgent.BaseUrl：$_" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "  ⏭  $apiCfgDir 不存在（未部署 api），跳过 api 配置渲染" -ForegroundColor DarkGray
}

# ── 5) nlq-agent/.env ────────────────────────────────────────
if (Test-Path $NlqDir) {
    # Redis 密码需 URL 编码后放进 redis:// URL
    $redisPassEnc = [uri]::EscapeDataString($RedisPass)
    if (-not $JwtSecret) {
        $JwtSecret = -join ((48..57) + (97..102) | Get-Random -Count 64 | ForEach-Object { [char]$_ })
        Write-Host "  ℹ️ NLQ_JWT_SECRET 为空，已生成随机值写入 nlq .env" -ForegroundColor DarkGray
    }
    $nlqEnv = @"
# 由 ops/windows/render-config.ps1 从主配置渲染生成，请勿手改（改 ops/windows/.env 后重新渲染）。
DATABASE_URL=mysql+aiomysql://${DbUser}:${DbPass}@${DbHost}:${DbPort}/${DbName}
DATABASE_POOL_SIZE=10
DATABASE_MAX_OVERFLOW=20

REDIS_URL=redis://:${redisPassEnc}@${RedisHost}:${RedisPort}/${RedisDbNlq}

LITELLM_BASE_URL=$LlmBase
LITELLM_API_KEY=$LlmKey
DEFAULT_MODEL_NAME=$LlmModel

AUTH_REQUIRED=false
JWT_SECRET_KEY=$JwtSecret
JWT_ALGORITHM=HS256
JWT_EXPIRE_MINUTES=1440

NEO4J_ENABLED=false
LIGHTRAG_ENABLED=false
SENTRY_DSN=

APP_ENV=production
APP_HOST=$BindAddr
APP_PORT=$NlqPort
APP_RELOAD=false
"@
    Write-Text (Join-Path $NlqDir ".env") $nlqEnv
} else {
    Write-Host "  ⏭  $NlqDir 不存在（未部署 nlq），跳过 nlq .env 渲染" -ForegroundColor DarkGray
}

Write-Host "【render-config】完成" -ForegroundColor Cyan
