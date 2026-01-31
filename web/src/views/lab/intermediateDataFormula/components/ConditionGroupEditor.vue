<template>
  <!-- 条件组卡片 -->
  <div class="condition-group-card">
    <!-- 组头部 -->
    <div class="group-header">
      <div class="group-title">
        <!-- 折叠图标 - 点击这里才折叠 -->
        <span
          class="collapse-icon"
          @click="handleToggleCollapse"
          :style="{ cursor: 'pointer' }"
        >
          <Icon
            :icon="isCollapsed ? 'ant-design:right-outlined' : 'ant-design:down-outlined'"
            :size="12"
          />
        </span>
        <!-- 条件组索引 - 点击这里也可以折叠 -->
        <span
          class="group-index"
          @click="handleToggleCollapse"
        >
          条件组 {{ groupIndex + 1 }}
        </span>
        <input
          v-model="localGroup.name"
          class="group-name-input"
          placeholder="输入组名(可选)"
          :disabled="readOnly"
          @input="handleUpdate"
        />
        <!-- 折叠时显示摘要 - 点击摘要也可以折叠 -->
        <span
          v-if="isCollapsed"
          class="group-summary"
          @click="handleToggleCollapse"
        >
          <a-tag :color="localGroup.mode === 'simple' ? 'blue' : 'purple'" size="small">
            {{ localGroup.mode === 'simple' ? '简单' : '嵌套' }}
          </a-tag>
          <span class="summary-text">
            {{ getGroupSummary(localGroup) }}
          </span>
        </span>
      </div>
      <div class="group-actions">
        <a-tooltip title="展开/折叠">
          <a-button type="text" size="small" @click="handleToggleCollapse">
            <template #icon>
              <Icon :icon="isCollapsed ? 'ant-design:expand-alt-outlined' : 'ant-design:compress-outlined'" :size="14" />
            </template>
          </a-button>
        </a-tooltip>
        <a-button v-if="!readOnly" type="text" size="small" danger @click="handleRemove">
          <template #icon><Icon icon="ant-design:close-outlined" :size="14" /></template>
        </a-button>
      </div>
    </div>

    <!-- 折叠内容区域 -->
    <div v-show="!isCollapsed" class="group-content">
    <!-- 模式切换 -->
    <div class="mode-selector">
      <span class="mode-label">条件模式:</span>
      <a-radio-group
        v-model:value="localGroup.mode"
        size="small"
        :disabled="readOnly"
        @change="handleModeChange"
      >
        <a-radio value="simple">
          <span class="mode-option">
            <Icon icon="ant-design:unordered-list-outlined" :size="14" />
            简单条件
          </span>
        </a-radio>
        <a-radio value="nested">
          <span class="mode-option">
            <Icon icon="ant-design:cluster-outlined" :size="14" />
            嵌套条件组
          </span>
        </a-radio>
      </a-radio-group>
      <a-tooltip>
        <template #title>
          <div>
            <p><b>简单条件</b>:多个条件用「且」或「或」连接</p>
            <p><b>嵌套条件组</b>:支持 OR(AND(...), AND(...)) 这样的复杂逻辑</p>
          </div>
        </template>
        <Icon icon="ant-design:question-circle-outlined" class="help-icon" />
      </a-tooltip>
    </div>

    <!-- 简单模式 -->
    <template v-if="localGroup.mode === 'simple'">
      <div class="simple-mode">
        <!-- 逻辑选择 -->
        <div class="logic-selector">
          <span class="logic-hint">条件之间的关系</span>
          <a-radio-group
            v-model:value="localGroup.logic"
            size="small"
            button-style="solid"
            :disabled="readOnly"
            @change="handleUpdate"
          >
            <a-radio-button value="AND">全部满足 (且)</a-radio-button>
            <a-radio-button value="OR">任一满足 (或)</a-radio-button>
          </a-radio-group>
        </div>

        <!-- 条件列表 -->
        <div class="conditions-list">
          <div
            v-if="localGroup.conditions.length > 1"
            :class="['logic-bar', localGroup.logic.toLowerCase()]"
          >
            <span>{{ localGroup.logic === 'AND' ? '且' : '或' }}</span>
          </div>
          <div class="conditions-content">
            <ConditionRow
              v-for="(cond, condIdx) in localGroup.conditions"
              :key="cond.id"
              :condition="cond"
              :field-options="fieldOptions"
              :read-only="readOnly"
              @update="handleUpdateCondition(condIdx, $event)"
              @remove="handleRemoveCondition(condIdx)"
              @open-formula-editor="handleOpenFormulaEditor"
            />
            <div v-if="localGroup.conditions.length === 0" class="empty-hint">
              暂无条件
            </div>
          </div>
        </div>
        <a-button v-if="!readOnly" type="dashed" block size="small" @click="handleAddCondition">
          <template #icon><Icon icon="ant-design:plus-outlined" /></template>
          添加条件
        </a-button>
      </div>
    </template>

    <!-- 嵌套模式 -->
    <template v-else>
      <div class="nested-mode">
        <!-- 子组之间的逻辑 -->
        <div class="logic-selector">
          <span class="logic-hint">子条件组之间的关系</span>
          <a-radio-group
            v-model:value="localGroup.logic"
            size="small"
            button-style="solid"
            :disabled="readOnly"
            @change="handleUpdate"
          >
            <a-radio-button value="AND">全部满足 (且)</a-radio-button>
            <a-radio-button value="OR">任一满足 (或)</a-radio-button>
          </a-radio-group>
        </div>

        <!-- 子条件组列表 -->
        <div class="sub-groups-list">
          <div
            v-if="localGroup.subGroups.length > 1"
            :class="['logic-bar', 'vertical', localGroup.logic.toLowerCase()]"
          >
            <span>{{ localGroup.logic === 'AND' ? '且' : '或' }}</span>
          </div>
          <div class="sub-groups-content">
            <template v-for="(subGroup, subIdx) in localGroup.subGroups" :key="subGroup.id">
              <!-- 子组连接线 -->
              <div v-if="subIdx > 0" class="sub-group-connector">
                <span :class="['connector-badge', localGroup.logic.toLowerCase()]">
                  {{ localGroup.logic === 'AND' ? '且' : '或' }}
                </span>
              </div>

              <!-- 子条件组 -->
              <div class="sub-group-card">
                <div class="sub-group-header">
                  <span class="sub-group-title">
                    <Icon icon="ant-design:block-outlined" :size="14" />
                    子组 {{ String.fromCharCode(65 + subIdx) }}
                  </span>
                  <div class="sub-group-logic">
                    <span>组内:</span>
                    <a-select
                      v-model:value="subGroup.logic"
                      size="small"
                      style="width: 100px"
                      :disabled="readOnly"
                      @change="handleUpdate"
                    >
                      <a-select-option value="AND">且(AND)</a-select-option>
                      <a-select-option value="OR">或(OR)</a-select-option>
                    </a-select>
                  </div>
                  <a-button
                    v-if="!readOnly"
                    type="text"
                    size="small"
                    danger
                    @click="handleRemoveSubGroup(subIdx)"
                  >
                    <template #icon><Icon icon="ant-design:close-outlined" :size="12" /></template>
                  </a-button>
                </div>

                <div class="sub-group-conditions">
                  <div
                    v-if="subGroup.conditions.length > 1"
                    :class="['logic-bar', 'small', subGroup.logic.toLowerCase()]"
                  >
                    <span>{{ subGroup.logic === 'AND' ? '且' : '或' }}</span>
                  </div>
                  <div class="conditions-content">
                    <ConditionRow
                      v-for="(cond, condIdx) in subGroup.conditions"
                      :key="cond.id"
                      :condition="cond"
                      :field-options="fieldOptions"
                      size="small"
                      :read-only="readOnly"
                      @update="handleUpdateSubCondition(subIdx, condIdx, $event)"
                      @remove="handleRemoveSubCondition(subIdx, condIdx)"
                      @open-formula-editor="handleOpenFormulaEditor"
                    />
                    <div v-if="subGroup.conditions.length === 0" class="empty-hint small">
                      暂无条件
                    </div>
                  </div>
                </div>
                <a-button
                  v-if="!readOnly"
                  type="dashed"
                  block
                  size="small"
                  @click="handleAddSubCondition(subIdx)"
                >
                  <template #icon><Icon icon="ant-design:plus-outlined" /></template>
                  添加条件
                </a-button>
              </div>
            </template>

            <div v-if="localGroup.subGroups.length === 0" class="empty-hint">
              暂无子条件组
            </div>
          </div>
        </div>
        <a-button v-if="!readOnly" type="dashed" block @click="handleAddSubGroup">
          <template #icon><Icon icon="ant-design:plus-square-outlined" /></template>
          添加子条件组
        </a-button>
      </div>
    </template>
    </div><!-- group-content 结束 -->
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, computed } from 'vue';
import { Icon } from '/@/components/Icon';
import {
  ConditionGroup,
  Condition,
  SubConditionGroup,
} from './advancedJudgmentTypes';
import ConditionRow from './ConditionRow.vue';

