<template>
  <transition name="slide">
    <div class="kg-panel" v-if="panel">
      <div class="panel-head">
        <span class="panel-type-badge" :style="{ background: panel.color }">{{ panel.typeLabel }}</span>
        <span class="panel-label" :title="panel.label">{{ panel.label }}</span>
        <a-button type="text" size="small" @click="$emit('close')" class="panel-close">
          <template #icon><CloseOutlined /></template>
        </a-button>
      </div>
      <a-divider style="margin: 8px 0" />

      <!-- 规格详情 -->
      <template v-if="panel.type === 'spec'">
        <div class="prop-row"><b>编码:</b> {{ panel.raw?.code }}</div>
        <div class="prop-row"><b>名称:</b> {{ panel.raw?.name }}</div>
        <div class="prop-row" v-if="panel.raw?.description"><b>描述:</b> {{ panel.raw.description }}</div>
        <div class="prop-row">
          <b>规则数:</b> <a-tag color="blue">{{ panel.ruleCount }}</a-tag>
        </div>
        <div class="prop-row">
          <b>公式数:</b> <a-tag color="purple">{{ panel.formulaCount }}</a-tag>
        </div>

        <!-- 属性 -->
        <div class="section-title">属性 ({{ (panel.raw?.attributes || []).length }})</div>
        <div class="attr-grid">
          <div class="attr-row" v-for="a in (panel.raw?.attributes || [])" :key="a.key">
            <span class="attr-key">{{ a.name }}</span>
            <span class="attr-val">{{ a.value }} {{ a.unit || '' }}</span>
          </div>
        </div>

        <!-- 动作 -->
        <div class="section-title">动作</div>
        <a-space direction="vertical" style="width:100%">
          <a-button type="primary" size="small" block @click="$emit('action', { type: 'explore', target: panel.raw?.id })">
            在图谱中展开
          </a-button>
          <a-button size="small" block @click="$emit('action', { type: 'records', specCode: panel.raw?.code })">
            查看检测记录
          </a-button>
        </a-space>
      </template>

      <!-- 规则聚合详情 -->
      <template v-if="panel.type === 'ruleCombo'">
        <div class="prop-row"><b>规格:</b> {{ panel.specName || '通用' }}</div>
        <div class="prop-row"><b>状态:</b>
          <a-tag :color="panel.qualityStatus === '合格' ? 'success' : 'error'">{{ panel.qualityStatus }}</a-tag>
        </div>
        <div class="prop-row"><b>规则数:</b> {{ panel.rules?.length || 0 }}</div>
        <div class="section-title">规则列表</div>
        <div class="rule-list">
          <div class="rule-item" v-for="r in panel.rules" :key="r.id"
               :class="{ 'rule-item-active': selectedRuleId === r.id }"
               @click="selectRule(r)">
            <span class="rule-name">{{ r.name }}</span>
            <span class="rule-priority">P{{ r.priority }}</span>
          </div>
        </div>
      </template>

      <!-- 单条规则详情 -->
      <template v-if="panel.type === 'rule'">
        <a-button type="link" size="small" @click="$emit('back')" style="padding:0;margin-bottom:4px">
          ← 返回列表
        </a-button>
        <div class="prop-row"><b>等级:</b> {{ panel.raw?.name }}</div>
        <div class="prop-row"><b>代码:</b> {{ panel.raw?.code || '-' }}</div>
        <div class="prop-row"><b>质量状态:</b>
          <a-tag :color="panel.raw?.quality_status === '合格' ? 'success' : 'error'">{{ panel.raw?.quality_status }}</a-tag>
        </div>
        <div class="prop-row"><b>优先级:</b> {{ panel.raw?.priority }}</div>
        <div class="prop-row" v-if="panel.raw?.product_spec_name"><b>规格:</b> {{ panel.raw.product_spec_name }}</div>
        <div class="prop-row" v-if="panel.raw?.formula_name"><b>公式:</b> {{ panel.raw.formula_name }}</div>
        <div class="prop-row" v-if="panel.raw?.description"><b>说明:</b> {{ panel.raw.description }}</div>

        <!-- 条件表格（新增） -->
        <template v-if="conditions.length > 0">
          <div class="section-title">判定条件 ({{ conditions.length }})</div>
          <div class="condition-table">
            <div class="cond-header">
              <span>字段</span>
              <span>期望</span>
              <span>实际</span>
              <span>结果</span>
            </div>
            <div class="cond-row" v-for="(c, i) in conditions" :key="i">
              <span :title="c.field">{{ c.label || c.field }}</span>
              <span>{{ c.expected }}</span>
              <span>{{ c.actual ?? '-' }}</span>
              <span>
                <a-tag v-if="c.satisfied === true" color="success" size="small">满足</a-tag>
                <a-tag v-else-if="c.satisfied === false" color="error" size="small">不满足</a-tag>
                <a-tag v-else size="small">未评估</a-tag>
              </span>
            </div>
          </div>
        </template>
      </template>

      <!-- 公式详情 -->
      <template v-if="panel.type === 'formula'">
        <div class="prop-row"><b>名称:</b> {{ panel.raw?.formula_name }}</div>
        <div class="prop-row"><b>类型:</b>
          <a-tag>{{ formulaTypeLabel }}</a-tag>
        </div>
        <div class="prop-row"><b>列名:</b> {{ panel.raw?.column_name }}</div>
        <div class="prop-row"><b>单位:</b> {{ panel.raw?.unit_name || '-' }}</div>
        <div class="section-title" v-if="panel.raw?.formula">公式</div>
        <pre class="formula-code" v-if="panel.raw?.formula">{{ panel.raw.formula }}</pre>
        <div class="section-title">关联规则 ({{ panel.ruleCount }})</div>
        <div class="rule-list">
          <div class="rule-item" v-for="r in panel.linkedRules" :key="r.id">
            <span class="rule-name">{{ r.name }}</span>
            <span class="rule-priority">P{{ r.priority }}</span>
          </div>
        </div>
      </template>

      <!-- 带材详情 -->
      <template v-if="panel.type === 'ribbon'">
        <div class="prop-row"><b>炉号:</b> {{ panel.raw?.furnace_no }}</div>
        <div class="prop-row" v-if="panel.raw?.furnace_no_formatted"><b>格式化:</b> {{ panel.raw.furnace_no_formatted }}</div>
        <div class="prop-row"><b>规格:</b> {{ panel.raw?.spec_name || panel.raw?.spec_code || '-' }}</div>
        <div class="prop-row"><b>检测日期:</b> {{ panel.raw?.detection_date || '-' }}</div>
        <div class="prop-row"><b>等级:</b>
          <a-tag :color="panel.raw?.labeling === 'A' ? 'success' : panel.raw?.labeling === 'C' ? 'error' : 'processing'">
            {{ panel.raw?.labeling || '未判定' }}
          </a-tag>
        </div>
        <div class="section-title">性能数据</div>
        <div class="attr-grid">
          <div class="attr-row"><span class="attr-key">Ps铁损</span><span class="attr-val">{{ panel.raw?.ps_loss ?? '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">Ss激磁功率</span><span class="attr-val">{{ panel.raw?.ss_power ?? '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">Hc</span><span class="attr-val">{{ panel.raw?.hc ?? '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">宽度</span><span class="attr-val">{{ panel.raw?.width ?? '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">平均厚度</span><span class="attr-val">{{ panel.raw?.avg_thickness ?? '-' }}</span></div>
        </div>
        <div class="section-title">判定结果</div>
        <div class="attr-grid">
          <div class="attr-row"><span class="attr-key">磁性能</span><span class="attr-val">{{ panel.raw?.magnetic_res || '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">厚度</span><span class="attr-val">{{ panel.raw?.thick_res || '-' }}</span></div>
          <div class="attr-row"><span class="attr-key">叠片系数</span><span class="attr-val">{{ panel.raw?.lam_factor_res || '-' }}</span></div>
        </div>
      </template>

      <!-- 叠片数据详情 -->
      <template v-if="panel.type === 'lamination'">
        <div class="prop-row"><b>宽度:</b> {{ panel.raw?.width ?? '-' }}</div>
        <div class="prop-row"><b>卷重:</b> {{ panel.raw?.coil_weight ?? '-' }}</div>
        <div class="prop-row"><b>断头数:</b> {{ panel.raw?.break_count ?? '-' }}</div>
        <div class="prop-row"><b>单卷重量:</b> {{ panel.raw?.single_coil_weight ?? '-' }}</div>
      </template>

      <!-- 单片性能详情 -->
      <template v-if="panel.type === 'singleSheet'">
        <div class="prop-row"><b>Ps铁损:</b> {{ panel.raw?.ps_loss ?? '-' }}</div>
        <div class="prop-row"><b>Ss激磁功率:</b> {{ panel.raw?.ss_power ?? '-' }}</div>
        <div class="prop-row"><b>Hc:</b> {{ panel.raw?.hc ?? '-' }}</div>
      </template>

      <!-- 外观特性详情 -->
      <template v-if="panel.type === 'appearance'">
        <div class="prop-row"><b>特性名称:</b> {{ panel.raw?.name }}</div>
        <div class="prop-row"><b>大类:</b> {{ panel.raw?.category || '-' }}</div>
        <div class="prop-row"><b>等级:</b> {{ panel.raw?.level || '-' }}</div>
      </template>
    </div>
  </transition>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';
import { CloseOutlined } from '@ant-design/icons-vue';
import type { PanelData, OntologyRule } from '../types/ontology';

const props = defineProps<{
  panel: PanelData | null;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'back'): void;
  (e: 'selectRule', rule: OntologyRule): void;
  (e: 'action', payload: Record<string, unknown>): void;
}>();

const selectedRuleId = ref('');

const formulaTypeLabel = computed(() => {
  const raw = props.panel?.raw as any;
  if (!raw) return '-';
  return raw.formula_type === 'CALC'
    ? '计算公式'
    : raw.formula_type === 'JUDGE' || raw.formula_type === '2'
      ? '判定公式'
      : raw.formula_type || '-';
});

interface ParsedCondition {
  field: string;
  label?: string;
  expected: string;
  actual?: string | number;
  satisfied?: boolean | null;
}

const conditions = computed<ParsedCondition[]>(() => {
  const raw = props.panel?.raw as any;
  if (!raw?.conditionJson) return [];
  try {
    const parsed = JSON.parse(raw.conditionJson);
    if (!Array.isArray(parsed)) return [];
    return parsed
      .filter((c: any) => c && typeof c === 'object')
      .map((c: any) => ({
        field: c.field || c.column || c.column_name || c.metric || '',
        label: c.label || c.field || '',
        expected: c.operator && c.value !== undefined
          ? `${c.operator} ${c.value}`
          : c.min !== undefined && c.max !== undefined
            ? `${c.min} ~ ${c.max}`
            : '-',
        actual: undefined,
        satisfied: undefined,
      }));
  } catch {
    return [];
  }
});

function selectRule(r: OntologyRule) {
  selectedRuleId.value = r.id;
  emit('selectRule', r);
}
</script>

<style lang="less" scoped>
.kg-panel {
  width: 340px;
  background: #fff;
  border-left: 1px solid #F1F5F9;
  padding: 16px;
  overflow-y: auto;
  flex-shrink: 0;
}

.panel-head {
  display: flex;
  align-items: center;
  gap: 8px;
}

.panel-type-badge {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 4px;
  color: #fff;
  font-size: 11px;
  font-weight: 600;
  flex-shrink: 0;
}

.panel-label {
  font-size: 15px;
  font-weight: 600;
  color: #1E293B;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  flex: 1;
}

.panel-close { flex-shrink: 0; }

.prop-row {
  font-size: 13px;
  color: #475569;
  margin-bottom: 5px;
  line-height: 1.6;
  b { color: #334155; }
}

.section-title {
  font-size: 12px;
  font-weight: 600;
  color: #64748B;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin: 14px 0 8px;
  padding-bottom: 4px;
  border-bottom: 1px solid #F1F5F9;
}

.attr-grid {
  display: flex;
  flex-direction: column;
  gap: 3px;
}

.attr-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #F8FAFC;
  border-radius: 4px;
  padding: 4px 8px;
  font-size: 12px;
}

.attr-key { color: #64748B; }
.attr-val { color: #1E293B; font-weight: 500; }

.rule-list {
  display: flex;
  flex-direction: column;
  gap: 2px;
  max-height: 260px;
  overflow-y: auto;
}

.rule-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 5px 8px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  transition: background 0.15s;

  &:hover { background: #F1F5F9; }
  &-active { background: #EFF6FF; }
}

.rule-name { color: #334155; }
.rule-priority { color: #94A3B8; font-size: 11px; }

.formula-code {
  background: #F8FAFC;
  border: 1px solid #E2E8F0;
  border-radius: 6px;
  padding: 8px;
  font-size: 12px;
  font-family: 'SFMono-Regular', Consolas, monospace;
  color: #334155;
  white-space: pre-wrap;
  word-break: break-all;
  max-height: 160px;
  overflow-y: auto;
  margin: 4px 0 0;
}

.condition-table {
  font-size: 12px;
  border: 1px solid #F1F5F9;
  border-radius: 6px;
  overflow: hidden;

  .cond-header, .cond-row {
    display: grid;
    grid-template-columns: 1.4fr 1fr 1fr 60px;
    gap: 4px;
    padding: 5px 8px;
    align-items: center;
  }

  .cond-header {
    background: #F8FAFC;
    font-weight: 600;
    color: #64748B;
  }

  .cond-row {
    border-top: 1px solid #F1F5F9;
    color: #475569;
  }
}

.slide-enter-active, .slide-leave-active {
  transition: transform 0.2s ease, opacity 0.2s ease;
}
.slide-enter-from { transform: translateX(100%); opacity: 0; }
.slide-leave-to { transform: translateX(100%); opacity: 0; }
</style>
