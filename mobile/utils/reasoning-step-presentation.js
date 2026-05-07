// Runtime presentation table for reasoning step kinds.
// Mirrors mobile/types/reasoning-step-presentation.d.ts (type declarations).
// Source of truth for label / color values consumed by kg-reasoning-chain component.
// Labels: mobile/components/kg-reasoning-chain/kg-reasoning-chain.vue (was KIND_LABEL, lines 39-46)
// Colors: mobile/components/kg-reasoning-chain/kg-reasoning-chain.vue (CSS foreground text colors, lines 130-137)

export const REASONING_STEP_PRESENTATION = {
  record:    { label: '命中记录', color: '#475569', icon: 'file-text' },
  spec:      { label: '产品规格', color: '#1e40af', icon: 'safety-certificate' },
  rule:      { label: '判定规则', color: '#6d28d9', icon: 'check-circle' },
  condition: { label: '条件评估', color: '#155e75', icon: 'filter' },
  grade:     { label: '最终结论', color: '#92400e', icon: 'star' },
  fallback:  { label: '降级',     color: '#b91c1c', icon: 'question-circle' },
};
