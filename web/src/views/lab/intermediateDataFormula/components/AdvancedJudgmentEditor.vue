<template>
  <div class="advanced-judgment-editor">
    <!-- 顶部说明 -->
    <div class="intro-banner">
      <div class="icon-wrapper">
        <Icon icon="ant-design:apartment-outlined" :size="20" />
      </div>
      <div class="content">
        <h3>高级判定规则配置</h3>
        <p>
          系统按<span class="bold">从上到下</span>的顺序匹配规则。每条规则内的<span class="bold">条件组之间是「且」关系</span>,
          条件组内部支持嵌套的「且/或」逻辑。
        </p>
      </div>
      <div class="banner-actions">
        <a-button size="small" @click="expandAllGroups">
          <template #icon><Icon icon="ant-design:expand-alt-outlined" :size="14" /></template>
          全部展开
        </a-button>
        <a-button size="small" @click="collapseAllGroups">
          <template #icon><Icon icon="ant-design:compress-outlined" :size="14" /></template>
          全部折叠
        </a-button>
      </div>
    </div>

    <!-- 规则列表 -->
    <div class="rules-container">
      <div v-for="(rule, ruleIdx) in rules" :key="rule.id" class="rule-wrapper">
        <div class="flow-line"></div>
        <div class="rule-number">{{ ruleIdx + 1 }}</div>

        <div class="rule-card-wrapper">
          <RuleCard
            :rule="rule"
            :rule-index="ruleIdx"
            :field-options="fieldOptions"
            :read-only-result="true"
            ref="ruleCardRefs"
            @update:rule="handleRuleUpdate(ruleIdx, $event)"
            @change="emitChange"
            @open-formula-editor="handleOpenFormulaEditor"
          />
        </div>
      </div>

      <!-- 规则由判定等级自动生成，无需手动添加 -->
    </div>

    <!-- 默认值 -->
    <div class="default-section">
      <div class="flow-line-short"></div>
      <div class="end-marker">
        <Icon icon="ant-design:check-outlined" :size="14" />
      </div>
      
      <div class="default-card">
        <div class="default-left">
          <div class="default-title">
            <Icon icon="ant-design:safety-outlined" :size="20" />
            <span>默认返回值</span>
          </div>
          <p class="default-desc">当以上所有规则都不满足时,返回此值</p>
        </div>
        <div class="default-right">
          <span class="return-label">返回:</span>
          <input 
            :value="defaultValue"
            class="default-value-input"
            readonly
            placeholder="例如: B"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, computed, PropType, nextTick } from 'vue';
import { Icon } from '/@/components/Icon';
import {
  AdvancedJudgmentRule,
  ConditionGroup,
  Condition,
} from './advancedJudgmentTypes';
import RuleCard from './RuleCard.vue';

// ============ Props & Emits ============


const props = defineProps({
  value: { type: String, default: '' },
  defaultValue: { type: String, default: '' },
  fields: { type: Array as PropType<any[]>, default: () => [] },
  levels: { type: Array as PropType<any[]>, default: () => [] },
});

const emit = defineEmits(['update:value', 'update:defaultValue', 'change', 'openFormulaEditor']);

// ============ 响应式数据 ============

const rules = ref<AdvancedJudgmentRule[]>([]);
const localFields = ref<any[]>([]);
const isInternalUpdate = ref(false); // 标记是否为内部更新 防止 watch 重新折叠
const ruleCardRefs = ref<any[]>([]);

// ============ 计算属性 ============

const fieldOptions = computed(() => {
  if (!localFields.value || localFields.value.length === 0) {
    return [];
  }
  return localFields.value.map(f => ({
    label: f.name || f.displayName || f.columnName || String(f),
    value: f.id || f.columnName || f.code,
    featureCategories: f.featureCategories || [],
    featureLevels: f.featureLevels || [],
  }));
});

// ============ Watchers ============

watch(() => props.value, (newVal) => {
  if (isInternalUpdate.value) {
    isInternalUpdate.value = false;
    return;
  }
  
  parseValue(newVal);
  
}, { immediate: true });

