# Windows 单机部署（NSSM 服务 + 单端口对外 + OSS 增量分发）

检测室数据分析系统在 Windows 上的部署工具。所有后端服务由 **NSSM** 托管为常驻 Windows 服务，
**只有 nginx 一个端口对外**，其余全部绑 `127.0.0.1`；配置**集中在一个文件**；升级走**阿里云 OSS 版本清单**。

## 目录角色

| 位置 | 角色 |
|------|------|
| 仓库 `ops/windows/` | 部署脚本的**源**（在这里维护、改动） |
| `D:\deploy\lab\` | **运行时根 + 自包含部署包**（二进制 + 构建产物 + 渲染出的配置 + 日志） |
| `D:\deploy\lab\ops\` | push 时 stage 进来的脚本副本，让整个 `D:\deploy\lab` 可独立部署（服务器只要这一个目录） |

## 一个配置源

**唯一需要编辑的文件是 [`ops/windows/.env`](.env.example)**（gitignored）。它含所有主机/端口/凭据/连接串/AI/OSS。
部署时 `render-config.ps1` 从它渲染出各服务真正读取的配置：

```
ops/windows/.env  ──render-config.ps1──▶  api/Configurations/ConnectionStrings.<Env>.json  (MySQL)
                                          api/Configurations/Cache.<Env>.json                (Redis)
                                          api/Configurations/EventBus.<Env>.json             (RabbitMQ)
                                          api/Configurations/AI.json                         (.NET AI)
                                          api/Configurations/OSS.json                        (.NET 文件服务)
                                          api/Configurations/AppSetting.json  的 NlqAgent.BaseUrl
                                          nlq-agent/.env  (DATABASE_URL / REDIS_URL / LITELLM / JWT)
                  ──install-services──▶   web/nginx.conf  (端口占位符)
```

改完 `.env` 后：完整重部署，或 `.\render-config.ps1` + `nssm restart lm-api lm-nlq-agent` 即生效。

## 服务与端口

| 服务 | NSSM 名 | 端口 | 绑定 | 对外 |
|------|---------|------|------|------|
| nginx 网关 | `lm-web` | **80** | 0.0.0.0 | **是（唯一）** |
| .NET 后端 | `lm-api` | 10089 | 127.0.0.1 | 否（经 `/api` 反代） |
| nlq 智能体 | `lm-nlq-agent` | 8000 | 127.0.0.1 | 否（经 `/nlq-agent` 反代，SSE 流式已调优） |
| Redis | `lm-redis` | 6380 | 127.0.0.1 | 否 |
| RabbitMQ | `lm-rabbitmq` | 8005 / 管理台 15672 | 127.0.0.1 | 否 |
| MySQL | `MySQL80`（已有） | 3306 | 由其 my.ini 决定 | 见下 |

浏览器入口：`http://<服务器IP>/`。WebSocket(`/api/message/websocket`) 与聊天 SSE(`/nlq-agent/...`) 均经 :80 同源反代。

> **MySQL 收口**：MySQL 不在本工具管理范围（用已有的 `MySQL80`）。要限制它仅本机，
> 在其 `my.ini` 的 `[mysqld]` 加 `bind-address=127.0.0.1` 后 `Restart-Service MySQL80`——
> **仅当所有客户端都在本机时**才这么做；否则用防火墙限制 3306。

## 首次部署

