<template>
  <div :class="['rule-card', { 'is-borderless': borderless }]">
    <!-- 规则头部 -->
    <div class="rule-header">
      <div class="header-title">
        <div class="title-badge">
          <Icon icon="ant-design:filter-outlined" :size="18" />
        </div>
        <div class="title-content">
          <span class="when-label">当满足以下条件时</span>
          <span class="return-divider">→</span>
          <span class="result-badge">{{ localRule.resultValue || '?' }}</span>
          <span v-if="hint" class="hint-text">
            <Icon icon="ant-design:info-circle-outlined" :size="14" />
            {{ hint }}
          </span>
        </div>
      </div>
      <div class="right">
        <slot name="extra"></slot>
      </div>
    </div>

    <!-- 条件组区域 -->
    <div class="groups-area">
      <div v-if="localRule.groups.length > 0" class="groups-list">
        <template v-for="(group, groupIdx) in localRule.groups" :key="group.id">
          <!-- 组间连接线 -->
          <div v-if="groupIdx > 0" class="group-connector">
            <div class="connector-line"></div>
            <span class="connector-label">且 (AND)</span>
            <div class="connector-line"></div>
          </div>

          <!-- 条件组编辑器组件 -->
          <ConditionGroupEditor
            :group="group"
            :group-index="groupIdx"
            :rule-index="ruleIndex"
            :field-options="fieldOptions"
            :is-collapsed="isGroupCollapsed(group.id)"
            @update="handleUpdateGroup(groupIdx, $event)"
            @remove="handleRemoveGroup(groupIdx)"
            @toggle-collapse="toggleGroupCollapse(group.id)"
            @add-condition="handleAddCondition(groupIdx, $event)"
            @update-condition="handleUpdateCondition(groupIdx, $event)"
            @remove-condition="handleRemoveCondition(groupIdx, $event)"

            @open-formula-editor="handleOpenFormulaEditor"
            :read-only="readOnly"
          />

        </template>
      </div>

      <!-- 空规则状态 -->
      <div v-else class="empty-groups">
        <Icon icon="ant-design:folder-open-outlined" :size="32" />
        <p>还没有添加条件组</p>
      </div>

      <a-button v-if="!readOnly" type="dashed" block class="add-group-btn" @click="handleAddGroup">
        <template #icon><Icon icon="ant-design:plus-square-outlined" /></template>
        添加条件组
      </a-button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, PropType } from 'vue';
import { Icon } from '/@/components/Icon';
import {
  AdvancedJudgmentRule,
  ConditionGroup,
  Condition,
} from './advancedJudgmentTypes';
import ConditionGroupEditor from './ConditionGroupEditor.vue';

const props = defineProps({
  rule: { type: Object as PropType<AdvancedJudgmentRule>, required: true },
  ruleIndex: { type: Number, default: 0 },
  fieldOptions: { type: Array as PropType<any[]>, default: () => [] },
  readOnlyResult: { type: Boolean, default: true },
  borderless: { type: Boolean, default: false },
  hint: { type: String, default: '' },
  readOnly: { type: Boolean, default: false },
});

const emit = defineEmits(['update:rule', 'change', 'openFormulaEditor']);

const localRule = ref<AdvancedJudgmentRule>({ ...props.rule });
const collapsedGroupIds = ref<Set<string>>(new Set());

watch(() => props.rule, (newRule) => {
  localRule.value = { ...newRule };
  // Sync collapsed state or re-initialize? 
  // If we want to persist collapsed state across updates, we should probably not clear it blindly.
  // But if the rule object is completely replaced, maybe we should.
  // For now, let's keep existing state but ensure new groups are handled?
  // AdvancedJudgmentEditor cleared it on value change.
  // Let's matching AdvancedJudgmentEditor: "only first load or external change defaults to collapsed"
  // Here we can just default collapse all if it's a new rule.
  
  // Actually, simplest is: calculate all group IDs from newRule
  // and set them as collapsed if this is a "reset".
  // relying on parent to reset is hard.
  // Let's just expose a method to "initCollapse" and call it?
}, { deep: true });

function emitChange() {
  emit('update:rule', localRule.value);
  emit('change', localRule.value);
}

// Group Collapse Logic
function isGroupCollapsed(groupId: string) {
  return collapsedGroupIds.value.has(groupId);
}

function toggleGroupCollapse(groupId: string) {
  if (collapsedGroupIds.value.has(groupId)) {
    collapsedGroupIds.value.delete(groupId);
  } else {
    collapsedGroupIds.value.add(groupId);
  }
  collapsedGroupIds.value = new Set(collapsedGroupIds.value);
}

function expandAll() {
  collapsedGroupIds.value.clear();
  collapsedGroupIds.value = new Set();
}

function collapseAll() {
  const ids = new Set<string>();
  localRule.value.groups.forEach(g => ids.add(g.id));
  collapsedGroupIds.value = ids;
}

// Initialize collapsed state
watch(() => localRule.value.groups, (groups) => {
   // If we have no state (first load), collapse all?
   // Or we can just call collapseAll() on mounted.
}, { immediate: true });

import { onMounted } from 'vue';
onMounted(() => {
  collapseAll();
});

defineExpose({
  expandAll,
  collapseAll
});

// Group Operations
function handleAddGroup() {
  const newGroupId = generateId();
  localRule.value.groups.push({
    id: newGroupId,
    name: '',
    mode: 'simple',
    logic: 'AND',
    conditions: [],
    subGroups: [],
  });
  // New group is expanded (not in collapsed set)
  emitChange();
}

function handleUpdateGroup(groupIdx: number, updatedGroup: ConditionGroup) {
  localRule.value.groups[groupIdx] = updatedGroup;
  emitChange();
}

