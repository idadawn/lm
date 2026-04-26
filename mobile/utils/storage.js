const TOKEN_KEY = 'lm_app_token'
const USER_INFO_KEY = 'lm_app_user_info'

export function getToken() {
  try {
    return uni.getStorageSync(TOKEN_KEY) || null
  } catch (e) {
    return null
  }
}

export function setToken(token) {
  if (token) uni.setStorageSync(TOKEN_KEY, token)
  else uni.removeStorageSync(TOKEN_KEY)
}

export function getUserInfo() {
  try {
    return uni.getStorageSync(USER_INFO_KEY) || null
  } catch (e) {
    return null
  }
}

export function setUserInfo(userInfo) {
  if (userInfo != null) uni.setStorageSync(USER_INFO_KEY, userInfo)
  else uni.removeStorageSync(USER_INFO_KEY)
}

export function clearAuth() {
  uni.removeStorageSync(TOKEN_KEY)
  uni.removeStorageSync(USER_INFO_KEY)
}