watch(() => props.fields, (newVal) => {
  localFields.value = newVal || [];
}, { immediate: true, deep: true });

watch(() => props.levels, (newLevels) => {
  // 当判定等级变化时,重新初始化规则
  if (newLevels && newLevels.length > 0) {
    initializeRulesFromLevels(newLevels);
  }
}, { immediate: true, deep: true });

// ============ 值解析与序列化 ============

function parseValue(value: string) {
  if (!value) {
    rules.value = [];
    return;
  }
  try {
    const parsed = JSON.parse(value);
    if (Array.isArray(parsed)) {
      rules.value = parsed;
    } else {
      rules.value = [];
    }
  } catch {
    rules.value = [];
  }
}

function emitChange() {
  const json = JSON.stringify(rules.value);
  isInternalUpdate.value = true;
  emit('update:value', json);
  emit('change', json);
}

function handleRuleUpdate(ruleIdx: number, newRule: AdvancedJudgmentRule) {
  rules.value[ruleIdx] = newRule;
  emitChange();
}

// ============ 规则初始化 ============

function initializeRulesFromLevels(levels: any[]) {
  // 为每个判定等级创建对应的规则(如果没有的话)
  const existingValues = new Set(rules.value.map(r => r.resultValue));
  
  for (const level of levels) {
    const value = level.value || level.code || level.name;
    if (!existingValues.has(value)) {
      rules.value.push({
        id: generateId(),
        resultValue: value,
        groups: [],
      });
      existingValues.add(value);
    }
  }
  
  // 按levels的顺序排序rules
  const levelOrder = new Map(levels.map((l, i) => [l.value || l.code || l.name, i]));
  rules.value.sort((a, b) => {
    const orderA = levelOrder.get(a.resultValue) ?? 999;
    const orderB = levelOrder.get(b.resultValue) ?? 999;
    return orderA - orderB;
  });
  
  emitChange();
}

// ============ 辅助函数 ============

function generateId() {
  return Date.now().toString(36) + Math.random().toString(36).substr(2);
}

function expandAllGroups() {
  ruleCardRefs.value.forEach(card => card?.expandAll());
}

function collapseAllGroups() {
  ruleCardRefs.value.forEach(card => card?.collapseAll());
}

// ============ 公式编辑器 ============

function handleOpenFormulaEditor(condition: Condition) {
  emit('openFormulaEditor', condition);
}
</script>

<style lang="less" scoped>
.advanced-judgment-editor {
  display: flex;
  flex-direction: column;
  gap: 20px;
  padding: 16px;
  background: #f8fafc;
  border-radius: 12px;
}

// ========== 顶部说明 ==========
.intro-banner {
  display: flex;
  gap: 12px;
  padding: 16px;
  background: linear-gradient(135deg, #eff6ff 0%, #dbeafe 100%);
  border: 1px solid #bfdbfe;
  border-radius: 10px;
  align-items: flex-start;

  .icon-wrapper {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 40px;
    height: 40px;
    background: #2563eb;
    border-radius: 10px;
    color: white;
    flex-shrink: 0;
  }

  .content {
    flex: 1;
    
    h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
      color: #1e40af;
    }

    p {
      margin: 6px 0 0;
      font-size: 13px;
      color: #3b82f6;
      line-height: 1.5;

      .bold {
        font-weight: 600;
        color: #1e40af;
      }
    }
  }

  .banner-actions {
    display: flex;
    gap: 8px;
    flex-shrink: 0;
  }
}

// ========== 规则容器 ==========
.rules-container {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.rule-wrapper {
  position: relative;
  padding-left: 36px;
}

.flow-line {
  position: absolute;
  left: 14px;
  top: 0;
  bottom: -16px;
  width: 2px;
  background: #cbd5e1;
}

.rule-number {
  position: absolute;
  left: 0;
  top: 20px;
  width: 28px;
  height: 28px;
  background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%);
  color: white;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  font-weight: 700;
  box-shadow: 0 2px 8px rgba(59, 130, 246, 0.3);
  z-index: 10;
}

// ========== 规则卡片 ==========
.rule-card {
  background: white;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  overflow: hidden;
}

