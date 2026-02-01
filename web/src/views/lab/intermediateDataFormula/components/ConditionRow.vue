<template>
  <div :class="['condition-row', size]">
    <!-- 左侧表达式 -->
    <div class="expr-input">
      <!-- 普通字段选择模式 -->
      <div v-if="!isCustomExprMode" class="field-select-wrapper">
        <a-select :value="condition.leftExpr" class="field-select" placeholder="搜索或选择字段" show-search allow-clear
          :disabled="readOnly" :filter-option="filterOption" :not-found-content="searchNotFound"
          option-filter-prop="label" @change="(val) => handleFieldChange(val)" @search="onSearch">
          <template #dropdownRender="{ menuNode: menu }">
            <div>
              <div class="dropdown-search-hint">
                <Icon icon="ant-design:search-outlined" :size="12" />
                <span>输入关键字搜索字段</span>
              </div>
              <component :is="menu" />
              <a-divider style="margin: 4px 0" />
              <div class="dropdown-custom-expr" @click.stop="switchToCustomExpr">
                <Icon icon="ant-design:function-outlined" :size="14" />
                <span>使用自定义表达式 (如: A - B)</span>
              </div>
            </div>
          </template>

          <a-select-option v-for="field in fieldOptions" :key="field.value" :value="field.value" :label="field.label">
            <div class="field-option">
              <span class="field-label">{{ field.label }}</span>
              <span v-if="field.value !== field.label" class="field-code">{{ field.value }}</span>
            </div>
          </a-select-option>
        </a-select>
      </div>

      <!-- 自定义表达式模式 -->
      <div v-else class="custom-expr-wrapper">
        <div class="custom-expr-display" @click="!readOnly && openExprEditor()"
          :style="{ cursor: readOnly ? 'default' : 'pointer' }">
          <span class="expr-text" :title="condition.leftExpr">
            {{ formatExprDisplay(condition.leftExpr) || '点击编辑表达式' }}
          </span>
          <Icon v-if="!readOnly" icon="ant-design:edit-outlined" :size="14" class="edit-icon" />
        </div>
        <a-tooltip v-if="!readOnly" title="切换回字段选择">
          <a-button type="text" size="small" class="switch-btn" @click="switchToFieldSelect">
            <template #icon>
              <Icon icon="ant-design:unordered-list-outlined" :size="12" />
            </template>
          </a-button>
        </a-tooltip>
      </div>
    </div>

    <!-- 操作符 -->
    <div class="operator-select">
      <a-select :value="condition.operator" class="w-full" show-search :disabled="readOnly"
        :filter-option="filterOperator" @change="(val) => emit('update', { operator: val })">
        <a-select-option v-for="op in OPERATORS" :key="op.value" :value="op.value" :label="op.label">
          {{ op.label }}
        </a-select-option>
      </a-select>
    </div>

    <!-- 右侧值 -->
    <div class="value-input">
      <template v-if="!['IS_NULL', 'NOT_NULL'].includes(condition.operator)">
        <a-select v-if="useFeatureValueSelect" :value="parseListValue(condition.rightValue)" class="w-full" show-search
          allow-clear mode="multiple" :disabled="readOnly" :options="featureValueOptions" option-filter-prop="label"
          @change="(val) => handleFeatureValueChange(val)" />
        <a-input v-else :value="condition.rightValue" placeholder="比较值" :disabled="readOnly"
          @change="(e) => emit('update', { rightValue: e.target.value })" />
      </template>
      <span v-else class="no-value-hint">(无需输入)</span>
    </div>

    <!-- 删除按钮 -->
    <a-button v-if="!readOnly" type="text" size="small" danger class="delete-btn" @click="emit('remove')">
      <template #icon>
        <Icon icon="ant-design:minus-circle-outlined" :size="14" />
      </template>
    </a-button>
  </div>
</template>

<script lang="ts" setup>
import { PropType, ref, computed } from 'vue';
import { Icon } from '/@/components/Icon';
import { Condition, OPERATORS } from './advancedJudgmentTypes';

