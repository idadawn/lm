<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# InfluxDB

## Purpose
Thin wrapper over `InfluxData.Net` — exposes only the calls the LIMS lab module actually uses: enumerate measurements, list a measurement's schema/series, fetch the latest data point, and pull recent windows by tag key.

## Key Files
| File | Description |
|------|-------------|
| `IInfluxDBManager.cs` | `Connect()`, `GetAllMeasurementsAsync()`, `GetMeasurementSchemaAsync(measurementName)`, `GetSeriesByMeasurementAsync(measurementName)` (returns `LinkAttribute` rows), `QueryByKeyAndTimeRangeAsync(measurementName, key, minutes)`, `QueryLastAsync(measurementName, key)`. |
| `InfluxDBManager.cs` | Implementation built on `InfluxData.Net.InfluxDb`. |

## For AI Agents

### Working in this directory
- The `int min` parameter on `QueryByKeyAndTimeRangeAsync` is **minutes** of look-back — keep that semantic when extending.
- Connection details come from configuration (not from `DbLinkEntity`); reuse `IInfluxDBManager.Connect()` rather than instantiating a client manually.
- Only the methods listed above are public surface; if you need bucket writes or alerting, add them here so the rest of the codebase has a single integration point.

## Dependencies
### External
- InfluxData.Net (`InfluxData.Net.InfluxDb`, `InfluxData.Net.InfluxDb.Models.Responses`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
