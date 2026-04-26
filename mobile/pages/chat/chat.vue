<template>
  <view class="chat-page">
    <view class="chat-empty">
      <text class="chat-empty-title">智能对话助手</text>
      <text class="chat-empty-desc">支持自然语言查询检测数据、分析趋势、生成报表</text>
      <view class="chat-start-btn" @click="startChat">
        <text>开始对话</text>
      </view>
    </view>
  </view>
</template>

<script setup>
function startChat() {
  uni.showToast({ title: '功能开发中', icon: 'none' })
}
</script>

<style lang="scss">
.chat-page {
  min-height: 100vh;
  background: #f7f9fc;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  box-sizing: border-box;
}

.chat-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.chat-empty-title {
  font-size: 20px;
  font-weight: 700;
  color: #262626;
  margin-bottom: 12px;
}

.chat-empty-desc {
  font-size: 14px;
  color: #8c8c8c;
  line-height: 1.6;
  margin-bottom: 32px;
}

.chat-start-btn {
  background: linear-gradient(135deg, #1890ff, #40a9ff);
  color: #ffffff;
  padding: 12px 40px;
  border-radius: 24px;
  font-size: 16px;
  font-weight: 600;
}
</style>
