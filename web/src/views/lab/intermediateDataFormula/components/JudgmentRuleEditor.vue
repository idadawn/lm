<template>
  <div class="judgment-editor">
    <!-- Intro -->
    <div class="intro-banner">
      <div class="icon-wrapper">
        <Icon icon="ant-design:branches-outlined" :size="20" />
      </div>
      <div class="content">
        <h3>判定逻辑配置</h3>
        <p>
          系统将按照<span class="bold">从上到下</span>
          的顺序执行规则。一旦满足某条规则的条件，即返回对应的结果，并停止后续判断。
        </p>
      </div>
    </div>

    <!-- Scrollable Content Area -->
    <div class="scrollable-content">
      <!-- Rule List -->
      <div v-for="(rule, ruleIdx) in rules" :key="rule.id" class="rule-container">
        <!-- Visual Flow Line -->
        <div class="flow-line"></div>
        <div class="rule-number">{{ ruleIdx + 1 }}</div>

        <div class="rule-card">
          <!-- Rule Header / Result -->
          <div class="rule-header">
            <div class="left">
              <span class="label">如果满足以下条件，则返回:</span>
              <div class="result-input-wrapper">
                <Icon icon="ant-design:arrow-right-outlined" :size="16" class="arrow-icon" />
                <input v-model="rule.resultValue" placeholder="例如: A" @change="emitChange" />
              </div>
            </div>
            <button @click="removeRule(ruleIdx)" class="delete-btn">删除规则</button>
          </div>

          <!-- Conditions Area -->
          <div class="conditions-area">
            <div class="conditions-wrapper">
              <!-- Logic Toggle -->
              <div class="logic-toggle">
                <span class="logic-label">当</span>
                <div class="toggle-group">
                  <button @click="updateGroupLogic(ruleIdx, 'AND')"
                    :class="{ 'active-and': rule.rootGroup.logic === 'AND' }">
                    满足所有 (AND)
                  </button>
                  <button @click="updateGroupLogic(ruleIdx, 'OR')"
                    :class="{ 'active-or': rule.rootGroup.logic === 'OR' }">
                    满足任一 (OR)
                  </button>
                </div>
                <span class="logic-label">条件时:</span>
              </div>

              <!-- Conditions List -->
              <div class="conditions-list">
                <div v-if="rule.rootGroup.conditions.length > 0"
                  :class="['logic-indicator', rule.rootGroup.logic === 'AND' ? 'and' : 'or']">
                </div>

                <div v-for="(condition, cIdx) in rule.rootGroup.conditions" :key="condition.id" class="condition-row">
                  <!-- Field Select -->
                  <div class="field-select">
                    <Select 
                      v-model:value="condition.fieldId" 
                      size="small" 
                      class="w-full" 
                      :options="fieldOptions"
                      placeholder="请选择字段" 
                      @change="emitChange"
                      :loading="fieldOptions.length === 0 && props.fields.length === 0"
                      show-search
                      :filter-option="(input, option) => {
                        const label = option?.label || '';
                        return label.toLowerCase().includes(input.toLowerCase());
                      }"
                    />
                  </div>

                  <!-- Operator Select -->
                  <div class="operator-select">
                    <Select v-model:value="condition.operator" size="small" class="w-full" :options="RULE_OPERATORS"
                      @change="emitChange" />
                  </div>

                  <!-- Value Input -->
                  <div class="value-input">
                    <Input v-if="!['IS_NULL', 'NOT_NULL'].includes(condition.operator)" v-model:value="condition.value"
                      size="small" placeholder="比较值" @change="emitChange" />
                    <span v-else class="no-value">无需输入值</span>
                  </div>

                  <button @click="removeCondition(ruleIdx, cIdx)" class="delete-condition-btn">
                    <Icon icon="ant-design:delete-outlined" :size="14" />
                  </button>
                </div>

                <div v-if="rule.rootGroup.conditions.length === 0" class="empty-state">
                  暂无条件，请添加
                </div>
              </div>

              <button @click="addCondition(ruleIdx)" class="add-condition-btn">
                <Icon icon="ant-design:plus-outlined" :size="12" /> 添加条件
              </button>
            </div>
          </div>
        </div>
      </div>

      <!-- Add Rule Button -->
      <div class="add-rule-container">
        <div class="flow-line"></div>
        <button @click="addRule" class="add-rule-btn">
          <Icon icon="ant-design:plus-outlined" :size="16" style="margin-right: 8px" /> 添加优先级规则
        </button>
      </div>
    </div>

    <!-- Default Fallback - Fixed at Bottom -->
    <div class="default-container">
      <div class="flow-line-short"></div>
      <div class="end-marker">终</div>

      <div class="default-card">
        <div class="left">
          <h4>
            <Icon icon="ant-design:check-circle-outlined" :size="20" class="check-icon" />
            默认结果 (Else)
          </h4>
          <p>如果以上所有规则都不满足，则返回此值。</p>
        </div>
        <div class="right">
          <span class="label">返回:</span>
          <input :value="defaultValueLocal" @input="updateDefaultValue" />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, PropType, computed } from 'vue';
