/**
 * Unit tests for ReasoningChain.vue
 *
 * Coverage:
 * - empty steps array → component does not render (v-if guard)
 * - single step with kind='spec' → correct label/color from REASONING_STEP_PRESENTATION
 * - multiple steps → sequential 1/2/3 index numbers rendered
 * - defaultOpen=false → collapsed by default; list region absent
 * - defaultOpen=true → expanded by default; list region present
 * - clicking header button → toggles expanded state
 * - all 6 step kinds (record/spec/rule/condition/grade/fallback) → each renders
 *   label and color matching REASONING_STEP_PRESENTATION
 * - condition step with satisfied=true → shows '满足' tag
 * - condition step with satisfied=false → shows '不满足' tag
 */

import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import { defineComponent, h } from 'vue';
import ReasoningChain from '../ReasoningChain.vue';
import { REASONING_STEP_PRESENTATION } from '/@/types/reasoning-step-presentation';
import type { ReasoningStep } from '/@/types/reasoning-protocol';

// ---------------------------------------------------------------------------
// Global stubs for Ant Design / icon components
// We stub at the component level so happy-dom doesn't have to handle them.
// ---------------------------------------------------------------------------

const globalStubs = {
  BulbOutlined: defineComponent({ name: 'BulbOutlined', render: () => h('span', { class: 'stub-bulb' }) }),
  DownOutlined: defineComponent({ name: 'DownOutlined', render: () => h('span', { class: 'stub-down' }) }),
  // a-tag renders its default slot text; color is passed as a prop
  'a-tag': defineComponent({
    name: 'ATag',
    props: ['color'],
    render() {
      return h(
        'span',
        { class: 'stub-tag', 'data-color': (this as { color: string }).color },
        (this as { $slots: { default?: () => unknown[] } }).$slots.default?.(),
      );
    },
  }),
};

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function mountChain(steps: ReasoningStep[], defaultOpen = false) {
  return mount(ReasoningChain, {
    props: { steps, defaultOpen },
    global: { stubs: globalStubs },
  });
}

// ---------------------------------------------------------------------------
// Test data
// ---------------------------------------------------------------------------

const SPEC_STEP: ReasoningStep = { kind: 'spec', label: '产品规格条目' };
const RECORD_STEP: ReasoningStep = { kind: 'record', label: '命中记录条目' };
const RULE_STEP: ReasoningStep = { kind: 'rule', label: '判定规则条目' };
const CONDITION_STEP_TRUE: ReasoningStep = {
  kind: 'condition',
  label: '条件评估条目',
  satisfied: true,
  expected: '>=90',
  actual: '95',
};
const CONDITION_STEP_FALSE: ReasoningStep = {
  kind: 'condition',
  label: '条件不满足条目',
  satisfied: false,
  expected: '>=90',
  actual: '80',
};
const GRADE_STEP: ReasoningStep = { kind: 'grade', label: '最终结论条目' };
const FALLBACK_STEP: ReasoningStep = { kind: 'fallback', label: '降级条目' };

// ---------------------------------------------------------------------------
// Tests: empty state
// ---------------------------------------------------------------------------

describe('ReasoningChain — empty steps', () => {
  it('does not render the root element when steps array is empty', () => {
    const wrapper = mountChain([]);
    // v-if guard: root div[data-testid="reasoning-chain"] should not exist
    expect(wrapper.find('[data-testid="reasoning-chain"]').exists()).toBe(false);
  });
});

// ---------------------------------------------------------------------------
// Tests: single step rendering
// ---------------------------------------------------------------------------

describe('ReasoningChain — single step', () => {
  it('renders the root element when given one step', () => {
    const wrapper = mountChain([SPEC_STEP], true);
    expect(wrapper.find('[data-testid="reasoning-chain"]').exists()).toBe(true);
  });

  it('shows step count "1 步" in title for a single step', () => {
    const wrapper = mountChain([SPEC_STEP], true);
    expect(wrapper.text()).toContain('1 步');
  });

  it('renders spec kind label from REASONING_STEP_PRESENTATION', () => {
    const wrapper = mountChain([SPEC_STEP], true);
    const expected = REASONING_STEP_PRESENTATION['spec'].label;
    expect(wrapper.text()).toContain(expected);
  });

  it('renders spec kind tag with color from REASONING_STEP_PRESENTATION', () => {
    const wrapper = mountChain([SPEC_STEP], true);
    const expectedColor = REASONING_STEP_PRESENTATION['spec'].color;
    const tags = wrapper.findAll('.stub-tag');
    const colorValues = tags.map((t) => t.attributes('data-color'));
    expect(colorValues).toContain(expectedColor);
  });

  it('renders the step label text', () => {
    const wrapper = mountChain([SPEC_STEP], true);
    expect(wrapper.text()).toContain(SPEC_STEP.label);
  });
});

// ---------------------------------------------------------------------------
// Tests: multiple steps — sequential index numbers
// ---------------------------------------------------------------------------

describe('ReasoningChain — multiple steps', () => {
  it('renders step count "3 步" for three steps', () => {
    const wrapper = mountChain([SPEC_STEP, RULE_STEP, GRADE_STEP], true);
    expect(wrapper.text()).toContain('3 步');
  });

  it('renders index numbers 1, 2, 3 in order for three steps', () => {
    const wrapper = mountChain([SPEC_STEP, RULE_STEP, GRADE_STEP], true);
    const rows = wrapper.findAll('[data-testid="reasoning-step"]');
    expect(rows).toHaveLength(3);
    // Each row's first text node (the index span) should show 1, 2, 3
    const indexTexts = rows.map((r) => r.find('.reasoning-chain__index').text());
    expect(indexTexts).toEqual(['1', '2', '3']);
  });
});