const props = defineProps({
  condition: {
    type: Object as PropType<Condition>,
    required: true,
  },
  fieldOptions: {
    type: Array as PropType<
      {
        label: string;
        value: string;
        featureCategories?: { id: string; name: string }[];
        featureLevels?: { id: string; name: string }[];
      }[]
    >,
    default: () => [],
  },
  size: {
    type: String as PropType<'default' | 'small'>,
    default: 'default',
  },
  readOnly: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['update', 'remove', 'openFormulaEditor']);

const searchValue = ref('');
const searchNotFound = ref('未找到匹配字段');

const selectedField = computed(() => {
  return props.fieldOptions.find(f => f.value === props.condition.leftExpr);
});

const useFeatureValueSelect = computed(() => {
  if (!selectedField.value) return false;
  if (props.condition.leftExpr === 'AppearanceFeatureCategoryIds') {
    return (selectedField.value.featureCategories?.length || 0) > 0;
  }
  if (props.condition.leftExpr === 'AppearanceFeatureLevelIds') {
    return (selectedField.value.featureLevels?.length || 0) > 0;
  }
  if (props.condition.leftExpr === 'AppearanceFeatureIds') {
    return (selectedField.value.featureCategories?.length || 0) > 0;
  }
  return false;
});

const featureValueOptions = computed(() => {
  if (!selectedField.value) return [];
  if (props.condition.leftExpr === 'AppearanceFeatureCategoryIds') {
    return (selectedField.value.featureCategories || []).map(item => ({
      label: item.name,
      value: item.id,
    }));
  }
  if (props.condition.leftExpr === 'AppearanceFeatureLevelIds') {
    return (selectedField.value.featureLevels || []).map(item => ({
      label: item.name,
      value: item.id,
    }));
  }

  if (props.condition.leftExpr === 'AppearanceFeatureIds') {
    // In backend, we mapped features to featureCategories for this specific column
    return (selectedField.value.featureCategories || []).map(item => ({
      label: item.name,
      value: item.id,
    }));
  }
  return [];
});

// 判断当前是否为自定义表达式模式
const isCustomExprMode = computed(() => {
  const expr = props.condition.leftExpr;
  if (!expr) return false;

  // 如果表达式包含运算符或括号,认为是自定义表达式
  if (/[\+\-\*\/\(\)]/.test(expr)) return true;

  // 如果包含方括号包裹的字段名,认为是自定义表达式
  if (/\[.+\]/.test(expr)) return true;

  return false;
});

// 格式化表达式显示(将 [ColumnName] 转换为更友好的显示)
function formatExprDisplay(expr: string): string {
  if (!expr) return '';

  // 尝试替换 [ColumnName] 为对应的 label
  let result = expr;
  const fieldMatches = expr.match(/\[([^\]]+)\]/g);
  if (fieldMatches) {
    fieldMatches.forEach(match => {
      const columnName = match.slice(1, -1);
      const field = props.fieldOptions.find(f => f.value === columnName);
      if (field) {
        // 只在显示时用简短形式
        result = result.replace(match, field.label);
      }
    });
  }
  return result;
}

// 字段搜索过滤
function filterOption(input: string, option: any): boolean {
  if (!input) return true;
  const searchLower = input.toLowerCase().trim();
  const label = String(option?.label || '').toLowerCase();
  const value = String(option?.value || '').toLowerCase();
  return label.includes(searchLower) || value.includes(searchLower);
}

// 操作符搜索过滤
function filterOperator(input: string, option: any): boolean {
  if (!input) return true;
  const searchLower = input.toLowerCase().trim();
  const label = String(option?.label || '').toLowerCase();
  return label.includes(searchLower);
}

// 搜索时更新提示
function onSearch(value: string) {
  searchValue.value = value;
  if (value && props.fieldOptions.length > 0) {
    const hasMatch = props.fieldOptions.some(f =>
      f.label.toLowerCase().includes(value.toLowerCase()) ||
      f.value.toLowerCase().includes(value.toLowerCase())
    );
    searchNotFound.value = hasMatch ? '' : `未找到 "${value}" 相关字段`;
  }
}

// 处理字段选择变化
function handleFieldChange(val: string) {
  const updateData: any = { leftExpr: val };

  // Check if selected field is a feature field
  const field = props.fieldOptions.find(f => f.value === val);
  if (field && (
    (field.featureCategories && field.featureCategories.length > 0) ||
    (field.featureLevels && field.featureLevels.length > 0) ||
    val === 'AppearanceFeatureIds'
  )) {
    // Removed forced defaults: updateData.operator = 'CONTAINS_ANY'; updateData.rightValue = '[]';
    // Default reset logic:
    updateData.rightValue = '';
  } else {
    // If switching away from feature field, maybe reset operator if it was CONTAINS_ANY?
    // Optional, but good UX.
    if (props.condition.operator === 'CONTAINS_ANY' || props.condition.operator === 'CONTAINS_ALL') {
      updateData.operator = '=';
    }
  }

  emit('update', updateData);
}

