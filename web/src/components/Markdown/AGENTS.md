<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Markdown

## Purpose
基于 [Vditor](https://github.com/Vanessa219/vditor) 的 Markdown 编辑器与查看器封装。`MarkDown` 用于编辑场景（笔记、富文本输入），`MarkdownViewer` 用于纯阅读（说明文档、检测报告备注），与系统主题/语言联动。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 通过 `withInstall` 暴露 `MarkDown` 与 `MarkdownViewer`，并 re-export 类型。 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 编辑器、查看器、主题映射 (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 修改导出名时注意 `MarkDown` 命名（与默认导入不同），全项目按此引用。
- 类型从 `./src/typing` re-export，新增类型须先在 typing 中声明再 `export *`。

### Common patterns
- 编辑/查看分离的两个组件共享 `getTheme.ts` 进行 dark/light 主题切换。

## Dependencies
### Internal
- `/@/utils`、`./src/typing`
### External
- `vditor`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
