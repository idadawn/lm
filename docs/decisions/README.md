# Architectural Decisions (ADR + Process Records)

本目录归档**值得跨季度回顾**的架构与流程决策。每条决策一个目录，按时间命名。

## 何时新建一条 ADR

- 跨多人/多模块的架构选型（数据库选型、缓存策略、协议变更）
- 用 `/oh-my-claudecode:ralplan` 跑过 Planner→Architect→Critic 共识、且后续真的合到 main 的实现切片
- 多 worker 并行实施（`/team`）后选优合并的决策（保留对比依据）
- 有人后来问"为啥当时这么做"会用到的任何重大转向

如果只是 bug fix、refactor、加测试、改文案——**不归档到这里**，git history 已经够了。

## 命名规范

```
docs/decisions/YYYY-MM-DD-<kebab-case-slug>/
├── README.md          ← 必需：背景 / 流程 / 结论 / 后续工作 + 链接到 merge commit
├── PLAN.md            ← 可选：ralplan 产出（含 RALPLAN-DR 总结 + ADR 骨架）
├── VERDICT.md         ← 可选：多方案评分对比
└── <worker>/...       ← 可选：多 worker 并行实施时各自的 prompt / REPORT
```

入口 `README.md` 至少要有：
- **Date** + **Merge commit** 的 git 哈希（点回主线）
- **Background**（为什么做这件事）
- **Process**（怎么决策的）
- **What landed**（实际改了什么文件）
- **Future work**（留尾巴）

## 索引

| 日期 | 决策 | 状态 | 入口 |
|---|---|---|---|
| 2026-04-30 | NLQ-Agent Stage1+Stage2 tracer-bullet（ralplan v2.1）— 双 CLI worker 并行实施，选 GLM 合并 | ✅ shipped (`2fe0fd8`) | [`2026-04-30-nlq-tracer-bullet-v2/`](2026-04-30-nlq-tracer-bullet-v2/) |

## 与 git history 的关系

ADR 不是 commit message 的副本——commit 写**做了什么**，ADR 写**为什么这么做、考虑过什么没采纳**。两者互补：

- 看代码变更 → `git log`
- 看决策轨迹 → 这里
- 看双方案对比 → 这里的 `VERDICT.md`
- 看共识规划全文 → 这里的 `PLAN.md`

## 与 `.omc/` 的关系

`.omc/plans/` 与 `.omc/team-nlq/` 是 OMC runtime 的**临时工作区**，git-ignored。决策完成后**只把决策档案 mirror 到 `docs/decisions/`**（不把 runtime 状态、heartbeat、output.log 等噪声带进来）。
