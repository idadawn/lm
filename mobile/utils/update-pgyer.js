/**
 * 蒲公英 (Pgyer) 应用更新检测
 * 文档: https://www.pgyer.com/doc/view/api#appUpdate
 *
 * 使用前需：
 * 1. 注册蒲公英账号 https://www.pgyer.com
 * 2. 上传应用并获取 appKey
 * 3. 在个人中心获取 API Key (_api_key)
 * 4. 将 Key 填入 utils/update.js 的 PGYER_CONFIG
 */

const PGYER_CHECK_URL = 'https://www.pgyer.com/apiv2/app/check'

/**
 * 检查蒲公英更新
 * @param {Object} options
 * @param {string} options.currentVersion 当前版本号（如 1.0.0）
 * @param {number} options.currentVersionCode 当前版本代码（如 100）
 * @param {Object} options.config { _api_key, appKey }
 * @param {Function} options.onUpdate 有更新时回调
 * @param {Function} options.onNoUpdate 无更新时回调
 * @param {Function} options.onError 出错时回调
 */
export function checkPgyerUpdate(options) {
  const { currentVersion, currentVersionCode, config, onUpdate, onNoUpdate, onError } = options

  if (!config || !config._api_key || !config.appKey) {
    console.error('[Pgyer] 缺少 _api_key 或 appKey，请检查 utils/update.js 中的 PGYER_CONFIG')
    if (onError) onError(new Error('缺少蒲公英配置'))
    return
  }

  uni.request({
    url: PGYER_CHECK_URL,
    method: 'POST',
    header: {
      'Content-Type': 'application/x-www-form-urlencoded'
    },
    data: {
      _api_key: config._api_key,
      appKey: config.appKey
    },
    success: (res) => {
      const body = res.data
      if (!body || body.code !== 0) {
        console.error('[Pgyer] API 返回异常:', body)
        if (onError) onError(new Error(body?.message || '蒲公英接口异常'))
        return
      }

      const data = body.data
      if (!data) {
        if (onNoUpdate) onNoUpdate()
        return
      }

      const latestVersion = data.buildVersion || ''
      const latestVersionCode = parseInt(data.buildVersionNo || '0')

      // 版本比较：优先比较 versionCode（如果蒲公英上有填写），否则比较 version 字符串
      let hasUpdate = false
      if (latestVersionCode > 0 && currentVersionCode > 0) {
        hasUpdate = latestVersionCode > currentVersionCode
      } else {
        // 如果 versionCode 无效，直接比较 versionName
        hasUpdate = compareVersion(latestVersion, currentVersion) > 0
      }

      if (!hasUpdate) {
        if (onNoUpdate) onNoUpdate()
        return
      }

      // 构建下载链接
      const downloadUrl = data.downloadURL || ''
      const updateLog = data.buildUpdateDescription || data.buildDescription || ''
      // 蒲公英没有 forceUpdate 字段，默认非强制
      const forceUpdate = false

      if (onUpdate) {
        onUpdate({
          latestVersion,
          latestVersionCode,
          downloadUrl,
          forceUpdate,
          updateLog
        })
      }
    },
    fail: (err) => {
      console.error('[Pgyer] 请求失败:', err)
      if (onError) onError(new Error(err?.errMsg || '网络请求失败'))
    }
  })
}

/**
 * 版本号比较（支持 x.y.z 格式）
 * @returns {number} 1: v1>v2, 0: v1==v2, -1: v1<v2
 */
function compareVersion(v1, v2) {
  const a1 = String(v1).split('.').map(Number)
  const a2 = String(v2).split('.').map(Number)
  const len = Math.max(a1.length, a2.length)
  for (let i = 0; i < len; i++) {
    const n1 = a1[i] || 0
    const n2 = a2[i] || 0
    if (n1 > n2) return 1
    if (n1 < n2) return -1
  }
  return 0
}
