// upstream-sha: f98655ac9b93aefad9476c297d1ef831bd6f1dc498bb3846a3fd32ee3c107344
// 此文件从 nlq-agent/packages/shared-types/src/reasoning-step-presentation.ts 复制。
// 修改 upstream 后必须更新此 sha — CI 自动校验。

/**
 * Visual presentation table for reasoning step kinds.
 * Source of truth for both web (web/src/types/) and mobile (mobile/types/) copies.
 * Sync via scripts/check-reasoning-protocol-sync.ps1 (CI gate).
 *
 * Color source: mobile/components/kg-reasoning-chain/kg-reasoning-chain.vue lines 130-137
 * (CSS foreground text colors used as tag color; background colors kept as-is in CSS).
 * Label source: mobile/components/kg-reasoning-chain/kg-reasoning-chain.vue lines 39-46 (KIND_LABEL).
 */

export type ReasoningStepKindPresentation = {
  /** Short Chinese label for UI display */
  label: string;
  /** Hex color for kind tag (foreground/text color from mobile CSS) */
  color: string;
  /** Icon name from project icon set; consumer maps to actual icon component */
  icon: string;
};

export const REASONING_STEP_PRESENTATION: Record<string, ReasoningStepKindPresentation> = {
  record:        { label: '命中记录', color: '#475569', icon: 'file-text' },
  spec:          { label: '产品规格', color: '#1e40af', icon: 'safety-certificate' },
  rule:          { label: '判定规则', color: '#6d28d9', icon: 'check-circle' },
  condition:     { label: '条件评估', color: '#155e75', icon: 'filter' },
  grade:         { label: '最终结论', color: '#92400e', icon: 'star' },
  fallback:      { label: '降级处理', color: '#b91c1c', icon: 'question-circle' },
  // NLQ Agent reasoning kinds (Chinese — backend already sends Chinese title/summary)
  intent:        { label: '意图识别', color: '#0891b2', icon: 'bulb' },
  sql:           { label: '数据查询', color: '#0e7490', icon: 'database' },
  answer:        { label: '整理回答', color: '#0d9488', icon: 'edit' },
  schema_pick:   { label: '选表',     color: '#1e40af', icon: 'database' },
  column_pick:   { label: '选列',     color: '#1d4ed8', icon: 'table' },
  sql_draft:     { label: 'SQL 草稿', color: '#7c3aed', icon: 'code' },
  sql_validate:  { label: 'SQL 校验', color: '#6d28d9', icon: 'safety' },
  execute_sql:   { label: '执行 SQL', color: '#059669', icon: 'play-circle' },
  result_summary:{ label: '结果总结', color: '#0d9488', icon: 'file-done' },
};
