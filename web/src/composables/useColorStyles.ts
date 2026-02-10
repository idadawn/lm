import { watchEffect, onUnmounted, type ShallowRef } from 'vue';

let instanceCounter = 0;

interface ColorStylesOptions {
  coloredCells: ShallowRef<Record<string, string>>;
}

/**
 * Convert a cell key (rowId + field) to a valid, deterministic CSS class name.
 * Same inputs always produce the same output.
 */
function cellCssClass(rowId: string, field: string): string {
  return `cc-${rowId}-${field}`.replace(/[^a-zA-Z0-9-_]/g, '-');
}

export function useColorStyles({ coloredCells }: ColorStylesOptions) {
  const styleId = `cell-colors-${++instanceCounter}`;
  let styleEl: HTMLStyleElement | null = null;

  function getStyleEl(): HTMLStyleElement {
    if (!styleEl) {
      styleEl = document.createElement('style');
      styleEl.id = styleId;
      document.head.appendChild(styleEl);
    }
    return styleEl;
  }

  // Reactively rebuild CSS rules when coloredCells changes.
  // flush: 'post' ensures this runs AFTER DOM updates, preventing recursive update loops.
  // Reading coloredCells.value here creates the reactive dependency so that
  // triggerRef(coloredCells) triggers a CSS rebuild.
  const stop = watchEffect(() => {
    const map = coloredCells.value;
    const el = getStyleEl();
    const rules: string[] = [];

    for (const [compositeKey, color] of Object.entries(map)) {
      if (!color) continue;
      const sep = compositeKey.indexOf('::');
      if (sep === -1) continue;
      const rowId = compositeKey.substring(0, sep);
      const field = compositeKey.substring(sep + 2);
      const cls = cellCssClass(rowId, field);
      rules.push(`.${cls}{background-color:${color}!important}`);
    }

    el.textContent = rules.join('\n');
  }, { flush: 'post' });

  /**
   * Returns a deterministic CSS class name for a cell.
   *
   * IMPORTANT: This function does NOT read from any reactive state.
   * This means calling it during template rendering creates NO reactive
   * dependencies. Color styling is applied purely via CSS rules generated
   * by the watchEffect above - the browser's CSS engine handles applying
   * and removing background colors when rules change.
   */
  const getCellClass = (rowId: string, field: string): string => {
    return cellCssClass(rowId, field);
  };

  onUnmounted(() => {
    stop();
    if (styleEl) {
      styleEl.remove();
      styleEl = null;
    }
  });

  return { getCellClass };
}