import { Icon } from '/@/components/Icon';
import { Select, Input } from 'ant-design-vue';
import { RULE_OPERATORS, JudgmentRule } from './types';
import { buildShortUUID } from '/@/utils/uuid';

const props = defineProps({
  value: { type: String, default: '' },
  defaultValue: { type: String, default: '' },
  fields: { type: Array as PropType<any[]>, default: () => [] }
});

const emit = defineEmits(['update:value', 'update:defaultValue', 'change']);

const rules = ref<JudgmentRule[]>([]);
const defaultValueLocal = ref('');

// 关键：创建一个本地响应式副本
const localFields = ref<any[]>([]);

// 监听 props.fields 变化，同步到本地
watch(
  () => props.fields,
  (newFields) => {
    
    if (newFields && Array.isArray(newFields) && newFields.length > 0) {
      // 深拷贝到本地
      localFields.value = JSON.parse(JSON.stringify(newFields));
    }
  },
  { immediate: true, deep: true }
);

// 使用 localFields 计算 options
const fieldOptions = computed(() => {
  
  if (!localFields.value || localFields.value.length === 0) {
    return [];
  }
  
  const options = localFields.value.map(f => ({
    label: f.name || f.displayName || f.columnName || String(f),
    value: f.id || f.columnName || f.code
  }));
  
  return options;
});

// Init from props.value
watch(
  () => props.value,
  (val) => {
    try {
      if (val) {
        const parsed = JSON.parse(val);
        if (Array.isArray(parsed)) {
          rules.value = parsed;
        } else {
          rules.value = [];
        }
      } else {
        rules.value = [];
      }
    } catch (e) {
      console.error('Failed to parse rules JSON', e);
      rules.value = [];
    }
  },
  { immediate: true }
);

watch(
  () => props.defaultValue,
  (val) => {
    defaultValueLocal.value = val;
  },
  { immediate: true }
);

function emitChange() {
  const json = JSON.stringify(rules.value);
  emit('update:value', json);
  emit('change', json);
}

function updateDefaultValue(e: any) {
  const val = e.target.value;
  defaultValueLocal.value = val;
  emit('update:defaultValue', val);
}

function addRule() {
  rules.value.push({
    id: buildShortUUID(),
    resultValue: '',
    rootGroup: {
      id: buildShortUUID(),
      logic: 'AND',
      conditions: [],
    },
  });
  emitChange();
}

function removeRule(index: number) {
  rules.value.splice(index, 1);
  emitChange();
}

function updateGroupLogic(ruleIndex: number, logic: 'AND' | 'OR') {
  if (rules.value[ruleIndex]?.rootGroup) {
    rules.value[ruleIndex].rootGroup.logic = logic;
    emitChange();
  }
}

function addCondition(ruleIndex: number) {
  const rule = rules.value[ruleIndex];
  if (!rule) return;

  
  // 使用 localFields 获取默认值
  const defaultFieldId = localFields.value.length > 0 
    ? (localFields.value[0].id || localFields.value[0].columnName || '') 
    : '';
  

  rule.rootGroup.conditions.push({
    id: buildShortUUID(),
    fieldId: defaultFieldId,
    operator: '=',
    value: '',
  });
  emitChange();
}

