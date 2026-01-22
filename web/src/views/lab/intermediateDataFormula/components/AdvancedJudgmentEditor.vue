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
          系统按<span class="bold">从上到下</span>的顺序匹配规则。
          每条规则内的<span class="bold">条件组之间是「且」关系</span>,
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

        <div class="rule-card">
          <!-- 规则头部 -->
          <div class="rule-header">
            <div class="left">
              <span class="when-label">当满足以下条件时,返回:</span>
              <div class="result-input">
                <Icon icon="ant-design:arrow-right-outlined" :size="16" class="arrow-icon" />
                <input 
                  v-model="rule.resultValue" 
                  placeholder="例如: A" 
                  @input="emitChange"
                  class="result-value-input"
                />
              </div>
            </div>
            <div class="right">
              <a-button type="text" danger size="small" @click="removeRule(ruleIdx)">
                <template #icon><Icon icon="ant-design:delete-outlined" /></template>
                删除规则
              </a-button>
            </div>
          </div>

          <!-- 条件组区域 -->
          <div class="groups-area">
            <div v-if="rule.groups.length > 0" class="groups-list">
              <template v-for="(group, groupIdx) in rule.groups" :key="group.id">
                <!-- 组间连接符 -->
                <div v-if="groupIdx > 0" class="group-connector">
                  <div class="connector-line"></div>
                  <span class="connector-label">且 (AND)</span>
                  <div class="connector-line"></div>
                </div>

                <!-- 条件组卡片 -->
                <div class="condition-group-card">
                  <!-- 组头部 -->
                  <div class="group-header">
                    <div class="group-title">
                      <!-- 折叠图标 - 点击这里才折叠 -->
                      <span 
                        class="collapse-icon"
                        @click="toggleGroupCollapse(ruleIdx, groupIdx)"
                      >
                        <Icon 
                          :icon="isGroupCollapsed(ruleIdx, groupIdx) ? 'ant-design:right-outlined' : 'ant-design:down-outlined'" 
                          :size="12" 
                        />
                      </span>
                      <!-- 条件组索引 - 点击这里也可以折叠 -->
                      <span 
                        class="group-index"
                        @click="toggleGroupCollapse(ruleIdx, groupIdx)"
                      >
                        条件组 {{ groupIdx + 1 }}
                      </span>
                      <input 
                        v-model="group.name" 
                        class="group-name-input"
                        placeholder="输入组名(可选)"
                        @input="emitChange"
                      />
                      <!-- 折叠时显示摘要 - 点击摘要也可以折叠 -->
                      <span 
                        v-if="isGroupCollapsed(ruleIdx, groupIdx)" 
                        class="group-summary"
                        @click="toggleGroupCollapse(ruleIdx, groupIdx)"
                      >
                        <a-tag :color="group.mode === 'simple' ? 'blue' : 'purple'" size="small">
                          {{ group.mode === 'simple' ? '简单' : '嵌套' }}
                        </a-tag>
                        <span class="summary-text">
                          {{ getGroupSummary(group) }}
                        </span>
                      </span>
                    </div>
                    <div class="group-actions">
                      <a-tooltip title="展开/折叠">
                        <a-button type="text" size="small" @click="toggleGroupCollapse(ruleIdx, groupIdx)">
                          <template #icon>
                            <Icon :icon="isGroupCollapsed(ruleIdx, groupIdx) ? 'ant-design:expand-alt-outlined' : 'ant-design:compress-outlined'" :size="14" />
                          </template>
                        </a-button>
                      </a-tooltip>
                      <a-button type="text" size="small" danger @click="removeGroup(ruleIdx, groupIdx)">
                        <template #icon><Icon icon="ant-design:close-outlined" :size="14" /></template>
                      </a-button>
                    </div>
                  </div>

                  <!-- 折叠内容区域 -->
                  <div v-show="!isGroupCollapsed(ruleIdx, groupIdx)" class="group-content">
                  <!-- 模式切换 -->
                  <div class="mode-selector">
                    <span class="mode-label">条件模式:</span>
                    <a-radio-group 
                      v-model:value="group.mode" 
                      size="small"
                      @change="onModeChange(ruleIdx, groupIdx)"
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
                  <template v-if="group.mode === 'simple'">
                    <div class="simple-mode">
                      <!-- 逻辑选择 -->
                      <div class="logic-selector">
                        <span class="logic-hint">条件之间的关系:</span>
                        <a-radio-group 
                          v-model:value="group.logic" 
                          size="small"
                          button-style="solid"
                          @change="emitChange"
                        >
                          <a-radio-button value="AND">全部满足 (且)</a-radio-button>
                          <a-radio-button value="OR">任一满足 (或)</a-radio-button>
                        </a-radio-group>
                      </div>

                      <!-- 条件列表 -->
                      <div class="conditions-list">
                        <div 
                          v-if="group.conditions.length > 1" 
                          :class="['logic-bar', group.logic.toLowerCase()]"
                        >
                          <span>{{ group.logic === 'AND' ? '且' : '或' }}</span>
                        </div>
                        <div class="conditions-content">
                          <ConditionRow
                            v-for="(cond, condIdx) in group.conditions"
                            :key="cond.id"
                            :condition="cond"
                            :field-options="fieldOptions"
                            @update="updateCondition(ruleIdx, groupIdx, condIdx, $event)"
                            @remove="removeCondition(ruleIdx, groupIdx, condIdx)"
                            @open-formula-editor="handleOpenFormulaEditor"
                          />
                          <div v-if="group.conditions.length === 0" class="empty-hint">
                            暂无条件
                          </div>
                        </div>
                      </div>
                      <a-button type="dashed" block size="small" @click="addCondition(ruleIdx, groupIdx)">
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
                        <span class="logic-hint">子条件组之间的关系:</span>
                        <a-radio-group 
                          v-model:value="group.logic" 
                          size="small"
                          button-style="solid"
                          @change="emitChange"
                        >
                          <a-radio-button value="AND">全部满足 (且)</a-radio-button>
                          <a-radio-button value="OR">任一满足 (或)</a-radio-button>
                        </a-radio-group>
                      </div>

                      <!-- 子条件组列表 -->
                      <div class="sub-groups-list">
                        <div 
                          v-if="group.subGroups.length > 1" 
                          :class="['logic-bar', 'vertical', group.logic.toLowerCase()]"
                        >
                          <span>{{ group.logic === 'AND' ? '且' : '或' }}</span>
                        </div>
                        <div class="sub-groups-content">
                          <template v-for="(subGroup, subIdx) in group.subGroups" :key="subGroup.id">
                            <!-- 子组连接符 -->
                            <div v-if="subIdx > 0" class="sub-group-connector">
                              <span :class="['connector-badge', group.logic.toLowerCase()]">
                                {{ group.logic === 'AND' ? '且' : '或' }}
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
                                    @change="emitChange"
                                  >
                                    <a-select-option value="AND">且 (AND)</a-select-option>
                                    <a-select-option value="OR">或 (OR)</a-select-option>
                                  </a-select>
                                </div>
                                <a-button 
                                  type="text" 
                                  size="small" 
                                  danger 
                                  @click="removeSubGroup(ruleIdx, groupIdx, subIdx)"
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
                                    @update="updateSubCondition(ruleIdx, groupIdx, subIdx, condIdx, $event)"
                                    @remove="removeSubCondition(ruleIdx, groupIdx, subIdx, condIdx)"
                                    @open-formula-editor="handleOpenFormulaEditor"
                                  />
                                  <div v-if="subGroup.conditions.length === 0" class="empty-hint small">
                                    暂无条件
                                  </div>
                                </div>
                              </div>
                              <a-button 
                                type="dashed" 
                                block 
                                size="small"
                                @click="addSubCondition(ruleIdx, groupIdx, subIdx)"
                              >
                                <template #icon><Icon icon="ant-design:plus-outlined" /></template>
                                添加条件
                              </a-button>
                            </div>
                          </template>

                          <div v-if="group.subGroups.length === 0" class="empty-hint">
                            暂无子条件组
                          </div>
                        </div>
                      </div>
                      <a-button type="dashed" block @click="addSubGroup(ruleIdx, groupIdx)">
                        <template #icon><Icon icon="ant-design:plus-square-outlined" /></template>
                        添加子条件组
                      </a-button>
                    </div>
                  </template>
                  </div><!-- group-content 结束 -->
                </div>
              </template>
            </div>

            <!-- 空规则状态 -->
            <div v-else class="empty-groups">
              <Icon icon="ant-design:folder-open-outlined" :size="32" />
              <p>还没有添加条件组</p>
            </div>

            <a-button type="dashed" block class="add-group-btn" @click="addGroup(ruleIdx)">
              <template #icon><Icon icon="ant-design:plus-square-outlined" /></template>
              添加条件组
            </a-button>
          </div>
        </div>
      </div>

      <!-- 添加规则按钮 -->
      <div class="add-rule-wrapper">
        <div class="flow-line"></div>
        <a-button type="dashed" block size="large" class="add-rule-btn" @click="addRule">
          <template #icon><Icon icon="ant-design:plus-outlined" /></template>
          添加判定规则
        </a-button>
      </div>
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
            @input="updateDefaultValue"
            class="default-value-input"
            placeholder="例如: B"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { ref, watch, computed, PropType } from 'vue';
