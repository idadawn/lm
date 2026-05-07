/**
 * useNlqSession — NLQ chat session 持久化 + 多 tab 协调 composable.
 *
 * Responsibilities:
 * - Persist a single rolling chat session to localStorage under
 *   `nlq-sessions` (schema_version=1) and surface a reactive
 *   `messages` ref for ChatAssistant.vue to render.
 * - Per-tab active session id is stored in sessionStorage under
 *   `nlq-active-id` so each tab pins its own current view.
 * - Multi-tab coordination uses BroadcastChannel('nlq-session') as the
 *   primary fast-path and `storage` events as the compatibility path
 *   for browsers without BroadcastChannel. Conflict arbitration:
 *     * received message with session_id == active id ⇒ append in-memory
 *     * received message with session_id != active id ⇒ update storage
 *       index only, leave in-memory untouched (avoid cross-session bleed)
 *     * local updated_at < received updated_at ⇒ reload from storage
 *       (last-write-wins on the per-session timeline).
 *
 * No new state-management library is introduced (per plan v3 W1.5):
 * vanilla Vue refs + browser primitives only.
 */

import { ref, onMounted, onBeforeUnmount, getCurrentInstance } from 'vue';
import type { Ref } from 'vue';
import type { ReasoningStep } from '/@/types/reasoning-protocol';

// ---------------------------------------------------------------------------
// Storage keys & schema constants
// ---------------------------------------------------------------------------

export const STORAGE_KEY = 'nlq-sessions';
export const ACTIVE_ID_KEY = 'nlq-active-id';
export const CHANNEL_NAME = 'nlq-session';
export const SCHEMA_VERSION = 1;

// ---------------------------------------------------------------------------
// Types
// ---------------------------------------------------------------------------

export interface NlqSessionMessage {
  role: 'user' | 'assistant';
  content: string;
  reasoningSteps?: ReasoningStep[];
}

export interface NlqSessionEntry {
  id: string;
  messages: NlqSessionMessage[];
  updated_at: number;
}

export interface NlqSessionsStorage {
  schema_version: 1;
  sessions: NlqSessionEntry[];
}

export interface NlqBroadcastPayload {
  type: 'append' | 'reset';
  session_id: string;
  updated_at: number;
  message?: NlqSessionMessage;
}

// ---------------------------------------------------------------------------
// Pure helpers (storage-only, no Vue, no lifecycle)
// ---------------------------------------------------------------------------

function generateSessionId(): string {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return `nlq-${Date.now()}-${Math.random().toString(36).slice(2, 10)}`;
}

function safeParseStorage(raw: string | null): NlqSessionsStorage | null {
  if (!raw) return null;
  try {
    const parsed = JSON.parse(raw) as Partial<NlqSessionsStorage>;
    if (!parsed || typeof parsed !== 'object') return null;
    if (parsed.schema_version !== SCHEMA_VERSION) return null;
    if (!Array.isArray(parsed.sessions)) return null;
    return parsed as NlqSessionsStorage;
  } catch {
    return null;
  }
}

function readStorage(): NlqSessionsStorage {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    const parsed = safeParseStorage(raw);
    if (parsed) return parsed;
  } catch {
    /* localStorage unavailable */
  }
  return { schema_version: SCHEMA_VERSION, sessions: [] };
}

function writeStorage(storage: NlqSessionsStorage): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(storage));
  } catch {
    /* quota / unavailable — silently degrade */
  }
}

function readActiveId(): string | null {
  try {
    return sessionStorage.getItem(ACTIVE_ID_KEY);
  } catch {
    return null;
  }
}

function writeActiveId(id: string): void {
  try {
    sessionStorage.setItem(ACTIVE_ID_KEY, id);
  } catch {
    /* ignore */
  }
}

function pickMostRecent(storage: NlqSessionsStorage): NlqSessionEntry | null {
  if (!storage.sessions.length) return null;
  let latest = storage.sessions[0];
  for (const s of storage.sessions) {
    if (s.updated_at > latest.updated_at) latest = s;
  }
  return latest;
}

function makeStorage(entry: NlqSessionEntry): NlqSessionsStorage {
  // MVP: only keep the most recent 1 session.
  return { schema_version: SCHEMA_VERSION, sessions: [entry] };
}

// ---------------------------------------------------------------------------
// Composable
// ---------------------------------------------------------------------------

export interface UseNlqSessionReturn {
  messages: Ref<NlqSessionMessage[]>;
  sessionId: Ref<string>;
  appendMessage: (message: NlqSessionMessage) => void;
  /** Replace the last message in-place (used for streaming assistant updates). */
  updateLastMessage: (patch: Partial<NlqSessionMessage>) => void;
  startNewSession: () => string;
  clearAll: () => void;
  reload: () => void;
  /** Manually trigger a broadcast for the current message tail. */
  broadcastTail: () => void;
  /** Test-only: simulate an inbound BroadcastChannel message. */
  __receiveBroadcast: (payload: NlqBroadcastPayload) => void;
  /** Test-only: simulate an inbound storage event. */
  __receiveStorageEvent: (event: { key: string | null; newValue: string | null }) => void;
  /** Test-only: dispose listeners (idempotent). */
  __dispose: () => void;
  /** Test-only: initialize without a Vue mount. */
  __init: () => void;
}

