<template>
  <!-- 添加调试遮罩层 -->
  <div v-if="debugVisible" class="debug-modal-overlay">
    <div class="debug-info">
      <h4>调试信息</h4>
      <div>模态框可见性: {{ visible }}</div>
      <div>原始visible: {{ props.visible }}</div>
      <div>本地排序规则: {{ localSortRules.length }} 条</div>
      <div>错误信息: {{ lastError || '无' }}</div>
      <div>渲染次数: {{ renderCount }}</div>
      <button @click="forceShow" class="debug-btn">强制显示模态框</button>
      <button @click="toggleDebug" class="debug-btn">关闭调试</button>
    </div>
  </div>

  <a-modal
    v-model:open="visible"
    title="自定义排序"
    width="600px"
    @ok="handleOk"
    @cancel="handleCancel"
    :confirmLoading="confirmLoading"
    :maskClosable="false"
    :forceRender="true"
    :getContainer="getContainer"
    @afterOpenChange="onAfterOpenChange"
  >
    <!-- 模态框内容 -->
    <div class="sort-editor-content" v-if="visible">
      <div class="sort-rules-list">
        <div class="sort-rules-header">
          <span>排序规则列表</span>
          <a-button type="link" @click="addRule" size="small">
            <PlusOutlined />
            添加规则
          </a-button>
        </div>

        <div class="sort-rules-body">
          <draggable
            v-model="localSortRules"
            item-key="id"
            handle=".drag-handle"
            @change="onRulesChange"
            :forceFallback="true"
          >
            <template #item="{ element, index }">
              <div class="sort-rule-item" :key="element.id"
                   :data-rule-id="element.id"
                   :data-rule-field="element.field"
              >
                <div class="drag-handle">
                  <HolderOutlined />
                </div>
                <div class="rule-content">
                  <a-select
                    v-model:value="element.field"
                    style="width: 160px"
                    placeholder="选择字段"
                    @change="onRuleChange"
                    :disabled="!element.id"
                  >
                    <a-select-option
                      v-for="field in availableFields"
                      :key="field.value"
                      :value="field.value"
                      :disabled="isFieldDisabled(field.value, element.id)"
                    >
                      {{ field.label }}
                    </a-select-option>
                  </a-select>

                  <a-radio-group
                    v-model:value="element.order"
                    size="small"
                    @change="onRuleChange"
                  >
                    <a-radio-button value="asc">正序 ↑</a-radio-button>
                    <a-radio-button value="desc">倒序 ↓</a-radio-button>
                  </a-radio-group>
                </div>
                <div class="rule-actions">
                  <a-popconfirm
                    title="确定要删除这条排序规则吗？"
                    @confirm="removeRule(index)"
                    okText="确定"
                    cancelText="取消"
                  >
                    <a-button type="text" danger size="small">
                      <DeleteOutlined />
                    </a-button>
                  </a-popconfirm>
                </div>
              </div>
            </template>
          </draggable>

          <div v-if="localSortRules.length === 0" class="empty-rules">
            <a-empty description="暂无排序规则" />
          </div>
        </div>
      </div>

      <div class="sort-preview">
        <div class="preview-header">当前排序预览：</div>
        <div class="preview-content">
          <div v-if="sortRules.length > 0" class="preview-rules">
            <div
              v-for="(rule, index) in sortRules"
              :key="index"
              class="preview-rule"
            >
              <span class="rule-index">{{ index + 1 }}.</span>
              <span class="rule-field">{{ getFieldLabel(rule.field) }}</span>
              <span class="rule-order">{{ rule.order === 'asc' ? '正序' : '倒序' }}</span>
            </div>
          </div>
          <div v-else class="preview-default">
            默认排序：检测日期 → 炉号 → 卷号 → 分卷号 → 产线
          </div>
        </div>
      </div>
    </div>

    <!-- 错误提示 -->
    <div v-if="hasError" class="error-message">
      <a-alert
        message="发生错误"
        :description="lastError"
        type="error"
        closable
        @close="clearError"
      />
    </div>
  </a-modal>
</template>

<script lang="ts" setup>
import { ref, reactive, computed, watch, nextTick } from 'vue';
import { Modal, message, Alert } from 'ant-design-vue';
import draggable from 'vuedraggable';
import { PlusOutlined, DeleteOutlined, HolderOutlined } from '@ant-design/icons-vue';

interface SortRule {
  id: string;
  field: string;
  order: 'asc' | 'desc';
}

