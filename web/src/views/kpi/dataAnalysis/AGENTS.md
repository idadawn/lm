<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# dataAnalysis

## Purpose
KPI 数据分析页面：两 Tab —— 「曲线分析」（按文件类型上传图片，调用对应 `state.uploadUrlMap[fileType]`，前端展示返回图与解读文本）和「一键分析」（打开 fastgpt AI 助手做检测数据快速洞察）。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 文件类型 Select + Upload + 结果图/文字；fastgpt 一键分析入口 + 异步 loading。 |

## For AI Agents

### Working in this directory
- 上传走 ant-design-vue `<a-upload :action="...">` 直传后端；不同 `fileType` 对应不同 `uploadUrlMap` URL，需后端先约定接口。
- AI 接口由 `/@/api/dataAnalysis` 的 `completions` 提供（fastgpt 兼容）；切换模型时改 API 而非视图。
- 页面 `defineOptions({ name: 'base' })` 命名过宽，建议改为 `kpi-dataAnalysis`。

### Common patterns
- `Spin` + `state.isFastgptPending` 控制按钮 loading
- 上传成功回填 `state.imageUrl`、`state.answer`

## Dependencies
### Internal
- `/@/api/dataAnalysis`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