// ---------------------------------------------------------------------------
// Tests: defaultOpen=false (collapsed by default)
// ---------------------------------------------------------------------------

describe('ReasoningChain — defaultOpen=false', () => {
  it('does not render the list region when defaultOpen is false', () => {
    const wrapper = mountChain([SPEC_STEP]);
    expect(wrapper.find('#reasoning-chain-list').exists()).toBe(false);
  });

  it('renders the header button even when collapsed', () => {
    const wrapper = mountChain([SPEC_STEP]);
    expect(wrapper.find('button.reasoning-chain__header').exists()).toBe(true);
  });

  it('sets aria-expanded="false" on the header button when collapsed', () => {
    const wrapper = mountChain([SPEC_STEP]);
    const btn = wrapper.find('button.reasoning-chain__header');
    expect(btn.attributes('aria-expanded')).toBe('false');
  });
});

// ---------------------------------------------------------------------------
// Tests: defaultOpen=true (expanded by default)
// ---------------------------------------------------------------------------

describe('ReasoningChain — defaultOpen=true', () => {
  it('renders the list region when defaultOpen is true', () => {
    const wrapper = mountChain([SPEC_STEP], true);
    expect(wrapper.find('#reasoning-chain-list').exists()).toBe(true);
  });

  it('sets aria-expanded="true" on the header button when expanded', () => {
    const wrapper = mountChain([SPEC_STEP], true);
    const btn = wrapper.find('button.reasoning-chain__header');
    expect(btn.attributes('aria-expanded')).toBe('true');
  });
});

// ---------------------------------------------------------------------------
// Tests: toggle behavior (clicking header)
// ---------------------------------------------------------------------------

describe('ReasoningChain — toggle on header click', () => {
  it('expands the list when header is clicked while collapsed', async () => {
    const wrapper = mountChain([SPEC_STEP], false);
    expect(wrapper.find('#reasoning-chain-list').exists()).toBe(false);
    await wrapper.find('button.reasoning-chain__header').trigger('click');
    expect(wrapper.find('#reasoning-chain-list').exists()).toBe(true);
  });

  it('collapses the list when header is clicked while expanded', async () => {
    const wrapper = mountChain([SPEC_STEP], true);
    expect(wrapper.find('#reasoning-chain-list').exists()).toBe(true);
    await wrapper.find('button.reasoning-chain__header').trigger('click');
    expect(wrapper.find('#reasoning-chain-list').exists()).toBe(false);
  });

  it('toggles aria-expanded from false to true on click', async () => {
    const wrapper = mountChain([SPEC_STEP], false);
    const btn = wrapper.find('button.reasoning-chain__header');
    expect(btn.attributes('aria-expanded')).toBe('false');
    await btn.trigger('click');
    expect(btn.attributes('aria-expanded')).toBe('true');
  });
});

// ---------------------------------------------------------------------------
// Tests: all 6 step kinds — label and color consistency
// ---------------------------------------------------------------------------

describe('ReasoningChain — all 6 step kinds', () => {
  const allKinds: Array<{ step: ReasoningStep; kind: string }> = [
    { step: RECORD_STEP, kind: 'record' },
    { step: SPEC_STEP, kind: 'spec' },
    { step: RULE_STEP, kind: 'rule' },
    { step: { kind: 'condition', label: '条件' }, kind: 'condition' },
    { step: GRADE_STEP, kind: 'grade' },
    { step: FALLBACK_STEP, kind: 'fallback' },
  ];

  for (const { step, kind } of allKinds) {
    it(`renders correct label for kind="${kind}"`, () => {
      const wrapper = mountChain([step], true);
      const expected = REASONING_STEP_PRESENTATION[kind].label;
      expect(wrapper.text()).toContain(expected);
    });

    it(`renders correct color for kind="${kind}"`, () => {
      const wrapper = mountChain([step], true);
      const expectedColor = REASONING_STEP_PRESENTATION[kind].color;
      const tags = wrapper.findAll('.stub-tag');
      const colorValues = tags.map((t) => t.attributes('data-color'));
      expect(colorValues).toContain(expectedColor);
    });
  }
});

// ---------------------------------------------------------------------------
// Tests: condition step — satisfied tag
// ---------------------------------------------------------------------------

describe('ReasoningChain — condition step satisfied tag', () => {
  it('shows "满足" tag when condition step has satisfied=true', () => {
    const wrapper = mountChain([CONDITION_STEP_TRUE], true);
    expect(wrapper.text()).toContain('满足');
  });

  it('shows "不满足" tag when condition step has satisfied=false', () => {
    const wrapper = mountChain([CONDITION_STEP_FALSE], true);
    expect(wrapper.text()).toContain('不满足');
  });

  it('renders expected and actual meta values for condition step', () => {
    const wrapper = mountChain([CONDITION_STEP_TRUE], true);
    expect(wrapper.text()).toContain('>=90');
    expect(wrapper.text()).toContain('95');
  });
});

// ---------------------------------------------------------------------------
// Tests: detail field for non-condition steps
// ---------------------------------------------------------------------------

describe('ReasoningChain — step detail field', () => {
  it('renders detail text for a non-condition step that has a detail property', () => {
    const stepWithDetail: ReasoningStep = {
      kind: 'rule',
      label: '规则条目',
      detail: '额外详情说明',
    };
    const wrapper = mountChain([stepWithDetail], true);
    expect(wrapper.text()).toContain('额外详情说明');
  });
});
