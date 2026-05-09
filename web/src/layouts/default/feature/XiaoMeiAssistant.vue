<template>
  <div class="nlq-assistant-entry">
    <button
      type="button"
      class="assistant-trigger"
      :aria-expanded="panelVisible"
      aria-label="打开智能问数"
      @click="togglePanel"
    >
      <img src="/img/chat.svg" alt="智能问数" class="assistant-trigger__icon" />
    </button>

    <Teleport to="body">
      <div class="nlq-assistant-layer" :class="{ 'is-fullscreen': fullscreenVisible }">
        <section
          v-if="panelVisible"
          ref="panelRef"
          class="assistant-panel"
          :class="{ 'is-fullscreen': fullscreenVisible, 'is-dragging': isDragging }"
          :style="panelStyle"
        >
          <header class="assistant-panel__header" @pointerdown="handleDragStart">
            <div class="assistant-panel__title">
              <img src="/img/chat.svg" alt="" class="assistant-panel__logo" />
              <span>智能问数</span>
            </div>
            <div class="assistant-panel__actions">
              <button
                type="button"
                class="assistant-panel__action"
                :title="fullscreenVisible ? '退出全屏' : '全屏'"
                @click.stop="toggleFullscreen"
              >
                <Icon
                  :icon="fullscreenVisible ? 'ant-design:fullscreen-exit-outlined' : 'ant-design:fullscreen-outlined'"
                  :size="16"
                />
              </button>
              <button
                type="button"
                class="assistant-panel__action"
                title="关闭"
                @click.stop="closePanel"
              >
                <Icon icon="ant-design:close-outlined" :size="16" />
              </button>
            </div>
          </header>

          <div class="assistant-panel__body">
            <ChatAssistant />
          </div>
        </section>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
  import { computed, nextTick, onBeforeUnmount, ref, watch } from 'vue';
  import { Icon } from '/@/components/Icon';
  import ChatAssistant from '/@/views/lab/monthly-dashboard/components/ChatAssistant.vue';

  const panelVisible = ref(false);
  const fullscreenVisible = ref(false);
  const panelRef = ref<HTMLElement | null>(null);
  const isDragging = ref(false);
  const compactBreakpoint = 960;
  const panelMargin = 12;
  const desktopGap = 24;
  const viewport = ref({ width: 0, height: 0 });
  const panelPosition = ref({ x: 0, y: 0 });
  const hasCustomPosition = ref(false);
  const dragState = {
    pointerId: -1,
    offsetX: 0,
    offsetY: 0,
  };

  const isCompactLayout = computed(() => viewport.value.width <= compactBreakpoint);

  const panelStyle = computed(() => {
    if (fullscreenVisible.value || isCompactLayout.value || !hasCustomPosition.value) {
      return undefined;
    }
    return {
      left: `${panelPosition.value.x}px`,
      top: `${panelPosition.value.y}px`,
      right: 'auto',
      transform: 'none',
    };
  });

  function syncViewport() {
    if (typeof window === 'undefined') return;
    viewport.value = { width: window.innerWidth, height: window.innerHeight };
  }

  function getViewportMargin() {
    return isCompactLayout.value ? panelMargin : desktopGap;
  }

  function clampPanelPosition(x: number, y: number) {
    const panelEl = panelRef.value;
    if (!panelEl) return { x, y };
    const margin = getViewportMargin();
    const maxX = Math.max(margin, viewport.value.width - panelEl.offsetWidth - margin);
    const maxY = Math.max(margin, viewport.value.height - panelEl.offsetHeight - margin);
    return {
      x: Math.min(Math.max(margin, x), maxX),
      y: Math.min(Math.max(margin, y), maxY),
    };
  }

  async function ensurePanelPosition() {
    await nextTick();
    if (!panelVisible.value) return;
    if (fullscreenVisible.value || isCompactLayout.value) {
      hasCustomPosition.value = false;
      return;
    }
    if (hasCustomPosition.value) {
      panelPosition.value = clampPanelPosition(panelPosition.value.x, panelPosition.value.y);
    }
  }

  function syncPanelPosition() {
    syncViewport();
    if (!panelVisible.value || fullscreenVisible.value) return;
    if (viewport.value.width <= compactBreakpoint) {
      hasCustomPosition.value = false;
      return;
    }
    if (hasCustomPosition.value) {
      panelPosition.value = clampPanelPosition(panelPosition.value.x, panelPosition.value.y);
    }
  }

  async function togglePanel() {
    panelVisible.value = !panelVisible.value;
    if (panelVisible.value) {
      await nextTick();
      await ensurePanelPosition();
    }
  }

  function closePanel() {
    panelVisible.value = false;
    fullscreenVisible.value = false;
    isDragging.value = false;
    dragState.pointerId = -1;
    cleanupDragListeners();
  }

  async function toggleFullscreen() {
    fullscreenVisible.value = !fullscreenVisible.value;
    await nextTick();
    await ensurePanelPosition();
  }

  function cleanupDragListeners() {
    window.removeEventListener('pointermove', handleDragMove);
    window.removeEventListener('pointerup', handleDragEnd);
    window.removeEventListener('pointercancel', handleDragEnd);
  }

  function handleDragStart(event: PointerEvent) {
    if (fullscreenVisible.value || isCompactLayout.value || event.button !== 0) return;
    const target = event.target as HTMLElement | null;
    if (target?.closest('.assistant-panel__action')) return;
    const panelEl = panelRef.value;
    if (!panelEl) return;
    const rect = panelEl.getBoundingClientRect();
    dragState.pointerId = event.pointerId;
    dragState.offsetX = event.clientX - rect.left;
    dragState.offsetY = event.clientY - rect.top;
    isDragging.value = true;
    hasCustomPosition.value = true;
    window.addEventListener('pointermove', handleDragMove);
    window.addEventListener('pointerup', handleDragEnd);
    window.addEventListener('pointercancel', handleDragEnd);
  }

  function handleDragMove(event: PointerEvent) {
    if (event.pointerId !== dragState.pointerId) return;
    panelPosition.value = clampPanelPosition(
      event.clientX - dragState.offsetX,
      event.clientY - dragState.offsetY,
    );
  }

  function handleDragEnd(event: PointerEvent) {
    if (event.pointerId !== dragState.pointerId) return;
    dragState.pointerId = -1;
    isDragging.value = false;
    cleanupDragListeners();
  }

  watch(panelVisible, (visible) => {
    if (!visible) {
      fullscreenVisible.value = false;
      hasCustomPosition.value = false;
    }
  });

  watch(fullscreenVisible, () => {
    window.setTimeout(() => { void ensurePanelPosition(); }, 0);
  });

  if (typeof window !== 'undefined') {
    syncViewport();
    window.addEventListener('resize', syncPanelPosition);
  }

  onBeforeUnmount(() => {
    cleanupDragListeners();
    if (typeof window !== 'undefined') {
      window.removeEventListener('resize', syncPanelPosition);
    }
  });