function removeCondition(ruleIndex: number, conditionIndex: number) {
  const rule = rules.value[ruleIndex];
  if (rule?.rootGroup?.conditions) {
    rule.rootGroup.conditions.splice(conditionIndex, 1);
    emitChange();
  }
}
</script>

<style scoped lang="less">
.judgment-editor {
  display: flex;
  flex-direction: column;
  gap: 24px;
  padding: 8px;
  width: 100%;
  max-width: 100%;
  height: 100%;
  max-height: 100%;
  position: relative;
  box-sizing: border-box;
  overflow-y: auto;
  overflow-x: hidden;
}

// Intro Banner
.intro-banner {
  background: #eff6ff;
  border: 1px solid #bfdbfe;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 8px;
  display: flex;
  gap: 12px;
  flex-shrink: 0;

  .icon-wrapper {
    background: #dbeafe;
    padding: 8px;
    border-radius: 50%;
    height: fit-content;
    color: #2563eb;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .content {
    h3 {
      font-weight: 700;
      color: #1e3a8a;
      margin: 0;
      font-size: 15px;
    }
    p {
      font-size: 13px;
      color: #1d4ed8;
      margin-top: 4px;
      margin-bottom: 0;

      .bold {
        font-weight: 700;
      }
    }
  }
}

// Scrollable Content
.scrollable-content {
  flex: 1;
  padding-bottom: 24px;
  min-height: 0;
  width: 100%;
}

// Rule Container
.rule-container {
  position: relative;
  padding-left: 32px;
}

.flow-line {
  position: absolute;
  left: 12px;
  top: 0;
  bottom: 0;
  width: 2px;
  background: #e2e8f0;
}

.rule-number {
  position: absolute;
  left: 0;
  top: 24px;
  width: 24px;
  height: 24px;
  background: #f1f5f9;
  border: 2px solid #cbd5e1;
  color: #64748b;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 12px;
  font-weight: 700;
  z-index: 10;
}

.rule-card {
  background: white;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  overflow: hidden;
}

// Rule Header
.rule-header {
  background: #f8fafc;
  padding: 12px 16px;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  align-items: center;
  justify-content: space-between;

  .left {
    display: flex;
    align-items: center;
    gap: 12px;

    .label {
      font-size: 13px;
      font-weight: 500;
      color: #64748b;
    }

    .result-input-wrapper {
      display: flex;
      align-items: center;
      gap: 8px;

      .arrow-icon {
        color: #94a3b8;
      }

      input {
        width: 160px;
        font-weight: 700;
        color: #1d4ed8;
        border: none;
        border-bottom: 1px solid #93c5fd;
        background: transparent;
        padding: 2px 4px;
        outline: none;
        font-size: 14px;

        &:focus {
          border-bottom-color: #2563eb;
        }

        &::placeholder {
          color: #94a3b8;
          font-weight: normal;
        }
      }
    }
  }

  .delete-btn {
    font-size: 12px;
    color: #ef4444;
    background: none;
    border: none;
    cursor: pointer;
    padding: 4px 8px;

    &:hover {
      text-decoration: underline;
      color: #dc2626;
    }
  }
}

// Conditions Area
.conditions-area {
  padding: 16px;
  background: rgba(248, 250, 252, 0.5);

  .conditions-wrapper {
    background: white;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    padding: 12px;
  }
}

// Logic Toggle
.logic-toggle {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 12px;

  .logic-label {
    font-size: 11px;
    font-weight: 700;
    color: #64748b;
    text-transform: uppercase;
  }

  .toggle-group {
    display: inline-flex;
    background: #f1f5f9;
    padding: 2px;
    border-radius: 6px;
    border: 1px solid #e2e8f0;

    button {
      padding: 2px 12px;
      font-size: 12px;
      border-radius: 4px;
      font-weight: 500;
      transition: all 0.2s;
      border: none;
      background: transparent;
      color: #64748b;
      cursor: pointer;

      &:hover {
        color: #334155;
      }

      &.active-and {
        background: white;
        box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
        color: #2563eb;
      }

      &.active-or {
        background: white;
        box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
        color: #ea580c;
      }
    }
  }
}

