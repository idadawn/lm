# Windows Server 2016 部署指南

本文档详细说明如何在 Windows Server 2016 上部署实验室数据分析系统。

## 目录

- [系统要求](#系统要求)
- [环境准备](#环境准备)
- [基础软件安装](#基础软件安装)
- [后端 API 部署](#后端-api-部署)
- [前端 Web 部署](#前端-web-部署)
- [IIS 配置](#iis-配置)
- [防火墙配置](#防火墙配置)
- [验证部署](#验证部署)
- [常见问题](#常见问题)

---

## 系统要求

### 硬件要求
- **操作系统**: Windows Server 2016 (Standard 或 Datacenter Edition)
- **CPU**: 4 核心或以上
- **内存**: 8 GB 或以上（推荐 16 GB）
- **磁盘**: 100 GB 或以上可用空间
- **网络**: 支持局域网连接

### 软件要求
- .NET Runtime 10.0 或以上
- IIS 10.0 或以上（Windows Server 2016 自带）
- MySQL 5.7 或 8.0
- Redis 5.0 或以上

---

## 环境准备

### 1. 检查系统信息

```powershell
# 检查 Windows 版本
systeminfo | findstr /B /C:"OS Name" /C:"OS Version"

# 检查 .NET 版本
reg query "HKLM\SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" /v Release

# 检查 IIS 是否安装
Get-WindowsFeature -Name Web-Server
```

### 2. 创建部署目录结构

```
C:\LM-System\
├── api\                    # 后端 API 目录
│   ├── app\               # 应用程序文件
│   ├── Configurations\    # 配置文件
│   ├── logs\              # 日志目录
│   ├── uploads\           # 上传文件目录
│   └── wwwroot\           # 静态资源
├── web\                   # 前端 Web 目录
│   └── dist\              # 构建后的静态文件
├── mysql\                 # MySQL 数据目录
└── redis\                 # Redis 数据目录
```

使用 PowerShell 创建目录：

```powershell
$baseDir = "C:\LM-System"
$dirs = @(
    "$baseDir\api\app",
    "$baseDir\api\Configurations",
    "$baseDir\api\logs",
    "$baseDir\api\uploads",
    "$baseDir\api\wwwroot",
    "$baseDir\api\resources",
    "$baseDir\api\lib",
    "$baseDir\web\dist",
    "$baseDir\mysql",
    "$baseDir\redis"
)

foreach ($dir in $dirs) {
    New-Item -ItemType Directory -Path $dir -Force | Out-Null
}
```

---

## 基础软件安装

### 1. 安装 IIS 功能

打开 PowerShell（以管理员身份运行）：

```powershell
# 安装 IIS 及所需功能
Install-WindowsFeature -Name Web-Server -IncludeManagementTools
Install-WindowsFeature -Name Web-Default-Doc
Install-WindowsFeature -Name Web-Dir-Browsing
Install-WindowsFeature -Name Web-Http-Errors
Install-WindowsFeature -Name Web-Static-Content
Install-WindowsFeature -Name Web-Http-Redirect
Install-WindowsFeature -Name Web-Http-Logging
Install-WindowsFeature -Name Web-Stat-Compression
Install-WindowsFeature -Name Web-Filtering
Install-WindowsFeature -Name Web-Net-Ext45
Install-WindowsFeature -Name Web-Asp-Net45

# 安装 CGI（可选，用于高级功能）
Install-WindowsFeature -Name Web-CGI

# 安装管理工具
Install-WindowsFeature -Name Web-Mgmt-Console
Install-WindowsFeature -Name Web-Mgmt-Tools

# 重启 IIS
iisreset
```

### 2. 安装 .NET Runtime 10.0

下载并安装 ASP.NET Core Hosting Bundle：

```powershell
# 下载地址（示例，请使用最新版本）
# https://dotnet.microsoft.com/download/dotnet/10.0

# 安装后验证
dotnet --list-runtimes
```

### 3. 安装 MySQL

#### 选项 A：使用 MySQL 安装包

1. 下载 MySQL Installer：https://dev.mysql.com/downloads/installer/
2. 运行安装程序，选择 "Custom" 安装
3. 选择 MySQL Server 8.0.x
4. 安装路径建议：`C:\Program Files\MySQL\MySQL Server 8.0`
5. 设置 root 密码（建议使用强密码）
6. 配置端口：默认 3306
7. 启动 MySQL 服务

#### 选项 B：使用 ZIP 包（推荐）

1. 下载 MySQL 8.0 ZIP Archive：https://dev.mysql.com/downloads/mysql/
2. 解压到 `C:\mysql-8.0.xx`
3. 创建配置文件 `C:\my.ini`：

```ini
[mysqld]
port=3306
basedir=C:\\mysql-8.0.xx
datadir=C:\\LM-System\\mysql\\data
max_connections=200
character-set-server=utf8mb4
collation-server=utf8mb4_unicode_ci
default-authentication-plugin=mysql_native_password
[mysql]
default-character-set=utf8mb4
[client]
default-character-set=utf8mb4
port=3306
```

4. 初始化数据库（以管理员身份运行 CMD）：

```cmd
cd C:\mysql-8.0.xx\bin
mysqld --initialize --console
# 记录生成的临时密码
mysqld --install MySQL80
net start MySQL80
```

5. 修改 root 密码：

```cmd
mysql -u root -p
# 输入临时密码
ALTER USER 'root'@'localhost' IDENTIFIED BY '新密码';
FLUSH PRIVILEGES;
EXIT;
```

### 4. 安装 Redis

Windows 上推荐使用 Memurai（Redis 兼容）或使用 WSL：

#### 选项 A：使用 Memurai（推荐）

1. 下载 Memurai Developer：https://www.memurai.com/get-memurai-developer
2. 安装后配置密码：
```cmd
memurai-cli.exe
CONFIG SET requirepass "你的Redis密码"
AUTH "你的Redis密码"
EXIT
```

#### 选项 B：使用 Redis for Windows

1. 下载 Redis Windows 移植版：https://github.com/microsoftarchive/redis/releases
2. 解压到 `C:\redis`
3. 配置密码：编辑 `redis.windows-service.conf`
```
requirepass 你的Redis密码
```
4. 安装服务：
```cmd
redis-server --service-install redis.windows-service.conf
redis-server --service-start
```

---

## 后端 API 部署

### 1. 发布应用程序

在开发机器上，使用命令行发布应用：

```bash
# 进入 API 项目目录
cd api/src/application/Poxiao.API.Entry

# 发布为自包含应用（包含运行时）
dotnet publish -c Release -r win-x64 --self-contained true -o ../../../../../publish/api

# 或发布为依赖框架的应用（需要服务器安装 .NET Runtime）
dotnet publish -c Release -o ../../../../../publish/api
```

### 2. 复制文件到服务器

将发布目录下的所有文件复制到服务器的 `C:\LM-System\api\app\` 目录。

可以使用以下方法：
- **方式 1**: 远程桌面共享文件夹
- **方式 2**: 压缩为 ZIP 后通过文件共享复制
- **方式 3**: 使用 robocopy 命令

```powershell
# 在服务器上（如果使用共享文件夹）
robocopy \\开发机\share\api "C:\LM-System\api\app" /E /R:0 /W:0
```

### 3. 配置应用程序

复制配置文件模板到 `C:\LM-System\api\Configurations\`：

#### ConnectionStrings.json
```json
{
  "ConnectionStrings": {
    "ConnectionConfigs": [
      {
        "ConfigId": "default",
        "DBName": "lumei",
        "DBType": "MySql",
        "Host": "localhost",
        "Port": 3306,
        "UserName": "root",
        "Password": "你的MySQL密码",
        "DefaultConnection": "server=localhost;Port=3306;Database=lumei;Uid=root;Pwd=你的MySQL密码;AllowLoadLocalInfile=true;Charset=utf8mb4;"
      }
    ]
  }
}
```

#### Cache.json
```json
{
  "Cache": {
    "CacheType": "RedisCache",
    "ip": "localhost",
    "port": 6379,
    "password": "你的Redis密码",
    "RedisConnectionString": "{0}:{1},password={2},poolsize=500,ssl=false,defaultDatabase=2"
  }
}
```

#### AppSetting.json
```json
{
  "AppSettings": {
    "InjectMiniProfiler": false
  },
  "Lab": {
    "Formula": {
      "EnablePrecisionAdjustment": false,
      "DefaultPrecision": 6,
      "MaxPrecision": 6
    }
  }
}
```

### 4. 配置 API 作为 Windows 服务运行

#### 选项 A：使用 IIS 托管（推荐）

1. 打开 IIS 管理器
2. 添加网站：
   - **网站名称**: LM-API
   - **物理路径**: `C:\LM-System\api\app`
   - **端口**: 9530
   - **应用程序池**: DefaultAppPool (.NET CLR 版本：无托管代码）

3. 配置 web.config（如果不存在，创建一个）：

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\Poxiao.API.Entry.dll"
                  stdoutLogEnabled="true"
                  stdoutLogFile=".\logs\stdout"
                  hostingModel="InProcess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="production" />
          <environmentVariable name="ASPNETCORE_URLS" value="http://+:9530" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

4. 配置应用程序池：
   - 打开应用程序池高级设置
   - .NET CLR 版本：无托管代码
   - 托管管道模式：集成
   - 启用 32 位应用程序：False
   - 空闲超时（分钟）：0
   - 进程模型：标识：ApplicationPoolIdentity

5. 测试网站：
```powershell
# 浏览访问
http://localhost:9530/health
```

#### 选项 B：作为 Windows 服务运行

使用 NSSM (Non-Sucking Service Manager)：

1. 下载 NSSM：https://nssm.cc/download
2. 安装服务：
```cmd
nssm install LMApi C:\LM-System\api\app\Poxiao.API.Entry.exe
nssm set LMApi AppDirectory C:\LM-System\api\app
nssm set LMApi DisplayName "LM System API"
nssm set LMApi Description "Laboratory Data Analysis System API Service"
nssm set LMApi Start SERVICE_AUTO_START

# 设置环境变量
nssm set LMApi AppEnvironmentExtra "ASPNETCORE_ENVIRONMENT=production"
nssm set LMApi AppEnvironmentExtra "ASPNETCORE_URLS=http://+:9530"

# 启动服务
nssm start LMApi
```

### 5. 导入数据库

```powershell
# 如果有数据库备份文件
mysql -u root -p lumei < database_backup.sql

# 或使用 MySQL 工具导入
```

---

## 前端 Web 部署

### 1. 构建前端应用

在开发机器上：

```bash
cd web

# 安装依赖（首次）
pnpm install

# 构建生产版本
pnpm build

# 或构建测试版本
pnpm build:test
```

构建完成后，文件输出到 `web/dist` 目录。

### 2. 复制到服务器

将 `web/dist` 目录下的所有文件复制到服务器的 `C:\LM-System\web\dist\` 目录。

### 3. 配置环境变量

在构建前，可以创建 `.env.production` 文件：

```env
# API 地址（如果前后端分离部署）
VITE_GLOB_API_URL=http://localhost:9530

# WebSocket 地址
VITE_GLOB_WEBSOCKET_URL=ws://localhost:9530

# API 前缀
VITE_GLOB_API_URL_PREFIX=/api

# 资源路径
VITE_PUBLIC_PATH = /

# 应用信息
VITE_GLOB_APP_TITLE=实验室数据分析系统
VITE_GLOB_APP_SHORT_NAME=lm

# 其他配置
VITE_DROP_CONSOLE = true
VITE_BUILD_COMPRESS = 'gzip'
VITE_CDN = false
VITE_USE_IMAGEMIN= false
VITE_USE_PWA = false
VITE_LEGACY = false
```

---

## IIS 配置

### 1. 添加前端网站

1. 打开 IIS 管理器
2. 添加网站：
   - **网站名称**: LM-Web
   - **物理路径**: `C:\LM-System\web\dist`
   - **端口**: 80 或其他（如 8923）
   - **主机名**: （可选）如果需要域名访问

### 2. 配置默认文档

确保 `index.html` 在默认文档列表中：
1. 选择网站
2. 双击 "默认文档"
3. 添加 `index.html`（如果不存在）
4. 将其移到列表顶部

### 3. 配置 URL 重写（SPA 路由支持）

安装 URL Rewrite Module：
1. 下载：https://www.iis.net/downloads/microsoft/url-rewrite
2. 安装后，在网站根目录创建 `web.config`：

```xml
<?xml version="1.0" encoding="UTF-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="Handle History Mode and Hash White list" stopProcessing="true">
          <match url="([\S]+[.]js|[\S]+[.]css|[\S]+[.]json|[\S]+[.]png|[\S]+[.]jpg|[\S]+[.]jpeg|[\S]+[.]gif|[\S]+[.]svg|[\S]+[.]ico|[\S]+[.]woff|[\S]+[.]woff2|[\S]+[.]ttf|[\S]+[.]eot)$" />
          <action type="None" />
        </rule>
        <rule name="SPA Fallback" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="/" />
        </rule>
      </rules>
    </rewrite>
    <staticContent>
      <mimeMap fileExtension=".json" mimeType="application/json" />
      <mimeMap fileExtension=".woff" mimeType="application/font-woff" />
      <mimeMap fileExtension=".woff2" mimeType="application/font-woff2" />
    </staticContent>
    <httpProtocol>
      <customHeaders>
        <add name="X-Content-Type-Options" value="nosniff" />
        <add name="X-Frame-Options" value="SAMEORIGIN" />
      </customHeaders>
    </httpProtocol>
  </system.webServer>
</configuration>
```

### 4. 配置反向代理（可选）

如果需要通过 IIS 反向代理访问 API：

1. 安装 Application Request Routing (ARR)：
   - 下载：https://www.iis.net/downloads/microsoft/application-request-routing
   - 安装后在 IIS 管理器中配置

2. 在前端网站的 web.config 中添加代理规则：

```xml
<rule name="ReverseProxyInboundRule1" stopProcessing="true">
  <match url="^api/(.*)" />
  <action type="Rewrite" url="http://localhost:9530/api/{R:1}" />
</rule>
```

### 5. 配置 HTTPS（可选）

1. 获取 SSL 证书（自签名或购买）
2. 在 IIS 中绑定证书：
   - 选择网站
   - 点击 "绑定"
   - 添加 HTTPS 绑定，选择证书
   - 端口：443

---

## 防火墙配置

### 开放必要端口

```powershell
# 允许 HTTP (80)
New-NetFirewallRule -DisplayName "LM System HTTP" -Direction Inbound -LocalPort 80 -Protocol TCP -Action Allow

# 允许 API 端口 (9530)
New-NetFirewallRule -DisplayName "LM System API" -Direction Inbound -LocalPort 9530 -Protocol TCP -Action Allow

# 允许 MySQL (3306) - 如果需要远程访问
# New-NetFirewallRule -DisplayName "MySQL Server" -Direction Inbound -LocalPort 3306 -Protocol TCP -Action Allow

# 允许 Redis (6379) - 如果需要远程访问
# New-NetFirewallRule -DisplayName "Redis Server" -Direction Inbound -LocalPort 6379 -Protocol TCP -Action Allow
```

---

## 验证部署

### 1. 检查服务状态

```powershell
# 检查 IIS 服务
Get-Service -Name W3SVC

# 检查 MySQL 服务
Get-Service -Name MySQL*

# 检查 Redis 服务
Get-Service -Name *Redis*

# 如果使用 Windows 服务运行 API
Get-Service -Name LMApi
```

### 2. 测试数据库连接

```cmd
mysql -u root -p
# 输入密码后
SHOW DATABASES;
USE lumei;
SHOW TABLES;
EXIT;
```

### 3. 测试 Redis 连接

```cmd
redis-cli
AUTH 你的密码
PING
# 应返回 PONG
EXIT
```

### 4. 测试 API

```powershell
# 测试健康检查
Invoke-WebRequest -Uri "http://localhost:9530/health" -UseBasicParsing

# 测试 API 端点（根据实际情况修改）
Invoke-WebRequest -Uri "http://localhost:9530/api/health" -UseBasicParsing
```

### 5. 测试前端

在浏览器中访问：
- http://localhost 或 http://服务器IP
- http://localhost:8923（如果使用了 8923 端口）

### 6. 检查日志

```powershell
# API 日志
notepad C:\LM-System\api\logs\*.log

# IIS 日志
notepad C:\inetpub\logs\LogFiles\*.log
```

---

## 常见问题

### 问题 1: API 无法启动

**症状**: 访问 API 端口无响应

**解决方案**:
1. 检查应用程序池是否运行
2. 查看 IIS 日志：`C:\inetpub\logs\LogFiles\`
3. 检查 API 日志：`C:\LM-System\api\logs\`
4. 确认端口 9530 未被占用
```powershell
netstat -ano | findstr :9530
```

### 问题 2: 数据库连接失败

**症状**: API 返回数据库错误

**解决方案**:
1. 确认 MySQL 服务正在运行
2. 检查 ConnectionStrings.json 配置
3. 测试数据库连接：
```cmd
mysql -h localhost -P 3306 -u root -p
```
4. 确认数据库用户权限

### 问题 3: Redis 连接失败

**症状**: 缓存相关功能报错

**解决方案**:
1. 确认 Redis 服务正在运行
2. 检查 Cache.json 配置
3. 测试 Redis 连接：
```cmd
redis-cli -a 你的密码 PING
```
4. 检查防火墙设置

### 问题 4: 前端页面空白

**症状**: 访问前端页面显示空白

**解决方案**:
1. 检查浏览器控制台错误
2. 确认 dist 目录文件完整
3. 检查 IIS 默认文档设置
4. 验证 URL 重写规则
5. 检查 API 地址配置

### 问题 5: 静态资源 404

**症状**: JS/CSS 文件加载失败

**解决方案**:
1. 检查 MIME 类型配置
2. 确认文件路径正确
3. 检查 IIS 静态内容功能是否启用
4. 检查文件夹权限

### 问题 6: 上传文件失败

**症状**: 文件上传功能不工作

**解决方案**:
1. 检查 `C:\LM-System\api\uploads` 目录权限
2. 确认 IIS 应用程序池用户有写入权限
3. 检查上传文件大小限制（web.config）：
```xml
<security>
  <requestFiltering>
    <requestLimits maxAllowedContentLength="104857600" /> <!-- 100MB -->
  </requestFiltering>
</security>
```

---

## 性能优化建议

### 1. IIS 性能优化

```xml
<!-- web.config -->
<system.webServer>
  <!-- 启用压缩 -->
  <urlCompression doStaticCompression="true" doDynamicCompression="true" />

  <!-- 设置缓存 -->
  <staticContent>
    <clientCache cacheControlMode="UseMaxAge" cacheControlMaxAge="7.00:00:00" />
  </staticContent>

  <!-- 启用输出缓存 -->
  <caching>
    <profiles>
      <add extension=".js" policy="CacheUntilChange" kernelCachePolicy="CacheUntilChange" duration="30.00:00:00" />
      <add extension=".css" policy="CacheUntilChange" kernelCachePolicy="CacheUntilChange" duration="30.00:00:00" />
    </profiles>
  </caching>
</system.webServer>
```

### 2. API 性能优化

1. 启用响应缓存
2. 配置数据库连接池
3. 优化 Redis 连接
4. 启用 gzip 压缩

### 3. 数据库优化

1. 创建适当的索引
2. 定期清理日志
3. 配置适当的缓存大小
4. 定期备份

---

## 备份与恢复

### 备份计划

```powershell
# 备份脚本示例
$backupPath = "D:\LM-Backups\$(Get-Date -Format 'yyyyMMdd')"
New-Item -ItemType Directory -Path $backupPath -Force

# 备份数据库
mysqldump -u root -p lumei > "$backupPath\database_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql"

# 备份配置文件
Copy-Item -Path "C:\LM-System\api\Configurations" -Destination "$backupPath\Configurations" -Recurse

# 备份上传文件
Copy-Item -Path "C:\LM-System\api\uploads" -Destination "$backupPath\uploads" -Recurse
```

---

## 安全建议

1. **修改默认密码**: MySQL root、Redis 密码
2. **启用 HTTPS**: 使用 SSL 证书
3. **限制网络访问**: 仅开放必要端口
4. **定期更新**: 保持系统和软件更新
5. **配置防火墙**: 限制远程访问
6. **文件权限**: 设置适当的文件夹权限
7. **日志审计**: 定期检查访问日志
8. **备份策略**: 定期备份数据和配置

---

## 维护命令

### 重启服务

```powershell
# 重启 IIS
iisreset

# 重启 API 应用程序池
Restart-WebAppPool -Name "DefaultAppPool"

# 重启 MySQL
Restart-Service -Name MySQL80

# 重启 Redis
Restart-Service -Name Memurai
```

### 查看日志

```powershell
# API 日志
Get-Content "C:\LM-System\api\logs\*.log" -Tail 50 -Wait

# IIS 日志
Get-ChildItem "C:\inetpub\logs\LogFiles\W3SVC*" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content | Select-Object -Last 50
```

---

## 联系支持

如遇到部署问题，请联系技术支持：

- Email: martinzhang777@foxmail.com
- 项目地址: https://github.com/your-org/lm

---

**文档版本**: 1.0
**最后更新**: 2025-02-05
**适用版本**: LM System 1.0.1
