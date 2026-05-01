<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# systemData

## Purpose
系统数据 (System Data) 模块视图根：管理在线开发依赖的元数据资源——数据接口、数据建模、数据源、数据同步、字典、第三方 OAuth 接口等。这些内容会被 `onlineDev/` 的设计器消费（如 `webDesign/components/TableModal.vue` 拉取 `dataModel`）。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `dataInterface/` | 数据接口配置 |
| `dataModel/` | 数据建模（被在线开发表单/视图引用） |
| `dataSource/` | 数据源连接管理 |
| `dataSync/` | 数据同步任务 |
| `dictionary/` | 数据字典 |
| `interfaceOauth/` | 第三方接口的 OAuth 凭证 |

(本批仅为该根目录生成文档；子目录未在本次 deepinit 列表中)

## For AI Agents

### Working in this directory
- 修改任何子模块的 API 路径前，先 grep `/@/api/systemData/<sub>`，因为在线开发设计器、表单设计器都会消费。
- 跨模块引用关键路径：`webDesign/components/TableModal.vue` → `getDataModelList`。

## Dependencies
### Internal
- 被 `views/onlineDev/*`、`views/workFlow/*` 消费。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
