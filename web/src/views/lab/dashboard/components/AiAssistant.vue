<template>
  <div class="ai-assistant-container">
    <div 
      class="ai-float-btn" 
      :class="{ 'active': isOpen }"
      @click="toggleChat"
    >
      <Icon icon="ant-design:robot-outlined" :size="28" color="#fff" />
      <span class="btn-text">AI助手</span>
    </div>

    <!-- 聊天窗口 -->
    <Transition name="chat-slide">
      <div v-if="isOpen" class="chat-window">
        <div class="chat-header">
          <div class="chat-title">
            <Icon icon="ant-design:robot-outlined" :size="20" color="#1890ff" />
            <span>智能助手</span>
          </div>
          <div class="chat-actions">
            <a-button type="text" size="small" @click="clearChat">
              <Icon icon="ant-design:delete-outlined" :size="14" />
            </a-button>
            <a-button type="text" size="small" @click="toggleChat">
              <Icon icon="ant-design:close-outlined" :size="14" />
            </a-button>
          </div>
        </div>
        
        <div class="chat-messages" ref="messagesRef">
          <div 
            v-for="(msg, index) in messages" 
            :key="index" 
            class="message-item"
            :class="msg.role"
          >
            <div class="message-avatar">
              <Icon 
                :icon="msg.role === 'user' ? 'ant-design:user-outlined' : 'ant-design:robot-outlined'" 
                :size="18" 
                :color="msg.role === 'user' ? '#1890ff' : '#52c41a'" 
              />
            </div>
            <div class="message-content">
              <div class="message-text" v-html="formatMessage(msg.content)"></div>
              <div class="message-time">{{ msg.time }}</div>
            </div>
          </div>
          
          <div v-if="isLoading" class="message-item assistant">
            <div class="message-avatar">
              <Icon icon="ant-design:robot-outlined" :size="18" color="#52c41a" />
            </div>
            <div class="message-content">
              <div class="typing-indicator">
                <span></span>
                <span></span>
                <span></span>
              </div>
            </div>
          </div>
        </div>

        <div class="chat-input-area">
          <div class="quick-questions">
            <a-tag 
              v-for="q in quickQuestions" 
              :key="q"
              class="quick-tag"
              @click="sendQuickQuestion(q)"
            >
              {{ q }}
            </a-tag>
          </div>
          <div class="input-wrapper">
            <a-textarea
              v-model:value="inputMessage"
              placeholder="请输入您的问题..."
              :auto-size="{ minRows: 1, maxRows: 3 }"
              @pressEnter="handleSend"
            />
            <a-button 
              type="primary" 
              class="send-btn"
              :loading="isLoading"
              @click="handleSend"
            >
              <Icon icon="ant-design:send-outlined" :size="16" />
            </a-button>
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script lang="ts" setup>
  import { ref, nextTick } from 'vue';
  import { Icon } from '/@/components/Icon';
  import dayjs from 'dayjs';

  const isOpen = ref(false);
  const isLoading = ref(false);
  const inputMessage = ref('');
  const messagesRef = ref<HTMLDivElement | null>(null);

  const quickQuestions = [
    '今日合格率趋势如何？',
    '分析最近缺陷原因',
    '优化生产参数建议',
    '异常数据预警说明',
  ];

  interface Message {
    role: 'user' | 'assistant';
    content: string;
    time: string;
  }

  const messages = ref<Message[]>([
    {
      role: 'assistant',
      content: '您好！我是您的智能生产助手。我可以帮您：\n• 分析质量数据趋势\n• 查询生产指标\n• 识别异常模式\n• 提供工艺优化建议\n\n请问有什么可以帮助您的？',
      time: dayjs().format('HH:mm'),
    },
  ]);

  function toggleChat() {
    isOpen.value = !isOpen.value;
  }

  function clearChat() {
    messages.value = [{
      role: 'assistant',
      content: '对话已清空。请问有什么可以帮助您的？',
      time: dayjs().format('HH:mm'),
    }];
  }

  function formatMessage(content: string) {
    return content.replace(/\n/g, '<br>');
  }

  async function handleSend(e?: KeyboardEvent) {
    if (e && !e.shiftKey) {
      e.preventDefault();
    }
    
    const msg = inputMessage.value.trim();
    if (!msg || isLoading.value) return;

    // 添加用户消息
    messages.value.push({
      role: 'user',
      content: msg,
      time: dayjs().format('HH:mm'),
    });

    inputMessage.value = '';
    isLoading.value = true;
    scrollToBottom();

    // 模拟AI响应
    setTimeout(() => {
      const response = generateResponse(msg);
      messages.value.push({
        role: 'assistant',
        content: response,
        time: dayjs().format('HH:mm'),
      });
      isLoading.value = false;
      scrollToBottom();
    }, 1500);
  }

  function sendQuickQuestion(question: string) {
    inputMessage.value = question;
    handleSend();
  }

  function generateResponse(question: string): string {
    const lowerQ = question.toLowerCase();
    
    if (lowerQ.includes('合格率')) {
      return '根据今日数据，合格率为 **91.2%**，较昨日上升 **1.5%**。\n\n主要质量分布：\n• A级品：91.2% (1,257卷)\n• B级品：5.5% (76卷)\n• 不合格品：3.3% (46卷)\n\n建议关注夜班后半段的质量波动。';
    }
    
    if (lowerQ.includes('缺陷')) {
      return '当前主要缺陷排名：\n\n1. **划痕** - 320次 (33.8%)\n2. **麻点** - 256次 (27.0%)\n3. **毛边** - 180次 (19.0%)\n4. **亮线** - 110次 (11.6%)\n5. **网眼** - 74次 (7.8%)\n\n建议优先处理划痕和麻点问题，可考虑优化切割工艺参数。';
    }
    
    if (lowerQ.includes('优化') || lowerQ.includes('建议')) {
      return '基于当前数据分析，提供以下优化建议：\n\n1. **工艺窗口优化**\n   建议将厚度控制在 5.22-5.25mm 范围，可获得最佳叠片系数。\n\n2. **班次管理**\n   夜班 0:00-6:00 合格率较低，建议加强该时段巡检。\n\n3. **设备维护**\n   2号机组厚度波动较大，建议安排检修。';
    }
    
    if (lowerQ.includes('异常') || lowerQ.includes('预警')) {
      return '当前有 **3个** 活跃预警：\n\n🔴 **高优先级** (2个)\n• 2号机组-厚度超标\n• 系统异常-数据延迟\n\n🟡 **中优先级** (1个)\n• 4号机组-划痕检测\n\n建议立即处理高优先级预警。';
    }
    
    return '我理解您想了解关于"' + question + '"的信息。\n\n目前我可以帮您查询：\n• 实时生产指标\n• 质量分析报告\n• 历史趋势对比\n• 异常事件追踪\n\n如需更详细的分析，请访问相应的功能模块。';
  }

  function scrollToBottom() {
    nextTick(() => {
      if (messagesRef.value) {
        messagesRef.value.scrollTop = messagesRef.value.scrollHeight;
      }
    });
  }