import { Icon } from '/@/components/Icon';
import { 
  AdvancedJudgmentRule, 
  ConditionGroup, 
  Condition,
  SubConditionGroup,
} from './advancedJudgmentTypes';
import ConditionRow from './ConditionRow.vue';

// ============ Props & Emits ============

const props = defineProps({
  value: { type: String, default: '' },
  defaultValue: { type: String, default: '' },
  fields: { type: Array as PropType<any[]>, default: () => [] },
});

const emit = defineEmits(['update:value', 'update:defaultValue', 'change', 'openFormulaEditor']);

// ============ 响应式数据 ============

const rules = ref<AdvancedJudgmentRule[]>([]);
const localFields = ref<any[]>([]);
const collapsedGroups = ref<Set<string>>(new Set()); // 存储折叠状态的 key: "ruleIdx-groupIdx"
const isInternalUpdate = ref(false); // 标记是否为内部更新,防止 watch 重新折叠

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

// ============ 折叠功能 ============

function getCollapseKey(ruleIdx: number, groupIdx: number): string {
  return `${ruleIdx}-${groupIdx}`;
}

function isGroupCollapsed(ruleIdx: number, groupIdx: number): boolean {
  return collapsedGroups.value.has(getCollapseKey(ruleIdx, groupIdx));
}

