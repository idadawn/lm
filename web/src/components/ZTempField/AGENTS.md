<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ZTempField

## Purpose
模板化动态字段渲染组件。根据传入的字段元数据(`item.dataType` / `factorType` 等)自动渲染为 input / number / switch / select / radio 等控件,并支持递归渲染子项 (`childItems`),用于"产品因子"类的可配置字段表单。通过 `withInstall(ZTempField)` 暴露。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 入口,`withInstall` 包装 `src/index.vue` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件实现 + 高度可定制的 prop 映射 (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 字段类型映射由 `state.inputTypes / numberTypes / switchTypes / selectTypes / radioTypes` 五个集合控制,扩展类型时同步五处。
- 控件 key 名(`factorType / factorValue / productFactorList` 等)全部通过 props 可覆盖,**不要硬编码**;调用方根据后端契约传 `typeProps / keyProps / listProps`。
- `displayProps` 控制字段是否显示(`'N'` 隐藏),保留 form-item 占位以维持表单顺序。

### Common patterns
- 与 `jnpf-input / jnpf-select / jnpf-switch / jnpf-input-number` 等全局组件协同。

## Dependencies
### Internal
- `/@/utils` (`withInstall`),全局 jnpf-* 组件
### External
- Ant Design Vue,`@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
