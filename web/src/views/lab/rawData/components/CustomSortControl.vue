<template>
  <div class="custom-sort-control" ref="controlRef">
    <a-button
      @click="openEditor"
      :loading="buttonLoading"
      @mousedown="onButtonMouseDown"
      @mouseup="onButtonMouseUp"
    >
      <SortAscendingOutlined />
      自定义排序
    </a-button>

    <div v-if="sortRules.length > 0" class="sort-rules-display">
      <span class="rules-label">当前排序：</span>
      <span class="rules-content">
        <template v-for="(rule, index) in displayRules" :key="index">
          <span v-if="index > 0" class="separator">→</span>
          <a-tooltip :title="getRuleTooltip(rule)">
            <span class="rule-item">
              {{ rule.label }}
              <span class="order-icon">{{ rule.order === 'asc' ? '↑' : '↓' }}</span>
            </span>
          </a-tooltip>
        </template>
      </span>
      <a-button
        type="link"
        size="small"
        @click="clearSort"
        class="clear-btn"
      >
        清除
      </a-button>
    </div>
    <div v-else class="sort-rules-placeholder">
      默认排序：生产日期→炉次号→卷号→分卷号→产线
    </div>

    <!-- 调试信息 -->
    <div v-if="debugMode" class="debug-info">
      <div>编辑器可见性: {{ editorVisible }}</div>
      <div>按钮加载状态: {{ buttonLoading }}</div>
      <div>排序规则数量: {{ sortRules.length }}</div>
      <div>错误信息: {{ lastError || '无' }}</div>
    </div>

    <CustomSortEditor
      v-model:visible="editorVisible"
      v-model:sortRules="sortRules"
      @change="handleSortChange"
      @error="handleEditorError"
      @open="onEditorOpen"
      @close="onEditorClose"
    />
  </div>
</template>

<script lang="ts" setup>
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from 'vue';
import { SortAscendingOutlined } from '@ant-design/icons-vue';
import CustomSortEditor from './CustomSortEditor.vue';
import { message } from 'ant-design-vue';

interface SortRule {
  field: string;
  order: 'asc' | 'desc';
}

interface FieldMap {
  value: string;
  label: string;
}

interface Props {
  modelValue?: SortRule[];
}

interface Emits {
  (e: 'update:modelValue', value: SortRule[]): void;
  (e: 'change', value: SortRule[]): void;
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: () => [],
});

const emit = defineEmits<Emits>();

// 调试模式
const debugMode = ref(false);
const lastError = ref('');
const controlRef = ref();

// 按钮加载状态
const buttonLoading = ref(false);

// 字段映射
const fieldMap: FieldMap[] = [
  { value: 'prodDate', label: '生产日期' },
  { value: 'furnaceBatchNo', label: '炉次号' },
  { value: 'coilNo', label: '卷号' },
  { value: 'subcoilNo', label: '分卷号' },
  { value: 'lineNo', label: '产线' },
  { value: 'productSpecName', label: '产品规格' },
  { value: 'creatorTime', label: '录入日期' },
];

// 排序规则
const sortRules = ref<SortRule[]>(props.modelValue || []);

// 编辑器可见性
const editorVisible = ref(false);

// 显示的排序规则
const displayRules = computed(() => {
  return sortRules.value.map(rule => {
    const field = fieldMap.find(f => f.value === rule.field);
    return {
      ...rule,
      label: field?.label || rule.field,
    };
  });
});

// 生命周期钩子
onMounted(() => {
  console.log('CustomSortControl mounted');
  console.log('Initial sort rules:', sortRules.value);
  console.log('Initial editor visible:', editorVisible.value);

  // 添加全局错误监听
  window.addEventListener('error', handleGlobalError);
});

onUnmounted(() => {
  window.removeEventListener('error', handleGlobalError);
});

function handleGlobalError(event: ErrorEvent) {
  // 忽略 ResizeObserver 的常见警告，这是浏览器的一个已知问题
  if (event.message && event.message.includes('ResizeObserver loop completed with undelivered notifications')) {
    // 这是一个无害的警告，可以安全忽略
    return;
  }
  console.error('Global error in CustomSortControl:', event);
  lastError.value = event.message;
}

function onButtonMouseDown() {
  console.log('Button mouse down');
}

function onButtonMouseUp() {
  console.log('Button mouse up');
}

// 打开编辑器
function openEditor() {
  console.log('=== 开始打开自定义排序编辑器 ===');
  console.log('当前时间:', new Date().toLocaleString());
  console.log('当前sortRules:', sortRules.value);
  console.log('当前editorVisible:', editorVisible.value);

  buttonLoading.value = true;
  lastError.value = '';

  try {
    // 直接设置可见性
    editorVisible.value = true;
    console.log('已设置 editorVisible = true');

    // 使用 nextTick 确保 DOM 更新
    nextTick(() => {
      console.log('nextTick 回调执行');
      console.log('更新后的 editorVisible:', editorVisible.value);
      buttonLoading.value = false;

      // 如果模态框仍然没有显示，尝试强制显示
      if (!editorVisible.value) {
        console.error('❌ 模态框未能成功显示！');
        lastError.value = '模态框未能成功显示';
        message.error('无法打开自定义排序编辑器，请检查控制台错误信息');
      } else {
        console.log('✅ 模态框显示成功');
      }
    });

    // 延迟检查，确保模态框已经渲染
    setTimeout(() => {
      console.log('延迟检查 - editorVisible:', editorVisible.value);

      // 检查 DOM 中是否有模态框元素
      const modalElements = document.querySelectorAll('.ant-modal-root, .ant-modal-mask, .ant-modal-wrap');
      console.log('DOM 中找到的模态框元素数量:', modalElements.length);

      modalElements.forEach((el, index) => {
        console.log(`模态框元素 ${index + 1}:`, {
          className: el.className,
          display: window.getComputedStyle(el).display,
          visibility: window.getComputedStyle(el).visibility,
          zIndex: window.getComputedStyle(el).zIndex
        });
      });
    }, 500);

  } catch (error) {
    console.error('❌ 打开编辑器时发生错误:', error);
    lastError.value = (error as Error).message;
    message.error('打开编辑器时出错：' + (error as Error).message);
    buttonLoading.value = false;
  }
}