// ============ Props & Emits ============
interface Props {
  group: ConditionGroup;
  groupIndex: number;
  ruleIndex: number;
  fieldOptions: any[];
  isCollapsed: boolean;
  readOnly?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  readOnly: false,
});

const emit = defineEmits<{
  'update': [group: ConditionGroup];
  'remove': [];
  'toggle-collapse': [];
  'open-formula-editor': [condition: Condition];
}>();

// ============ 响应式数据 ============
const localGroup = ref<ConditionGroup>({ ...props.group });

// ============ Watchers ============
watch(() => props.group, (newGroup) => {
  localGroup.value = { ...newGroup };
}, { deep: true });

// ============ 辅助函数 ============
function getGroupSummary(group: ConditionGroup): string {
  if (group.mode === 'simple') {
    const count = group.conditions?.length || 0;
    return count === 0 ? '暂无条件' : `${count} 个条件`;
  } else {
    const subCount = group.subGroups?.length || 0;
    const totalCond = group.subGroups?.reduce((sum, sg) => sum + (sg.conditions?.length || 0), 0) || 0;
    return subCount === 0 ? '暂无子组' : `${subCount} 子组, ${totalCond} 条件`;
  }
}

function createCondition(): Condition {
  const defaultField = props.fieldOptions.length > 0 ? props.fieldOptions[0].value : '';
  return {
    id: generateId(),
    leftExpr: defaultField,
    operator: '=',
    rightValue: '',
  };
}

