<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# codegen

## Purpose
代码生成 (server-side code generator) module group. Hosts the project that produces controller/service/entity/Vue scaffolding from VisualDev metadata — the back-office tool exposed under `api/visualdev/...` for accelerating feature delivery in the Laboratory Data Analysis System.

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Poxiao.CodeGen/` | Implementation project containing `CodeGenService` and the `.csproj` (see `Poxiao.CodeGen/AGENTS.md`). |

## For AI Agents

### Working in this directory
- This module group has **no Entitys / Interfaces sub-projects** of its own — its DTOs and entities live in `modularity/visualdev/Poxiao.VisualDev.Entitys` (referenced from `Poxiao.CodeGen.csproj`). Add new types there, not here.
- The generator templates and view-engine integration belong to `Poxiao.VisualDev.Engine` (a project reference); only orchestration/HTTP surface belongs in this group.

### Common patterns
- Single-project module group: most other `modularity/*` groups have three projects (impl + Entitys + Interfaces) but `codegen` only has the impl because it consumes VisualDev's contracts.

## Dependencies
### Internal
- `api/src/modularity/engine/Poxiao.VisualDev.Engine` — view engine + code-gen models/templates.
- `api/src/modularity/visualdev/Poxiao.VisualDev.Interfaces` — VisualDev service contracts.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
