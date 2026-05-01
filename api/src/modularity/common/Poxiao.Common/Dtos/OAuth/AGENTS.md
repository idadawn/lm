<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# OAuth

## Purpose
Shared DTOs for multi-tenant OAuth/SSO integration — currently exposes the local network-link summary returned by tenant connectivity probes (used by the tenant-aware OAuth bootstrapping flow).

## Key Files
| File | Description |
|------|-------------|
| `TenantInterFaceOutput.cs` | Tenant probe output — `dotnet` (runtime version) + optional `linkList` of `TenantLinkModel`. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Dtos.OAuth`.
- `TenantLinkModel` referenced here lives elsewhere (likely under `Models/` or `Const/`); keep this DTO thin and don't inline it.
- `[SuppressSniffer]` required.
- The folder spelling is `OAuth` (capital A) — keep it consistent.

### Common patterns
- One DTO per file, camelCase props.
- Nullable navigation lists (`linkList?`).

## Dependencies
### Internal
- `Poxiao.DependencyInjection` — `[SuppressSniffer]`.
- Consumed by tenant/OAuth probe controllers.
### External
- None.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