// ============ 事件处理 ============
function handleToggleCollapse() {
  emit('toggle-collapse');
}

function handleRemove() {
  emit('remove');
}

function handleModeChange() {
  // 切换模式时清空数据
  if (localGroup.value.mode === 'simple') {
    localGroup.value.subGroups = [];
  } else {
    localGroup.value.conditions = [];
  }
  handleUpdate();
}

function handleUpdate() {
  emit('update', localGroup.value);
}

function handleAddCondition() {
  if (!localGroup.value.conditions) {
    localGroup.value.conditions = [];
  }
  localGroup.value.conditions.push(createCondition());
  handleUpdate();
}

function handleUpdateCondition(conditionIndex: number, condition: Partial<Condition>) {
  if (localGroup.value.conditions[conditionIndex]) {
    Object.assign(localGroup.value.conditions[conditionIndex], condition);
    handleUpdate();
  }
}

function handleRemoveCondition(conditionIndex: number) {
  localGroup.value.conditions.splice(conditionIndex, 1);
  handleUpdate();
}

function handleAddSubGroup() {
  if (!localGroup.value.subGroups) {
    localGroup.value.subGroups = [];
  }
  localGroup.value.subGroups.push({
    id: generateId(),
    logic: 'AND',
    conditions: [],
  });
  handleUpdate();
}

function handleRemoveSubGroup(subGroupIndex: number) {
  localGroup.value.subGroups.splice(subGroupIndex, 1);
  handleUpdate();
}

function handleAddSubCondition(subGroupIndex: number) {
  const subGroup = localGroup.value.subGroups[subGroupIndex];
  if (subGroup) {
     if (!subGroup.conditions) subGroup.conditions = [];
     subGroup.conditions.push(createCondition());
     handleUpdate();
  }
}

function handleUpdateSubCondition(subGroupIndex: number, conditionIndex: number, condition: Partial<Condition>) {
  const subGroup = localGroup.value.subGroups[subGroupIndex];
  if (subGroup && subGroup.conditions[conditionIndex]) {
    Object.assign(subGroup.conditions[conditionIndex], condition);
    handleUpdate();
  }
}

function handleRemoveSubCondition(subGroupIndex: number, conditionIndex: number) {
  const subGroup = localGroup.value.subGroups[subGroupIndex];
  if (subGroup) {
    subGroup.conditions.splice(conditionIndex, 1);
    handleUpdate();
  }
}

function handleOpenFormulaEditor(condition: Condition) {
  emit('open-formula-editor', condition);
}

function generateId() {
  return Date.now().toString(36) + Math.random().toString(36).substr(2);
}
</script>

<style lang="less" scoped>
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

  &.vertical {
    width: auto;
    height: 6px;
    writing-mode: horizontal-tb;
    align-self: center;
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

:deep(.ant-radio-group) {
  display: flex;
  gap: 8px;
}
</style>