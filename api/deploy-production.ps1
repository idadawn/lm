# Production Deployment Script
# Run this script to deploy the API in production mode

param(
    [Parameter(Mandatory=$false)]
    [string]$Environment = "Production",

    [Parameter(Mandatory=$false)]
    [string]$Configuration = "Release"
)

Write-Host "Starting production deployment..." -ForegroundColor Green

# 1. Check if .env.production exists
if (-not (Test-Path ".env.production")) {
    Write-Host "Warning: .env.production not found. Using .env.production.template" -ForegroundColor Yellow
    if (Test-Path ".env.production.template") {
        Copy-Item ".env.production.template" ".env.production"
        Write-Host "Please update .env.production with your production values before continuing." -ForegroundColor Yellow
        exit 1
    }
}

# 2. Build the application
Write-Host "Building application..." -ForegroundColor Yellow
dotnet publish -c $Configuration -o ./publish

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}

# 3. Create production directories
Write-Host "Creating production directories..." -ForegroundColor Yellow
$dirs = @("logs", "temp", "resources", "mysql_config", "mysql_data", "redis_data")
foreach ($dir in $dirs) {
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir | Out-Null
    }
}

# 4. Copy configuration files
Write-Host "Copying configuration files..." -ForegroundColor Yellow
Copy-Item "Configurations/appsettings.Production.json" "publish/Configurations/" -Force

# 5. Create systemd service file (for Linux)
$serviceContent = @"
[Unit]
Description=Poxiao API Service
After=network.target mysql.service redis.service

[Service]
Type=notify
User=poxiao
Group=poxiao
WorkingDirectory=/opt/poxiao/api
ExecStart=/usr/bin/dotnet /opt/poxiao/api/Poxiao.API.Entry.dll
Restart=always
RestartSec=10
SyslogIdentifier=poxiao-api
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

# Environment variables from file
EnvironmentFile=/opt/poxiao/api/.env.production

# Process limits
LimitNOFILE=65536
LimitNPROC=4096

# Hardening
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ReadWritePaths=/opt/poxiao/api/logs /opt/poxiao/api/temp /opt/poxiao/api/resources

[Install]
WantedBy=multi-user.target
"@

$serviceContent | Out-File -FilePath "poxiao-api.service" -Encoding UTF8

# 6. Create Windows Service installation script
$windowsServiceScript = @"
# Windows Service Installation Script
# Run as Administrator

$serviceName = "PoxiaoAPI"
$serviceDisplayName = "Poxiao API Service"
$serviceDescription = "Poxiao Laboratory Data Analysis System API"
$exePath = "$PSScriptRoot\publish\Poxiao.API.Entry.exe"

# Stop and remove existing service
Stop-Service -Name $serviceName -ErrorAction SilentlyContinue
Start-Sleep -Seconds 5
$service = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($service) {
    sc.exe delete $serviceName
}

# Create new service
New-Service -Name $serviceName `
    -BinaryPathName $exePath `
    -DisplayName $serviceDisplayName `
    -Description $serviceDescription `
    -StartupType Automatic `
    -ErrorAction Stop

# Set recovery options
sc.exe failure $serviceName reset=86400 actions=restart/60000/restart/60000/restart/60000

# Set environment variable for production
[Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production", "Machine")
[Environment]::SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Production", "Machine")

Write-Host "Service installed successfully!" -ForegroundColor Green
Write-Host "Start the service with: Start-Service -Name $serviceName" -ForegroundColor Yellow
"@

$windowsServiceScript | Out-File -FilePath "install-windows-service.ps1" -Encoding UTF8

# 7. Create Docker deployment files
$dockerfileContent = @"
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Poxiao.API.Entry.csproj", "./"]
RUN dotnet restore "Poxiao.API.Entry.csproj"
COPY . .
RUN dotnet build "Poxiao.API.Entry.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Poxiao.API.Entry.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directories
RUN mkdir -p /app/logs /app/temp /app/resources /app/Configurations

# Copy production config
COPY --from=publish /app/publish/Configurations/appsettings.Production.json /app/Configurations/appsettings.json

# Set environment
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Poxiao.API.Entry.dll"]
"@

$dockerfileContent | Out-File -FilePath "Dockerfile.production" -Encoding UTF8

Write-Host @"

Production deployment preparation complete!

Next steps:
1. Update .env.production with your production values
2. Choose your deployment method:

   A. Windows Service:
      - Run install-windows-service.ps1 as Administrator
      - Start the service: Start-Service PoxiaoAPI

   B. Linux Systemd:
      - Copy files to /opt/poxiao/api/
      - Copy poxiao-api.service to /etc/systemd/system/
      - systemctl enable poxiao-api
      - systemctl start poxiao-api

   C. Docker:
      - Ensure docker-compose.yml has all services
      - docker-compose up -d

3. Verify the API is running by checking /health endpoint

"@ -ForegroundColor Green