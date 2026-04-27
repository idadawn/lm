<template>
  <view class="server-page">
    <!-- 当前服务器卡片 -->
    <view class="current-card">
      <text class="current-label">当前服务器</text>
      <text class="current-value">{{ currentUrl }}</text>
      <view class="status-dot" :class="{ online: isOnline }">
        <text class="status-text">{{ isOnline ? '连接正常' : '未检测' }}</text>
      </view>
    </view>

    <!-- 服务器地址输入 -->
    <view class="input-section">
      <view class="input-item">
        <text class="input-icon">&#x1F310;</text>
        <input
          class="input-field"
          type="text"
          v-model="serverUrl"
          placeholder="请输入服务器地址"
          placeholder-class="input-placeholder"
        />
      </view>
    </view>

    <!-- 操作按钮 -->
    <view class="action-section">
      <button class="save-btn" :loading="testing" @click="saveServer">
        {{ testing ? '检测中...' : '保存并切换' }}
      </button>
      <button class="test-btn" @click="testConnection">
        测试连接
      </button>
    </view>

    <!-- 底部提示 -->
    <view class="footer-tips">
      <text class="tips-text">切换服务器后需重新登录</text>
    </view>
  </view>
</template>

<script setup>
import { ref } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import { getApiBaseUrl, setApiBaseUrl, clearAuth } from '@/utils/storage.js'

const currentUrl = ref('')
const serverUrl = ref('')
const isOnline = ref(false)
const testing = ref(false)

function loadCurrent() {
  const url = getApiBaseUrl() || 'http://47.105.59.151:8928'
  currentUrl.value = url
  serverUrl.value = url
}

function validateUrl(url) {
  if (!url) return '请输入服务器地址'
  url = url.trim()
  if (!/^https?:\/\//.test(url)) return '地址必须以 http:// 或 https:// 开头'
  return ''
}

async function testConnection() {
  const err = validateUrl(serverUrl.value)
  if (err) {
    uni.showToast({ title: err, icon: 'none' })
    return
  }

  testing.value = true
  const url = serverUrl.value.trim().replace(/\/$/, '')

  try {
    const res = await uni.request({
      url: url + '/api/oauth/ImageCode/4/0',
      method: 'GET',
      timeout: 10000,
      header: { 'Poxiao-Origin': 'app' }
    })

    if (res.statusCode >= 200 && res.statusCode < 300) {
      isOnline.value = true
      uni.showToast({ title: '连接成功', icon: 'success' })
    } else {
      isOnline.value = false
      uni.showToast({ title: '连接失败: ' + res.statusCode, icon: 'none' })
    }
  } catch (e) {
    isOnline.value = false
    uni.showToast({ title: '无法连接到服务器', icon: 'none' })
  } finally {
    testing.value = false
  }
}

async function saveServer() {
  const err = validateUrl(serverUrl.value)
  if (err) {
    uni.showToast({ title: err, icon: 'none' })
    return
  }

  const url = serverUrl.value.trim().replace(/\/$/, '')
  setApiBaseUrl(url)
  currentUrl.value = url

  uni.showModal({
    title: '切换成功',
    content: '服务器地址已更新为：' + url + '\n\n是否清除登录状态并返回登录页？',
    success: (res) => {
      if (res.confirm) {
        clearAuth()
        uni.reLaunch({ url: '/pages/login/login' })
      } else {
        uni.navigateBack()
      }
    }
  })
}

onShow(() => {
  loadCurrent()
})
</script>

<style lang="scss">
.server-page {
  min-height: 100vh;
  background: #f7f9fc;
  padding: 16px;
  box-sizing: border-box;
}

/* 当前服务器卡片 */
.current-card {
  background: #ffffff;
  border-radius: 16px;
  padding: 24px;
  margin-bottom: 16px;
  display: flex;
  flex-direction: column;
  align-items: center;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.04);
}

.current-label {
  font-size: 13px;
  color: #8c8c8c;
  margin-bottom: 8px;
}

.current-value {
  font-size: 16px;
  font-weight: 600;
  color: #1890ff;
  word-break: break-all;
  text-align: center;
  margin-bottom: 12px;
}

.status-dot {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 4px 12px;
  border-radius: 12px;
  background: #f5f5f5;
}

.status-dot::before {
  content: '';
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: #bfbfbf;
}

.status-dot.online::before {
  background: #52c41a;
}

.status-text {
  font-size: 12px;
  color: #595959;
}

/* 输入区域 */
.input-section {
  background: #ffffff;
  border-radius: 16px;
  padding: 16px 20px;
  margin-bottom: 16px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.04);
}

.input-item {
  display: flex;
  align-items: center;
  gap: 12px;
  border-bottom: 1px solid #f0f0f0;
  padding: 8px 0;
}

.input-icon {
  font-size: 20px;
  color: #1890ff;
  flex-shrink: 0;
}

.input-field {
  flex: 1;
  font-size: 15px;
  color: #262626;
  height: 40px;
}

.input-placeholder {
  color: #bfbfbf;
  font-size: 15px;
}

/* 预设列表 */
/* 操作按钮 */
.action-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 24px;
}

.save-btn {
  width: 100%;
  height: 48px;
  line-height: 48px;
  background: linear-gradient(135deg, #1890ff, #40a9ff);
  color: #ffffff;
  font-size: 16px;
  font-weight: 600;
  border-radius: 8px;
  border: none;
}

.save-btn::after {
  border: none;
}

.save-btn[disabled] {
  opacity: 0.6;
}

.test-btn {
  width: 100%;
  height: 48px;
  line-height: 48px;
  background: #ffffff;
  color: #1890ff;
  font-size: 16px;
  font-weight: 600;
  border-radius: 8px;
  border: 1px solid #1890ff;
}

.test-btn::after {
  border: none;
}

/* 底部提示 */
.footer-tips {
  text-align: center;
  padding-bottom: 24px;
}

.tips-text {
  font-size: 12px;
  color: #bfbfbf;
}
</style>