.rule-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 20px;
  background: #f8fafc;
  border-bottom: 1px solid #e2e8f0;

  .left {
    display: flex;
    align-items: center;
    gap: 12px;
    flex-wrap: wrap;

    .when-label {
      font-size: 14px;
      color: #64748b;
    }

    .result-input {
      display: flex;
      align-items: center;
      gap: 8px;

      .arrow-icon {
        color: #94a3b8;
      }

      .result-value-input {
        width: 200px;
        padding: 6px 12px;
        border: 2px solid #3b82f6;
        border-radius: 6px;
        font-size: 15px;
        font-weight: 700;
        color: #1e40af;
        background: white;
        outline: none;

        &:focus {
          box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.2);
        }
      }
    }
  }
}

// ========== 条件组区域 ==========
.groups-area {
  padding: 20px;
}

.groups-list {
  display: flex;
  flex-direction: column;
}

.group-connector {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 0;

  .connector-line {
    flex: 1;
    height: 1px;
    background: #cbd5e1;
  }

  .connector-label {
    padding: 4px 16px;
    background: #fef3c7;
    border: 1px solid #fcd34d;
    border-radius: 20px;
    font-size: 12px;
    font-weight: 600;
    color: #92400e;
  }
}

// ========== 条件组卡片 ==========
.condition-group-card {
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  overflow: hidden;
}

.group-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  background: white;
  border-bottom: 1px solid #e2e8f0;
  user-select: none;

  .group-title {
    display: flex;
    align-items: center;
    gap: 10px;
    flex: 1;
    min-width: 0;

    .collapse-icon {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 24px;
      height: 24px;
      color: #64748b;
      cursor: pointer;
      border-radius: 4px;
      transition: all 0.2s;

      &:hover {
        background: #f1f5f9;
        color: #3b82f6;
      }
    }

    .group-index {
      font-size: 13px;
      font-weight: 600;
      color: #475569;
      background: #e2e8f0;
      padding: 4px 10px;
      border-radius: 4px;
      flex-shrink: 0;
      cursor: pointer;
      transition: all 0.2s;

      &:hover {
        background: #cbd5e1;
      }
    }

    .group-name-input {
      width: 120px;
      padding: 4px 8px;
      border: 1px dashed #cbd5e1;
      border-radius: 4px;
      font-size: 13px;
      color: #64748b;
      background: transparent;
      outline: none;
      flex-shrink: 0;
      cursor: text;

      &:focus {
        border-color: #3b82f6;
        border-style: solid;
        background: white;
      }
    }

    .group-summary {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-left: 8px;
      flex: 1;
      min-width: 0;
      cursor: pointer;
      padding: 4px 8px;
      border-radius: 4px;
      transition: all 0.2s;

      &:hover {
        background: #f1f5f9;
      }

      .summary-text {
        font-size: 12px;
        color: #94a3b8;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }
    }
  }

  .group-actions {
    display: flex;
    align-items: center;
    gap: 4px;
    flex-shrink: 0;
  }
}

// 折叠内容过渡
.group-content {
  transition: all 0.3s ease;
}

// 模式选择
.mode-selector {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background: #fffbeb;
  border-bottom: 1px solid #fef3c7;

  .mode-label {
    font-size: 13px;
    color: #92400e;
    font-weight: 500;
  }

  .mode-option {
    display: flex;
    align-items: center;
    gap: 6px;
  }

  .help-icon {
    color: #d97706;
    cursor: help;
  }
}

// 逻辑选择
.logic-selector {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background: white;
  border-bottom: 1px solid #f1f5f9;

  .logic-hint {
    font-size: 13px;
    color: #64748b;
  }

  :deep(.ant-radio-button-wrapper-checked) {
    &:first-child {
      background: #dbeafe;
      border-color: #3b82f6;
      color: #1e40af;
    }
    &:last-child {
      background: #fef3c7;
      border-color: #f59e0b;
      color: #92400e;
    }
  }
}

// ========== 简单模式 ==========
.simple-mode {
  .conditions-list {
    display: flex;
    padding: 16px;
    gap: 8px;
  }
}

