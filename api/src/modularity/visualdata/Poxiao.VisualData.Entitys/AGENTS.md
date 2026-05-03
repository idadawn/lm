<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.VisualData.Entitys

## Purpose
数据大屏共享实体工程：BLADE_VISUAL_* 系列实体、Screen/ScreenCategory/ScreenConfig/ScreenDataSource/ScreenMap 子域 DTO，以及大屏图片枚举。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.VisualData.Entitys.csproj` | 工程文件 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Dto/` | 五个子域 DTO 集合 (see `Dto/AGENTS.md`) |
| `Entity/` | `Visual*Entity` 系列（大屏/配置/数据源/地图/分类）(see `Entity/AGENTS.md`) |
| `Enum/` | 大屏图片分类枚举 (see `Enum/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 实体直接实现 `ITenantFilter`，不继承 `CLDEntityBase`；字段写法是 BladeX 旧约定（`CREATE_USER`、`CREATE_TIME`、`IS_DELETED`），与其它 Poxiao 模块不一致——**新表也保持此约定**以便前端 BladeX 通用组件复用。
- `Create()` / `LastModify()` / `Delete()` 是实体内置方法，调用 `App.User` 写入审计字段。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
