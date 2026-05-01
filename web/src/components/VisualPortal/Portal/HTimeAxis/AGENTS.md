<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HTimeAxis

## Purpose
门户运行时时间轴卡片。基于 `ant-design-vue` 的 `Timeline` 渲染 8 种布局(左/右/交替/垂直顶/底等),支持卡片化样式与空数据占位图,数据由 `useCommon` 从 `activeData` 派生。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Timeline 容器,按 `layout` 取值切换横/纵向与对齐;`styleType` 切换纯文本与卡片样式 |

## For AI Agents

### Working in this directory
- 不要在此处直接 fetch 数据;`timeList` 由 `../../Design/hooks/useCommon` 统一处理(static/dynamic)。
- 添加新布局时同步扩展 `getMode` / `getPosition` / `getColor` 帮助函数,保持 layout 编号连续。

### Common patterns
- 通过 `useCommon(props.activeData)` 解构 `CardHeader / timeList / getOption`,与同级其他 H* 组件一致。
- 空状态统一使用 `assets/images/portal-nodata.png` + `暂无数据` 文案。

## Dependencies
### Internal
- `../../Design/hooks/useCommon`
### External
- `ant-design-vue` 的 `Timeline`、`TimelineItem`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
