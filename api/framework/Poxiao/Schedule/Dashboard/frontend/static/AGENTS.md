<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# static

## Purpose
看板前端构建产物的静态资源根目录，仅作为生成内容承载层。具体 JS / CSS 拆分到子目录中，最终由 `EmbeddedFileProvider` 通过 `frontend.static.*` 资源名暴露。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `css/` | 编译后的样式与 source map (see `css/AGENTS.md`) |
| `js/` | 编译后的脚本、license 与 source map (see `js/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 此目录与子目录都是构建产物，禁止手工修改。版本升级请整体替换，并保持目录结构与现有命名一致。
- 不要新增运行时资源到该层级——所有引用须从 `index.html` 出发。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