</script>

<style lang="less" scoped>
  .ai-assistant-container {
    position: fixed;
    right: 24px;
    bottom: 24px;
    z-index: 9999;
  }

  .ai-float-btn {
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 12px 20px;
    background: linear-gradient(135deg, #1890ff 0%, #36cfc9 100%);
    border-radius: 28px;
    cursor: pointer;
    box-shadow: 0 4px 12px rgba(24, 144, 255, 0.4);
    transition: all 0.3s ease;

    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 16px rgba(24, 144, 255, 0.5);
    }

    &.active {
      background: linear-gradient(135deg, #52c41a 0%, #95de64 100%);
      box-shadow: 0 4px 12px rgba(82, 196, 26, 0.4);
    }

    .btn-text {
      color: #fff;
      font-size: 14px;
      font-weight: 500;
    }
  }

  .chat-window {
    position: absolute;
    right: 0;
    bottom: 70px;
    width: 380px;
    height: 500px;
    background: #fff;
    border-radius: 12px;
    box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
    display: flex;
    flex-direction: column;
    overflow: hidden;
  }

  .chat-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 16px;
    background: linear-gradient(135deg, #f6ffed 0%, #fff 100%);
    border-bottom: 1px solid #f0f0f0;

    .chat-title {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 15px;
      font-weight: 600;
      color: #262626;
    }

    .chat-actions {
      display: flex;
      gap: 4px;
    }
  }

  .chat-messages {
    flex: 1;
    overflow-y: auto;
    padding: 16px;
    background: #f5f7fa;

    .message-item {
      display: flex;
      gap: 10px;
      margin-bottom: 16px;

      &.user {
        flex-direction: row-reverse;

        .message-content {
          align-items: flex-end;

          .message-text {
            background: #1890ff;
            color: #fff;
            border-radius: 12px 12px 4px 12px;
          }
        }
      }

      &.assistant {
        .message-content {
          align-items: flex-start;

          .message-text {
            background: #fff;
            color: #262626;
            border-radius: 12px 12px 12px 4px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
          }
        }
      }
    }

    .message-avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: #fff;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
    }

    .message-content {
      display: flex;
      flex-direction: column;
      max-width: calc(100% - 50px);

      .message-text {
        padding: 10px 14px;
        font-size: 13px;
        line-height: 1.6;
        word-break: break-word;
      }

      .message-time {
        font-size: 11px;
        color: #999;
        margin-top: 4px;
      }
    }
  }

  .typing-indicator {
    display: flex;
    gap: 4px;
    padding: 12px;

    span {
      width: 8px;
      height: 8px;
      background: #ccc;
      border-radius: 50%;
      animation: typing 1.4s infinite ease-in-out both;

      &:nth-child(1) { animation-delay: -0.32s; }
      &:nth-child(2) { animation-delay: -0.16s; }
    }
  }

  @keyframes typing {
    0%, 80%, 100% { transform: scale(0.6); opacity: 0.5; }
    40% { transform: scale(1); opacity: 1; }
  }

  .chat-input-area {
    padding: 12px 16px;
    background: #fff;
    border-top: 1px solid #f0f0f0;

    .quick-questions {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
      margin-bottom: 12px;

      .quick-tag {
        cursor: pointer;
        font-size: 12px;
        padding: 2px 8px;
        transition: all 0.2s;

        &:hover {
          color: #1890ff;
          border-color: #1890ff;
        }
      }
    }

    .input-wrapper {
      display: flex;
      gap: 8px;

      :deep(.ant-input) {
        border-radius: 8px;
        resize: none;
      }

      .send-btn {
        width: 40px;
        height: 40px;
        padding: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 8px;
      }
    }
  }

  // 动画
  .chat-slide-enter-active,
  .chat-slide-leave-active {
    transition: all 0.3s ease;
  }

  .chat-slide-enter-from,
  .chat-slide-leave-to {
    opacity: 0;
    transform: translateY(20px) scale(0.95);
  }
</style>
