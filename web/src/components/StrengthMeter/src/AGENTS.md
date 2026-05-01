<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
Single-file implementation of `StrengthMeter`. Hosts `Input.Password`, runs `zxcvbn(value)` on change, and stores the score in a `data-score` attribute consumed by `index.less` (or scoped LESS) to switch the colour of the strength bar.

## Key Files
| File | Description |
|------|-------------|
| `StrengthMeter.vue` | SFC: `InputPassword` + `[prefixCls]-bar` div with `[prefixCls]-bar--fill` styled by `data-score` (0..4). |

## For AI Agents

### Working in this directory
- The visual strength is encoded as a CSS attribute selector (`[data-score="3"]`), not via class — preserve attribute name on edits.
- Component is `inheritAttrs: true` by default; pass-through `$attrs` lands on the inner `InputPassword`.
- `disabled` and `value` are explicit props; everything else flows through.

### Common patterns
- Watch `value` to keep `innerValueRef` in sync; emit both `change` (string) and `score-change` (number).

## Dependencies
### Internal
- `/@/hooks/web/useDesign`, `/@/utils/propTypes`.
### External
- `ant-design-vue` (`Input.Password`), `@zxcvbn-ts/core` (`zxcvbn`).

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
