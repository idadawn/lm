<template>
  <!-- Cavos聊天机器人将通过脚本自动加载，不需要模板内容 -->
</template>

<script setup lang="ts">
  import { onMounted, onUnmounted } from 'vue';

  // 扩展Window接口以支持Cavos配置
  declare global {
    interface Window {
      cavosChatbotConfig?: {
        token: string;
        agentId: string;
        baseUrl: string;
        buttonStyle: string;
        windowWidth: string;
        windowHeight: string;
        iframeSrc: string;
      };
    }
  }

  const SCRIPT_ID = '7e908f41-55d7-47b8-aa9c-36dbb6addda7';

  onMounted(() => {
    // 检查是否已经加载过嵌入脚本
    if (document.getElementById(SCRIPT_ID)) {
      return;
    }

    // 检查是否已经加载过配置
    if (!window.cavosChatbotConfig) {
      // 创建配置脚本
      const configScript = document.createElement('script');
      configScript.setAttribute('data-cavos-config', 'true');
      configScript.textContent = `
        window.cavosChatbotConfig = {
          token: '7e908f41-55d7-47b8-aa9c-36dbb6addda7',
          agentId: '7e908f41-55d7-47b8-aa9c-36dbb6addda7',
          baseUrl: 'https://cavos.emergen.cn',
          buttonStyle: 'siri',
          windowWidth: '24rem',
          windowHeight: '40rem',
          iframeSrc: 'https://cavos.emergen.cn/chat/7e908f41-55d7-47b8-aa9c-36dbb6addda7?shared=1&from=b8cd435d-7c64-44ab-ad1a-dc9356cd6bf6&mode=drawer&sidebar=0&embed=true'
        }
      `;
      document.head.appendChild(configScript);
    }

    // 创建嵌入脚本
    const embedScript = document.createElement('script');
    embedScript.src = 'https://cavos.emergen.cn/embed.min.js';
    embedScript.id = SCRIPT_ID;
    embedScript.defer = true;
    document.head.appendChild(embedScript);
  });

  onUnmounted(() => {
    // 清理脚本（可选，通常不需要）
    const configScript = document.querySelector('script[data-cavos-config]');
    const embedScript = document.getElementById(SCRIPT_ID);
    if (configScript) {
      configScript.remove();
    }
    if (embedScript) {
      embedScript.remove();
    }
    // 清理全局配置
    if (window.cavosChatbotConfig) {
      delete window.cavosChatbotConfig;
    }
  });
</script>

<style lang="less" scoped>
  /* Cavos聊天机器人使用自己的样式，不需要额外样式 */
</style>
