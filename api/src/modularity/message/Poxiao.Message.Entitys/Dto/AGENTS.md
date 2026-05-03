<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dto

## Purpose
消息模块全部 API DTO 的根目录，按业务子域划分。控制器 `[FromQuery]/[FromBody]` 输入与 `Output` 输出严格隔离实体层，方便前后端独立演进。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `IM/` | IM 即时通讯输入/输出（消息基类、文本/图片/语音、在线列表）(see `IM/AGENTS.md`) |
| `ImReply/` | 聊天会话列表与对象 ID 输出 (see `ImReply/AGENTS.md`) |
| `Message/` | 系统消息/公告 CRUD 输入与列表/详情/已读 DTO (see `Message/AGENTS.md`) |
| `MessageAccount/` | 账号配置查询/输出/邮件测试 (see `MessageAccount/AGENTS.md`) |
| `MessageMonitor/` | 监控列表查询/输出/批量删除 (see `MessageMonitor/AGENTS.md`) |
| `MessageTemplate/` | 模板列表与查询 (see `MessageTemplate/AGENTS.md`) |
| `SendMessage/` | 发送配置（渠道-模板-账号编排）输出 (see `SendMessage/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 字段一律 camelCase（前端 axios 直接消费），不可使用 PascalCase。
- 输入 DTO 文件后缀：`*CrInput`（创建）、`*UpInput`（更新）、`*Query` / `*QueryInput`（列表查询，多继承 `PageInputBase`）；输出后缀：`*ListOutput` / `*InfoOutput` / `*Output`。
- 新增字段需在对应控制器读取/写入，无对应实体字段则在 `Mapper.cs` 显式映射。

### Common patterns
- `[SuppressSniffer]` 标注用于关闭框架的对象监听器（SuppressSniffer 来自 `Poxiao.DependencyInjection`）。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
