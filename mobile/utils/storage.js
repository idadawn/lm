const TOKEN_KEY = 'lm_app_token'
const USER_INFO_KEY = 'lm_app_user_info'
const API_BASE_URL_KEY = 'lm_api_base_url'

// nlq-agent (FastAPI 智能问答服务) 单独的 base URL。
// 默认走主服务器的 /nlq-agent 反向代理路径（如 http://47.105.59.151:8928/nlq-agent）。
// 注意：主服务器地址来自 APP 的"服务器设置"页面（lm_api_base_url），这里只拼接路径后缀。
// 如果客户单独部署，可以通过 setNlqAgentBaseUrl() 覆盖到独立域名。
const NLQ_AGENT_BASE_URL_KEY = 'NLQ_AGENT_API_BASE'
const NLQ_AGENT_PATH_SUFFIX = '/nlq-agent'

export function getToken() {
  try {
    return uni.getStorageSync(TOKEN_KEY) || null
  } catch (e) {
    return null
  }
}

export function setToken(token) {
  if (token) uni.setStorageSync(TOKEN_KEY, token)
  else uni.removeStorageSync(TOKEN_KEY)
}

export function getUserInfo() {
  try {
    return uni.getStorageSync(USER_INFO_KEY) || null
  } catch (e) {
    return null
  }
}

export function setUserInfo(userInfo) {
  if (userInfo != null) uni.setStorageSync(USER_INFO_KEY, userInfo)
  else uni.removeStorageSync(USER_INFO_KEY)
}

export function clearAuth() {
  uni.removeStorageSync(TOKEN_KEY)
  uni.removeStorageSync(USER_INFO_KEY)
}

export function getApiBaseUrl() {
  try {
    return uni.getStorageSync(API_BASE_URL_KEY) || ''
  } catch (e) {
    return ''
  }
}

export function setApiBaseUrl(url) {
  if (url) uni.setStorageSync(API_BASE_URL_KEY, url)
  else uni.removeStorageSync(API_BASE_URL_KEY)
}

/**
 * 获取 nlq-agent 服务地址。
 * 优先读 NLQ_AGENT_API_BASE（用户在"服务器设置"页面显式覆盖时使用）；
 * 没设的话基于主 API base 加 /nlq-agent 路径派生。
 * 例：主 API = http://47.105.59.151:8928 → NLQ Agent = http://47.105.59.151:8928/nlq-agent
 */
export function getNlqAgentBaseUrl() {
  try {
    const explicit = uni.getStorageSync(NLQ_AGENT_BASE_URL_KEY)
    if (explicit) return explicit
  } catch (e) {}
  const apiBase = getApiBaseUrl()
  if (!apiBase) return ''
  return apiBase.replace(/\/$/, '') + NLQ_AGENT_PATH_SUFFIX
}

export function setNlqAgentBaseUrl(url) {
  if (url) uni.setStorageSync(NLQ_AGENT_BASE_URL_KEY, url)
  else uni.removeStorageSync(NLQ_AGENT_BASE_URL_KEY)
}
