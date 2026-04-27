<template>
  <view class="mine-page">
    <!-- 用户信息卡片 -->
    <view class="user-card" @click="goProfile">
      <view class="user-avatar">
        <image
          v-if="headIconUrl"
          class="avatar-img"
          :src="headIconUrl"
          mode="aspectFill"
        />
        <text v-else class="avatar-text">{{ avatarText }}</text>
      </view>
      <view class="user-info">
        <text class="user-name">{{ userInfo.userName || '未登录' }}</text>
        <text class="user-dept" v-if="userInfo.organizeName">{{ userInfo.organizeName }}</text>
      </view>
      <text class="user-arrow">›</text>
    </view>

    <!-- 功能菜单 -->
    <view class="menu-section">
      <view class="menu-group">
        <view class="menu-item" @click="goProfile">
          <view class="menu-icon" style="background: #e6f7ff; color: #1890ff;">
            <text class="menu-icon-text">资</text>
          </view>
          <text class="menu-label">个人资料</text>
          <text class="menu-arrow">›</text>
        </view>
        <view class="menu-item" @click="goPassword">
          <view class="menu-icon" style="background: #fff2f0; color: #ff4d4f;">
            <text class="menu-icon-text">密</text>
          </view>
          <text class="menu-label">修改密码</text>
          <text class="menu-arrow">›</text>
        </view>
        <view class="menu-item" @click="configApi">
          <view class="menu-icon" style="background: #e6fffb; color: #13c2c2;">
            <text class="menu-icon-text">接</text>
          </view>
          <text class="menu-label">接口配置</text>
          <text class="menu-arrow">›</text>
        </view>
        <view class="menu-item" @click="clearCache">
          <view class="menu-icon" style="background: #f6ffed; color: #52c41a;">
            <text class="menu-icon-text">清</text>
          </view>
          <text class="menu-label">清除缓存</text>
          <text class="menu-arrow">›</text>
        </view>
        <view class="menu-item" @click="checkVersion">
          <view class="menu-icon" style="background: #fff7e6; color: #faad14;">
            <text class="menu-icon-text">更</text>
          </view>
          <text class="menu-label">检查更新</text>
          <text class="menu-extra">v{{ appVersion }}</text>
          <text class="menu-arrow">›</text>
        </view>
      </view>

      <view class="menu-group">
        <view class="menu-item" @click="goAbout">
          <view class="menu-icon" style="background: #f9f0ff; color: #722ed1;">
            <text class="menu-icon-text">关</text>
          </view>
          <text class="menu-label">关于我们</text>
          <text class="menu-arrow">›</text>
        </view>
      </view>
    </view>

    <!-- 退出登录 -->
    <view class="logout-section">
      <button class="logout-btn" @click="handleLogout">退出登录</button>
    </view>
  </view>
</template>

<script setup>
import { ref, computed } from 'vue'
import { onShow } from '@dcloudio/uni-app'
import { getUserInfo, clearAuth } from '@/utils/storage.js'
import { doLogoutApi } from '@/api/user.js'
import { checkUpdate } from '@/utils/update.js'
import { getCurrentBaseUrl } from '@/utils/http.js'

const userInfo = ref({})
const appVersion = ref('1.0.0')

const avatarText = computed(() => {
  const name = userInfo.value.userName || '用'
  return name.substring(0, 1)
})

const headIconUrl = computed(() => {
  const icon = userInfo.value.headIcon || ''
  if (!icon) return ''
  if (icon.startsWith('http')) return icon
  return getCurrentBaseUrl() + icon
})

function loadUserInfo() {
  const info = getUserInfo()
  if (info) {
    userInfo.value = info
  }
  // 获取版本号：APP-PLUS 优先用 plus.runtime.getProperty（支持 wgt 热更新后版本）
  // #ifdef APP-PLUS
  plus.runtime.getProperty(plus.runtime.appid, (wgtinfo) => {
    appVersion.value = wgtinfo?.version || '1.0.0'
  })
  // #endif
  // #ifndef APP-PLUS
  try {
    const baseInfo = uni.getAppBaseInfo ? uni.getAppBaseInfo() : {}
    appVersion.value = baseInfo?.appVersion || '1.0.0'
  } catch (e) {
    appVersion.value = '1.0.0'
  }
  // #endif
}

