<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Poxiao.SubDev.Interfaces

## Purpose
Service-contract project for the 二次开发 module. Defines the `I*Service` interfaces that `Poxiao.SubDev` (impl) will fulfill and that other modules will consume. Currently empty (only `.csproj`); scaffolded so dependent modules can declare a SubDev dependency without coupling to its implementation.

## Key Files
| File | Description |
|------|-------------|
| `Poxiao.SubDev.Interfaces.csproj` | Empty project; references `Poxiao.SubDev.Entitys`; pulls StyleCop analyzers. |

## For AI Agents

### Working in this directory
- Add only **interfaces** here (`public interface IXxxService`), with XML doc comments in Chinese matching the project style.
- Each interface should be marked `: IScoped` / `: ITransient` / `: ISingleton` from `Poxiao.DependencyInjection` so the impl can be auto-registered (sibling modules use this pattern, e.g. `Poxiao.Systems.Interfaces`).
- Method signatures must use the entities/DTOs from `Poxiao.SubDev.Entitys` — don't redeclare data shapes here.
- This project must remain **implementation-free** — no service classes, no logic. The impl lives in `Poxiao.SubDev`.

## Dependencies
### Internal
- `Poxiao.SubDev.Entitys` — entity / DTO types referenced by the interface signatures.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
