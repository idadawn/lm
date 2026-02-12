import { useMessage } from '/@/hooks/web/useMessage';
import { useUserStore } from '/@/store/modules/user';
import { useGlobSetting } from '/@/hooks/setting';
import { getToken } from '/@/utils/auth';
import { isDevMode } from '/@/utils/env';

const userStore = useUserStore();
const { createMessage } = useMessage();
const globSetting = useGlobSetting();

// 开发环境：走 Vite 代理 (/dev → .env.development 中 VITE_PROXY 配置的地址，通常为 127.0.0.1:9530)
// 生产环境：通过 webSocketUrl 或 origin 直连
function getWebSocketBaseUrl() {
  const url = isDevMode()
    ? window.location.origin + '/dev/api/message/websocket'
    : globSetting.webSocketUrl
    ? globSetting.webSocketUrl + '/api/message/websocket'
    : window.location.origin + '/api/message/websocket';
  return url.replace('https://', 'wss://').replace('http://', 'ws://');
}

const reconnectRetries = isDevMode() ? 3 : 10;
const reconnectDelay = isDevMode() ? 10000 : 5000;
const heartbeatInterval = 50000;

const listeners = new Map<((data: object) => void), null>();
let ws: WebSocket | null = null;
let currentToken = '';
let reconnectTimer: ReturnType<typeof setTimeout> | null = null;
let heartbeatTimer: ReturnType<typeof setInterval> | null = null;
let retryCount = 0;

function buildWsUrl(token: string) {
  const base = getWebSocketBaseUrl();
  const tokenParam = typeof token === 'string' && token ? encodeURIComponent(token) : '';
  return tokenParam ? `${base}?token=${tokenParam}` : base;
}

function clearTimers() {
  if (reconnectTimer) {
    clearTimeout(reconnectTimer);
    reconnectTimer = null;
  }
  if (heartbeatTimer) {
    clearInterval(heartbeatTimer);
    heartbeatTimer = null;
  }
}

function sendHeartbeat() {
  if (ws?.readyState === WebSocket.OPEN) {
    try {
      ws.send(JSON.stringify({ method: 'heartCheck', token: currentToken }));
    } catch (_) {}
  }
}

function handleOpen() {
  retryCount = 0;
  clearTimers();
  try {
    ws?.send(JSON.stringify({ method: 'OnConnection', token: currentToken, mobileDevice: false }));
  } catch (_) {}
  heartbeatTimer = setInterval(sendHeartbeat, heartbeatInterval);
}

function handleMessage(event: MessageEvent) {
  if (!event.data) return;
  try {
    const data = JSON.parse(event.data as string);
    for (const callback of listeners.keys()) {
      try {
        callback(data);
      } catch (err) {
        console.error(err);
      }
    }
    switch (data.method) {
      case 'refresh':
        location.reload();
        break;
      case 'closeSocket':
        if (ws) {
          ws.close();
          ws = null;
        }
        clearTimers();
        break;
      case 'logout':
        if (data.token && data.token !== currentToken) return location.reload();
        if (ws) {
          ws.close();
          ws = null;
        }
        clearTimers();
        createMessage.error(data.msg || '登录过期,请重新登录').then(() => {
          userStore.resetToken();
        });
        break;
      default:
        break;
    }
  } catch (err) {
    console.error('[WebSocket] data解析失败：', err);
  }
}

function connect() {
  const url = buildWsUrl(currentToken);
  try {
    const socket = new WebSocket(url);
    ws = socket;
    socket.onopen = handleOpen;
    socket.onerror = () => {};
    socket.onclose = () => {
      clearTimers();
      if (retryCount < reconnectRetries) {
        retryCount += 1;
        reconnectTimer = setTimeout(connect, reconnectDelay);
      } else {
        createMessage.error('WebSocket连接失败，请联系管理员');
      }
    };
    socket.onmessage = handleMessage;
  } catch (_) {
    if (retryCount < reconnectRetries) {
      retryCount += 1;
      reconnectTimer = setTimeout(connect, reconnectDelay);
    } else {
      createMessage.error('WebSocket连接失败，请联系管理员');
    }
  }
}

/**
 * 消息 WebSocket（原生实现，避免 VueUse 与 Ant Design Vue eagerComputed 的响应式循环导致栈溢出）
 */
export function useWebSocket() {
  currentToken = getToken();

  function initWebSocket() {
    clearTimers();
    if (ws) {
      ws.close();
      ws = null;
    }
    retryCount = 0;
    connect();
  }

  function onWebSocket(callback: (data: object) => void) {
    if (typeof callback === 'function' && !listeners.has(callback)) {
      listeners.set(callback, null);
    }
  }

  function offWebSocket(callback: (data: object) => void) {
    listeners.delete(callback);
  }

  function getWebSocket() {
    return ws;
  }

  function sendWsMsg(msg: string) {
    try {
      const msgObj = JSON.parse(msg);
      msgObj.token = currentToken;
      ws?.send(JSON.stringify(msgObj));
    } catch (_) {}
  }

  function closeWebSocket() {
    clearTimers();
    if (ws) {
      ws.close();
      ws = null;
    }
  }

  return {
    initWebSocket,
    sendWsMsg,
    onWebSocket,
    offWebSocket,
    getWebSocket,
    closeWebSocket,
  };
}