function handleRemoveGroup(groupIdx: number) {
  localRule.value.groups.splice(groupIdx, 1);
  emitChange();
}

// Condition Operations
function generateId() {
  return Date.now().toString(36) + Math.random().toString(36).substr(2);
}

function getDefaultField(): string {
  return props.fieldOptions.length > 0 
    ? (props.fieldOptions[0].value || '')
    : '';
}

function createCondition(): Condition {
  return {
    id: generateId(),
    leftExpr: getDefaultField(),
    operator: '=',
    rightValue: '',
  };
}

function handleAddCondition(groupIdx: number, data: { isSubGroup?: boolean; subGroupIndex?: number }) {
  const group = localRule.value.groups[groupIdx];
  if (data.isSubGroup) {
     const subGroup = group.subGroups[data.subGroupIndex!];
     subGroup.conditions.push(createCondition());
  } else {
     group.conditions.push(createCondition());
  }
  emitChange();
}

function handleUpdateCondition(groupIdx: number, data: { isSubGroup?: boolean; subGroupIndex?: number; conditionIndex: number; condition: Partial<Condition> }) {
  const group = localRule.value.groups[groupIdx];
  if (data.isSubGroup) {
     const cond = group.subGroups[data.subGroupIndex!].conditions[data.conditionIndex];
     Object.assign(cond, data.condition);
  } else {
     const cond = group.conditions[data.conditionIndex];
     Object.assign(cond, data.condition);
  }
  emitChange();
}

function handleRemoveCondition(groupIdx: number, data: { isSubGroup?: boolean; subGroupIndex?: number; conditionIndex: number }) {
  const group = localRule.value.groups[groupIdx];
  if (data.isSubGroup) {
     group.subGroups[data.subGroupIndex!].conditions.splice(data.conditionIndex, 1);
  } else {
     group.conditions.splice(data.conditionIndex, 1);
  }
  emitChange();
}

function handleOpenFormulaEditor(condition: Condition) {
  emit('openFormulaEditor', condition);
}
</script>

<style lang="less" scoped>
// ========== 规则卡片 ==========
.rule-card {
  background: white;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05), 0 4px 12px rgba(0, 0, 0, 0.04);
  overflow: hidden;
  transition: box-shadow 0.3s ease;

  &:hover {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08), 0 8px 24px rgba(59, 130, 246, 0.08);
  }

  // 无框模式
  &.is-borderless {
    border: none;
    box-shadow: none;
    background: transparent;

    &:hover {
      box-shadow: none;
    }

    .rule-header {
      background: transparent;
      border-bottom: none;
      padding: 0 0 16px 0;
    }

    .groups-area {
      padding: 0;
      background: transparent;
    }
  }
}

.rule-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
  border-bottom: 1px solid #bae6fd;

  .header-title {
    display: flex;
    align-items: center;
    gap: 14px;

    .title-badge {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      background: linear-gradient(135deg, #0ea5e9 0%, #0284c7 100%);
      border-radius: 10px;
      color: white;
      box-shadow: 0 4px 14px rgba(14, 165, 233, 0.35);
    }

    .title-content {
      display: flex;
      align-items: center;
      gap: 10px;

      .when-label {
        font-size: 16px;
        color: #0369a1;
        font-weight: 600;
      }

      .return-divider {
        font-size: 18px;
        color: #7dd3fc;
        font-weight: 500;
      }

      .result-badge {
        display: inline-flex;
        align-items: center;
        justify-content: center;
        min-width: 44px;
        height: 32px;
        padding: 0 14px;
        background: white;
        border: 2px solid #0ea5e9;
        border-radius: 8px;
        font-size: 16px;
        font-weight: 700;
        color: #0284c7;
        box-shadow: 0 2px 8px rgba(14, 165, 233, 0.15);
      }

      .hint-text {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        margin-left: 16px;
        padding: 6px 12px;
        background: rgba(255, 255, 255, 0.7);
        border-radius: 6px;
        font-size: 13px;
        color: #64748b;
        font-weight: 400;

        .app-iconify {
          color: #94a3b8;
        }
      }
    }
  }

  .right {
    display: flex;
    align-items: center;
    gap: 8px;
  }
}

// ========== 条件组区域 ==========
.groups-area {
  padding: 20px;
  background: #fafbfc;
}

.groups-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.group-connector {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 0;
  animation: fadeIn 0.3s ease;

  .connector-line {
    flex: 1;
    height: 2px;
    background: linear-gradient(90deg, #e2e8f0 0%, #fcd34d 50%, #e2e8f0 100%);
    border-radius: 1px;
  }

  .connector-label {
    padding: 6px 18px;
    background: linear-gradient(135deg, #fef3c7 0%, #fde68a 100%);
    border: 1px solid #fbbf24;
    border-radius: 20px;
    font-size: 12px;
    font-weight: 600;
    color: #92400e;
    box-shadow: 0 1px 2px rgba(251, 191, 36, 0.2);
  }
}

.empty-groups {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 30px;
  background: #f8fafc;
  border: 2px dashed #e2e8f0;
  border-radius: 12px;
  margin-bottom: 16px;
  color: #94a3b8;
  transition: all 0.3s ease;

  &:hover {
    border-color: #cbd5e1;
    background: #f1f5f9;
    color: #64748b;
  }
  
  p {
    margin-top: 12px;
    font-size: 14px;
    font-weight: 500;
  }
}

.add-group-btn {
  border-radius: 8px;
  height: 44px;
  font-weight: 500;
  border-style: dashed;
  border-width: 2px;
  transition: all 0.2s ease;

  &:hover {
    border-style: solid;
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(59, 130, 246, 0.15);
  }
}

// 动画
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-4px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
