/**
 * Contract test for nlqAgent.ts SSE parser.
 *
 * Feeds the shared golden-trace fixture through the real streamNlqChat
 * implementation to lock down callback dispatch against sse-golden-trace.ndjson.
 */

import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { streamNlqChat } from '../nlqAgent';

// ---------------------------------------------------------------------------
// Load fixture
// ---------------------------------------------------------------------------

const FIXTURE_PATH = resolve(__dirname, '../../../tests/fixtures/sse-golden-trace.ndjson');

function fixtureBytes(): Uint8Array {
  const content = readFileSync(FIXTURE_PATH);
  return new Uint8Array(content.buffer, content.byteOffset, content.byteLength);
}

// ---------------------------------------------------------------------------
// fetch mock helpers
// ---------------------------------------------------------------------------

function makeMockFetch(chunks: Uint8Array[]): typeof fetch {
  let chunkIndex = 0;
  const reader = {
    read: vi.fn(async () => {
      if (chunkIndex >= chunks.length) {
        return { done: true, value: undefined };
      }
      return { done: false, value: chunks[chunkIndex++] };
    }),
    cancel: vi.fn(),
    releaseLock: vi.fn(),
  };

  return vi.fn().mockResolvedValue({
    ok: true,
    status: 200,
    body: { getReader: () => reader },
  });
}

// ---------------------------------------------------------------------------
// Helper: run streamNlqChat and collect all callback invocations
// ---------------------------------------------------------------------------

interface CollectedEvents {
  texts: string[];
  reasoningSteps: Array<Record<string, unknown>>;
  responseMetadataPayloads: Array<Record<string, unknown>>;
  errors: Error[];
  doneCount: number;
}

async function runWithFixture(chunks: Uint8Array[]): Promise<CollectedEvents> {
  const mockFetch = makeMockFetch(chunks);
  vi.stubGlobal('fetch', mockFetch);

  const collected: CollectedEvents = {
    texts: [],
    reasoningSteps: [],
    responseMetadataPayloads: [],
    errors: [],
    doneCount: 0,
  };

  await streamNlqChat(
    { messages: [{ role: 'user', content: 'test' }] },
    {
      onText: (chunk) => collected.texts.push(chunk),
      onReasoningStep: (step) => collected.reasoningSteps.push(step as unknown as Record<string, unknown>),
      onResponseMetadata: (payload) => collected.responseMetadataPayloads.push(payload),
      onError: (err) => collected.errors.push(err),
      onDone: () => { collected.doneCount += 1; },
    },
  );

  return collected;
}

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('nlqAgent SSE contract — golden-trace fixture', () => {
  beforeEach(() => {
    vi.unstubAllGlobals();
  });

  it('dispatches onReasoningStep twice when fixture delivered as single chunk', async () => {
    const bytes = fixtureBytes();
    const collected = await runWithFixture([bytes]);

    expect(collected.reasoningSteps).toHaveLength(2);
  });

  it('first reasoning_step has kind="spec"', async () => {
    const bytes = fixtureBytes();
    const collected = await runWithFixture([bytes]);

    expect(collected.reasoningSteps[0]).toMatchObject({ kind: 'spec' });
  });

  it('second reasoning_step has kind="rule"', async () => {
    const bytes = fixtureBytes();
    const collected = await runWithFixture([bytes]);

    expect(collected.reasoningSteps[1]).toMatchObject({ kind: 'rule' });
  });

  it('dispatches onText once with valid UTF-8 Chinese content', async () => {
    const bytes = fixtureBytes();
    const collected = await runWithFixture([bytes]);

    expect(collected.texts).toHaveLength(1);
    // Verify Chinese characters are intact — not garbled by String.fromCharCode
    expect(collected.texts[0]).toContain('检测结果符合标准');
    expect(collected.texts[0]).toContain('中文内容验证');
    expect(collected.texts[0]).toContain('UTF-8');
  });

  it('dispatches onResponseMetadata once with sql field', async () => {
    const bytes = fixtureBytes();
    const collected = await runWithFixture([bytes]);

    expect(collected.responseMetadataPayloads).toHaveLength(1);
    expect(collected.responseMetadataPayloads[0]).toHaveProperty('sql');
    expect(typeof collected.responseMetadataPayloads[0].sql).toBe('string');
    expect(collected.responseMetadataPayloads[0].sql).toContain('SELECT');
  });

  it('dispatches onDone at least once', async () => {
    const bytes = fixtureBytes();
    const collected = await runWithFixture([bytes]);

    // streamNlqChat calls onDone from the 'done' event AND from the finally block
    expect(collected.doneCount).toBeGreaterThanOrEqual(1);
  });

  it('does not dispatch onError for a valid fixture', async () => {
    const bytes = fixtureBytes();
    const collected = await runWithFixture([bytes]);

    expect(collected.errors).toHaveLength(0);
  });

  it('produces identical callback sequence when fixture split across two chunks', async () => {
    const bytes = fixtureBytes();
    // Split at midpoint so SSE event boundaries may span chunks
    const mid = Math.floor(bytes.length / 2);
    const chunk1 = bytes.slice(0, mid);
    const chunk2 = bytes.slice(mid);

    const collectedSingle = await runWithFixture([bytes]);
    vi.unstubAllGlobals();
    const collectedSplit = await runWithFixture([chunk1, chunk2]);

    expect(collectedSplit.reasoningSteps).toHaveLength(collectedSingle.reasoningSteps.length);
    expect(collectedSplit.texts).toHaveLength(collectedSingle.texts.length);
    expect(collectedSplit.texts[0]).toBe(collectedSingle.texts[0]);
    expect(collectedSplit.responseMetadataPayloads).toHaveLength(
      collectedSingle.responseMetadataPayloads.length,
    );
    expect(collectedSplit.errors).toHaveLength(0);
  });
});