</script>

<style lang="less" scoped>
  .nlq-assistant-entry {
    display: flex;
    align-items: center;
  }

  .assistant-trigger {
    width: 34px;
    height: 34px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 0;
    border: 0;
    background: transparent;
    cursor: pointer;
    transition: transform 0.18s ease, opacity 0.18s ease;

    &:hover { transform: translateY(-1px); opacity: 0.92; }

    &:focus-visible {
      outline: 2px solid rgba(22, 119, 255, 0.25);
      outline-offset: 3px;
      border-radius: 8px;
    }
  }

  .assistant-trigger__icon {
    width: 24px;
    height: 24px;
    display: block;
    object-fit: contain;
  }

  .nlq-assistant-layer {
    position: fixed;
    inset: 0;
    z-index: 1300;
    pointer-events: none;
  }

  .assistant-panel {
    position: absolute;
    top: 76px;            /* layout header (60px) + 16px gap */
    right: 24px;
    bottom: 16px;
    width: clamp(440px, 38vw, 720px);
    display: flex;
    flex-direction: column;
    overflow: hidden;
    pointer-events: auto;
    background: rgba(255, 255, 255, 0.96);
    border: 1px solid rgba(148, 163, 184, 0.22);
    border-radius: 18px;
    box-shadow: 0 28px 80px rgba(15, 23, 42, 0.16);
    backdrop-filter: blur(10px);
  }

  .assistant-panel.is-dragging {
    user-select: none;
    box-shadow: 0 34px 90px rgba(15, 23, 42, 0.2);
  }

  .assistant-panel.is-fullscreen {
    inset: 12px;
    width: auto;
    height: auto;
    min-height: 0;
    transform: none;
    border-radius: 20px;
  }

  .assistant-panel__header {
    height: 56px;
    padding: 0 10px 0 18px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    border-bottom: 1px solid rgba(15, 23, 42, 0.08);
    background: linear-gradient(180deg, rgba(248, 250, 252, 0.98) 0%, rgba(255, 255, 255, 0.94) 100%);
    cursor: move;
  }

  .assistant-panel__body {
    flex: 1;
    min-height: 0;
    display: flex;
    background: rgba(255, 255, 255, 0.98);
  }

  .assistant-panel__body > :deep(.chat-assistant-panel) {
    flex: 1;
    min-width: 0;
  }

  .assistant-panel__title {
    display: flex;
    align-items: center;
    gap: 10px;
    color: #1f2937;
    font-size: 15px;
    font-weight: 600;
  }

  .assistant-panel__logo { width: 20px; height: 20px; display: block; }

  .assistant-panel__actions { display: flex; align-items: center; gap: 4px; }

  .assistant-panel__action {
    width: 32px;
    height: 32px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 0;
    border: 0;
    border-radius: 8px;
    background: transparent;
    color: #475569;
    cursor: pointer;
    transition: background-color 0.18s ease, color 0.18s ease;

    &:hover { background: rgba(37, 99, 235, 0.08); color: #2563eb; }
  }

  @media screen and (max-width: 1280px) {
    .assistant-panel { width: min(46vw, 640px); }
  }

  @media screen and (max-width: 960px) {
    .assistant-panel {
      inset: 72px 12px 12px;
      width: auto;
      height: auto;
      min-height: 0;
      transform: none;
      border-radius: 16px;
    }
  }
</style>
