/**
 * nlq-agent SSE client wrapper for the host project (lm/web).
 *
 * 连接到 nlq-agent 的 services/agent-api 部署 URL（环境变量
 * VITE_NLQ_AGENT_API_BASE，默认 http://127.0.0.1:18100），消费 SSE 事件流，
 * 把 reasoning_step 事件按到达顺序累积，把 response_metadata 事件中的
 * reasoning_steps（state-first canonical list）作为最终源覆盖。
 */

import type { ReasoningStep } from '/@/types/reasoning-protocol';

export interface NlqAgentChatRequest {
  messages: Array<{ role: 'user' | 'assistant' | 'system'; content: string }>;
  session_id?: string;
  model_name?: string;
}

export interface NlqAgentChatHandlers {
  onText?: (chunk: string) => void;
  onReasoningStep?: (step: ReasoningStep) => void;
  onResponseMetadata?: (payload: Record<string, unknown>) => void;
  onError?: (err: Error) => void;
  onDone?: () => void;
  signal?: AbortSignal;
}

const BASE_URL = import.meta.env?.VITE_NLQ_AGENT_API_BASE || '';

function parseEvent(line: string): Record<string, unknown> | null {
  if (!line.startsWith('data:')) return null;
  const payload = line.slice(5).trim();
  if (!payload || payload === '[DONE]') return null;
  try {
    return JSON.parse(payload) as Record<string, unknown>;
  } catch {
    return null;
  }
}

export async function streamNlqChat(
  request: NlqAgentChatRequest,
  handlers: NlqAgentChatHandlers = {},
): Promise<void> {
  const response = await fetch(`${BASE_URL}/api/v1/chat/stream`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'X-Request-Origin': 'embedded',
    },
    body: JSON.stringify(request),
    signal: handlers.signal,
  });

  if (!response.ok || !response.body) {
    handlers.onError?.(new Error(`upstream ${response.status}`));
    return;
  }

  const reader = response.body.getReader();
  const decoder = new TextDecoder();
  let buffer = '';

  try {
    while (true) {
      const { done, value } = await reader.read();
      if (done) break;
      buffer += decoder.decode(value, { stream: true });
      const chunks = buffer.split('\n\n');
      buffer = chunks.pop() ?? '';

      for (const chunk of chunks) {
        const dataLine = chunk
          .split('\n')
          .find((line) => line.startsWith('data:'));
        if (!dataLine) continue;
        const event = parseEvent(dataLine);
        if (!event) continue;

        const eventType = String(event.type ?? '');
        if (eventType === 'text' && typeof event.content === 'string') {
          handlers.onText?.(event.content);
        } else if (eventType === 'reasoning_step' && event.reasoning_step) {
          handlers.onReasoningStep?.(event.reasoning_step as ReasoningStep);
        } else if (eventType === 'response_metadata' && event.response_payload) {
          handlers.onResponseMetadata?.(
            event.response_payload as Record<string, unknown>,
          );
        } else if (eventType === 'error') {
          handlers.onError?.(new Error(String(event.error ?? 'unknown error')));
        } else if (eventType === 'done') {
          handlers.onDone?.();
        }
      }
    }
  } catch (err) {
    handlers.onError?.(err instanceof Error ? err : new Error(String(err)));
  } finally {
    handlers.onDone?.();
  }
}
