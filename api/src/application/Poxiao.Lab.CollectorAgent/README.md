# Poxiao.Lab.CollectorAgent（采集网关）

部署在检测设备电脑（Windows）上的后台服务：轮询设备本机数据库，增量采集数据，
断网时本地暂存，联网后批量上报到中心服务器。

## 定位与链路

```
设备数据库 (Mock / Access .mdb) --PollingWorker--> 本地 Spool (JSON 文件) --UploadWorker--> 中心服务器
                                                                    ^
                                                        HeartbeatWorker 定时心跳
```

- **PollingWorker**：为每个启用的数据源独立轮询，增量拉取新记录，先落盘 Spool 再推进采集位点（保证至少一次投递）。
- **UploadWorker**：循环消费 Spool（最旧优先），成功即删除文件；失败按错误类型区分重试策略。
- **HeartbeatWorker**：定时向服务器上报存活状态。
- **零 ProjectReference**：不依赖 `Poxiao.Lab` 或任何仓库内模块，可独立发布、独立部署到设备端电脑，不需要整套后端代码。

## 配置说明（Configurations/appsettings.json）

```jsonc
{
  "Collector": {
    "ServerBaseUrl": "http://192.168.1.34",   // 中心服务器地址
    "AppId": "",                               // 服务端签发的应用标识
    "AppSecret": "",                           // 服务端签发的应用密钥
    "CollectorId": "collector-001",            // 本采集网关标识（上报时携带，服务端用于区分来源）
    "SpoolDir": "spool",                       // 断网暂存目录（相对程序目录）
    "StateFile": "state/source-state.json",    // 采集位点持久化文件
    "HeartbeatIntervalSeconds": 300,           // 心跳间隔（秒）
    "Sources": [
      {
        "Name": "stacking-mock",               // 数据源名称，需在 Sources 中唯一
        "DeviceCode": "stacking",              // 设备编码：stacking / ring-sample / single-sheet
        "Enabled": true,
        "Type": "mock",                        // mock（仿真数据）/ access（本机 Access 数据库）
        "PollIntervalSeconds": 10,
        "BatchSize": 200,
        "Mock": { "IntervalSeconds": 15 }
      },
      {
        "Name": "ring-sample-access",
        "DeviceCode": "ring-sample",
        "Enabled": false,
        "Type": "access",
        "PollIntervalSeconds": 10,
        "BatchSize": 200,
        "Access": {
          "FilePath": "C:\\Device\\Data\\ringsample.mdb",
          "Password": "",
          "Table": "TestResult",
          "KeyColumn": "Id",                    // 自增主键或时间戳列，用于增量查询 KeyColumn > lastPosition
          "CompleteFlagColumn": "IsComplete",    // 可选：完成标志列
          "CompleteFlagValue": "1",              // 可选：完成标志目标值
          "Columns": []                          // 空 = 全列（SELECT *）
        }
      }
    ]
  }
}
```

默认自带一个启用的 `mock` 数据源（`stacking`），可不接任何真实数据库直接跑通全链路联调。
`access` 数据源默认 `Enabled: false`，接入真实 Access 数据库前请：

1. 安装与进程位数匹配的 **Access Database Engine 2016 Redistributable**
   （<https://www.microsoft.com/download/details.aspx?id=54920>）。
2. 按上表模板修改 `FilePath` / `Table` / `KeyColumn` 等字段。
3. 把 `Enabled` 改为 `true`。

也可以新增 `Configurations/appsettings.<环境名>.json` 覆盖任意字段（Program.cs 会加载
`Configurations` 目录下所有 `*.json`，同名节点后加载者覆盖先加载者，文件名按字典序加载）。

## 服务端凭证配置指引

上报鉴权走 HTTP 头 `X-Collector-AppId` / `X-Collector-Secret`
（常量定义于 `Upload/CollectorHeaders.cs`，头名以服务端实现为准，如服务端调整需同步修改此文件）。

服务端凭证在系统后台的 **接口认证**（`BASE_INTERFACEOAUTH` 表，对应实体
`Poxiao.Systems.Entitys.System.InterfaceOauthEntity`）中新增一条记录：

- `F_APPID` / `F_APPSECRET`：填入并同步到本采集网关的 `Collector.AppId` / `Collector.AppSecret`。
- `F_WHITELIST`：填入设备电脑的出口 IP（IP 白名单），限制上报来源。
- `F_USEFULLIFE`：按需设置凭证有效期。

## 编译

```bash
dotnet build api/src/application/Poxiao.Lab.CollectorAgent/Poxiao.Lab.CollectorAgent.csproj
```

## 发布

```bash
dotnet publish api/src/application/Poxiao.Lab.CollectorAgent/Poxiao.Lab.CollectorAgent.csproj `
  -c Release -r win-x64 --self-contained true -o <发布目录>
```

自包含发布，目标设备电脑无需预装 .NET 运行时。

## 安装为 Windows 服务

发布产物复制到设备电脑后，以管理员身份运行：

```powershell
ops\windows\collector\install-collector.ps1 -CollectorDir "D:\collector"
```

默认服务名 `lm-collector`，AUTO_START，日志重定向到 `<CollectorDir>\logs`，异常退出自动重启。

卸载：

```powershell
ops\windows\collector\install-collector.ps1 -Uninstall
```

详见脚本内注释（`Get-Help ops\windows\collector\install-collector.ps1 -Full`）。

## 目录结构

| 目录/文件 | 说明 |
|-----------|------|
| `Program.cs` | 入口：Serilog + Host.CreateDefaultBuilder，加载 `Configurations/*.json`，注册所有服务与 Worker。 |
| `Options/CollectorOptions.cs` | 根配置 + 数据源 / Access / Mock 子配置。 |
| `Sources/IDeviceDataSource.cs` | 数据源统一接口 + `CollectedRecord` 模型。 |
| `Sources/MockDataSource.cs` | 仿真数据源（叠片类记录），用于联调全链路。 |
| `Sources/AccessDataSource.cs` | Access（OleDb/ACE）数据源骨架，仅 Windows。 |
| `State/SourceStateStore.cs` | 采集位点 JSON 持久化（原子写）。 |
| `Spool/SpoolQueue.cs` | 批次本地暂存（落盘 / drain）。 |
| `Upload/ServerClient.cs` | 批量上报 + 心跳 HTTP 客户端，失败类型分类。 |
| `Upload/CollectorHeaders.cs` | 鉴权头名 / 接口路径常量。 |
| `Workers/PollingWorker.cs` | 各数据源独立轮询循环。 |
| `Workers/UploadWorker.cs` | Spool drain + 重试退避。 |
| `Workers/HeartbeatWorker.cs` | 定时心跳。 |
