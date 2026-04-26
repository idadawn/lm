<template>
  <view class="login-page">
    <view class="login-header">
      <image class="login-logo" src="/static/logo.png" mode="aspectFit" />
      <text class="login-title">检测室数据分析</text>
      <text class="login-subtitle">下一代实验室智能分析平台</text>
    </view>

    <view class="login-form">
      <view class="form-item">
        <text class="form-label">账号</text>
        <input
          class="form-input"
          type="text"
          v-model="form.account"
          placeholder="请输入账号"
          @blur="onAccountBlur"
        />
      </view>

      <view class="form-item">
        <text class="form-label">密码</text>
        <input
          class="form-input"
          type="password"
          v-model="form.password"
          placeholder="请输入密码"
        />
      </view>

      <view class="form-item" v-if="needCode">
        <text class="form-label">验证码</text>
        <view class="code-row">
          <input
            class="form-input code-input"
            type="text"
            v-model="form.code"
            placeholder="请输入验证码"
          />
          <image
            class="code-image"
            :src="codeImgUrl"
            mode="aspectFit"
            @click="refreshCode"
          />
        </view>
      </view>

      <view class="remember-row">
        <view class="remember-check" @click="remember = !remember">
          <view class="check-box" :class="{ checked: remember }">
            <text v-if="remember" class="check-icon">✓</text>
          </view>
          <text class="remember-label">记住密码</text>
        </view>
      </view>

      <button
        class="login-btn"
        :loading="loading"
        :disabled="loading"
        @click="handleLogin"
      >
        {{ loading ? '登录中...' : '登录' }}
      </button>

      <!-- 调试用：直接跳转首页 -->
      <button class="login-btn debug-btn" @click="debugGoHome">
        调试：直接跳转首页
      </button>
    </view>

    <view class="login-footer">
      <text class="copyright">{{ copyright }}</text>
    </view>
  </view>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { loginApi, getUserInfoApi, getConfigApi } from '@/api/user.js'
import { setToken, setUserInfo } from '@/utils/storage.js'
import { md5 } from '@/utils/md5.js'
import { API_BASE_URL } from '@/utils/http.js'

const REMEMBER_KEY = 'lm_app_remember'

const loading = ref(false)
const needCode = ref(false)
const codeLength = ref(4)
const timestamp = ref(0)
const codeImgUrl = ref('')
const copyright = ref('')
const remember = ref(false)

const form = reactive({
  account: '',
  password: '',
  code: ''
})

function loadRemember() {
  try {
    const saved = uni.getStorageSync(REMEMBER_KEY)
    if (saved) {
      form.account = saved.account || ''
      form.password = saved.password || ''
      remember.value = true
      if (form.account) onAccountBlur()
    }
  } catch (e) {}
}

function refreshCode() {
  const ts = Date.now()
  timestamp.value = ts
  codeImgUrl.value = API_BASE_URL + '/api/oauth/ImageCode/' + codeLength.value + '/' + ts + '?t=' + ts
}

function onAccountBlur() {
  if (!form.account) return
  getConfigApi(form.account).then((res) => {
    const data = res.data
    if (data) {
      needCode.value = !!data.enableVerificationCode
      if (needCode.value) {
        codeLength.value = data.verificationCodeNumber || 4
        refreshCode()
      }
    }
  }).catch(() => {})
}