// 清除排序
function clearSort() {
  console.log('清除排序规则');
  sortRules.value = [];
  emit('update:modelValue', []);
  emit('change', []);
}

// 处理排序变化
function handleSortChange(newRules: SortRule[]) {
  console.log('Sort rules changed:', newRules);
  sortRules.value = newRules;
  emit('update:modelValue', newRules);
  emit('change', newRules);
}

// 处理编辑器错误
function handleEditorError(error: Error) {
  console.error('Editor error:', error);
  lastError.value = error.message;
  message.error('编辑器错误：' + error.message);
}

// 编辑器打开事件
function onEditorOpen() {
  console.log('CustomSortEditor opened');
}

// 编辑器关闭事件
function onEditorClose() {
  console.log('CustomSortEditor closed');
}

// 获取规则提示
function getRuleTooltip(rule: SortRule & { label: string }) {
  return `${rule.label} - ${rule.order === 'asc' ? '正序' : '倒序'}`;
}

// 监听外部值变化
watch(
  () => props.modelValue,
  (newValue) => {
    console.log('modelValue changed:', newValue);
    sortRules.value = newValue || [];
  },
  { deep: true }
);

// 监听编辑器可见性变化
watch(editorVisible, (newValue) => {
  console.log('Editor visibility changed to:', newValue);

  // 如果变为true，检查模态框是否真的显示
  if (newValue) {
    setTimeout(() => {
      const visibleModals = document.querySelectorAll('.ant-modal[style*="display: block"], .ant-modal:not([style*="display: none"])');
      console.log('实际显示的模态框数量:', visibleModals.length);
    }, 300);
  }
});
</script>

<style lang="less" scoped>
.custom-sort-control {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 6px 0;

  * {
    box-sizing: border-box;
  }

  .ant-btn {
    height: 32px;
    padding: 4px 14px;
    font-size: 13px;
    border-radius: 6px;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);

    &:not(.ant-btn-link) {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      font-weight: 500;

      &:hover {
        transform: translateY(-1px);
        box-shadow: 0 4px 8px rgba(24, 144, 255, 0.2);
      }

      &:active {
        transform: translateY(0);
      }
    }
  }

  .debug-info {
    position: fixed;
    top: 10px;
    right: 10px;
    background: #f0f0f0;
    border: 1px solid #ccc;
    padding: 10px;
    border-radius: 4px;
    font-size: 12px;
    z-index: 9999;
    max-width: 300px;

    div {
      margin: 2px 0;
    }
  }

  .sort-rules-display {
    flex: 1;
    display: flex;
    align-items: center;
    gap: 8px;
    padding: 6px 12px;
    background: linear-gradient(135deg, #f8f9fa 0%, #ffffff 100%);
    border: 1px solid #e8e8e8;
    border-radius: 6px;
    font-size: 13px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
    transition: all 0.3s;

    &:hover {
      border-color: #d4edda;
      box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
    }

    .rules-label {
      color: #595959;
      white-space: nowrap;
      flex-shrink: 0;
      font-weight: 500;
    }

    .rules-content {
      flex: 1;
      min-width: 0;
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 6px;

      .rule-item {
        display: inline-flex;
        align-items: center;
        padding: 2px 8px;
        background: linear-gradient(135deg, #e6f7ff 0%, #bae7ff 100%);
        border: 1px solid #91d5ff;
        border-radius: 4px;
        cursor: default;
        transition: all 0.3s;
        font-size: 12px;
        font-weight: 500;
        color: #1890ff;

        &:hover {
          background: linear-gradient(135deg, #bae7ff 0%, #91d5ff 100%);
          border-color: #69c0ff;
          transform: translateY(-1px);
          box-shadow: 0 2px 4px rgba(24, 144, 255, 0.2);
        }

        .order-icon {
          margin-left: 6px;
          font-size: 14px;
          font-weight: 600;
        }
      }

      .separator {
        color: #bfbfbf;
        font-weight: 500;
        font-size: 16px;
        margin: 0 2px;
      }
    }

    .clear-btn {
      flex-shrink: 0;
      margin-left: 8px;
      padding: 0 8px;
      height: auto;
      line-height: 1.5;
      color: #8c8c8c;
      font-size: 13px;
      transition: all 0.3s;

      &:hover {
        color: #ff4d4f;
        background-color: #fff1f0;
      }
    }
  }

  .sort-rules-placeholder {
    flex: 1;
    color: #8c8c8c;
    font-size: 13px;
    padding: 6px 12px;
    background: #fafafa;
    border: 1px dashed #d9d9d9;
    border-radius: 6px;
    text-align: center;
    transition: all 0.3s;

    &:hover {
      border-color: #91d5ff;
      background: #f0f9ff;
      color: #595959;
    }
  }
}

@media (max-width: 768px) {
  .custom-sort-control {
    flex-direction: column;
    align-items: stretch;

    .sort-rules-display {
      flex-direction: column;
      align-items: flex-start;
      gap: 4px;
    }
  }
}
</style>

<style lang="less">
// 修复tooltip
.ant-tooltip {
  max-width: 200px;
  z-index: 1070;

  .ant-tooltip-inner {
    word-wrap: break-word;
  }
}
</style>