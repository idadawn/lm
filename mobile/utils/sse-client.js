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
      // UTF-8 解码:静态平台分支 + 语义正确的运行时探测。
      // 旧代码用 uni.arrayBufferToBase64 做探测是错误的 — 该 API 与 TextDecoder
      // 是否存在没有因果关系;String.fromCharCode.apply 把每个字节当独立 code unit,
      // 会在不支持 TextDecoder 的旧版微信小程序中把中文输出为乱码。
      let text;
      try {
        // #ifdef MP-WEIXIN
        // 微信小程序:新版有 TextDecoder,旧版没有 — 用 typeof 做语义正确的探测,
        // 不可用时 fallback 到正确处理 multi-byte 的 UTF-8 polyfill。
        if (typeof TextDecoder !== 'undefined') {
          text = new TextDecoder('utf-8').decode(data);
        } else {
          // decodeUtf8Bytes: 正确处理 1/2/3/4 字节 UTF-8 序列及 surrogate pair。
          // 不能用 String.fromCharCode.apply(null, new Uint8Array(data)) —
          // 那只适用于 Latin-1 范围,多字节中文会乱码。
          const bytes = new Uint8Array(data);
          let result = '';
          let i = 0;
          while (i < bytes.length) {
            const b1 = bytes[i++];
            if (b1 < 0x80) {
              result += String.fromCharCode(b1);
              continue;
            }
            let codepoint = 0;
            let extraBytes = 0;
            if ((b1 & 0xe0) === 0xc0) { codepoint = b1 & 0x1f; extraBytes = 1; }
            else if ((b1 & 0xf0) === 0xe0) { codepoint = b1 & 0x0f; extraBytes = 2; }
            else if ((b1 & 0xf8) === 0xf0) { codepoint = b1 & 0x07; extraBytes = 3; }
            else { result += '�'; continue; } // 无效起始字节
            while (extraBytes-- && i < bytes.length) {
              codepoint = (codepoint << 6) | (bytes[i++] & 0x3f);
            }
            if (codepoint > 0xffff) {
              // BMP 以外 → surrogate pair
              const c = codepoint - 0x10000;
              result += String.fromCharCode(0xd800 + (c >> 10), 0xdc00 + (c & 0x3ff));
            } else {
              result += String.fromCharCode(codepoint);
            }
          }
          text = result;
        }
        // #endif
        // #ifdef APP-PLUS || H5
        // App / H5 运行时均内置 TextDecoder,直接使用。
        text = new TextDecoder('utf-8').decode(data);
        // #endif
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
