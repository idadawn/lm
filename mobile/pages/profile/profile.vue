<template>
  <view class="profile-page">
    <!-- 头像区域 -->
    <view class="avatar-section" @click="chooseAvatar">
      <image
        v-if="userInfo.headIcon"
        class="avatar-img"
        :src="headIconUrl"
        mode="aspectFill"
      />
      <view v-else class="avatar-text">
        <text>{{ avatarText }}</text>
      </view>
      <text class="avatar-tip">点击更换头像</text>
    </view>

    <!-- 账户信息 -->
    <view class="section-card">
      <view class="section-title">账户信息</view>
      <view class="info-list">
        <view class="info-item">
          <text class="info-label">账号</text>
          <text class="info-value">{{ accountInfo.account || '-' }}</text>
        </view>
        <view class="info-item">
          <text class="info-label">所属组织</text>
          <text class="info-value">{{ accountInfo.organize || '-' }}</text>
        </view>
        <view class="info-item">
          <text class="info-label">岗位</text>
          <text class="info-value">{{ accountInfo.position || '-' }}</text>
        </view>
        <view class="info-item">
          <text class="info-label">角色</text>
          <text class="info-value">{{ accountInfo.role || '-' }}</text>
        </view>
        <view class="info-item">
          <text class="info-label">注册时间</text>
          <text class="info-value">{{ accountInfo.creatorTime || '-' }}</text>
        </view>
        <view class="info-item">
          <text class="info-label">上次登录</text>
          <text class="info-value">{{ accountInfo.prevLoginTime || '-' }}</text>
        </view>
      </view>
    </view>

    <!-- 个人资料 -->
    <view class="section-card">
      <view class="section-title">个人资料</view>
      <view class="form-list">
        <view class="form-item">
          <text class="form-label">姓名</text>
          <input
            class="form-input"
            v-model="form.realName"
            placeholder="请输入姓名"
            maxlength="50"
          />
        </view>
        <view class="form-item">
          <text class="form-label">性别</text>
          <picker mode="selector" :range="genderOptions" :value="genderIndex" @change="onGenderChange">
            <view class="form-input picker-input">{{ form.gender || '请选择性别' }}</view>
          </picker>
        </view>
        <view class="form-item">
          <text class="form-label">手机号码</text>
          <input
            class="form-input"
            v-model="form.mobilePhone"
            placeholder="请输入手机号码"
            maxlength="20"
            type="number"
          />
        </view>
        <view class="form-item">
          <text class="form-label">电子邮箱</text>
          <input
            class="form-input"
            v-model="form.email"
            placeholder="请输入电子邮箱"
            maxlength="50"
          />
        </view>
        <view class="form-item">
          <text class="form-label">通讯地址</text>
          <input
            class="form-input"
            v-model="form.postalAddress"
            placeholder="请输入通讯地址"
            maxlength="300"
          />
        </view>
      </view>
    </view>

    <!-- 保存按钮 -->
    <view class="action-section">
      <button class="save-btn" :loading="saving" :disabled="saving" @click="handleSave">
        {{ saving ? '保存中...' : '保存' }}
      </button>
    </view>
  </view>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { getUserInfo, setUserInfo } from '@/utils/storage.js'
import { getUserSettingInfoApi, updateUserInfoApi, uploadAvatarFile, updateAvatarApi } from '@/api/user.js'
import { API_BASE_URL } from '@/utils/http.js'

const genderOptions = ['男', '女', '保密']

const userInfo = ref({})
const accountInfo = ref({})
const saving = ref(false)

const form = ref({
  realName: '',
  gender: '',
  mobilePhone: '',
  email: '',
  postalAddress: ''
})

const genderIndex = computed(() => {
  const idx = genderOptions.indexOf(form.value.gender)
  return idx >= 0 ? idx : 0
})

const avatarText = computed(() => {
  const name = userInfo.value.userName || form.value.realName || '用'
  return name.substring(0, 1)
})

const headIconUrl = computed(() => {
  const icon = userInfo.value.headIcon || ''
  if (!icon) return ''
  if (icon.startsWith('http')) return icon
  return API_BASE_URL + icon
})

function onGenderChange(e) {
  const idx = e.detail.value
  form.value.gender = genderOptions[idx] || ''
}

