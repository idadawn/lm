import { getApiBaseUrl } from './storage.js'

export const API_BASE_URL = getApiBaseUrl() || 'http://47.105.59.151:8928'

export function getCurrentBaseUrl() {
  return getApiBaseUrl() || API_BASE_URL
}

export function request(options) {
  return new Promise((resolve, reject) => {
    const token = options.needToken !== false ? uni.getStorageSync('lm_app_token') : null

    let url = getCurrentBaseUrl() + options.url
    if (options.params) {
      const qs = Object.entries(options.params)
        .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v != null ? String(v) : '')}`)
        .join('&')
      if (qs) url = url + (url.includes('?') ? '&' : '?') + qs
    }

    const header = {
      Accept: 'application/json',
      'Poxiao-Origin': 'app',
      ...(token ? { Authorization: token } : {}),
      ...(options.headers || {})
    }

    let sendData = options.data
    // uni.request 不会自动将对象序列化为 form-urlencoded，需手动处理
    if (header['Content-Type'] === 'application/x-www-form-urlencoded' && sendData && typeof sendData === 'object') {
      sendData = Object.entries(sendData)
        .map(([k, v]) => `${encodeURIComponent(k)}=${encodeURIComponent(v != null ? String(v) : '')}`)
        .join('&')
    }

    console.log('[HTTP Request]', options.method || 'GET', url)
    console.log('[HTTP Headers]', JSON.stringify(header))
    if (sendData) console.log('[HTTP Body]', typeof sendData === 'string' ? sendData : JSON.stringify(sendData))

    uni.request({
      url,
      method: options.method || 'GET',
      data: sendData,
      header,
      timeout: 30000,
      success: (res) => {
        const { statusCode, data: responseData } = res
        console.log('[HTTP Response]', statusCode, JSON.stringify(responseData))
        if (statusCode >= 200 && statusCode < 300) {
          const code = responseData?.code ?? 200
          if (code === 200 || code === 0) {
            resolve({ code, data: responseData?.data, msg: responseData?.msg || '' })
          } else if (code === 401) {
            uni.removeStorageSync('lm_app_token')
            uni.removeStorageSync('lm_app_user_info')
            uni.showToast({ title: '登录已过期，请重新登录', icon: 'none' })
            setTimeout(() => uni.reLaunch({ url: '/pages/login/login' }), 1500)
            reject(new Error('Unauthorized'))
          } else {
            const msg = responseData?.msg || '请求失败'
            uni.showToast({ title: msg, icon: 'none' })
            reject(new Error(msg))
          }
        } else {
          uni.showToast({ title: `网络请求异常: ${statusCode}`, icon: 'none' })
          reject(new Error('HTTP ' + statusCode))
        }
      },
      fail: (err) => {
        console.error('[HTTP Fail]', err)
        uni.showToast({ title: '网络连接失败', icon: 'none' })
        reject(err)
      }
    })
  })
}

export function get(url, params, needToken = true) {
  return request({ url, method: 'GET', params, needToken })
}

export function post(url, data, needToken = true) {
  return request({ url, method: 'POST', data, needToken })
}
