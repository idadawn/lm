<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.VisualDev.Entitys

## Purpose
低代码模块共享实体工程：功能模板/发布版本、模型数据、门户、外链等实体；按 CodeGen/Dashboard/Portal/VisualDev/VisualDevModelData 组织 DTO。

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.VisualDev.Entitys.csproj` | 工程文件 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Dto/` | 五个子域 DTO 集合 (see `Dto/AGENTS.md`) |
| `Entity/` | 模板/发布/模型数据/外链/门户/门户数据 (see `Entity/AGENTS.md`) |
| `Enum/` | 代码生成模式枚举 (see `Enum/AGENTS.md`) |
| `Mapper/` | Mapster 映射（`VisualDevEntity` → `Selector` 输出）(see `Mapper/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 实体使用 `CLDEntityBase`（与 message 模块同），`F_*` 字段名混合大小写都有出现（`F_FORMDATA` 全大写，`F_Type`/`F_LinkType` 混合）；务必逐字段 `[SugarColumn(ColumnName)]` 写明。
- `VisualDevEntity` 与 `VisualDevReleaseEntity` 字段几乎一致，是「编辑模板」和「线上发布」双副本——两边新增字段需同步。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
