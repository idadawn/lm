<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ProvinceAtlas

## Purpose
DTOs for "行政区划地图" — serves the GeoJSON polygon data for a region code (used by ECharts / Leaflet map components on dashboards) and lists available atlas regions.

## Key Files
| File | Description |
|------|-------------|
| `ProvinceAtlasGeojsonInput.cs` | Single-field input — `code` (administrative region code) used to look up the matching GeoJSON document. |
| `ProvinceAtlasListOutput.cs` | List projection enumerating available atlas region records (id, name, code, has-geojson flag). |

## For AI Agents

### Working in this directory
- The actual GeoJSON payload is large; the controller is expected to stream/return it as a string or `JsonElement`, not as a typed DTO. Keep this directory limited to lookup / list shapes.
- `code` corresponds to the same `enCode` used in the sibling `Province` DTO — keep them aligned.
- Namespace `Poxiao.Systems.Entitys.Dto.ProvinceAtlas` (no `.System.` segment).
- `[SuppressSniffer]` applied.

### Common patterns
- Minimal input shape (single `code`) for read endpoints.

## Dependencies
### Internal
- Cross-references `Province` records via shared `enCode` / `code` values.

### External
- `Poxiao.DependencyInjection` (`[SuppressSniffer]`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
