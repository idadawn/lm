<script>
import { checkUpdate } from '@/utils/update.js'
import { getToken, getApiBaseUrl, setApiBaseUrl } from '@/utils/storage.js'

// 默认服务器地址。第一次启动 / 用户没在「我的 → 服务器设置」改过时使用。
// nlq-agent 通过 /nlq-agent 反向代理挂在主服务器上 → http://.../nlq-agent/api/v1/...
const DEFAULT_API_BASE = 'http://47.105.59.151:8928'

export default {
  onLaunch() {
    console.log('App Launch')

    // 首次启动给一个默认主 API 地址，用户在「我的 → 服务器设置」可覆盖
    if (!getApiBaseUrl()) {
      setApiBaseUrl(DEFAULT_API_BASE)
      console.log('Initialized default API base:', DEFAULT_API_BASE)
    }

    // 注册 FiraCode 等宽字体，给对话气泡里的代码块用。
    // uni.loadFontFace 是 APP/MP 必须的；H5 也走它（内部转 @font-face）。
    // 注意：APP-PLUS / 微信小程序 要求 ttf 必须放在 /static 下，且 url 用绝对路径。
    try {
      uni.loadFontFace({
        family: 'FiraCode',
        source: 'url("/static/fonts/FiraCode-Regular.ttf")',
        scopes: ['webview', 'native'],
        global: true,
        success: () => console.log('[font] FiraCode loaded'),
        fail: (e) => console.warn('[font] FiraCode load failed:', e)
      })
    } catch (e) {
      console.warn('[font] loadFontFace not supported:', e)
    }

    checkUpdate()
    const token = getToken()
    if (token) {
      uni.switchTab({ url: '/pages/index/index' })
    }
  },
  onShow() {
    console.log('App Show')
  },
  onHide() {
    console.log('App Hide')
  }
}
</script>

<style lang="scss">
@import "uni.scss";

page {
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
  background-color: #F7F9FC;
  color: #262626;
}
</style>
