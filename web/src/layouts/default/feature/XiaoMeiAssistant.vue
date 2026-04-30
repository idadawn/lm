<template>
  <div class="nlq-assistant-entry">
    <button
      type="button"
      class="assistant-trigger"
      :aria-expanded="panelVisible"
      aria-label="打开智能问数"
      @click="openPanel"
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
            <aside class="assistant-panel__sidebar">
              <div class="assistant-sidebar__header">
                <span>历史会话</span>
                <button
                  type="button"
                  class="assistant-sidebar__create"
                  title="新建会话"
                  @click.stop="handleCreateSession"
                >
                  <Icon icon="ant-design:plus-outlined" :size="14" />
                </button>
              </div>

              <div class="assistant-sidebar__list">
                <div
                  v-for="session in sessions"
                  :key="session.id"
                  class="assistant-sidebar__item"
                  :class="{ 'is-active': session.id === activeSessionId }"
                  @click.stop="selectSession(session.id)"
                >
                  <span class="assistant-sidebar__item-title">{{ session.title }}</span>
                  <span class="assistant-sidebar__item-time">{{ formatSessionTime(session.updatedAt) }}</span>
                  <button
                    type="button"
                    class="assistant-sidebar__delete"
                    title="删除会话"
                    @click.stop="removeSession(session.id)"
                  >
                    <Icon icon="ant-design:delete-outlined" :size="12" />
                  </button>
                </div>

                <div v-if="!sessions.length" class="assistant-sidebar__empty">
                  暂无历史会话
                </div>
              </div>
            </aside>

            <iframe
              ref="activeFrameRef"
              class="assistant-panel__iframe"
              :src="iframeSrc"
              frameborder="0"
              @load="postAuthToFrame"
            />
          </div>
        </section>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
  import { computed, nextTick, onBeforeUnmount, ref, watch } from 'vue';
  import dayjs from 'dayjs';
  import { Icon } from '/@/components/Icon';
  import { getToken, getAuthCache } from '/@/utils/auth';
  import { PERMISSIONS_KEY, USER_INFO_KEY } from '/@/enums/cacheEnum';

  interface UserInfo {
    userId?: string;
    userAccount?: string;
    organizeId?: string;
  }

  interface PermissionInfo {
    modelId?: string;
    moduleName?: string;
  }

  interface AssistantSession {
    id: string;
    title: string;
    createdAt: string;
    updatedAt: string;
  }

  const panelVisible = ref(false);
  const fullscreenVisible = ref(false);
  const activeFrameRef = ref<HTMLIFrameElement | null>(null);
  const panelRef = ref<HTMLElement | null>(null);
  const isDragging = ref(false);
  const compactBreakpoint = 960;
  const panelMargin = 12;
  const desktopGap = 24;
  const viewport = ref({ width: 0, height: 0 });
  const panelPosition = ref({ x: 0, y: 0 });
  const hasCustomPosition = ref(false);
  const sessions = ref<AssistantSession[]>([]);
  const activeSessionId = ref('');
  const dragState = {
    pointerId: -1,
    offsetX: 0,
    offsetY: 0,
  };
  const SESSION_STORAGE_KEY = 'LAB_NLQ_ASSISTANT_SESSIONS';
  const ACTIVE_SESSION_STORAGE_KEY = 'LAB_NLQ_ASSISTANT_ACTIVE_SESSION';

  // 智能问数后端：默认指向 nlq-vanna-service 8088 的 demo chat 页面。
  // 可通过 VITE_NLQ_AGENT_API_BASE 环境变量在 .env.development 切换。
  const assistantBaseUrl =
    (import.meta as any).env?.VITE_NLQ_AGENT_API_BASE || 'http://127.0.0.1:8088';

  const iframeSrc = computed(() => {
    const mode = fullscreenVisible.value ? 'fullscreen' : 'float';
    const params = new URLSearchParams({
      embed: '1',
      mode,
    });
    if (activeSessionId.value) {
      params.set('session_id', activeSessionId.value);
    }
    return `${assistantBaseUrl}/chat?${params.toString()}`;
  });
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

  function cleanupDragListeners() {
    window.removeEventListener('pointermove', handleDragMove);
    window.removeEventListener('pointerup', handleDragEnd);
    window.removeEventListener('pointercancel', handleDragEnd);
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

  function setDefaultPanelPosition() {
    const panelEl = panelRef.value;
    if (!panelEl || fullscreenVisible.value || isCompactLayout.value) return;

    const nextPosition = clampPanelPosition(
      viewport.value.width - panelEl.offsetWidth - desktopGap,
      (viewport.value.height - panelEl.offsetHeight) / 2,
    );
    panelPosition.value = nextPosition;
    hasCustomPosition.value = true;
  }

  async function ensurePanelPosition() {
    await nextTick();
    if (!panelVisible.value) return;

    if (fullscreenVisible.value || isCompactLayout.value) {
      hasCustomPosition.value = false;
      return;
    }

    if (!hasCustomPosition.value) {
      setDefaultPanelPosition();
      return;
    }

    panelPosition.value = clampPanelPosition(panelPosition.value.x, panelPosition.value.y);
  }

  function syncPanelPosition() {
    syncViewport();
    if (!panelVisible.value || fullscreenVisible.value) return;

    if (viewport.value.width <= compactBreakpoint) {
      hasCustomPosition.value = false;
      return;
    }

    if (!hasCustomPosition.value) {
      window.setTimeout(() => {
        void ensurePanelPosition();
      }, 0);
      return;
    }

    panelPosition.value = clampPanelPosition(panelPosition.value.x, panelPosition.value.y);
  }

  function syncViewport() {
    if (typeof window === 'undefined') return;
    viewport.value = {
      width: window.innerWidth,
      height: window.innerHeight,
    };
  }

  function sortSessions(list: AssistantSession[]) {
    return [...list].sort((a, b) => dayjs(b.updatedAt).valueOf() - dayjs(a.updatedAt).valueOf());
  }

  function persistSessions() {
    if (typeof window === 'undefined') return;
    window.localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(sessions.value));
    if (activeSessionId.value) {
      window.localStorage.setItem(ACTIVE_SESSION_STORAGE_KEY, activeSessionId.value);
    } else {
      window.localStorage.removeItem(ACTIVE_SESSION_STORAGE_KEY);
    }
  }

  function loadSessions() {
    if (typeof window === 'undefined') return;

    try {
      const rawSessions = window.localStorage.getItem(SESSION_STORAGE_KEY);
      const rawActiveId = window.localStorage.getItem(ACTIVE_SESSION_STORAGE_KEY);
      const parsed = rawSessions ? (JSON.parse(rawSessions) as AssistantSession[]) : [];
      sessions.value = sortSessions(parsed);
      activeSessionId.value = rawActiveId || sessions.value[0]?.id || '';
    } catch {
      sessions.value = [];
      activeSessionId.value = '';
    }
  }

  function buildSessionTitle(index: number) {
    return index === 1 ? '当前会话' : `历史会话 ${index}`;
  }

  function createSession(title?: string) {
    const now = dayjs().toISOString();
    const nextIndex = sessions.value.length + 1;
    const session: AssistantSession = {
      id: `session_${Date.now()}`,
      title: title?.trim() || buildSessionTitle(nextIndex),
      createdAt: now,
      updatedAt: now,
    };

    sessions.value = sortSessions([session, ...sessions.value]);
    activeSessionId.value = session.id;
    persistSessions();
    return session;
  }

  function handleCreateSession() {
    createSession();
  }

  function ensureActiveSession() {
    if (!sessions.value.length) {
      createSession('当前会话');
      return;
    }

    if (!activeSessionId.value || !sessions.value.some(session => session.id === activeSessionId.value)) {
      activeSessionId.value = sessions.value[0].id;
      persistSessions();
    }
  }

  function touchSession(sessionId: string, title?: string) {
    const nextUpdatedAt = dayjs().toISOString();
    sessions.value = sortSessions(
      sessions.value.map(session =>
        session.id === sessionId
          ? {
              ...session,
              title: title?.trim() || session.title,
              updatedAt: nextUpdatedAt,
            }
          : session,
      ),
    );
    persistSessions();
  }

  function selectSession(sessionId: string) {
    if (activeSessionId.value === sessionId) return;
    activeSessionId.value = sessionId;
    touchSession(sessionId);
  }

  function removeSession(sessionId: string) {
    sessions.value = sessions.value.filter(session => session.id !== sessionId);
    if (activeSessionId.value === sessionId) {
      activeSessionId.value = sessions.value[0]?.id || '';
    }
    if (!sessions.value.length) {
      createSession('当前会话');
      return;
    }
    persistSessions();
  }

  function formatSessionTime(value: string) {
    const date = dayjs(value);
    if (date.isSame(dayjs(), 'day')) return date.format('HH:mm');
    if (date.isSame(dayjs().subtract(1, 'day'), 'day')) return '昨天';
    return date.format('MM-DD');
  }

  function buildPermissions(): string[] {
    const permissionList = (getAuthCache(PERMISSIONS_KEY) || []) as PermissionInfo[];
    return permissionList.flatMap(item => [item.moduleName, item.modelId]).filter(Boolean) as string[];
  }

  function buildAuthPayload() {
    const token = getToken();
    const userInfo = (getAuthCache(USER_INFO_KEY) || {}) as UserInfo;

    return {
      access_token: token,
      token_type: 'Bearer',
      user_id: userInfo.userId,
      account: userInfo.userAccount,
      tenant_id: userInfo.organizeId,
      origin: 'embedded',
      permissions: buildPermissions(),
    };
  }

  function postAuthToFrame() {
    const frameWindow = activeFrameRef.value?.contentWindow;
    if (!frameWindow) return;

    frameWindow.postMessage(
      {
        type: 'NLQ_AUTH_CONTEXT',
        payload: buildAuthPayload(),
      },
      assistantBaseUrl,
    );

    frameWindow.postMessage(
      {
        type: 'NLQ_SESSION_CONTEXT',
        payload: {
          session_id: activeSessionId.value,
        },
      },
      assistantBaseUrl,
    );
  }

  async function openPanel() {
    ensureActiveSession();
    panelVisible.value = true;
    await nextTick();
    await ensurePanelPosition();
    window.setTimeout(() => postAuthToFrame(), 300);
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
    window.setTimeout(() => postAuthToFrame(), 150);
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

  function handleFrameMessage(event: MessageEvent) {
    if (event.origin !== assistantBaseUrl) return;
    const payload = event.data as Record<string, any> | undefined;
    if (!payload?.type) return;

    if (payload.type === 'NLQ_SESSION_META') {
      const sessionId = String(payload.payload?.session_id || activeSessionId.value || '');
      if (!sessionId) return;

      if (!sessions.value.some(session => session.id === sessionId)) {
        sessions.value = sortSessions([
          {
            id: sessionId,
            title: payload.payload?.title || `历史会话 ${sessions.value.length + 1}`,
            createdAt: payload.payload?.created_at || dayjs().toISOString(),
            updatedAt: payload.payload?.updated_at || dayjs().toISOString(),
          },
          ...sessions.value,
        ]);
      }

      activeSessionId.value = sessionId;
      touchSession(sessionId, payload.payload?.title);
      return;
    }

    if (payload.type === 'NLQ_CREATE_SESSION') {
      createSession(payload.payload?.title);
      return;
    }
  }

  watch(panelVisible, visible => {
    if (!visible) {
      fullscreenVisible.value = false;
      hasCustomPosition.value = false;
    }
  });

  watch(fullscreenVisible, () => {
    window.setTimeout(() => {
      void ensurePanelPosition();
    }, 0);
  });

  watch(activeSessionId, async () => {
    if (!panelVisible.value) {
      persistSessions();
      return;
    }
    await nextTick();
    postAuthToFrame();
    persistSessions();
  });

  if (typeof window !== 'undefined') {
    loadSessions();
    ensureActiveSession();
    syncViewport();
    window.addEventListener('resize', syncPanelPosition);
    window.addEventListener('message', handleFrameMessage);
  }

  onBeforeUnmount(() => {
    cleanupDragListeners();
    if (typeof window !== 'undefined') {
      window.removeEventListener('resize', syncPanelPosition);
      window.removeEventListener('message', handleFrameMessage);
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

    &:hover {
      transform: translateY(-1px);
      opacity: 0.92;
    }

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
    top: 50%;
    right: 24px;
    width: clamp(440px, 38vw, 720px);
    height: 60vh;
    min-height: 560px;
    transform: translateY(-50%);
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

  .assistant-panel__sidebar {
    width: 220px;
    flex-shrink: 0;
    display: flex;
    flex-direction: column;
    border-right: 1px solid rgba(15, 23, 42, 0.08);
    background: linear-gradient(180deg, #fcfdff 0%, #f8fafc 100%);
  }

  .assistant-sidebar__header {
    height: 52px;
    padding: 0 14px 0 16px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    color: #1f2937;
    font-size: 14px;
    font-weight: 600;
    border-bottom: 1px solid rgba(15, 23, 42, 0.06);
  }

  .assistant-sidebar__create {
    width: 28px;
    height: 28px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 0;
    border: 0;
    border-radius: 8px;
    background: transparent;
    color: #475569;
    cursor: pointer;

    &:hover {
      color: #2563eb;
      background: rgba(37, 99, 235, 0.08);
    }
  }

  .assistant-sidebar__list {
    flex: 1;
    overflow-y: auto;
    padding: 10px 8px 12px;
  }

  .assistant-sidebar__item {
    width: 100%;
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: flex-start;
    gap: 4px;
    margin-bottom: 6px;
    padding: 12px 36px 12px 12px;
    border: 1px solid transparent;
    border-radius: 12px;
    background: transparent;
    color: #334155;
    text-align: left;
    cursor: pointer;
    transition: background-color 0.18s ease, border-color 0.18s ease, box-shadow 0.18s ease;

    &:hover {
      background: rgba(255, 255, 255, 0.9);
      border-color: rgba(59, 130, 246, 0.16);
    }

    &.is-active {
      background: #fff;
      border-color: rgba(59, 130, 246, 0.22);
      box-shadow: 0 8px 22px rgba(37, 99, 235, 0.08);
    }
  }

  .assistant-sidebar__item-title {
    width: 100%;
    color: #0f172a;
    font-size: 13px;
    font-weight: 600;
    line-height: 1.4;
    overflow: hidden;
    white-space: nowrap;
    text-overflow: ellipsis;
  }

  .assistant-sidebar__item-time {
    color: #94a3b8;
    font-size: 12px;
    line-height: 1;
  }

  .assistant-sidebar__delete {
    position: absolute;
    top: 10px;
    right: 8px;
    width: 24px;
    height: 24px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 0;
    border: 0;
    border-radius: 7px;
    background: transparent;
    color: #94a3b8;
    opacity: 0;
    cursor: pointer;
    transition: opacity 0.18s ease, background-color 0.18s ease, color 0.18s ease;

    .assistant-sidebar__item:hover &,
    .assistant-sidebar__item.is-active & {
      opacity: 1;
    }

    &:hover {
      color: #ef4444;
      background: rgba(239, 68, 68, 0.08);
    }
  }

  .assistant-sidebar__empty {
    padding: 24px 12px;
    color: #94a3b8;
    font-size: 13px;
    text-align: center;
  }

  .assistant-panel__title {
    display: flex;
    align-items: center;
    gap: 10px;
    color: #1f2937;
    font-size: 15px;
    font-weight: 600;
  }

  .assistant-panel__logo {
    width: 20px;
    height: 20px;
    display: block;
  }

  .assistant-panel__actions {
    display: flex;
    align-items: center;
    gap: 4px;
  }

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

    &:hover {
      background: rgba(37, 99, 235, 0.08);
      color: #2563eb;
    }
  }

  .assistant-panel__iframe {
    flex: 1;
    width: 100%;
    height: 100%;
    border: 0;
    display: block;
    background: #fff;
  }

  @media screen and (max-width: 1280px) {
    .assistant-panel {
      width: min(46vw, 640px);
    }
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

    .assistant-panel__sidebar {
      width: 180px;
    }
  }
</style>
