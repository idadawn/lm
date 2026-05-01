<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Mapper

## Purpose
Mapster `IRegister` registrations applied at startup. Defines cross-module mappings that don't naturally belong inside any one module's DTO — currently `UserEntity → UserInfoModel` and `JobTriggers → TimeTaskEntity`.

## Key Files
| File | Description |
|------|-------------|
| `Mapper.cs` | `public class Mapper : IRegister`. Maps `UserEntity → UserInfoModel` (id→userId, Account→userAccount, RealName→userName, HeadIcon → `/api/File/Image/userAvatar/{HeadIcon}`, PrevLogTime/PrevLogIP → prevLoginTime/prevLoginIPAddress). Maps `JobTriggers → TimeTaskEntity` (NumberOfRuns→RunCount, LastRunTime, NextRunTime). |

## For AI Agents

### Working in this directory
- Add new cross-module mappings here, not inside individual module DTOs. Single-module mappings should stay close to the module that owns them.
- `IRegister.Register` is auto-discovered at startup by `Poxiao.Extras.ObjectMapper.Mapster`; no manual wiring needed.
- Don't put behaviour or `AfterMapping` lambdas with side effects — only field-level mapping.

## Dependencies
### Internal
- Mapster, `Poxiao.Systems.Entitys.Permission.UserEntity`, `Poxiao.Infrastructure.Models.User.UserInfoModel`, `Poxiao.TaskScheduler.Entitys` (JobTriggers, TimeTaskEntity).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
