<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# helper

## Purpose
`FormGenerator` 设计器的"逻辑核"：默认配置、左侧组件库定义、`tag -> Vue Component` 映射、JSX render 工厂、设计 JSON 与运行时表单 props 的双向 transform、drawing list 持久化。所有不属于具体组件的纯工具/数据放在这里。

## Key Files
| File | Description |
|------|-------------|
| `componentMap.ts` | `tag -> JNPF/antdv 组件` 的运行时映射（~42KB，覆盖所有可拖拽字段） |
| `config.ts` | 表单全局默认 (`formConf`)、左侧组件库分组定义 (`useInputList`/`useDateList`/`useSelectList` 等) |
| `render.ts` | TSX render 函数工厂：根据 `GenItem` 生成 `<JnpfXxx />` 渲染节点，处理 v-model、事件、slots |
| `transform.ts` | `getRealProps` — 设计器字段名（`clearable`/`filterable`/`props`）转 antdv 字段名（`allowClear`/`showSearch`/`fieldNames`）等兼容层 |
| `utils.ts` | 通用工具：唯一字段名生成、字段查找、扁平化遍历 drawing list |
| `db.ts` | drawing list 全局存取（设计器跨组件共享当前画布字段树） |
| `rightPanel.ts` | 右侧面板字段分组配置 |

## For AI Agents

### Working in this directory
- 添加新字段控件：① `config.ts` 加默认 `__config__`；② `componentMap.ts` 注册；③ 必要时在 `transform.ts` 加字段重命名规则；④ `rightComponents/` 加 `R*.vue` 配置面板。
- `transform.getRealProps(data, jnpfKey)` 直接 mutate 入参 `data` — 调用方自行 cloneDeep。
- `db.ts` 使用单例 ref 模式存 drawing list；不要在测试或多实例场景共享同一 module。

### Common patterns
- 大量使用 `cloneDeep` + `lodash-es` 的 `upperFirst/lowerFirst` 进行 schema 规范化。
- 字段层级通过 `__config__.children` 表达（grid/row/table 等容器）。

## Dependencies
### Internal
- `../types/genItem`、`/@/components/FormGenerator/src/helper/config`（自引用，循环依赖通过 lazy import 规避）
### External
- `lodash-es`、`vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
