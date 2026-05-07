/**
 * Unit tests for useNlqSession composable.
 *
 * Coverage:
 * - schema_version mismatch wipes state and creates a new session
 * - appendMessage round-trips through localStorage (reload restores)
 * - BroadcastChannel append for the active session updates in-memory
 * - BroadcastChannel append for a different session leaves in-memory alone
 *   while still keeping localStorage as the canonical source
 * - storage event with newer updated_at triggers in-memory reload
 *
 * Notes on environment:
 * happy-dom v14 does not expose a BroadcastChannel constructor on the
 * jsdom-equivalent window, so we install a tiny in-process polyfill on
 * `globalThis` for the duration of these tests. The polyfill mirrors the
 * web spec for the surface area we use (postMessage + onmessage on
 * channels sharing the same name).
 */

import { afterEach, beforeEach, describe, it, expect } from 'vitest';
import {
  useNlqSession,
  STORAGE_KEY,
  ACTIVE_ID_KEY,
  SCHEMA_VERSION,
  type NlqSessionsStorage,
  type NlqSessionMessage,
} from '../useNlqSession';

// ---------------------------------------------------------------------------
// BroadcastChannel polyfill (happy-dom omits it)
// ---------------------------------------------------------------------------

interface FakeChannel {
  name: string;
  onmessage: ((ev: { data: unknown }) => void) | null;
  postMessage: (data: unknown) => void;
  close: () => void;
}

const fakeChannelRegistry = new Map<string, Set<FakeChannel>>();

class FakeBroadcastChannel implements FakeChannel {
  name: string;
  onmessage: ((ev: { data: unknown }) => void) | null = null;
  private closed = false;

  constructor(name: string) {
    this.name = name;
    if (!fakeChannelRegistry.has(name)) {
      fakeChannelRegistry.set(name, new Set());
    }
    fakeChannelRegistry.get(name)!.add(this);
  }

  postMessage(data: unknown): void {
    if (this.closed) return;
    const peers = fakeChannelRegistry.get(this.name);
    if (!peers) return;
    for (const peer of peers) {
      if (peer === this) continue;
      // Mimic async dispatch (microtask) to match real BC semantics
      queueMicrotask(() => {
        if (peer.onmessage) peer.onmessage({ data });
      });
    }
  }

  close(): void {
    this.closed = true;
    fakeChannelRegistry.get(this.name)?.delete(this);
  }
}

// ---------------------------------------------------------------------------
// Test setup / teardown
// ---------------------------------------------------------------------------

let originalBC: typeof globalThis.BroadcastChannel | undefined;

beforeEach(() => {
  // Reset storages
  localStorage.clear();
  sessionStorage.clear();
  fakeChannelRegistry.clear();

  // Install fake BroadcastChannel
  originalBC = (globalThis as { BroadcastChannel?: typeof globalThis.BroadcastChannel })
    .BroadcastChannel;
  (globalThis as { BroadcastChannel?: unknown }).BroadcastChannel = FakeBroadcastChannel as unknown;
});

afterEach(() => {
  if (originalBC) {
    (globalThis as { BroadcastChannel?: unknown }).BroadcastChannel = originalBC;
  } else {
    delete (globalThis as { BroadcastChannel?: unknown }).BroadcastChannel;
  }
  fakeChannelRegistry.clear();
});

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function bootSession() {
  const session = useNlqSession();
  session.__init();
  return session;
}

