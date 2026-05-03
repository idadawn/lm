<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# types

## Purpose
`FormGenerator` 设计器的核心 schema 类型契约。定义画布字段对象 (`GenItem`) 与字段元数据 (`ItemCfg`)，是设计器/Parser/transform/render 共享的数据结构基础。

## Key Files
| File | Description |
|------|-------------|
| `genItem.ts` | `ItemCfg`（jnpfKey/label/labelWidth/showLabel/tag/tagIcon/className/defaultValue/required/layout/span/visibility 等）+ `GenItem`（`__config__: ItemCfg, on?: { change, blur, click, tabClick }, [prop]: any`） |

## For AI Agents

### Working in this directory
- `GenItem` 是设计器输出 JSON 的最小单元；新增字段属性应优先放入 `ItemCfg.__config__` 而非顶层，避免与 antdv 控件原生 props 冲突。
- `[prop: string]: any` 的 index signature 是有意保留的逃生口 — 修改时不要收紧到具体类型，否则破坏向后兼容。
- 任何修改这些类型的提交都应该在 `helper/config.ts` 的默认值字典里同步更新。

### Common patterns
- `__config__` 命名空间存储设计器元信息，运行时 `Parser.vue` 通过 `transform.ts` 把它分离出来。

## Dependencies
### Internal
- 无（纯类型）
### External
- 无（仅 TS 类型）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