function goProfile() {
  uni.navigateTo({ url: '/pages/profile/profile' })
}

function goPassword() {
  uni.navigateTo({ url: '/pages/password/password' })
}

function configApi() {
  uni.navigateTo({ url: '/pages/server/server' })
}

function clearCache() {
  uni.showModal({
    title: '提示',
    content: '确定要清除缓存吗？',
    success: (res) => {
      if (res.confirm) {
        uni.clearStorageSync()
        uni.showToast({ title: '缓存已清除', icon: 'success' })
        setTimeout(() => {
          uni.reLaunch({ url: '/pages/login/login' })
        }, 800)
      }
    }
  })
}

function checkVersion() {
  // #ifdef APP-PLUS
  checkUpdate()
  // #endif
  // #ifndef APP-PLUS
  uni.showToast({ title: '当前版本 v' + appVersion.value, icon: 'none' })
  // #endif
}

function goAbout() {
  uni.showModal({
    title: '关于',
    content: '检测室数据分析系统\n版本：v' + appVersion.value + '\n\n下一代实验室智能分析平台',
    showCancel: false
  })
}

function handleLogout() {
  uni.showModal({
    title: '提示',
    content: '确定要退出登录吗？',
    success: async (res) => {
      if (res.confirm) {
        try {
          await doLogoutApi()
        } catch (e) {}
        clearAuth()
        uni.reLaunch({ url: '/pages/login/login' })
      }
    }
  })
}

onShow(() => {
  loadUserInfo()
})
</script>

<style lang="scss">
.mine-page {
  min-height: 100vh;
  background: #f7f9fc;
  padding-bottom: constant(safe-area-inset-bottom);
  padding-bottom: env(safe-area-inset-bottom);
}

/* 用户信息卡片 */
.user-card {
  background: linear-gradient(135deg, #1890ff, #40a9ff);
  margin: 12px;
  border-radius: 16px;
  padding: 24px;
  display: flex;
  align-items: center;
  gap: 16px;
  box-shadow: 0 4px 16px rgba(24, 144, 255, 0.2);
}

.user-avatar {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  background: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  overflow: hidden;
}

.avatar-img {
  width: 64px;
  height: 64px;
  border-radius: 50%;
}

.avatar-text {
  font-size: 28px;
  font-weight: 700;
  color: #1890ff;
}

.user-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
  flex: 1;
}

.user-name {
  font-size: 18px;
  font-weight: 600;
  color: #ffffff;
}

.user-dept {
  font-size: 13px;
  color: rgba(255, 255, 255, 0.85);
}

.user-arrow {
  font-size: 20px;
  color: rgba(255, 255, 255, 0.7);
}

/* 菜单 */
.menu-section {
  padding: 0 12px;
}

.menu-group {
  background: #ffffff;
  border-radius: 12px;
  margin-bottom: 12px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
}

.menu-item {
  display: flex;
  align-items: center;
  padding: 14px 16px;
  border-bottom: 1px solid #f5f5f5;
}

.menu-item:last-child {
  border-bottom: none;
}

.menu-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin-right: 12px;
  flex-shrink: 0;
}

.menu-icon-text {
  font-size: 13px;
  font-weight: 600;
  line-height: 1;
}

.menu-label {
  flex: 1;
  font-size: 15px;
  color: #262626;
}

.menu-extra {
  font-size: 13px;
  color: #8c8c8c;
  margin-right: 4px;
}

.menu-arrow {
  font-size: 18px;
  color: #bfbfbf;
}

/* 退出登录 */
.logout-section {
  padding: 24px 12px;
}

.logout-btn {
  width: 100%;
  height: 48px;
  line-height: 48px;
  background: #ffffff;
  color: #f5222d;
  font-size: 16px;
  font-weight: 600;
  border-radius: 12px;
  border: none;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
}

.logout-btn::after {
  border: none;
}
</style>