1. 目标机装好 .NET 10 运行时、[uv](https://docs.astral.sh/uv/)、MySQL（建好 `lumei` 库）。
2. 准备 `D:\deploy\lab`：二进制(nginx/redis/erlang+rabbitmq/nssm)、构建产物(api/web/nlq)、以及 `ops\`（含填好的 `.env`）。
   - 可在开发机构建后用 `publish-release.ps1` 发布到 OSS，服务器用 `upgrade.ps1` 拉取升级；
   - 或直接把整个 `D:\deploy\lab` 拷到服务器。
3. **管理员** PowerShell 运行 `D:\deploy\lab\ops\deploy-all.ps1`。

## 增量升级（OSS）

```powershell
# 开发/构建机：重新构建产物后
.\ops\windows\publish-release.ps1 -PackageMode Full       # 完整发布到 OSS
.\ops\windows\publish-release.ps1 -PackageMode ToolPatch  # 只发布部署工具/控制台补丁

# 服务器（管理员）：
D:\deploy\lab\ops\upgrade.ps1          # 检查 release.json，下载并应用新版本
```

发布与升级脚本使用 OSS REST 签名请求上传/下载对象，不要求服务器安装或调用 `ossutil`。
`ToolPatch` 只更新脚本和 WPF 控制台，不会停止业务服务；`Full` 包含应用服务产物、脚本和控制台，会在解压后执行完整部署。基础软件由 `PREREQ_PREFIX` 下的独立包提供。

## 防火墙与备份

- 控制台保存端口时可调用 `configure-firewall.ps1`，默认只放行 Web 入口端口。
- `publish-prereqs.ps1` 会把 nssm/nginx/redis/erlang/rabbitmq 基础软件上传到 `OSS_PREFIX/PREREQ_PREFIX`；控制台下载基础软件默认从该 OSS 目录拉取。
- `publish-console-package.ps1` 会上传一个 `lm-deploy-console.zip` 启动包；首次部署只需要这个控制台包，后续资源由控制台从 OSS 拉取。
- `backup-database.ps1 -RunNow` 立即备份 MySQL；`backup-database.ps1 -InstallSchedule` 注册每日计划任务。
- 备份时间与保留天数由 `DB_BACKUP_TIME`、`DB_BACKUP_KEEP_DAYS` 控制，文件输出到 `<DEPLOY_ROOT>\backups\db`。

### 升级授权码（WPF 控制台）

在线升级/发布所需的 OSS 凭据不必手工编辑 `.env`：WPF 控制台首次启动检测不到凭据时会自动弹出
「升级授权码」对话框；之后也可随时通过左侧「输入授权码/修改授权码」按钮或
「部署与升级 → 在线升级 → 输入授权码」修改。点「检查更新/立即在线升级/发布」时若未授权也会先提示输入。

- 授权码格式：`Base64("LMAUTH1|endpoint|bucket|accessKeyId|accessKeySecret|prefix")`，
  保存后自动写入 `ops/.env` 的 `OSS_*` 键（`.env` 不存在时以 `.env.example` 为底渲染）。
- 生成方式：在已配置好 OSS 的机器（如构建机）上打开授权码对话框，点「复制当前授权码」，
  把这一串码发给现场粘贴即可。

> **安全**：`.env` 含 DB/Redis/RabbitMQ/LLM/OSS 密钥，随包进入你的**私有** bucket。
> 请确保 bucket 非公开、AccessKey 用仅授权该 bucket 的 RAM 子账号。

## 常用运维

```powershell
Get-Service lm-*                       # 看所有服务状态
nssm restart lm-api                    # 重启单个服务
Get-Content D:\deploy\lab\logs\lm-api\stderr.log -Tail 50   # 看日志
.\install-services.ps1 -Only web -SkipBuild   # 只重装网关（改了端口/配置、产物已最新）
.\install-services.ps1 -Uninstall             # 卸载应用服务
.\install-infra.ps1 -Uninstall                # 卸载 Redis/RabbitMQ
```

## 脚本清单

| 脚本 | 作用 |
|------|------|
| `deploy-all.ps1` | 一键：install-infra → install-services（日志落 `logs\deploy-all.log`） |
| `install-infra.ps1` | 注册 `lm-redis` / `lm-rabbitmq`（含 Erlang 静默安装、cookie 同步、建 admin 账号） |
| `install-services.ps1` | 注册 `lm-api` / `lm-web` / `lm-nlq-agent`；启动前调用 render-config |
| `render-config.ps1` | 从 `.env` 渲染各服务配置文件（配置集中化的核心） |
| `configure-firewall.ps1` | 放行 Web 入口端口到 Windows 防火墙 |
| `backup-database.ps1` | 立即备份 MySQL 或注册每日备份计划任务 |
| `publish-prereqs.ps1` | 上传 Windows 基础软件包到 OSS，供安装页下载 |
| `publish-console-package.ps1` | 上传单文件控制台启动包，作为首次部署入口 |
| `publish-all-assets.ps1` | 一键上传基础软件、服务产物、脚本和完整版本清单 |
| `publish-release.ps1` | 开发/构建机：打包并通过 OSS REST 上传版本包和 `release.json` |
| `upgrade.ps1` | 服务器：检查 `release.json`，下载版本包并执行升级 |
| `install-auto-upgrade.ps1` | 注册计划任务，定时执行 `upgrade.ps1` |
| `build-desktop-console.ps1` | 构建 WPF 部署控制台到部署目录 |
| `_dotenv.ps1` | `.env` 解析（被其他脚本 dot-source） |

## 待办

- **LLM 网关**：`.env` 的 `LITELLM_BASE_URL` / `LITELLM_API_KEY` 填真实值前，nlq 聊天(Chat2SQL)不可用（其余功能正常）。