function formatDate(ts) {
  if (!ts) return '-'
  const d = new Date(typeof ts === 'string' ? ts.replace(/-/g, '/') : ts)
  if (isNaN(d.getTime())) return ts
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

async function loadData() {
  // 先从本地缓存取基础信息
  const cached = getUserInfo()
  if (cached) {
    userInfo.value = cached
  }

  // 从后端拉详细资料
  try {
    const res = await getUserSettingInfoApi()
    const data = res.data
    if (!data) return

    // 账户信息（只读）
    accountInfo.value = {
      account: data.account || cached?.userAccount || '-',
      organize: data.organize || cached?.organizeName || '-',
      position: data.position || cached?.positionName || '-',
      role: data.roleId || cached?.roleName || '-',
      creatorTime: formatDate(data.creatorTime),
      prevLoginTime: formatDate(data.prevLoginTime)
    }

    // 填充可编辑表单
    form.value.realName = data.realName || ''
    form.value.gender = data.gender || ''
    form.value.mobilePhone = data.mobilePhone || ''
    form.value.email = data.email || ''
    form.value.postalAddress = data.postalAddress || ''

    // 更新本地缓存中的头像和姓名
    if (cached) {
      cached.userName = data.realName || cached.userName
      cached.headIcon = data.headIcon || cached.headIcon
      setUserInfo(cached)
      userInfo.value = cached
    }
  } catch (e) {
    console.error('[Profile] 加载资料失败:', e)
    uni.showToast({ title: '加载资料失败', icon: 'none' })
  }
}

function chooseAvatar() {
  uni.chooseImage({
    count: 1,
    sizeType: ['compressed'],
    sourceType: ['album', 'camera'],
    success: async (res) => {
      const filePath = res.tempFilePaths[0]
      uni.showLoading({ title: '上传中...', mask: true })
      try {
        const uploadRes = await uploadAvatarFile(filePath)
        const fileData = uploadRes.data
        if (!fileData?.name) {
          throw new Error('上传返回数据异常')
        }
        // 更新用户头像关联
        await updateAvatarApi(fileData.name)
        // 更新本地缓存
        const cached = getUserInfo() || {}
        cached.headIcon = fileData.url || fileData.name
        setUserInfo(cached)
        userInfo.value = cached
        uni.showToast({ title: '头像更新成功', icon: 'success' })
      } catch (err) {
        console.error('[Profile] 头像上传失败:', err)
        uni.showToast({ title: err.message || '上传失败', icon: 'none' })
      } finally {
        uni.hideLoading()
      }
    }
  })
}

async function handleSave() {
  saving.value = true
  try {
    const payload = {
      realName: form.value.realName,
      gender: form.value.gender,
      mobilePhone: form.value.mobilePhone,
      email: form.value.email,
      postalAddress: form.value.postalAddress
    }
    await updateUserInfoApi(payload)

    // 更新本地缓存
    const cached = getUserInfo() || {}
    cached.userName = payload.realName || cached.userName
    setUserInfo(cached)

    uni.showToast({ title: '保存成功', icon: 'success' })
  } catch (e) {
    console.error('[Profile] 保存失败:', e)
    uni.showToast({ title: e.message || '保存失败', icon: 'none' })
  } finally {
    saving.value = false
  }
}

onMounted(() => {
  loadData()
})
</script>

<style lang="scss">
.profile-page {
  min-height: 100vh;
  background: #f7f9fc;
  padding: 12px;
  padding-bottom: constant(safe-area-inset-bottom);
  padding-bottom: env(safe-area-inset-bottom);
  box-sizing: border-box;
}

/* 头像 */
.avatar-section {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 24px 0;
  gap: 8px;
}

.avatar-img {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  border: 2px solid #ffffff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.avatar-text {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  background: #1890ff;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 2px solid #ffffff;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.avatar-text text {
  font-size: 32px;
  font-weight: 700;
  color: #ffffff;
}

.avatar-tip {
  font-size: 13px;
  color: #8c8c8c;
}

/* 卡片 */
.section-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 16px;
  margin-bottom: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.03);
}

.section-title {
  font-size: 15px;
  font-weight: 600;
  color: #262626;
  margin-bottom: 12px;
  padding-bottom: 8px;
  border-bottom: 1px solid #f0f0f0;
}

/* 只读信息 */
.info-list {
  display: flex;
  flex-direction: column;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
  border-bottom: 1px solid #f5f5f5;
}

.info-item:last-child {
  border-bottom: none;
}

.info-label {
  font-size: 14px;
  color: #595959;
}

.info-value {
  font-size: 14px;
  color: #262626;
  text-align: right;
  flex: 1;
  margin-left: 16px;
}

/* 表单 */
.form-list {
  display: flex;
  flex-direction: column;
}

.form-item {
  display: flex;
  align-items: center;
  padding: 10px 0;
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
  text-align: right;
}

.picker-input {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  color: #262626;
}

/* 保存 */
.action-section {
  padding: 12px 0 24px;
}

.save-btn {
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

.save-btn::after {
  border: none;
}

.save-btn[disabled] {
  opacity: 0.6;
}
</style>