// Conditions List
.conditions-list {
  position: relative;

  .logic-indicator {
    position: absolute;
    top: 8px;
    bottom: 8px;
    left: -10px;
    width: 6px;
    border-radius: 3px 0 0 3px;

    &.and {
      background: #bfdbfe;
    }

    &.or {
      background: #fed7aa;
    }
  }
}

// Condition Row
.condition-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 8px;
  background: white;
  padding: 8px;
  border-radius: 6px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.03);
  transition: border-color 0.2s;

  &:hover {
    border-color: #93c5fd;
  }

  .field-select {
    flex: 1;
    min-width: 140px;
  }

  .operator-select {
    width: 110px;
    flex-shrink: 0;
  }

  .value-input {
    flex: 1;
  }

  .no-value {
    font-size: 12px;
    color: #94a3b8;
    font-style: italic;
    background: #f8fafc;
    padding: 4px 8px;
    border-radius: 4px;
    text-align: center;
    border: 1px solid #e2e8f0;
    flex: 1;
  }

  .delete-condition-btn {
    padding: 6px;
    color: #ef4444;
    background: none;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    transition: background 0.2s;
    display: flex;
    align-items: center;
    justify-content: center;

    &:hover {
      background: #fef2f2;
    }
  }
}

// Empty State
.empty-state {
  text-align: center;
  padding: 16px;
  color: #94a3b8;
  font-size: 13px;
  border: 1px dashed #e2e8f0;
  border-radius: 6px;
  background: #f8fafc;
}

// Add Condition Button
.add-condition-btn {
  width: 100%;
  padding: 6px;
  margin-top: 12px;
  font-size: 12px;
  border: 1px dashed #cbd5e1;
  color: #64748b;
  border-radius: 6px;
  background: transparent;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  transition: all 0.2s;

  &:hover {
    background: #f8fafc;
    border-color: #93c5fd;
    color: #2563eb;
  }
}

// Add Rule Button Container
.add-rule-container {
  padding-left: 32px;
  position: relative;

  .flow-line {
    position: absolute;
    left: 12px;
    top: 0;
    bottom: 0;
    width: 2px;
    background: #e2e8f0;
  }

  .add-rule-btn {
    width: 100%;
    height: 48px;
    border: 2px dashed #cbd5e1;
    color: #64748b;
    border-radius: 8px;
    background: transparent;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 500;
    transition: all 0.2s;

    &:hover {
      border-color: #60a5fa;
      color: #2563eb;
      background: #eff6ff;
    }
  }
}

// Default Fallback
.default-container {
  padding-left: 32px;
  position: sticky;
  bottom: 0;
  flex-shrink: 0;
  background: white;
  z-index: 10;
  margin-top: auto;

  .flow-line-short {
    position: absolute;
    left: 12px;
    top: 0;
    height: 32px;
    width: 2px;
    background: #e2e8f0;
  }

  .end-marker {
    position: absolute;
    left: 0;
    top: 8px;
    width: 24px;
    height: 24px;
    background: #1e293b;
    color: white;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 11px;
    font-weight: 700;
    z-index: 10;
  }

  .default-card {
    background: #1e293b;
    color: white;
    border-radius: 12px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    padding: 20px;
    display: flex;
    align-items: center;
    justify-content: space-between;

    .left {
      h4 {
        font-weight: 700;
        display: flex;
        align-items: center;
        gap: 8px;
        margin: 0;
        font-size: 15px;

        .check-icon {
          color: #4ade80;
        }
      }

      p {
        color: #94a3b8;
        font-size: 12px;
        margin-top: 4px;
        margin-bottom: 0;
      }
    }

    .right {
      display: flex;
      align-items: center;
      gap: 12px;

      .label {
        font-size: 13px;
        font-weight: 500;
        color: #94a3b8;
      }

      input {
        width: 128px;
        background: #334155;
        border: 1px solid #475569;
        color: white;
        font-weight: 700;
        padding: 4px 8px;
        border-radius: 4px;
        outline: none;

        &:focus {
          border-color: #64748b;
        }
      }
    }
  }
}
</style>

