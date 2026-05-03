<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# type

## Purpose
`JnpfColorPicker` 的 TypeScript 类型定义。集中存放 `Options`（`enableAlpha`/`format`/`value`）、`Nullable<T>`、`ComponentSize` 等供主面板与 lib 使用。

## Key Files
| File | Description |
|------|-------------|
| `types.ts` | 导出 `Options` 接口、`Nullable<T>` 泛型与 `ComponentSize`（`large/default/small`） |

## For AI Agents

### Working in this directory
- 新增配置项请扩展 `Options` 而不是在组件内重新声明。
- `ComponentSize` 与 `lib/validators.ts#isValidComponentSize` 保持一致。
- 不要在此目录放运行时代码。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
