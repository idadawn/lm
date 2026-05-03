<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Model

## Purpose
跨控制器/服务复用的内部视图 Model（既不是 Entity 也不是公开 DTO），按业务子域分组。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `IM/` | IM 未读数据 Model (see `IM/AGENTS.md`) |
| `MessageTemplate/` | 模板/发送/SMS 字段/参数四个 Model (see `MessageTemplate/AGENTS.md`) |

## For AI Agents

### Working in this directory
- Model 用于 `Mapster` 中转、Service 内部组装；如需对外暴露请放到 `Dto/`。
- 命名后缀 `Model`，区别于 `*Output` / `*Input`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
