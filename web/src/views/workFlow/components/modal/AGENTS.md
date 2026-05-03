<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# modal

## Purpose
Modal dialogs used inside `FlowParser` and other flow pages — approve/reject, 加签, 候选人, 常用语, 错误 etc.

## Key Files
| File | Description |
|------|-------------|
| `ApprovalModal.vue` | 审批/加签 modal driven by `eventType` (audit/freeApprover) and branch selection. |
| `CandidateModal.vue` | Candidate-user picker for next-step assignment. |
| `CandidateUserSelect.vue` | Lower-level user selector reused by candidate flow. |
| `ActionModal.vue` | Generic action-confirm modal. |
| `CommentModal.vue` | Comment input modal. |
| `CommonWordsPopover.vue` | Quick-insert 常用语 popover. |
| `ResurgenceModal.vue` | 复活/重启流程 modal. |
| `ErrorModal.vue` | Error-detail modal for failed approvals. |

## For AI Agents

### Working in this directory
- Modals share an `eventType` contract with `FlowParser`. When adding a new event, extend the enum in `FlowParser` first, then add the modal here.
- Keep `jnpf-user-select` for user pickers — the candidate flow expects its data shape.
