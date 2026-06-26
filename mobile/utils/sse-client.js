/**
 * uni-app SSE client backed by uni.request with enableChunked: true.
 *
 * 解析服务器端 `text/event-stream`，按 `data: <json>\n\n` 分包，回调消费方处理
 * 文本、工具调用、图表与 reasoning_step 事件。设计为不依赖 ai-sdk 或浏览器 EventSource，因为
 * 微信小程序环境不支持原生 EventSource。
 *
 * 用法：
 *   import { streamNlqChat } from '@/utils/sse-client.js'
 *   const task = streamNlqChat({ messages: [...], session_id }, {
 *     onText: chunk => append(chunk),
 *     onReasoningStep: step => steps.push(step),
 *     onToolStart: tool => showTool(tool),
 *     onToolEnd: tool => updateTool(tool),
 *     onChart: chart => renderChart(chart),
 *     onResponseMetadata: payload => commit(payload),
 *     onError: err => toast(err.message),
 *     onDone: () => loading = false,
 *   })
 *   // 停止生成
 *   task.abort && task.abort()
 */

// 优先读独立的 NLQ_AGENT_API_BASE；没设就基于主 API base 派生（追加 /nlq-agent）。
// 主 API base 来自 APP 的"服务器设置"页面（lm_api_base_url），这里只负责拼路径后缀。
function getBaseUrl() {
  try {
    // 显式覆盖
    const explicit = uni.getStorageSync('NLQ_AGENT_API_BASE');
    if (explicit) return explicit;
    // 派生：主 API base + /nlq-agent（反代路径，跟 nginx 配置对齐）
    const apiBase = uni.getStorageSync('lm_api_base_url');
    if (apiBase) return apiBase.replace(/\/$/, '') + '/nlq-agent';
  } catch (e) {
    // ignore
  }
  throw new Error('NLQ Agent 地址未配置 — 请先在「我的 → 服务器设置」里设置主服务器地址');
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

// 平台无关的 chunk 解码。
// ★ 鸿蒙(APP-HARMONY) 踩坑：旧实现用 `#ifdef APP-PLUS || H5` 网住 TextDecoder，
//   但 APP-PLUS 在鸿蒙不编译（官方：安卓/苹果编译，鸿蒙不编译），导致解码分支被整段剔除、
//   text 变 undefined、`buffer += undefined` 把字面量 "undefined" 灌进缓冲区，
//   SSE 永远解析不出 data: 行 → 回答空白。改为运行时特性检测，新增端无需再维护 #ifdef。
// onChunkReceived 各端返回 ArrayBuffer；个别运行时可能直接给 string，先判类型，
// 避免对 string 调 TextDecoder 抛错。任何异常都退化为 ''（不再污染缓冲区）。
function decodeChunk(data) {
  if (data == null) return '';
  if (typeof data === 'string') return data;
  try {
    if (typeof TextDecoder !== 'undefined') {
      return new TextDecoder('utf-8').decode(data);
    }
  } catch (e) {
    // 落到手动解码兜底
  }
  // 手动 UTF-8 解码：无 TextDecoder 的旧运行时（老版微信小程序 / HBuilderX < 4.75 的鸿蒙）
  try {
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
      else { result += '�'; continue; }
      while (extraBytes-- && i < bytes.length) {
        codepoint = (codepoint << 6) | (bytes[i++] & 0x3f);
      }
      if (codepoint > 0xffff) {
        const c = codepoint - 0x10000;
        result += String.fromCharCode(0xd800 + (c >> 10), 0xdc00 + (c & 0x3ff));
      } else {
        result += String.fromCharCode(codepoint);
      }
    }
    return result;
  } catch (e) {
    return '';
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
    case 'tool_start':
      if (event.tool_input && handlers.onToolStart) {
        handlers.onToolStart(event.tool_input);
      }
      break;
    case 'tool_end':
      if (event.tool_output && handlers.onToolEnd) {
        handlers.onToolEnd(event.tool_output);
      }
      break;
    case 'chart':
      if (event.chart_spec && handlers.onChart) {
        handlers.onChart(event.chart_spec);
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
  let aborted = false;
  let chunkCount = 0;

  const url = `${getBaseUrl()}/api/v1/chat/stream`;
  console.log('[sse] POST', url, '| messages:', (request.messages || []).length);

  // #ifdef H5
  // H5 上 uni.request 的 enableChunked 不工作 → 用原生 fetch + ReadableStream 真流式
  return _streamViaFetch(url, request, handlersSafe);
  // #endif

  // #ifndef H5

  const requestTask = uni.request({
    url,
    method: 'POST',
    enableChunked: true,
    timeout: 120000,
    header: {
      'Content-Type': 'application/json',
      'X-Request-Origin': 'embedded'
    },
    data: request,
    success: (res) => {
      console.log('[sse] success | statusCode:', res && res.statusCode, '| chunks received:', chunkCount, '| body:', res && res.data && typeof res.data === 'string' ? res.data.slice(0, 200) : '(non-string)');
      // 如果服务端把全部 SSE 数据一次性放到 data 里（enableChunked 在某些平台不生效时），
      // 这里兜底解析一遍——保证非流式回落也能跑通。
      if (chunkCount === 0 && res && res.data) {
        let bodyText = '';
        if (typeof res.data === 'string') bodyText = res.data;
        else if (res.data instanceof ArrayBuffer) {
          try { bodyText = new TextDecoder('utf-8').decode(res.data); } catch (_) {}
        } else {
          try { bodyText = JSON.stringify(res.data); } catch (_) {}
        }
        if (bodyText) {
          console.log('[sse] fallback: parsing full body as one batch (chunked disabled on this platform?)');
          bodyText.split('\n\n').forEach((chunk) => {
            const line = chunk.split('\n').find((l) => l.indexOf('data:') === 0);
            if (line) dispatchEvent(parseSseLine(line), handlersSafe);
          });
        }
      }
      if (!aborted && handlersSafe.onDone) handlersSafe.onDone();
    },
    fail: (err) => {
      console.warn('[sse] fail:', err && err.errMsg, err);
      if (aborted) return;
      if (handlersSafe.onError) {
        handlersSafe.onError(new Error(err && err.errMsg ? err.errMsg : 'request failed'));
      }
    }
  });

  if (requestTask && requestTask.onChunkReceived) {
    console.log('[sse] onChunkReceived hooked (enableChunked supported)');
    requestTask.onChunkReceived((res) => {
      if (aborted) return;
      chunkCount++;
      const data = res && res.data ? res.data : null;
      if (!data) {
        console.warn('[sse] empty chunk #' + chunkCount);
        return;
      }
      // 平台无关解码（含鸿蒙 APP-HARMONY）：见上方 decodeChunk 注释。
      buffer += decodeChunk(data);
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

  if (!(requestTask && requestTask.onChunkReceived)) {
    console.warn('[sse] onChunkReceived NOT supported on this platform — will rely on success() fallback parsing the full body');
  }

  // 包装 abort 方法，标记 aborted 状态避免回调泄漏
  const originalAbort = requestTask && requestTask.abort;
  const wrappedTask = {
    ...requestTask,
    abort: () => {
      aborted = true;
      if (originalAbort) originalAbort();
    }
  };

  return wrappedTask;
  // #endif
}

// #ifdef H5
// H5 上用原生 fetch + ReadableStream 真流式（uni.request 的 enableChunked 在 H5 不工作）
function _streamViaFetch(url, request, handlers) {
  console.log('[sse] H5 fetch streaming (ReadableStream)');
  const controller = (typeof AbortController !== 'undefined') ? new AbortController() : null;
  let aborted = false;
  let buffer = '';

  (async () => {
    try {
      const resp = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'X-Request-Origin': 'embedded'
        },
        body: JSON.stringify(request),
        signal: controller ? controller.signal : undefined
      });
      if (!resp.ok) {
        if (!aborted && handlers.onError) {
          handlers.onError(new Error('upstream ' + resp.status));
        }
        return;
      }
      if (!resp.body || !resp.body.getReader) {
        // 极老浏览器不支持 ReadableStream → 整段读一次
        const text = await resp.text();
        text.split('\n\n').forEach((chunk) => {
          const line = chunk.split('\n').find((l) => l.indexOf('data:') === 0);
          if (line) dispatchEvent(parseSseLine(line), handlers);
        });
        if (!aborted && handlers.onDone) handlers.onDone();
        return;
      }
      const reader = resp.body.getReader();
      const decoder = new TextDecoder('utf-8');
      while (true) {
        if (aborted) {
          try { reader.cancel(); } catch (_) {}
          return;
        }
        const { done, value } = await reader.read();
        if (done) break;
        buffer += decoder.decode(value, { stream: true });
        const chunks = buffer.split('\n\n');
        buffer = chunks.pop() || '';
        chunks.forEach((chunk) => {
          const line = chunk.split('\n').find((l) => l.indexOf('data:') === 0);
          if (line) dispatchEvent(parseSseLine(line), handlers);
        });
      }
      if (!aborted && handlers.onDone) handlers.onDone();
    } catch (err) {
      if (aborted) return;
      if (handlers.onError) {
        handlers.onError(err instanceof Error ? err : new Error(String(err)));
      }
    }
  })();

  return {
    abort: () => {
      aborted = true;
      if (controller) {
        try { controller.abort(); } catch (_) {}
      }
    }
  };
}
// #endif