function readRawStorage(): NlqSessionsStorage | null {
  const raw = localStorage.getItem(STORAGE_KEY);
  if (!raw) return null;
  return JSON.parse(raw) as NlqSessionsStorage;
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('useNlqSession — schema versioning', () => {
  it('wipes state when stored schema_version does not match current', () => {
    // Seed legacy / future schema
    localStorage.setItem(
      STORAGE_KEY,
      JSON.stringify({ schema_version: 999, sessions: [{ id: 'old', messages: [], updated_at: 1 }] }),
    );

    const session = bootSession();

    // Should have created a fresh session (not 'old')
    expect(session.sessionId.value).not.toBe('old');
    expect(session.sessionId.value).toBeTruthy();
    expect(session.messages.value).toEqual([]);

    const stored = readRawStorage();
    expect(stored?.schema_version).toBe(SCHEMA_VERSION);
    expect(stored?.sessions[0].id).toBe(session.sessionId.value);

    session.__dispose();
  });

  it('writes schema_version=1 on fresh init', () => {
    const session = bootSession();
    const stored = readRawStorage();
    expect(stored?.schema_version).toBe(SCHEMA_VERSION);
    expect(stored?.sessions).toHaveLength(1);
    session.__dispose();
  });
});

describe('useNlqSession — persistence round-trip', () => {
  it('appendMessage persists to localStorage and a fresh boot restores it', () => {
    const a = bootSession();
    const userMsg: NlqSessionMessage = { role: 'user', content: '本月合格率？' };
    const aiMsg: NlqSessionMessage = {
      role: 'assistant',
      content: '本月合格率为 95.2%。',
      reasoningSteps: [{ kind: 'spec', label: '检索月度数据' }],
    };
    a.appendMessage(userMsg);
    a.appendMessage(aiMsg);
    const idA = a.sessionId.value;
    a.__dispose();

    // Boot a brand-new session instance — it should load the persisted one.
    const b = bootSession();
    expect(b.sessionId.value).toBe(idA);
    expect(b.messages.value).toHaveLength(2);
    expect(b.messages.value[0]).toMatchObject({ role: 'user', content: '本月合格率？' });
    expect(b.messages.value[1].reasoningSteps?.[0].kind).toBe('spec');
    b.__dispose();
  });

  it('clearAll wipes localStorage + sessionStorage', () => {
    const s = bootSession();
    s.appendMessage({ role: 'user', content: 'hi' });
    s.clearAll();
    expect(localStorage.getItem(STORAGE_KEY)).toBeNull();
    expect(sessionStorage.getItem(ACTIVE_ID_KEY)).toBeNull();
    expect(s.messages.value).toEqual([]);
    s.__dispose();
  });
});

describe('useNlqSession — BroadcastChannel coordination', () => {
  it('inbound broadcast with same session_id appends to in-memory messages', () => {
    const s = bootSession();
    const id = s.sessionId.value;
    const before = s.messages.value.length;

    s.__receiveBroadcast({
      type: 'append',
      session_id: id,
      updated_at: Date.now() + 1000,
      message: { role: 'assistant', content: 'from another tab' },
    });

    expect(s.messages.value).toHaveLength(before + 1);
    expect(s.messages.value[s.messages.value.length - 1].content).toBe('from another tab');
    s.__dispose();
  });

  it('inbound broadcast with different session_id leaves in-memory unchanged', () => {
    const s = bootSession();
    const beforeMessages = s.messages.value.slice();

    s.__receiveBroadcast({
      type: 'append',
      session_id: 'some-other-session-id',
      updated_at: Date.now() + 1000,
      message: { role: 'assistant', content: 'cross-session noise' },
    });

    expect(s.messages.value).toEqual(beforeMessages);
    s.__dispose();
  });

  it('two harnesses on same session see appendMessage broadcast across', async () => {
    const a = bootSession();
    const sharedId = a.sessionId.value;

    // Boot a peer harness — same session id should be picked up via storage.
    const b = bootSession();
    expect(b.sessionId.value).toBe(sharedId);

    a.appendMessage({ role: 'user', content: 'cross-tab hello' });

    // Allow the queued microtask in FakeBroadcastChannel to flush.
    await Promise.resolve();
    await Promise.resolve();

    expect(b.messages.value.some((m) => m.content === 'cross-tab hello')).toBe(true);
    a.__dispose();
    b.__dispose();
  });
});

describe('useNlqSession — storage event compatibility path', () => {
  it('storage event with newer updated_at reloads in-memory messages', () => {
    const s = bootSession();
    const id = s.sessionId.value;

    // Simulate another tab having written a newer state.
    const newer: NlqSessionsStorage = {
      schema_version: SCHEMA_VERSION,
      sessions: [
        {
          id,
          messages: [{ role: 'assistant', content: 'written by other tab' }],
          updated_at: Date.now() + 60_000,
        },
      ],
    };
    const newValue = JSON.stringify(newer);
    localStorage.setItem(STORAGE_KEY, newValue);

    s.__receiveStorageEvent({ key: STORAGE_KEY, newValue });

    expect(s.messages.value).toHaveLength(1);
    expect(s.messages.value[0].content).toBe('written by other tab');
    s.__dispose();
  });

  it('storage event for unrelated key is ignored', () => {
    const s = bootSession();
    const before = s.messages.value.slice();
    s.__receiveStorageEvent({ key: 'unrelated', newValue: 'whatever' });
    expect(s.messages.value).toEqual(before);
    s.__dispose();
  });
});
