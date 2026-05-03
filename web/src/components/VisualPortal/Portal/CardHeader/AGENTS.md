<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# CardHeader

## Purpose
Shared title bar used by every portal widget's `<a-card>` `#title` slot. Renders the title text with icon, configurable color/size/weight/alignment/background, and an optional right-side `web-link` button (`card.cardRightBtn`).

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Single component — props `title`, `card`. Uses inline styles driven by `card.titleFontColor`, `card.titleFontSize`, `card.titleLeft`, `card.titleFontWeight`, `card.titleBgColor`, `card.cardIcon`, `card.cardIconColor`. Right link wraps `<a-button type="link">{{ card.cardRightBtn }}</a-button>` in `webLink`. |

## For AI Agents

### Working in this directory
- The shape of `card` matches what `RCard.vue` (designer) produces — keep field names in sync if either side changes.
- Right-link uses the local `Link` widget under `../Link/`, not a generic anchor; preserve `linkType`/`urlAddress`/`linkTarget`/`type`/`propertyJson` plumbing.

### Common patterns
- Inline `:style` object built directly in template; no helper computed.
- Class `portal-common-title` styled by global portal LESS.

## Dependencies
### Internal
- `../Link/index.vue`

### External
- `vue` (script setup only)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
