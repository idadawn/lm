<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# types

## Purpose
mobile 端 TypeScript 类型声明目录。集中存放跨文件共享的接口/枚举定义，目前主要承载 NLQ Agent 推理链协议的类型快照，与 `nlq-agent/packages/shared-types` 保持同步。

## Key Files
| File | Description |
|------|-------------|
| `reasoning-protocol.d.ts` | `ReasoningStep` / `ReasoningChainEvent` / `ReasoningStepKind` 类型声明，被 `kg-reasoning-chain` 组件与 `utils/sse-client.js` 消费 |

## For AI Agents

### Working in this directory
- 文件头部的 `upstream-sha` 注释是同步校验锚点；**不要手工修改**该 SHA。
- 修改字段时必须先改 `nlq-agent/packages/shared-types/src/reasoning-protocol.ts`，再通过 `pwsh scripts/check-reasoning-protocol-sync.ps1` 拷贝下游。
- 仅放置纯类型（`.d.ts`），不要写运行时代码；如需共享常量请放 `mobile/utils/`。
- 所有字段保持 optional 友好，避免后端协议演进导致前端解析报错。

### Common patterns
- `export type Foo = "a" | "b"` 字面量联合表示枚举。
- `export interface` 描述事件载荷，字段名严格 snake_case 与服务端保持一致。

## Dependencies
### Internal
- `nlq-agent/packages/shared-types/src/reasoning-protocol.ts`（上游真值）
- `mobile/components/kg-reasoning-chain/`、`mobile/utils/sse-client.js` 消费方

### External
- TypeScript 仅作类型支持，不参与运行时打包

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