// ========== 嵌套模式 ==========
.nested-mode {
  .sub-groups-list {
    display: flex;
    padding: 16px;
    gap: 8px;
  }

  .sub-groups-content {
    flex: 1;
    display: flex;
    flex-direction: column;
    gap: 12px;
  }

  .sub-group-connector {
    display: flex;
    justify-content: center;
    padding: 4px 0;

    .connector-badge {
      padding: 2px 12px;
      border-radius: 10px;
      font-size: 11px;
      font-weight: 600;

      &.and {
        background: #dbeafe;
        color: #1e40af;
      }

      &.or {
        background: #fef3c7;
        color: #92400e;
      }
    }
  }

  .sub-group-card {
    background: white;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    overflow: hidden;

    .sub-group-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 8px 12px;
      background: #f1f5f9;
      border-bottom: 1px solid #e2e8f0;

      .sub-group-title {
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 12px;
        font-weight: 600;
        color: #475569;
      }

      .sub-group-logic {
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 12px;
        color: #64748b;
      }
    }

    .sub-group-conditions {
      display: flex;
      padding: 12px;
      gap: 6px;

      .conditions-content {
        flex: 1;
      }
    }
  }
}

// ========== 逻辑指示条 ==========
.logic-bar {
  width: 6px;
  border-radius: 3px;
  display: flex;
  align-items: center;
  justify-content: center;
  writing-mode: vertical-rl;
  flex-shrink: 0;

  &.and {
    background: linear-gradient(180deg, #bfdbfe 0%, #93c5fd 100%);
  }

  &.or {
    background: linear-gradient(180deg, #fde68a 0%, #fcd34d 100%);
  }

  &.small {
    width: 4px;
  }

  span {
    font-size: 10px;
    font-weight: 700;
    color: white;
    text-shadow: 0 1px 2px rgba(0, 0, 0, 0.2);
  }
}

.conditions-content {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

// 空状态
.empty-hint {
  padding: 16px;
  text-align: center;
  color: #94a3b8;
  font-size: 13px;
  background: #f8fafc;
  border: 1px dashed #e2e8f0;
  border-radius: 6px;

  &.small {
    padding: 8px;
    font-size: 12px;
  }
}

.empty-groups {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 32px;
  color: #94a3b8;
  background: white;
  border: 2px dashed #e2e8f0;
  border-radius: 10px;
  margin-bottom: 16px;

  p {
    margin: 12px 0 0;
    font-size: 14px;
    color: #64748b;
  }
}

.add-group-btn {
  margin-top: 16px;
}

// ========== 添加规则 ==========
.add-rule-wrapper {
  position: relative;
  padding-left: 36px;

  .add-rule-btn {
    height: 50px;
    font-size: 15px;
  }
}

// ========== 默认值区域 ==========
.default-section {
  position: relative;
  padding-left: 36px;
  margin-top: 8px;
}

.flow-line-short {
  position: absolute;
  left: 14px;
  top: 0;
  height: 24px;
  width: 2px;
  background: #cbd5e1;
}

.end-marker {
  position: absolute;
  left: 0;
  top: 16px;
  width: 28px;
  height: 28px;
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 8px rgba(16, 185, 129, 0.3);
  z-index: 10;
}

.default-card {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
  border-radius: 12px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.15);

  .default-left {
    .default-title {
      display: flex;
      align-items: center;
      gap: 10px;
      color: white;
      font-size: 16px;
      font-weight: 600;
    }

    .default-desc {
      margin: 6px 0 0;
      color: #94a3b8;
      font-size: 13px;
    }
  }

  .default-right {
    display: flex;
    align-items: center;
    gap: 12px;

    .return-label {
      color: #94a3b8;
      font-size: 14px;
    }

    .default-value-input {
      width: 120px;
      padding: 8px 14px;
      background: #334155;
      border: 2px solid #475569;
      border-radius: 8px;
      color: white;
      font-size: 15px;
      font-weight: 700;
      outline: none;

      &:focus {
        border-color: #4ade80;
      }
    }
  }
}
</style>
