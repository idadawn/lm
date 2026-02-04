/**
 * 生产环境运行时配置
 */
window.__PRODUCTION__LM__CONF__ = {
  VITE_GLOB_APP_TITLE: '实验室数据分析系统',
  VITE_GLOB_API_URL: '/api',
  VITE_GLOB_APP_SHORT_NAME: 'lm',
  VITE_GLOB_API_URL_PREFIX: '',
  VITE_GLOB_WEBSOCKET_URL: '/ws',
};
Object.freeze(window.__PRODUCTION__LM__CONF__);
Object.defineProperty(window, "__PRODUCTION__LM__CONF__", {
  configurable: false,
  writable: false,
});