function toggleGroupCollapse(ruleIdx: number, groupIdx: number) {
  const key = getCollapseKey(ruleIdx, groupIdx);
  if (collapsedGroups.value.has(key)) {
    collapsedGroups.value.delete(key);
  } else {
    collapsedGroups.value.add(key);
  }
  // 触发响应式更新
  collapsedGroups.value = new Set(collapsedGroups.value);
}

function collapseAllGroups() {
  rules.value.forEach((rule, ruleIdx) => {
    rule.groups.forEach((_, groupIdx) => {
      collapsedGroups.value.add(getCollapseKey(ruleIdx, groupIdx));
    });
  });
  collapsedGroups.value = new Set(collapsedGroups.value);
}

function expandAllGroups() {
  collapsedGroups.value.clear();
  collapsedGroups.value = new Set();
}

// 获取条件组摘要(折叠时显示)
function getGroupSummary(group: ConditionGroup): string {
  if (group.mode === 'simple') {
    const count = group.conditions.length;
    if (count === 0) return '暂无条件';
    return `${count} 个条件,${group.logic === 'AND' ? '全部满足' : '任一满足'}`;
  } else {
    const count = group.subGroups.length;
    if (count === 0) return '暂无子组';
    const condCount = group.subGroups.reduce((sum, sub) => sum + sub.conditions.length, 0);
    return `${count} 个子组,共 ${condCount} 个条件`;
  }
}

// ============ 处理公式编辑器 ============

function handleOpenFormulaEditor(data: any) {
  // 向父组件转发事件
  emit('openFormulaEditor', data);
}

// ============ 监听器 ============

watch(
  () => props.fields,
  (newFields) => {
    if (newFields && Array.isArray(newFields) && newFields.length > 0) {
      localFields.value = JSON.parse(JSON.stringify(newFields));
    }
  },
  { immediate: true, deep: true }
);

watch(
  () => props.value,
  (val) => {
    // 如果是内部更新触发的,不重新处理折叠状态
    if (isInternalUpdate.value) {
      return;
    }
    
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
      
      // 只有首次加载或外部数据变化时,才默认折叠所有条件组
      collapsedGroups.value.clear();
      rules.value.forEach((rule, ruleIdx) => {
        rule.groups.forEach((_, groupIdx) => {
          collapsedGroups.value.add(getCollapseKey(ruleIdx, groupIdx));
        });
      });
      collapsedGroups.value = new Set(collapsedGroups.value);
      
    } catch (e) {
      console.error('[AdvancedJudgmentEditor] 解析失败:', e);
      rules.value = [];
    }
  },
  { immediate: true }
);

