<script>
import { checkUpdate } from '@/utils/update.js'
import { getToken } from '@/utils/storage.js'

export default {
  onLaunch() {
    console.log('App Launch')
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