function parseListValue(val: string): string[] {
  if (!val) return [];
  try {
    if (val.startsWith('[')) {
      return JSON.parse(val);
    }
    return val.split(',').filter(s => s);
  } catch {
    return [];
  }
}

function handleFeatureValueChange(val: string[]) {
  // Serialize to JSON string for backend
  emit('update', { rightValue: JSON.stringify(val) });
}

// 切换到自定义表达式模式
function switchToCustomExpr() {
  // 如果当前有选中的字段,转换为表达式格式
  const currentExpr = props.condition.leftExpr;
  const initialExpr = currentExpr && !currentExpr.includes('[')
    ? `[${currentExpr}]`
    : currentExpr || '';

  // 发射事件,请求打开公式编辑器
  emit('openFormulaEditor', {
    currentValue: initialExpr,
    conditionId: props.condition.id,
    onSave: (formula: string) => {
      emit('update', { leftExpr: formula });
    }
  });
}

// 切换回字段选择模式
function switchToFieldSelect() {
  emit('update', { leftExpr: '' });
}

// 打开表达式编辑器
function openExprEditor() {
  emit('openFormulaEditor', {
    currentValue: props.condition.leftExpr || '',
    conditionId: props.condition.id,
    onSave: (formula: string) => {
      emit('update', { leftExpr: formula });
    }
  });
}
</script>

<style lang="less" scoped>
.condition-row {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  transition: all 0.2s;

  &:hover {
    border-color: #93c5fd;
    box-shadow: 0 2px 8px rgba(59, 130, 246, 0.1);
  }

  &.small {
    padding: 6px 8px;
    gap: 6px;

    .expr-input {
      min-width: 100px;
    }

    .operator-select {
      width: 180px;
    }

    .value-input {
      min-width: 80px;
    }
  }

  .expr-input {
    flex: 1.5;
    min-width: 130px;

    .field-select-wrapper {
      width: 100%;

      .field-select {
        width: 100%;
      }
    }

    .custom-expr-wrapper {
      display: flex;
      align-items: center;
      gap: 4px;
      width: 100%;

      .custom-expr-display {
        flex: 1;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 4px 8px;
        background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
        border: 1px solid #7dd3fc;
        border-radius: 4px;
        cursor: pointer;
        min-height: 32px;
        transition: all 0.2s;

        &:hover {
          border-color: #38bdf8;
          background: linear-gradient(135deg, #e0f2fe 0%, #bae6fd 100%);

          .edit-icon {
            color: #0284c7;
          }
        }

        .expr-text {
          flex: 1;
          font-family: 'Consolas', 'Monaco', monospace;
          font-size: 12px;
          color: #0369a1;
          overflow: hidden;
          text-overflow: ellipsis;
          white-space: nowrap;
        }

        .edit-icon {
          color: #38bdf8;
          flex-shrink: 0;
          margin-left: 4px;
        }
      }

      .switch-btn {
        flex-shrink: 0;
        color: #94a3b8;

        &:hover {
          color: #3b82f6;
          background: #eff6ff;
        }
      }
    }
  }

  .operator-select {
    width: 240px;
    flex-shrink: 0;
  }

  .value-input {
    flex: 1;
    min-width: 100px;

    .no-value-hint {
      display: block;
      padding: 4px 8px;
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      border-radius: 4px;
      color: #94a3b8;
      font-size: 12px;
      text-align: center;
    }
  }

  .delete-btn {
    flex-shrink: 0;
    color: #ef4444;

    &:hover {
      background: #fef2f2;
    }
  }
}

// 下拉框选项样式
.field-option {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;

  .field-label {
    flex: 1;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .field-code {
    font-size: 11px;
    color: #94a3b8;
    background: #f1f5f9;
    padding: 2px 6px;
    border-radius: 3px;
    font-family: monospace;
    max-width: 100px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
}

// 下拉框搜索提示
.dropdown-search-hint {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  color: #64748b;
  font-size: 12px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;
}

// 自定义表达式选项
.dropdown-custom-expr {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 12px;
  color: #7c3aed;
  font-size: 13px;
  cursor: pointer;
  transition: all 0.2s;
  background: #faf5ff;

  &:hover {
    background: #f3e8ff;
    color: #6d28d9;
  }
}

// 确保下拉框有足够宽度显示内容
:deep(.ant-select-dropdown) {
  min-width: 220px !important;
}

:deep(.ant-select-item-option-content) {
  overflow: visible;
}
</style>
