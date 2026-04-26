import { request, get, API_BASE_URL } from '@/utils/http.js'

export function loginApi(account, password, code, timestamp) {
  const data = { account, password, grant_type: 'password', origin: 'app' }
  if (code) data.code = code
  if (timestamp) data.timestamp = timestamp

  return request({
    url: '/api/oauth/Login',
    method: 'POST',
    data,
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    needToken: false
  })
}

export function getUserInfoApi() {
  return get('/api/oauth/CurrentUser')
}

export function doLogoutApi() {
  return get('/api/oauth/Logout')
}

export function getConfigApi(account) {
  return get('/api/oauth/getConfig/' + account, null, false)
}

export function getLoginConfigApi() {
  return get('/api/oauth/getLoginConfig', null, false)
}

// ========== 个人资料相关 ==========

/** 获取当前用户详细资料 */
export function getUserSettingInfoApi() {
  return get('/api/permission/Users/Current/BaseInfo')
}

/** 更新当前用户个人资料 */
export function updateUserInfoApi(data) {
  return request({
    url: '/api/permission/Users/Current/BaseInfo',
    method: 'PUT',
    data
  })
}

/** 修改当前用户密码（oldPassword/password 需已 MD5 加密） */
export function updatePasswordApi(data) {
  return request({
    url: '/api/permission/Users/Current/Actions/ModifyPassword',
    method: 'POST',
    data,
    headers: { 'Content-Type': 'application/json' }
  })
}

/** 更新用户头像（上传文件后拿到 name 再调用） */
export function updateAvatarApi(name) {
  return request({
    url: '/api/permission/Users/Current/Avatar/' + encodeURIComponent(name),
    method: 'PUT'
  })
}

/**
 * 上传头像文件
 * @param {string} filePath 本地图片路径（uni.chooseImage 返回的 tempFilePath）
 * @returns {Promise} uploadFile 结果，response.data 里会有 { name, url }
 */
export function uploadAvatarFile(filePath) {
  return new Promise((resolve, reject) => {
    const token = uni.getStorageSync('lm_app_token')
    uni.uploadFile({
      url: API_BASE_URL + '/api/file/userAvatar',
      filePath,
      name: 'file',
      header: {
        Authorization: token,
        'Poxiao-Origin': 'app'
      },
      success: (res) => {
        let data = res.data
        try {
          data = JSON.parse(data)
        } catch (e) {}
        if (data && data.code === 200) {
          resolve(data)
        } else {
          reject(new Error(data?.msg || '上传失败'))
        }
      },
      fail: (err) => {
        reject(new Error(err?.errMsg || '上传失败'))
      }
    })
  })
}
