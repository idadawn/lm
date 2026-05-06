/**
 * uni-app SSE client backed by uni.request with enableChunked: true.
 *
 * 解析服务器端 `text/event-stream`，按 `data: <json>\n\n` 分包，回调消费方处理
 * 文本与 reasoning_step 事件。设计为不依赖 ai-sdk 或浏览器 EventSource，因为
 * 微信小程序环境不支持原生 EventSource。
 *
 * 用法：
 *   import { streamNlqChat } from '@/utils/sse-client.js'
 *   streamNlqChat({ messages: [...], session_id }, {
 *     onText: chunk => append(chunk),
 *     onReasoningStep: step => steps.push(step),
 *     onResponseMetadata: payload => commit(payload),
 *     onError: err => toast(err.message),
 *     onDone: () => loading = false,
 *   })
 */

function getBaseUrl() {
  // uni.getStorageSync 在主入口存储 NLQ_AGENT_API_BASE，未设置时抛出错误。
  try {
    const stored = uni.getStorageSync('NLQ_AGENT_API_BASE');
    if (stored) return stored;
  } catch (e) {
    // ignore
  }
  throw new Error('NLQ_AGENT_API_BASE 未配置 — 请在 app 启动配置中设置');
}

function parseSseLine(line) {
  if (!line || !line.indexOf) return null;
  if (line.indexOf('data:') !== 0) return null;
  const payload = line.slice(5).trim();
  if (!payload || payload === '[DONE]') return null;
  try {
    return JSON.parse(payload);
  } catch (e) {
    return null;
  }
}

function dispatchEvent(event, handlers) {
  if (!event) return;
  switch (event.type) {
    case 'text':
      if (typeof event.content === 'string' && handlers.onText) {
        handlers.onText(event.content);
      }
      break;
    case 'reasoning_step':
      if (event.reasoning_step && handlers.onReasoningStep) {
        handlers.onReasoningStep(event.reasoning_step);
      }
      break;
    case 'response_metadata':
      if (event.response_payload && handlers.onResponseMetadata) {
        handlers.onResponseMetadata(event.response_payload);
      }
      break;
    case 'error':
      if (handlers.onError) {
        handlers.onError(new Error(event.error || 'unknown error'));
      }
      break;
    case 'done':
      if (handlers.onDone) handlers.onDone();
      break;
    default:
      break;
  }
}

export function streamNlqChat(request, handlers) {
  const handlersSafe = handlers || {};
  let buffer = '';

  const requestTask = uni.request({
    url: `${getBaseUrl()}/api/v1/chat/stream`,
    method: 'POST',
    enableChunked: true,
    timeout: 60000,
    header: {
      'Content-Type': 'application/json',
      'X-Request-Origin': 'embedded'
    },
    data: request,
    success: () => {
      if (handlersSafe.onDone) handlersSafe.onDone();
    },
    fail: (err) => {
      if (handlersSafe.onError) {
        handlersSafe.onError(new Error(err && err.errMsg ? err.errMsg : 'request failed'));
      }
    }
  });

  if (requestTask && requestTask.onChunkReceived) {
    requestTask.onChunkReceived((res) => {
      const data = res && res.data ? res.data : null;
      if (!data) return;
      let text;
      try {
        text = uni.arrayBufferToBase64
          ? new TextDecoder('utf-8').decode(data)
          : String.fromCharCode.apply(null, new Uint8Array(data));
      } catch (e) {
        text = '';
      }
      buffer += text;
      const chunks = buffer.split('\n\n');
      buffer = chunks.pop() || '';
      chunks.forEach((chunk) => {
        const line = chunk
          .split('\n')
          .find((entry) => entry.indexOf('data:') === 0);
        if (!line) return;
        dispatchEvent(parseSseLine(line), handlersSafe);
      });
    });
  }

  return requestTask;
}
