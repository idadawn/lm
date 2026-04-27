<template>
  <view class="password-page">
    <view class="form-card">
      <view class="form-item">
        <text class="form-label">旧密码</text>
        <input
          class="form-input"
          type="password"
          v-model="form.oldPassword"
          placeholder="请输入旧密码"
        />
      </view>

      <view class="form-item">
        <text class="form-label">新密码</text>
        <input
          class="form-input"
          type="password"
          v-model="form.password"
          placeholder="请输入新密码"
        />
      </view>

      <view class="form-item">
        <text class="form-label">重复密码</text>
        <input
          class="form-input"
          type="password"
          v-model="form.password2"
          placeholder="请再次输入新密码"
        />
      </view>

      <view class="form-item code-item">
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
    </view>

    <view class="tips">
      <text class="tips-text">修改密码成功后需要重新登录</text>
    </view>

    <view class="action-section">
      <button class="submit-btn" :loading="submitting" :disabled="submitting" @click="handleSubmit">
        {{ submitting ? '提交中...' : '确认修改' }}
      </button>
    </view>
  </view>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { updatePasswordApi } from '@/api/user.js'
import { clearAuth } from '@/utils/storage.js'
import { md5 } from '@/utils/md5.js'
import { getCurrentBaseUrl } from '@/utils/http.js'

const form = ref({
  oldPassword: '',
  password: '',
  password2: '',
  code: ''
})

const timestamp = ref(0)
const codeImgUrl = ref('')
const submitting = ref(false)

function refreshCode() {
  const ts = Date.now()
  timestamp.value = ts
  codeImgUrl.value = getCurrentBaseUrl() + '/api/oauth/ImageCode/4/' + ts + '?t=' + ts
}

function validate() {
  if (!form.value.oldPassword) {
    uni.showToast({ title: '请输入旧密码', icon: 'none' })
    return false
  }
  if (!form.value.password) {
    uni.showToast({ title: '请输入新密码', icon: 'none' })
    return false
  }
  if (form.value.password !== form.value.password2) {
    uni.showToast({ title: '两次密码输入不一致', icon: 'none' })
    return false
  }
  if (form.value.password === form.value.oldPassword) {
    uni.showToast({ title: '新密码不能与旧密码相同', icon: 'none' })
    return false
  }
  if (!form.value.code) {
    uni.showToast({ title: '请输入验证码', icon: 'none' })
    return false
  }
  return true
}

async function handleSubmit() {
  if (!validate()) return

  submitting.value = true
  try {
    const payload = {
      oldPassword: md5(form.value.oldPassword),
      password: md5(form.value.password),
      code: form.value.code,
      timestamp: timestamp.value
    }
    const res = await updatePasswordApi(payload)
    uni.showToast({ title: res.msg || '修改成功', icon: 'success' })
    setTimeout(() => {
      clearAuth()
      uni.reLaunch({ url: '/pages/login/login' })
    }, 1500)
  } catch (e) {
    console.error('[Password] 修改失败:', e)
    uni.showToast({ title: e.message || '修改失败', icon: 'none' })
    refreshCode()
  } finally {
    submitting.value = false
  }
}

onMounted(() => {
  refreshCode()
})
</script>

<style lang="scss">
.password-page {
  min-height: 100vh;
  background: #f7f9fc;
  padding: 12px;
  box-sizing: border-box;
}

.form-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 8px 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
}

.form-item {
  display: flex;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #f5f5f5;
  gap: 12px;
}

.form-item:last-child {
  border-bottom: none;
}

.form-label {
  font-size: 14px;
  color: #595959;
  width: 80px;
  flex-shrink: 0;
}

.form-input {
  flex: 1;
  height: 36px;
  font-size: 14px;
  color: #262626;
}

.code-item {
  align-items: flex-start;
}

.code-row {
  flex: 1;
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
  flex-shrink: 0;
}

.tips {
  padding: 16px;
  text-align: center;
}

.tips-text {
  font-size: 13px;
  color: #8c8c8c;
}

.action-section {
  padding: 0 0 24px;
}

.submit-btn {
  width: 100%;
  height: 48px;
  line-height: 48px;
  background: linear-gradient(135deg, #1890ff, #40a9ff);
  color: #ffffff;
  font-size: 16px;
  font-weight: 600;
  border-radius: 12px;
  border: none;
}

.submit-btn::after {
  border: none;
}

.submit-btn[disabled] {
  opacity: 0.6;
}
</style>