function handleLogin() {
  if (!form.account) {
    uni.showToast({ title: '请输入账号', icon: 'none' })
    return
  }
  if (!form.password) {
    uni.showToast({ title: '请输入密码', icon: 'none' })
    return
  }
  if (needCode.value && !form.code) {
    uni.showToast({ title: '请输入验证码', icon: 'none' })
    return
  }

  loading.value = true
  loginApi(
    form.account,
    md5(form.password),
    needCode.value ? form.code : undefined,
    timestamp.value > 0 ? timestamp.value : undefined
  ).then((res) => {
    const data = res.data
    const token = data?.token || ''
    if (!token) {
      uni.showToast({ title: '登录失败，未获取到令牌', icon: 'none' })
      loading.value = false
      return
    }

    // 记住密码
    if (remember.value) {
      uni.setStorageSync(REMEMBER_KEY, { account: form.account, password: form.password })
    } else {
      uni.removeStorageSync(REMEMBER_KEY)
    }

    setToken(token)
    console.log('[Login] token 已保存，准备跳转')
    // 先去掉 Toast 干扰，直接跳转
    uni.switchTab({
      url: '/pages/index/index',
      success: () => console.log('[Login] switchTab success'),
      fail: (err) => console.error('[Login] switchTab fail', JSON.stringify(err))
    })

    // 异步获取用户信息，失败也不影响跳转
    getUserInfoApi().then((userRes) => {
      const userData = userRes.data
      if (userData?.userInfo) {
        setUserInfo(userData.userInfo)
      }
      if (userData?.sysConfigInfo?.copyright) {
        copyright.value = userData.sysConfigInfo.copyright
      }
    }).catch((err) => {
      console.error('获取用户信息失败:', err)
    })
  }).catch((err) => {
    const msg = err?.message || '登录失败'
    uni.showToast({ title: msg, icon: 'none' })
    if (needCode.value) refreshCode()
  }).finally(() => {
    loading.value = false
  })
}

function debugGoHome() {
  console.log('[Login] 调试跳转')
  uni.switchTab({
    url: '/pages/index/index',
    success: () => console.log('[Login] 调试 switchTab success'),
    fail: (err) => console.error('[Login] 调试 switchTab fail', JSON.stringify(err))
  })
}

onMounted(() => {
  loadRemember()
})
</script>

<style lang="scss">
.login-page {
  min-height: 100vh;
  background: linear-gradient(180deg, #f7f9fc 0%, #eef2f7 100%);
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 0 32px;
  box-sizing: border-box;
}

.login-header {
  margin-top: 60px;
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 32px;
}

.login-logo {
  width: 120px;
  height: 120px;
  margin-bottom: 16px;
}

.login-title {
  font-size: 22px;
  font-weight: 700;
  color: #262626;
  margin-bottom: 8px;
}

.login-subtitle {
  font-size: 14px;
  color: #8c8c8c;
}

.login-form {
  width: 100%;
  background: #ffffff;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
}

.form-item {
  margin-bottom: 20px;
}

.form-label {
  display: block;
  font-size: 14px;
  color: #595959;
  margin-bottom: 8px;
}

.form-input {
  width: 100%;
  height: 44px;
  background: #f5f7fa;
  border: 1px solid #e4e7ed;
  border-radius: 8px;
  padding: 0 12px;
  font-size: 15px;
  color: #262626;
  box-sizing: border-box;
}

.code-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.code-input {
  flex: 1;
}

.code-image {
  width: 100px;
  height: 44px;
  border-radius: 8px;
  background: #f5f7fa;
}

.login-btn {
  width: 100%;
  height: 48px;
  line-height: 48px;
  background: linear-gradient(135deg, #1890ff, #40a9ff);
  color: #ffffff;
  font-size: 16px;
  font-weight: 600;
  border-radius: 8px;
  margin-top: 8px;
  border: none;
}

.login-btn::after {
  border: none;
}

.login-btn[disabled] {
  opacity: 0.6;
}

.debug-btn {
  margin-top: 12px;
  background: linear-gradient(135deg, #52c41a, #73d13d);
}

.remember-row {
  display: flex;
  align-items: center;
  margin-bottom: 16px;
  padding: 0 4px;
}

.remember-check {
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
}

.check-box {
  width: 18px;
  height: 18px;
  border: 2px solid #d9d9d9;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.check-box.checked {
  background: #1890ff;
  border-color: #1890ff;
}

.check-icon {
  color: #ffffff;
  font-size: 12px;
  font-weight: 700;
}

.remember-label {
  font-size: 14px;
  color: #595959;
}

.login-footer {
  margin-top: auto;
  padding-bottom: 32px;
  padding-top: 32px;
}

.copyright {
  font-size: 12px;
  color: #bfbfbf;
  text-align: center;
}
</style>