interface Props {
  visible: boolean;
  sortRules: SortRule[];
}

interface Emits {
  (e: 'update:visible', visible: boolean): void;
  (e: 'update:sortRules', rules: SortRule[]): void;
  (e: 'change', rules: SortRule[]): void;
  (e: 'error', error: Error): void;
  (e: 'open'): void;
  (e: 'close'): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

// 调试状态
const debugVisible = ref(true);
const renderCount = ref(0);
const hasError = ref(false);
const lastError = ref('');

// 确认加载状态
const confirmLoading = ref(false);

// 可用字段
const availableFields = [
  { value: 'prodDate', label: '检测日期' },
  { value: 'furnaceNoParsed', label: '炉号' },
  { value: 'coilNo', label: '卷号' },
  { value: 'subcoilNo', label: '分卷号' },
  { value: 'lineNo', label: '产线' },
  { value: 'productSpecName', label: '产品规格' },
  { value: 'creatorTime', label: '录入日期' },
];

// 本地排序规则副本
const localSortRules = ref<SortRule[]>([]);

// 监听外部规则变化
watch(
  () => props.sortRules,
  (newRules) => {
    try {
      localSortRules.value = newRules.map((rule, index) => ({
        ...rule,
        id: rule.id || `rule-${index}`,
      }));
    } catch (error) {
      console.error('CustomSortEditor: Error updating local sort rules:', error);
      lastError.value = (error as Error).message;
      hasError.value = true;
      emit('error', error as Error);
    }
  },
  { immediate: true }
);

// 计算是否可见
const visible = computed({
  get: () => {
    return props.visible;
  },
  set: (val) => {
    emit('update:visible', val);
  },
});

// 渲染计数
watch(() => [props.visible, localSortRules.value], () => {
  renderCount.value++;
}, { immediate: true });

// 获取容器元素
function getContainer() {
  // 尝试返回body，确保模态框在顶层渲染
  return document.body;
}

// 模态框打开状态变化后的回调
function onAfterOpenChange(open: boolean) {

  if (open) {
    emit('open');

    // 强制检查DOM
    nextTick(() => {
      const modalElement = document.querySelector('.ant-modal-wrap');

      if (modalElement) {
      }
    });
  } else {
    emit('close');
  }
}

// 添加规则
function addRule() {
  try {
    const newRule: SortRule = {
      id: `rule-${Date.now()}`,
      field: '',
      order: 'asc',
    };
    localSortRules.value.push(newRule);
  } catch (error) {
    console.error('CustomSortEditor: Error adding rule:', error);
    lastError.value = (error as Error).message;
    hasError.value = true;
    emit('error', error as Error);
  }
}

// 删除规则
function removeRule(index: number) {
  try {
    localSortRules.value.splice(index, 1);
    onRuleChange();
  } catch (error) {
    console.error('CustomSortEditor: Error removing rule:', error);
    lastError.value = (error as Error).message;
    hasError.value = true;
    emit('error', error as Error);
  }
}

// 规则变化处理
function onRuleChange() {
  try {
    emit('update:sortRules', [...localSortRules.value]);
  } catch (error) {
    console.error('CustomSortEditor: Error on rule change:', error);
    lastError.value = (error as Error).message;
    hasError.value = true;
    emit('error', error as Error);
  }
}

// 规则顺序变化
function onRulesChange() {
  try {
    onRuleChange();
  } catch (error) {
    console.error('CustomSortEditor: Error on rules change:', error);
    lastError.value = (error as Error).message;
    hasError.value = true;
    emit('error', error as Error);
  }
}

// 获取字段标签
function getFieldLabel(field: string) {
  const fieldObj = availableFields.find(f => f.value === field);
  return fieldObj ? fieldObj.label : field;
}

// 检查字段是否已禁用（已在其他规则中使用）
function isFieldDisabled(field: string, currentRuleId: string) {
  return localSortRules.value.some(rule => rule.field === field && rule.id !== currentRuleId);
}

// 确认
async function handleOk() {
  try {
    confirmLoading.value = true;

    // 过滤掉未选择字段的规则
    const validRules = localSortRules.value.filter(rule => rule.field);

    if (validRules.length === 0 && localSortRules.value.length > 0) {
      message.warning('请为所有排序规则选择字段');
      confirmLoading.value = false;
      return;
    }

    emit('update:sortRules', validRules);
    emit('change', validRules);

    // 延迟关闭以确保状态更新
    setTimeout(() => {
      visible.value = false;
      confirmLoading.value = false;
    }, 100);

  } catch (error) {
    console.error('CustomSortEditor: Error confirming sort rules:', error);
    lastError.value = (error as Error).message;
    hasError.value = true;
    emit('error', error as Error);
    confirmLoading.value = false;
    message.error('保存排序规则失败：' + (error as Error).message);
  }
}

// 取消
function handleCancel() {
  try {
    // 恢复原始规则
    localSortRules.value = props.sortRules.map((rule, index) => ({
      ...rule,
      id: rule.id || `rule-${index}`,
    }));
    visible.value = false;
  } catch (error) {
    console.error('CustomSortEditor: Error canceling editor:', error);
    lastError.value = (error as Error).message;
    hasError.value = true;
    emit('error', error as Error);
  }
}

// 切换调试模式
function toggleDebug() {
  debugVisible.value = !debugVisible.value;
}

// 强制显示模态框
function forceShow() {
  visible.value = true;
  nextTick(() => {
    // 强制修改DOM样式
    const modalWrap = document.querySelector('.ant-modal-wrap');
    if (modalWrap) {
      (modalWrap as HTMLElement).style.display = 'block';
      (modalWrap as HTMLElement).style.visibility = 'visible';
    }
  });
}

// 清除错误
function clearError() {
  hasError.value = false;
  lastError.value = '';
}

// 监听可见性变化
watch(visible, (newValue) => {
  if (newValue) {
    // 模态框打开时重新同步数据
    localSortRules.value = props.sortRules.map((rule, index) => ({
      ...rule,
      id: rule.id || `rule-${index}`,
    }));

    // 强制重新渲染
    nextTick(() => {
      renderCount.value++;
    });
  }
});
</script>

<style lang="less" scoped>
.sort-editor-content {
  .sort-rules-list {
    margin-bottom: 20px;

    .sort-rules-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 12px;
      padding-bottom: 8px;
      border-bottom: 1px solid #f0f0f0;
      font-weight: 500;
    }

    .sort-rules-body {
      max-height: 300px;
      overflow-y: auto;

      .sort-rule-item {
        display: flex;
        align-items: center;
        padding: 8px 12px;
        margin-bottom: 8px;
        background-color: #fafafa;
        border-radius: 6px;
        cursor: move;
        transition: all 0.3s;

        &:hover {
          background-color: #f5f5f5;
          box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        .drag-handle {
          margin-right: 12px;
          color: #999;
          cursor: move;

          &:hover {
            color: #666;
          }
        }

        .rule-content {
          flex: 1;
          display: flex;
          align-items: center;
          gap: 12px;
        }

        .rule-actions {
          margin-left: 12px;
        }
      }

      .empty-rules {
        padding: 40px 0;
        text-align: center;
      }
    }
  }

  .sort-preview {
    padding: 16px;
    background-color: #f6f7f9;
    border-radius: 6px;

    .preview-header {
      margin-bottom: 12px;
      font-size: 14px;
      color: #666;
    }

    .preview-content {
      .preview-rules {
        .preview-rule {
          display: flex;
          align-items: center;
          padding: 4px 0;
          font-size: 14px;

          .rule-index {
            margin-right: 8px;
            color: #999;
            min-width: 20px;
          }

          .rule-field {
            margin-right: 8px;
            color: #262626;
            font-weight: 500;
          }

          .rule-order {
            color: #666;
            font-size: 12px;
          }
        }
      }

      .preview-default {
        color: #999;
        font-size: 14px;
      }
    }
  }

  .error-message {
    margin-top: 16px;
  }
}

// 调试遮罩层
.debug-modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;

  .debug-info {
    background: white;
    padding: 20px;
    border-radius: 8px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    max-width: 400px;
    width: 90%;

    h4 {
      margin-top: 0;
      margin-bottom: 12px;
      color: #333;
    }

    div {
      margin: 4px 0;
      font-size: 14px;
    }

    .debug-btn {
      margin-top: 12px;
      margin-right: 8px;
      padding: 6px 12px;
      background: #1890ff;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-size: 14px;

      &:hover {
        background: #40a9ff;
      }

      &:active {
        background: #096dd9;
      }
    }
  }
}
</style>

<style lang="less">
// 修复拖拽时的样式问题
.sort-rule-item.sortable-ghost {
  opacity: 0.5;
}
</style>