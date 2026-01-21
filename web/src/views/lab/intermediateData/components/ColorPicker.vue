<template>
  <div class="color-picker-wrapper">
    <a-popover trigger="click" placement="bottomLeft" v-model:open="popoverVisible">
      <template #content>
        <div class="color-picker-panel">
          <div class="color-grid">
            <div
              v-for="color in standardColors"
              :key="color"
              class="color-cell"
              :style="{ backgroundColor: color }"
              :class="{ selected: selectedColor === color }"
              @click="selectColor(color)"
              :title="color"
            >
              <CheckOutlined v-if="selectedColor === color" class="check-icon" />
            </div>
          </div>
          <div class="color-actions">
            <a-button size="small" type="text" @click="clearColor">清除颜色</a-button>
          </div>
        </div>
      </template>
      <a-button size="small" class="color-picker-btn">
        <div class="color-preview" :style="{ backgroundColor: selectedColor || 'transparent' }"></div>
        填充颜色
      </a-button>
    </a-popover>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch } from 'vue';
import { CheckOutlined } from '@ant-design/icons-vue';

// WPS 10个标准色
const standardColors = [
  '#C00000', // 深红
  '#FF0000', // 红色
  '#FFC000', // 橙色
  '#FFFF00', // 黄色
  '#92D050', // 浅绿
  '#00B050', // 绿色
  '#00B0F0', // 天蓝
  '#0070C0', // 蓝色
  '#002060', // 深蓝
  '#7030A0', // 紫色
];

const props = defineProps<{
  modelValue?: string;
}>();

const emit = defineEmits<{
  'update:modelValue': [color: string];
  'change': [color: string];
}>();

const popoverVisible = ref(false);
const selectedColor = ref(props.modelValue || '');

watch(() => props.modelValue, (newVal) => {
  selectedColor.value = newVal || '';
});

function selectColor(color: string) {
  selectedColor.value = color;
  emit('update:modelValue', color);
  emit('change', color);
  popoverVisible.value = false;
}

function clearColor() {
  selectedColor.value = '';
  emit('update:modelValue', '');
  emit('change', '');
  popoverVisible.value = false;
}
</script>

<style scoped>
.color-picker-wrapper {
  display: inline-block;
}

.color-picker-panel {
  padding: 8px;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.color-grid {
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 8px;
  margin-bottom: 8px;
}

.color-cell {
  width: 24px;
  height: 24px;
  border: 1px solid #d9d9d9;
  border-radius: 2px;
  cursor: pointer;
  position: relative;
  transition: all 0.3s;
}

.color-cell:hover {
  transform: scale(1.1);
  border-color: #1890ff;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}

.color-cell.selected {
  border-color: #1890ff;
  border-width: 2px;
}

.check-icon {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  color: #fff;
  font-size: 14px;
  text-shadow: 0 0 2px rgba(0, 0, 0, 0.5);
}

.color-actions {
  border-top: 1px solid #f0f0f0;
  padding-top: 8px;
  text-align: right;
}

.color-picker-btn {
  display: flex;
  align-items: center;
  gap: 4px;
}

.color-preview {
  width: 14px;
  height: 14px;
  border: 1px solid #d9d9d9;
  border-radius: 2px;
  display: inline-block;
}
</style>