<template>
  <div class="ai-config-page">
    <a-card title="AI 助手配置" :bordered="false">
      <a-form :model="formState" :label-col="{ span: 4 }" :wrapper-col="{ span: 14 }">
        <a-form-item label="系统提示词 (System Prompt)">
          <a-textarea v-model:value="formState.systemPrompt" :rows="4" placeholder="请设置AI的系统提示词，例如：你是一个乐于助人的AI助手..." />
        </a-form-item>
        
        <a-form-item label="系统开场白 (System Greeting)">
          <a-input v-model:value="formState.systemGreeting" placeholder="例如：您好，我是智能助手小美，有什么可以帮您？" />
          <div class="tip">AI助手初始化时发送的第一条消息。</div>
        </a-form-item>
        
        <a-form-item label="用户自定义开场白 (User Greeting)">
          <a-input v-model:value="formState.userGreeting" placeholder="例如：帮我分析一下今天的报表。" />
          <div class="tip">可选。如果设置，将作为用户的第一条消息自动发送。</div>
        </a-form-item>
        
        <a-form-item :wrapper-col="{ span: 14, offset: 4 }">
          <a-button type="primary" @click="handleSave" :loading="saving">保存配置</a-button>
          <a-button style="margin-left: 10px" @click="handleReset">重置默认</a-button>
        </a-form-item>
      </a-form>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue';
import { message } from 'ant-design-vue';
import { Card as ACard, Form as AForm, FormItem as AFormItem, Input as AInput, Textarea as ATextarea, Button as AButton } from 'ant-design-vue';

const STORAGE_KEY = 'XIAO_MEI_CONFIG';

interface ConfigState {
  systemPrompt: string;
  systemGreeting: string;
  userGreeting: string;
}

const defaultState: ConfigState = {
  systemPrompt: '',
  systemGreeting: '您好，我是智能助手小美，有什么可以帮您？',
  userGreeting: ''
};

const formState = reactive<ConfigState>({ ...defaultState });
const saving = ref(false);

const loadConfig = () => {
  const saved = localStorage.getItem(STORAGE_KEY);
  if (saved) {
    try {
      const parsed = JSON.parse(saved);
      Object.assign(formState, parsed);
    } catch (e) {
      console.error('Failed to load AI config', e);
    }
  }
};

const handleSave = () => {
  saving.value = true;
  // Simulate API delay
  setTimeout(() => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(formState));
    message.success('配置已保存');
    saving.value = false;
    // Dispatch event to notify Assistant component if needed, 
    // or rely on page refresh/next mount.
  }, 500);
};

const handleReset = () => {
  Object.assign(formState, defaultState);
  handleSave();
};

onMounted(() => {
  loadConfig();
});
</script>

<style scoped>
.ai-config-page {
  padding: 24px;
}
.tip {
  font-size: 12px;
  color: #999;
  margin-top: 4px;
}
</style>
