<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Net

## Purpose
Network/HTTP utilities — UA-string parsing and IP geo-lookup. Powers the login-log "登录摘要" / "登录IP" fields, the operation-log device column, and any feature that needs to know the client's OS/browser/device or origin city.

## Key Files
| File | Description |
|------|-------------|
| `IUserAgent.cs` | UA contract — `RawValue`、`userAgent`、`Device`、`OS`、`IsMobileDevice`、`IsTablet`. Also defines `DeviceInfo`、`OSInfo`、`UserAgentInfo` value objects with version-aware `ToString()` (`Family Major.Minor.Patch`). |
| `UserAgent.cs` | UA parser implementation — produces an `IUserAgent` from a raw header string (≈5 KB). |
| `IpLocator.cs` | IP→geolocation resolver (≈11 KB) — the heavy lifter for `IpLocation`. |
| `IpLocation.cs` | Result POCO — `Ip`、`Country`、`Local`. |

## For AI Agents

### Working in this directory
- Namespace `Poxiao.Infrastructure.Net`.
- The UA parser is a hand-rolled rules engine (no external UA-parser library) — extend rules by adding cases inside `UserAgent.cs`, do not pull in a NuGet replacement without checking license/perf trade-offs.
- `IpLocator` likely loads a binary geo database (qqwry/ip2region style) at startup; treat its lifetime as singleton.
- `IUserAgent` is interface-first so tests/mocks can stub it; keep new accessors there as well as the implementation.

### Common patterns
- Sealed value objects (`DeviceInfo`/`OSInfo`/`UserAgentInfo`) with positional ctors.
- `VersionString.Format` helper for "skip empty parts" version stringification.

## Dependencies
### Internal
- Consumed by the SysLog write path (login/operation log enrichment).
### External
- None at the contract level; impl files may load embedded geo data.

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
