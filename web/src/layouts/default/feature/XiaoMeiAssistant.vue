<template>
  <div ref="el" class="xiao-mei-assistant" :style="style">
    <!-- Floating Button -->
    <div v-if="!visible" class="float-btn" @click="toggleChat" @mousedown="handlePointerDown" @mouseup="handlePointerUp" @touchstart="handlePointerDown" @touchend="handlePointerUp">
      <div class="avatar-wrapper">
        <RobotOutlined style="font-size: 24px; color: #fff;" />
      </div>
    </div>

    <!-- Chat Window -->
    <div v-show="visible" class="chat-window shadow-xl">
      <div class="chat-header">
        <div class="flex items-center">
          <RobotOutlined class="mr-2" />
          <span>智能助手小美</span>
        </div>
        <CloseOutlined class="cursor-pointer hover:text-red-500" @click="toggleChat" />
      </div>
      
      <div class="chat-body-iframe">
        <iframe 
          ref="iframeRef" 
          :src="chatbotUrl" 
          frameborder="0" 
          style="width: 100%; height: 100%;"
          @load="handleIframeLoad"
        ></iframe>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted, onUnmounted, watch } from 'vue';
  import { RobotOutlined, CloseOutlined } from '@ant-design/icons-vue';
  import { useDraggable, useWindowSize } from '@vueuse/core';

  const props = defineProps({
    systemGreeting: {
      type: String,
      default: '您好，我是智能助手小美，有什么可以帮您？'
    },
    userGreeting: {
      type: String,
      default: ''
    }
  });

  const visible = ref(false);
  const iframeRef = ref<HTMLIFrameElement | null>(null);
  
  // 获取聊天机器人URL - 从环境变量或使用默认值
  const chatbotUrl = import.meta.env.VITE_CHATBOT_URL || 'http://localhost:5174';
  
  // Draggable Logic
  const el = ref<HTMLElement | null>(null);
  const { width, height } = useWindowSize();
  const { style, x, y } = useDraggable(el, {
    initialValue: { x: window.innerWidth - 100, y: window.innerHeight - 100 },
  });

  // Boundary Constraint
  watch([x, y, width, height], () => {
    if (!el.value) return;
    const boxWidth = el.value.offsetWidth || 56;
    const boxHeight = el.value.offsetHeight || 56;
    
    if (x.value < 0) x.value = 0;
    if (x.value > width.value - boxWidth) x.value = width.value - boxWidth;
    if (y.value < 0) y.value = 0;
    if (y.value > height.value - boxHeight) y.value = height.value - boxHeight;
  });
  
  // Click vs Drag Detection
  const startPos = { x: 0, y: 0 };
  const isDragging = ref(false);
  let dragStartTime = 0;

  const handlePointerDown = (e: MouseEvent | TouchEvent) => {
    isDragging.value = false;
    dragStartTime = Date.now();
    const clientX = 'touches' in e ? e.touches[0].clientX : e.clientX;
    const clientY = 'touches' in e ? e.touches[0].clientY : e.clientY;
    startPos.x = clientX;
    startPos.y = clientY;
  };
  
  const handlePointerUp = (e: MouseEvent | TouchEvent) => {
    const clientX = 'changedTouches' in e ? e.changedTouches[0].clientX : e.clientX;
    const clientY = 'changedTouches' in e ? e.changedTouches[0].clientY : e.clientY;
    const dist = Math.sqrt(Math.pow(clientX - startPos.x, 2) + Math.pow(clientY - startPos.y, 2));
    const dragDuration = Date.now() - dragStartTime;
    
    // 如果移动距离超过5px，或者拖拽时间超过200ms，认为是拖拽
    if (dist > 5 || dragDuration > 200) {
      isDragging.value = true;
      // 延迟重置，确保toggleChat不会触发
      setTimeout(() => {
        isDragging.value = false;
      }, 100);
    } else {
      isDragging.value = false;
    }
  };

  // Update initial pos on mount to ensure it's bottom right
  onMounted(() => {
    x.value = window.innerWidth - 100;
    y.value = window.innerHeight - 100;
    window.addEventListener('message', handleMessage);
  });
  
  onUnmounted(() => {
      window.removeEventListener('message', handleMessage);
  });

  const toggleChat = () => {
    if (isDragging.value) return;
    visible.value = !visible.value;
  };

  // Handle connection with React Chatbot
  const handleMessage = (event: MessageEvent) => {
      // Check origin if strictly needed, skipping for localhost dev
      if (event.data && event.data.type === 'CHATBOT_READY') {
          sendConfig();
      }
  };

  const handleIframeLoad = () => {
      // Also try sending config on load, just in case
      sendConfig();
  };

  const sendConfig = () => {
    if (!iframeRef.value || !iframeRef.value.contentWindow) return;

    let sysGreeting = props.systemGreeting;
    let usrGreeting = props.userGreeting;
    let systemPrompt = '';
    
    try {
        const saved = localStorage.getItem('XIAO_MEI_CONFIG');
        if (saved) {
            const config = JSON.parse(saved);
            if (config.systemGreeting) sysGreeting = config.systemGreeting;
            if (config.userGreeting) usrGreeting = config.userGreeting;
            if (config.systemPrompt) systemPrompt = config.systemPrompt;
        }
    } catch (e) {
        console.warn('Failed to load AI config', e);
    }

    iframeRef.value.contentWindow.postMessage({
        type: 'INIT_CONFIG',
        payload: {
            systemGreeting: sysGreeting,
            userGreeting: usrGreeting,
            systemPrompt: systemPrompt
        }
    }, '*');
  };

  watch(() => [props.systemGreeting, props.userGreeting, visible.value], () => {
      if (visible.value) {
        setTimeout(sendConfig, 500); // Send again when opening or props changing
      }
  });
</script>

<style lang="less" scoped>
  .xiao-mei-assistant {
    position: fixed;
    z-index: 9999;
    /* bottom/right removed, controlled by style (top/left) via useDraggable */
    touch-action: none;
  }

  .float-btn {
    width: 56px;
    height: 56px;
    border-radius: 50%;
    background: linear-gradient(135deg, #1890ff 0%, #096dd9 100%);
    box-shadow: 0 4px 12px rgba(24, 144, 255, 0.4);
    cursor: move; /* Indicate draggable */
    display: flex;
    align-items: center;
    justify-content: center;
    transition: transform 0.1s; /* Faster transition for drag feel, or remove */

    &:hover {
      transform: scale(1.05);
      box-shadow: 0 6px 16px rgba(24, 144, 255, 0.6);
    }
    
    &:active {
       cursor: grabbing;
    }
  }

  .chat-window {
    width: 380px;
    height: 500px;
    background: #fff;
    border-radius: 12px;
    display: flex;
    flex-direction: column;
    overflow: hidden;
    animation: slideUp 0.3s ease-out;
  }

  @keyframes slideUp {
    from {
      opacity: 0;
      transform: translateY(20px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  .chat-header {
    height: 50px;
    background: linear-gradient(to right, #1890ff, #52c41a); /* Branding colors */
    color: #fff;
    padding: 0 16px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    font-size: 16px;
    font-weight: 500;
  }

  .chat-body-iframe {
    flex: 1;
    overflow: hidden;
    background-color: #fff;
  }



</style>
