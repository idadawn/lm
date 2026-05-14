<template>
  <div class="evidence-panel">
    <div class="panel-header">
      <span class="panel-title">
        <FileTextOutlined class="panel-icon" />
        证据与结论
      </span>
    </div>

    <!-- 结论卡片 -->
    <div v-if="answerCard" class="answer-card">
      <div class="answer-grade" v-if="answerCard.grade">
        <span class="grade-label">判定等级</span>
        <span class="grade-value" :class="gradeClass(answerCard.grade)">{{ answerCard.grade }} 级</span>
      </div>
      <div class="answer-meta" v-if="answerCard.furnace_no">
        <span>炉号：{{ answerCard.furnace_no }}</span>
        <span v-if="answerCard.spec_code">规格：{{ answerCard.spec_code }}</span>
      </div>
    </div>

    <!-- 证据表 -->
    <div class="evidence-section" v-if="evidenceTable && evidenceTable.length > 0">
      <div class="section-title">条件证据表</div>
      <div class="evidence-table">
        <div class="table-header">
          <span>字段</span>
          <span>期望</span>
          <span>实际</span>
          <span>结果</span>
        </div>
        <div
          class="table-row"
          v-for="(row, idx) in evidenceTable"
          :key="idx"
          :class="{ failed: row.satisfied === false }"
        >
          <span class="cell-field" :title="row.label || row.field">{{ row.label || row.field }}</span>
          <span class="cell-expected">{{ row.expected || '-' }}</span>
          <span class="cell-actual">{{ formatValue(row.actual) }}</span>
          <span class="cell-result">
            <a-tag v-if="row.satisfied === true" color="success" size="small">满足</a-tag>
            <a-tag v-else-if="row.satisfied === false" color="error" size="small">不满足</a-tag>
            <a-tag v-else size="small">未评估</a-tag>
          </span>
        </div>
      </div>
    </div>

    <!-- 建议动作 -->
    <div class="actions-section" v-if="suggestedActions && suggestedActions.length > 0">
      <div class="section-title">建议动作</div>
      <div class="action-list">
        <a-button
          v-for="(action, idx) in suggestedActions"
          :key="idx"
          size="small"
          block
          :type="idx === 0 ? 'primary' : 'default'"
          class="action-btn"
          @click="$emit('action', action)"
        >
          {{ action.label }}
        </a-button>
      </div>
    </div>

    <!-- 自然语言答案 -->
    <div class="answer-text" v-if="answer">
      <div class="section-title">结论说明</div>
      <div class="answer-content">{{ answer }}</div>
    </div>

    <a-empty v-if="!answer && (!evidenceTable || evidenceTable.length === 0)" description="暂无证据数据" class="empty-evidence" />
  </div>
</template>

<script lang="ts" setup>
import { FileTextOutlined } from '@ant-design/icons-vue';

defineProps<{
  answer?: string;
  answerCard?: Record<string, unknown> | null;
  evidenceTable?: Array<Record<string, unknown>>;
  suggestedActions?: Array<{ label: string; action: string; params?: Record<string, unknown> }>;
}>();

defineEmits<{
  (e: 'action', action: { label: string; action: string; params?: Record<string, unknown> }): void;
}>();

function formatValue(val: unknown): string {
  if (val === undefined || val === null) return '-';
  if (typeof val === 'number') return Number.isInteger(val) ? String(val) : val.toFixed(3);
  return String(val);
}

function gradeClass(grade: unknown): string {
  const g = String(grade).toUpperCase();
  if (g === 'A') return 'grade-a';
  if (g === 'B') return 'grade-b';
  if (g === 'C') return 'grade-c';
  if (g === 'D') return 'grade-d';
  return '';
}
</script>

<style lang="less" scoped>
.evidence-panel {
  width: 300px;
  background: #fff;
  border-left: 1px solid #F1F5F9;
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.panel-header {
  padding: 12px 16px;
  border-bottom: 1px solid #F1F5F9;
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-shrink: 0;
}

.panel-title {
  font-size: 14px;
  font-weight: 600;
  color: #1E293B;
  display: flex;
  align-items: center;
  gap: 6px;
}

.panel-icon {
  color: #2563EB;
  font-size: 16px;
}

.answer-card {
  margin: 12px 16px;
  padding: 12px;
  background: linear-gradient(135deg, #EFF6FF, #F0FDF4);
  border-radius: 10px;
  border: 1px solid #BFDBFE;
}

.answer-grade {
  display: flex;
  align-items: baseline;
  gap: 8px;
  margin-bottom: 6px;
}

.grade-label {
  font-size: 12px;
  color: #64748B;
}

.grade-value {
  font-size: 24px;
  font-weight: 700;
  color: #1E293B;

  &.grade-a { color: #16A34A; }
  &.grade-b { color: #2563EB; }
  &.grade-c { color: #F59E0B; }
  &.grade-d { color: #DC2626; }
}

.answer-meta {
  font-size: 12px;
  color: #64748B;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.evidence-section,
.actions-section,
.answer-text {
  padding: 0 16px;
  margin-top: 12px;
}

.section-title {
  font-size: 12px;
  font-weight: 600;
  color: #64748B;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  margin-bottom: 8px;
}

.evidence-table {
  border: 1px solid #F1F5F9;
  border-radius: 6px;
  overflow: hidden;
  font-size: 12px;
}

.table-header,
.table-row {
  display: grid;
  grid-template-columns: 1.2fr 1fr 1fr 50px;
  gap: 4px;
  padding: 6px 8px;
  align-items: center;
}

.table-header {
  background: #F8FAFC;
  font-weight: 600;
  color: #64748B;
}

.table-row {
  border-top: 1px solid #F1F5F9;
  color: #475569;

  &.failed {
    background: #FEF2F2;
  }
}

.cell-field {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.cell-expected,
.cell-actual {
  text-align: right;
  font-family: 'SFMono-Regular', Consolas, monospace;
}

.action-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.action-btn {
  font-size: 12px;
}

.answer-content {
  font-size: 13px;
  color: #334155;
  line-height: 1.7;
  background: #F8FAFC;
  border-radius: 6px;
  padding: 10px;
  white-space: pre-wrap;
}

.empty-evidence {
  margin-top: 40px;
}
</style>
