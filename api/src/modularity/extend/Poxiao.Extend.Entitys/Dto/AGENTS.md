<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Dto

## Purpose
扩展模块全部业务的 DTO 根目录，按功能拆分子目录。每个子目录下放置当前业务的 Input（Cr/Up/Query/Actions...）与 Output（List/Info/Tree/...）。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `BigData/` | 大数据列表 DTO |
| `Document/` | 知识管理（文件夹/文件/分享/回收/上传） |
| `DocumentPreview/` | 文档预览输入输出 |
| `Email/` | 邮件配置/收发/草稿/列表 |
| `Employee/` | 职员列表/导入 |
| `Order/` | 订单 + 收款计划 + 商品/客户行 |
| `Product/` / `ProductClassify/` / `ProductCustomer/` / `ProductEntry/` / `ProductGoods/` | 产品体系（产品/分类/客户/录入/商品行） |
| `ProjectGantt/` | 项目甘特（项目 + 任务 + 树视图） |
| `Schedule/` | 日程 CRUD + 列表查询 |
| `TableExample/` | 表格演示（含批注/签字/行编辑） |
| `WorkLog/` | 工作日志 CRUD |

## For AI Agents

### Working in this directory
- 命名严格：`<Feature><Action>Input/Output/Query`。`Cr`=创建、`Up`=更新、`Info`=详情、`List`=列表、`Tree`=树、`Actions<Verb>` 为动作型端点。
- 每个 DTO 类都标 `[SuppressSniffer]`，关闭 StyleCop 的命名警告。
- DTO 字段使用 camelCase；时间字段如果与前端约定时间戳，类型用 `long?`，否则 `DateTime?`。

## Dependencies
### Internal
- `Poxiao.Infrastructure.Filter.PageInputBase`（分页查询基类）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
