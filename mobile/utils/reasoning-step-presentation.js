// Runtime presentation table for reasoning step kinds.
// Mirrors web/src/types/reasoning-step-presentation.ts.

export const REASONING_STEP_PRESENTATION = {
  // 旧版（判定根因路径）
  record:    { label: '命中记录', color: '#475569' },
  spec:      { label: '产品规格', color: '#1e40af' },
  rule:      { label: '判定规则', color: '#6d28d9' },
  condition: { label: '条件评估', color: '#155e75' },
  grade:     { label: '最终结论', color: '#92400e' },
  fallback:  { label: '降级处理', color: '#b91c1c' },
  // NLQ Agent 通过 adispatch_custom_event 推送的 kind
  intent:        { label: '意图识别', color: '#0891b2' },
  sql:           { label: '数据查询', color: '#0e7490' },
  answer:        { label: '整理回答', color: '#0d9488' },
  schema_pick:   { label: '选表',     color: '#1e40af' },
  column_pick:   { label: '选列',     color: '#1d4ed8' },
  sql_draft:     { label: 'SQL 草稿', color: '#7c3aed' },
  sql_validate:  { label: 'SQL 校验', color: '#6d28d9' },
  execute_sql:   { label: '执行 SQL', color: '#059669' },
  result_summary:{ label: '结果总结', color: '#0d9488' },
};
