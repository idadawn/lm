import { get } from './http.js'
import { checkPgyerUpdate } from './update-pgyer.js'

// ==================== 更新源配置 ====================
// 支持 'backend'（自建后端，推荐）或 'pgyer'（蒲公英）
// ⚠️ 安全：切勿在客户端内置蒲公英账号级 _api_key —— 它会随 APK 分发、可被反编译提取，
//    进而以该账号身份调用蒲公英 OpenAPI（上传/列举/删除应用）。更新检测统一改走
//    自建后端 /api/app/version，由后端持有蒲公英 Key 代理 app/check。
export const UPDATE_SOURCE = 'backend'

// 已弃用：仅保留结构以兼容旧代码路径。禁止在此填入任何真实 Key（留空即可）。
// 后端代理已接管更新检测，客户端不再需要、也不应携带蒲公英 Key。
export const PGYER_CONFIG = {
  _api_key: '',
  appKey: ''
}

// ==================== 自建后端更新 ====================
const CHECK_URL = '/api/app/version'

// 鸿蒙 Bundle Name（与 manifest.json app-harmony.distribute.bundleName 保持一致）
// 用于构造 AppGallery 深链接：store://appgallery.huawei.com/app/detail?id=<BUNDLE>
const HARMONY_BUNDLE_NAME = 'com.emergen.lm'

export function checkUpdate() {
  // #ifdef APP-PLUS
  // Android / iOS：优先使用 plus.runtime.getProperty 获取准确版本（含 wgt 热更新后版本）
  plus.runtime.getProperty(plus.runtime.appid, (wgtinfo) => {
    const currentVersion = wgtinfo?.version || '1.0.0'
    const currentVersionCode = parseInt(wgtinfo?.versionCode || '100')
    doCheckUpdate(currentVersion, currentVersionCode, false)
  })
  // #endif

  // #ifdef APP-HARMONY
  // 鸿蒙：plus.* 不存在，改用 uni.getAppBaseInfo() 读取版本，更新走 AppGallery 引导
  try {
    const baseInfo = uni.getAppBaseInfo()
    const currentVersion = baseInfo?.appVersion || '1.0.0'
    const currentVersionCode = parseInt(baseInfo?.appVersionCode || '100')
    doCheckUpdate(currentVersion, currentVersionCode, true)
  } catch (e) {
    uni.showToast({ title: '检查更新失败', icon: 'none' })
  }
  // #endif

  // #ifndef APP-PLUS
  // #ifndef APP-HARMONY
  // 非 App 平台（H5 / 小程序）：不支持自动更新（嵌套条件编译在 uni-app 受支持，见 mine.vue 同款用法）
  uni.showToast({ title: '检查更新仅在 APP 中支持', icon: 'none' })
  // #endif
  // #endif
}

function doCheckUpdate(currentVersion, currentVersionCode, isHarmony) {
  // 根据配置选择更新源
  if (UPDATE_SOURCE === 'pgyer' && PGYER_CONFIG._api_key && PGYER_CONFIG.appKey) {
    // 蒲公英不支持 .hap，鸿蒙下不走此分支
    if (!isHarmony) {
      checkUpdateByPgyer(currentVersion, currentVersionCode)
    }
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

    if (isHarmony) {
      // 鸿蒙：pure mode 禁止旁加载，只能引导用户去华为应用市场
      doHarmonyUpdate(latestVersion, content, forceUpdate)
    } else {
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
            // #ifdef APP-PLUS
            plus.runtime.quit()
            // #endif
          }
        }
      })
    }
  }).catch(() => {
    uni.showToast({ title: '检查更新失败', icon: 'none' })
  })
}

// ==================== Android / iOS 下载安装 ====================
// 仅在 APP-PLUS（Android/iOS）下编译，鸿蒙不包含此代码
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

// ==================== 鸿蒙：引导去华为应用市场 ====================
// HarmonyOS NEXT 的 pure mode 默认开启，禁止旁加载 .hap 包。
// 无法使用 plus.downloader + plus.runtime.install，更新必须通过华为应用市场完成。
// 正确做法：版本检测由后端完成，有新版本时弹窗引导用户跳转应用市场更新。
function doHarmonyUpdate(latestVersion, content, forceUpdate) {
  // #ifdef APP-HARMONY
  uni.showModal({
    title: '发现新版本',
    content: content + '\n\n请前往华为应用市场（AppGallery）搜索「检测室数据分析」下载更新。',
    showCancel: !forceUpdate,
    confirmText: '去应用市场',
    cancelText: '稍后再说',
    success: (modalRes) => {
      if (modalRes.confirm) {
        // 尽力跳转华为应用市场详情页：store://appgallery.huawei.com/app/detail?id=<bundleName>
        // uni-app 鸿蒙端暂无标准外跳 API（plus.runtime.openURL 不可用）。uni.openURL 若由 UTS
        // 插件提供则尝试跳转；否则上方弹窗文案已告知用户手动前往，不用会报错的 navigateTo 兜底。
        if (typeof uni.openURL === 'function') {
          try { uni.openURL({ url: `store://appgallery.huawei.com/app/detail?id=${HARMONY_BUNDLE_NAME}` }) } catch (_) {}
        }
      }
      // forceUpdate 时用户取消：鸿蒙无 plus.runtime.quit()，强制更新场景交由后端接口拦截
      // （返回 403/维护页），客户端不需要也无法强制退出
    }
  })
  // #endif
}

// ==================== 蒲公英更新（仅 Android / iOS）====================
// 蒲公英不支持 HarmonyOS .hap 分发，此函数仅在 APP-PLUS 下调用
function checkUpdateByPgyer(currentVersion, currentVersionCode) {
  // #ifdef APP-PLUS
  checkPgyerUpdate({
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
  // #endif
}