export function useNlqSession(): UseNlqSessionReturn {
  const messages = ref<NlqSessionMessage[]>([]);
  const sessionId = ref<string>('');
  let lastUpdatedAt = 0;
  let channel: BroadcastChannel | null = null;
  let storageListener: ((ev: StorageEvent) => void) | null = null;
  let unloadListener: (() => void) | null = null;

  function loadFromStorage(): void {
    const storage = readStorage();
    let activeId = readActiveId();

    let entry: NlqSessionEntry | null = null;
    if (activeId) {
      entry = storage.sessions.find((s) => s.id === activeId) ?? null;
    }
    if (!entry) {
      entry = pickMostRecent(storage);
    }
    if (!entry) {
      activeId = generateSessionId();
      entry = { id: activeId, messages: [], updated_at: Date.now() };
      writeStorage(makeStorage(entry));
    }

    sessionId.value = entry.id;
    messages.value = entry.messages.slice();
    lastUpdatedAt = entry.updated_at;
    writeActiveId(entry.id);
  }

  function persistCurrent(): number {
    // Ensure monotonic timestamps within a single tab so two writes in the
    // same millisecond still satisfy the strict-greater arbitration rule
    // on the receiver side (last-write-wins).
    const now = Math.max(Date.now(), lastUpdatedAt + 1);
    lastUpdatedAt = now;
    writeStorage(
      makeStorage({
        id: sessionId.value,
        messages: messages.value.slice(),
        updated_at: now,
      }),
    );
    return now;
  }

  function broadcast(payload: NlqBroadcastPayload): void {
    if (!channel) return;
    try {
      channel.postMessage(payload);
    } catch {
      /* channel closed mid-flight */
    }
  }

  function appendMessage(message: NlqSessionMessage): void {
    messages.value.push(message);
    const ts = persistCurrent();
    broadcast({
      type: 'append',
      session_id: sessionId.value,
      updated_at: ts,
      message,
    });
  }

  function updateLastMessage(patch: Partial<NlqSessionMessage>): void {
    const last = messages.value[messages.value.length - 1];
    if (!last) return;
    Object.assign(last, patch);
    persistCurrent();
    // Streaming updates aren't broadcast per-chunk to avoid flooding;
    // callers should invoke `broadcastTail()` once the stream completes.
  }

  function broadcastTail(): void {
    const last = messages.value[messages.value.length - 1];
    if (!last) return;
    broadcast({
      type: 'append',
      session_id: sessionId.value,
      updated_at: lastUpdatedAt,
      message: last,
    });
  }

  function startNewSession(): string {
    const id = generateSessionId();
    sessionId.value = id;
    messages.value = [];
    writeActiveId(id);
    const ts = persistCurrent();
    broadcast({ type: 'reset', session_id: id, updated_at: ts });
    return id;
  }

  function clearAll(): void {
    try { localStorage.removeItem(STORAGE_KEY); } catch { /* ignore */ }
    try { sessionStorage.removeItem(ACTIVE_ID_KEY); } catch { /* ignore */ }
    messages.value = [];
    sessionId.value = '';
    lastUpdatedAt = 0;
  }

  function reload(): void {
    loadFromStorage();
  }

  function handleBroadcast(payload: NlqBroadcastPayload): void {
    if (!payload || typeof payload !== 'object') return;
    // Cross-session message: refresh storage view but don't touch in-memory.
    if (payload.session_id !== sessionId.value) {
      readStorage();
      return;
    }
    if (payload.type === 'reset') {
      loadFromStorage();
      return;
    }
    if (payload.updated_at <= lastUpdatedAt) return;
    if (payload.type === 'append' && payload.message) {
      messages.value.push(payload.message);
      lastUpdatedAt = payload.updated_at;
    } else {
      loadFromStorage();
    }
  }

  function handleStorageEvent(event: { key: string | null; newValue: string | null }): void {
    if (event.key !== STORAGE_KEY) return;
    const parsed = safeParseStorage(event.newValue);
    if (!parsed) return;
    const entry = parsed.sessions.find((s) => s.id === sessionId.value);
    if (!entry) {
      loadFromStorage();
      return;
    }
    if (entry.updated_at > lastUpdatedAt) {
      messages.value = entry.messages.slice();
      lastUpdatedAt = entry.updated_at;
    }
  }

  function init(): void {
    loadFromStorage();

    if (typeof BroadcastChannel !== 'undefined') {
      try {
        channel = new BroadcastChannel(CHANNEL_NAME);
        channel.onmessage = (ev: MessageEvent) => {
          handleBroadcast(ev.data as NlqBroadcastPayload);
        };
      } catch {
        channel = null;
      }
    }

    if (typeof window !== 'undefined') {
      storageListener = (ev: StorageEvent) => handleStorageEvent(ev);
      unloadListener = () => {
        if (sessionId.value) persistCurrent();
      };
      window.addEventListener('storage', storageListener);
      window.addEventListener('beforeunload', unloadListener);
    }
  }

  function dispose(): void {
    if (channel) {
      try { channel.close(); } catch { /* ignore */ }
      channel = null;
    }
    if (typeof window !== 'undefined') {
      if (storageListener) window.removeEventListener('storage', storageListener);
      if (unloadListener) window.removeEventListener('beforeunload', unloadListener);
    }
    storageListener = null;
    unloadListener = null;
  }

  // Auto-wire lifecycle when called inside a Vue setup. Outside of a
  // component (e.g. unit tests), the caller drives `__init` / `__dispose`.
  if (getCurrentInstance()) {
    onMounted(init);
    onBeforeUnmount(dispose);
  }

  return {
    messages,
    sessionId,
    appendMessage,
    updateLastMessage,
    startNewSession,
    clearAll,
    reload,
    broadcastTail,
    __receiveBroadcast: handleBroadcast,
    __receiveStorageEvent: handleStorageEvent,
    __dispose: dispose,
    __init: init,
  };
}
