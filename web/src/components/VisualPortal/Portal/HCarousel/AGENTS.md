<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HCarousel

## Purpose
Image carousel widget for the portal. Renders `activeData.option.carouselList` inside Ant Design Vue's `Carousel`, with autoplay, indicator-position, arrow modes (`never`/`hover`/`always`), each slide wrapped in `web-link` for click-through.

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | The component — binds `autoplaySpeed`, `autoplay`, `dotPosition` (computed from `carouselIndicatorPosition`), `dots`, `arrows`. Custom `prevArrow`/`nextArrow` slots use `LeftCircleOutlined`/`RightCircleOutlined`. |

## For AI Agents

### Working in this directory
- Configuration field names (`carouselInterval`, `carouselAutoplay`, `carouselIndicatorPosition`, `carouselArrow`) must match `RCarouselModal.vue` in the designer.
- Each slide is a `web-link` so static/internal/external link types all work — do not replace with raw `<a>`.
- `'hover'` arrow mode toggles `carousel-arrows-hover` class for CSS-only show/hide.

### Common patterns
- `useCommon(activeData)` resolves the slide list and supplies `CardHeader`.

## Dependencies
### Internal
- `../../Design/hooks/useCommon`, `../CardHeader`, `../Link`

### External
- `ant-design-vue` (Carousel), `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
