<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HText

## Purpose
门户(Portal)运行时文本卡片组件。在 VisualPortal 已发布的门户中渲染富文本/纯文本块,支持跑马灯、字号字色样式、点击跳转(经 Link 组件)以及通过 `dataInterface` 拉取动态数据。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 文本卡片渲染入口,包含 `marquee` 滚动、富文本 v-html、动态数据接口接入 |

## For AI Agents

### Working in this directory
- 配置态在 `../../Design/HText` 中,本目录只负责运行时渲染,不要在此处加入面板/属性编辑逻辑。
- 数据来源由 `activeData.dataType`(`static`/`dynamic`)决定;dynamic 模式必须走 `getDataInterfaceRes`,不要直连业务接口。

### Common patterns
- `defineProps(['activeData'])` 接收门户配置节点;通过 `activeData.option.*` 读取样式/链接配置。
- 复用同级 `CardHeader`、`Link` 包装,保持卡片头与跳转语义统一。

## Dependencies
### Internal
- `../CardHeader/index.vue`、`../Link/index.vue`
- `/@/api/systemData/dataInterface` (`getDataInterfaceRes`)
### External
- Ant Design Vue (`a-card`)、原生 `marquee`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
