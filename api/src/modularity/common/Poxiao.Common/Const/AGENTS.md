<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Const

## Purpose
Cross-module string/int constants. Cache key prefixes, JWT claim names, message-centre type ids, aggregation type buckets, and the canonical list of online-form `poxiaoKey` control identifiers. Anything string-typed that is referenced from more than one module belongs here.

## Key Files
| File | Description |
|------|-------------|
| `ClaimConst.cs` | JWT claim names: `UserId`, `UserName`, `Account`, `Administrator`, `TenantId`, `OnlineTicket`. |
| `CommonConst.cs` | Cache prefixes (`poxiao:permission:user`, `menu_`, `permission_`, `datascope_`, `vercode_`, `billrule_`, `poxiao:user:online`, `position_`, `role_`, `visualdev_`, `codegendynamic_`, `timerjob_`, `poxiao:portal:schedule`), `GLOBALTENANT`, `DEFAULTPASSWORD` (read from `AppSettings:DefaultPassword`, falls back to `lm@2025`), and JsonSerializerSettings `options` (Local timezone, `yyyy-MM-dd HH:mm:ss`). |
| `PoxiaoKeyConst.cs` | Visual form control ids — basic (`comInput`, `numInput`, `select`, `date`, …), advanced (`relationForm`, `popupSelect`, `usersSelect`, `treeSelect`, …), system (`createUser`, `currOrganize`, …), layout (`row`, `tab`, `tableGrid`, …). |
| `AggTypeConst.cs` | Aggregation buckets: `DbIntTypes` / `DbByValueFilterModel` (numeric-aggregable types) and `DbByDateRangeFilterModel` (date types). |
| `MessageConst.cs` | Message-centre constants — currently only `METRICNOTICETYPE = "4"` (指标通知). |

## For AI Agents

### Working in this directory
- Cache keys use `_` suffixes for tenant-scoped buckets (`menu_`, `role_`, …) and `:` colons for global ones (`poxiao:user:online`). Match the existing convention when adding new entries.
- `PoxiaoKeyConst` values mirror the front-end form designer's `poxiaoKey` field — change them in lockstep with `web/src/components/poxiao/Generator/.../config.js`.
- Don't move `DEFAULTPASSWORD` to `const` — the property reads `App.Configuration` at call time so deployment-specific defaults can override the literal `lm@2025`.

### Common patterns
- All classes are `[SuppressSniffer] public class …Const` with `public const` fields.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