// ============ 工具方法 ============

function generateId(): string {
  return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
}

function emitChange() {
  isInternalUpdate.value = true; // 标记为内部更新
  const json = JSON.stringify(rules.value);
  emit('update:value', json);
  emit('change', json);
  // 在下一个 tick 后重置标志
  setTimeout(() => {
    isInternalUpdate.value = false;
  }, 0);
}

function updateDefaultValue(e: Event) {
  const val = (e.target as HTMLInputElement).value;
  emit('update:defaultValue', val);
}

function getDefaultField(): string {
  return localFields.value.length > 0 
    ? (localFields.value[0].id || localFields.value[0].columnName || '')
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

// ============ 规则操作 ============

function addRule() {
  rules.value.push({
    id: generateId(),
    resultValue: '',
    groups: [],
  });
  emitChange();
}

function removeRule(ruleIdx: number) {
  rules.value.splice(ruleIdx, 1);
  emitChange();
}

// ============ 条件组操作 ============

function addGroup(ruleIdx: number) {
  const rule = rules.value[ruleIdx];
  if (!rule) return;

  const newGroupIdx = rule.groups.length;
  
  rule.groups.push({
    id: generateId(),
    name: '',
    mode: 'simple',
    logic: 'AND',
    conditions: [],
    subGroups: [],
  });
  
  // 新添加的条件组默认展开(确保不在折叠集合中)
  const key = getCollapseKey(ruleIdx, newGroupIdx);
  collapsedGroups.value.delete(key);
  collapsedGroups.value = new Set(collapsedGroups.value);
  
  emitChange();
}

function removeGroup(ruleIdx: number, groupIdx: number) {
  const rule = rules.value[ruleIdx];
  if (!rule) return;
  rule.groups.splice(groupIdx, 1);
  emitChange();
}

function onModeChange(ruleIdx: number, groupIdx: number) {
  const group = rules.value[ruleIdx]?.groups[groupIdx];
  if (!group) return;
  
  // 切换模式时清空数据
  if (group.mode === 'simple') {
    group.subGroups = [];
  } else {
    group.conditions = [];
  }
  emitChange();
}

// ============ 简单条件操作 ============

function addCondition(ruleIdx: number, groupIdx: number) {
  const group = rules.value[ruleIdx]?.groups[groupIdx];
  if (!group) return;
  group.conditions.push(createCondition());
  emitChange();
}

function updateCondition(ruleIdx: number, groupIdx: number, condIdx: number, data: Partial<Condition>) {
  const cond = rules.value[ruleIdx]?.groups[groupIdx]?.conditions[condIdx];
  if (!cond) return;
  Object.assign(cond, data);
  emitChange();
}

function removeCondition(ruleIdx: number, groupIdx: number, condIdx: number) {
  const group = rules.value[ruleIdx]?.groups[groupIdx];
  if (!group) return;
  group.conditions.splice(condIdx, 1);
  emitChange();
}

// ============ 子条件组操作 ============

function addSubGroup(ruleIdx: number, groupIdx: number) {
  const group = rules.value[ruleIdx]?.groups[groupIdx];
  if (!group) return;
  
  group.subGroups.push({
    id: generateId(),
    logic: 'AND',
    conditions: [],
  });
  emitChange();
}

function removeSubGroup(ruleIdx: number, groupIdx: number, subIdx: number) {
  const group = rules.value[ruleIdx]?.groups[groupIdx];
  if (!group) return;
  group.subGroups.splice(subIdx, 1);
  emitChange();
}

function addSubCondition(ruleIdx: number, groupIdx: number, subIdx: number) {
  const subGroup = rules.value[ruleIdx]?.groups[groupIdx]?.subGroups[subIdx];
  if (!subGroup) return;
  subGroup.conditions.push(createCondition());
  emitChange();
}

function updateSubCondition(ruleIdx: number, groupIdx: number, subIdx: number, condIdx: number, data: Partial<Condition>) {
  const cond = rules.value[ruleIdx]?.groups[groupIdx]?.subGroups[subIdx]?.conditions[condIdx];
  if (!cond) return;
  Object.assign(cond, data);
  emitChange();
}

function removeSubCondition(ruleIdx: number, groupIdx: number, subIdx: number, condIdx: number) {
  const subGroup = rules.value[ruleIdx]?.groups[groupIdx]?.subGroups[subIdx];
  if (!subGroup) return;
  subGroup.conditions.splice(condIdx, 1);
  emitChange();
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
