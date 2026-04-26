import { get } from './http.js'

// ==================== 更新源配置 ====================
// 支持 'backend'（自建后端）或 'pgyer'（蒲公英）
export const UPDATE_SOURCE = 'backend'

// 蒲公英配置（仅在 UPDATE_SOURCE = 'pgyer' 时生效）
// 请前往 https://www.pgyer.com 注册并获取以下 Key
export const PGYER_CONFIG = {
  _api_key: '',   // 蒲公英 API Key
  appKey: ''      // 应用 Key
}

// ==================== 自建后端更新 ====================
const CHECK_URL = '/api/app/version'

export function checkUpdate() {
  // #ifdef APP-PLUS
  const systemInfo = uni.getAppBaseInfo()
  const currentVersion = systemInfo.appVersion || '1.0.0'
  const currentVersionCode = parseInt(systemInfo.appVersionCode || '100')

  // 根据配置选择更新源
  if (UPDATE_SOURCE === 'pgyer' && PGYER_CONFIG._api_key && PGYER_CONFIG.appKey) {
    checkUpdateByPgyer(currentVersion, currentVersionCode)
    return
  }

  // 默认使用自建后端
  get(CHECK_URL, {
    platform: uni.getSystemInfoSync().platform,
    version: currentVersion,
    versionCode: currentVersionCode
  }, false).then((res) => {
    const data = res.data
    if (!data || !data.hasUpdate) {
      uni.showToast({ title: '当前已是最新版本', icon: 'none' })
      return
    }

    const { latestVersion, downloadUrl, forceUpdate, updateLog } = data
    const content = `最新版本: ${latestVersion}\n${updateLog || '发现新版本，建议立即更新。'}`

    uni.showModal({
      title: '发现新版本',
      content,
      showCancel: !forceUpdate,
      confirmText: '立即更新',
      cancelText: '稍后更新',
      success: (modalRes) => {
        if (modalRes.confirm) {
          doUpdate(downloadUrl)
        } else if (forceUpdate) {
          plus.runtime.quit()
        }
      }
    })
  }).catch(() => {
    uni.showToast({ title: '检查更新失败', icon: 'none' })
  })
  // #endif
}

function doUpdate(downloadUrl) {
  if (!downloadUrl) {
    uni.showToast({ title: '下载地址为空', icon: 'none' })
    return
  }
  // #ifdef APP-PLUS
  uni.showLoading({ title: '下载中...', mask: true })
  const dtask = plus.downloader.createDownload(downloadUrl, {}, (d, status) => {
    uni.hideLoading()
    if (status === 200) {
      const filePath = d.filename
      if (filePath) {
        plus.runtime.install(filePath, { force: false }, () => {
          uni.showModal({
            title: '安装完成',
            content: '新版本已安装，是否立即重启？',
            showCancel: false,
            confirmText: '立即重启',
            success: () => plus.runtime.restart()
          })
        }, (e) => {
          uni.showToast({ title: '安装失败: ' + e.message, icon: 'none' })
        })
      }
    } else {
      uni.showToast({ title: '下载失败', icon: 'none' })
    }
  })
  dtask.start()
  // #endif
}

// ==================== 蒲公英更新 ====================
function checkUpdateByPgyer(currentVersion, currentVersionCode) {
  // #ifdef APP-PLUS
  import('./update-pgyer.js').then((mod) => {
    mod.checkPgyerUpdate({
      currentVersion,
      currentVersionCode,
      config: PGYER_CONFIG,
      onUpdate: (info) => {
        const { latestVersion, downloadUrl, forceUpdate, updateLog } = info
        const content = `最新版本: ${latestVersion}\n${updateLog || '发现新版本，建议立即更新。'}`
        uni.showModal({
          title: '发现新版本',
          content,
          showCancel: !forceUpdate,
          confirmText: '立即更新',
          cancelText: '稍后更新',
          success: (modalRes) => {
            if (modalRes.confirm) {
              doUpdate(downloadUrl)
            } else if (forceUpdate) {
              plus.runtime.quit()
            }
          }
        })
      },
      onNoUpdate: () => {
        uni.showToast({ title: '当前已是最新版本', icon: 'none' })
      },
      onError: () => {
        uni.showToast({ title: '检查更新失败', icon: 'none' })
      }
    })
  })
  // #endif
}
