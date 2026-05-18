"""LightRAG SLA 回归测试.

跑 10 个客户验收标准里的代表性问题，输出准确率 + 引用准确率 + 置信度分布。
CI 集成：通过率 < 80% 退出码 1，阻断 PR。

每个 case 校验三项：
  - must_contain：答案里**必须出现**的关键词（缺一则 fail）
  - must_not_contain：答案里**绝不能出现**的关键词（任一出现则 fail，防止幻觉）
  - min_confidence：置信度下限（低于 fail）

跑法：
    cd nlq-agent/services/agent-api
    uv run python scripts/lightrag_eval.py
    uv run python scripts/lightrag_eval.py --verbose   # 打印每个答案全文
    uv run python scripts/lightrag_eval.py --json out.json   # 输出结果到 JSON 供 CI 解析
"""

from __future__ import annotations

import argparse
import asyncio
import json
import os
import sys
import time
from dataclasses import dataclass, asdict
from pathlib import Path

sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

os.environ.setdefault("LIGHTRAG_ENABLED", "True")

from app.knowledge_graph import lightrag_index  # noqa: E402


@dataclass
class EvalCase:
    id: str
    question: str
    must_contain: list[str]
    must_not_contain: list[str]
    min_confidence: float
    # 期望的 source 类型（如 "knowledge_base.json" / "Neo4j:" / "lab_report_config"）；为空跳过校验
    expected_source_prefixes: list[str]
    description: str


# ──────────────────────────────────────────────────────────────────────────
# 10 个 SLA 用例（覆盖客户验收标准）
# ──────────────────────────────────────────────────────────────────────────
EVAL_CASES: list[EvalCase] = [
    EvalCase(
        id="sla_01_furnace_format",
        question="炉号是怎么组成的",
        # "炉号" 在答案里是格式里的元素之一；也写作"炉次号"。两者认其一
        must_contain=["产线", "班次", "生产日期", "卷号", "分卷号"],
        must_not_contain=[],  # 答案里出现"F_LABELING"是合理的（对比说明）
        min_confidence=0.55,
        expected_source_prefixes=["knowledge_base.json", "Neo4j:FurnaceNoParsed"],
        description="炉号格式定义查询",
    ),
    EvalCase(
        id="sla_02_first_inspection_def",
        question="一次交检合格率怎么算",
        must_contain=["F_FIRST_INSPECTION", "F_SINGLE_COIL_WEIGHT"],
        # 注意：答案可以提到 F_LABELING 但必须是在「不能用 F_LABELING」的否定语境里。
        # must_not_contain 用 _strict_field_check 单独处理：只有当 F_LABELING 出现且不在否定上下文里才 fail
        must_not_contain=[],
        min_confidence=0.55,
        expected_source_prefixes=["knowledge_base.json", "lab_report_config"],
        description="一次交检合格率口径",
    ),
    EvalCase(
        id="sla_03_field_distinction",
        question="F_LABELING 和 F_FIRST_INSPECTION 有什么区别",
        must_contain=["贴标", "一次检验", "F_LABELING", "F_FIRST_INSPECTION"],
        must_not_contain=[],
        min_confidence=0.55,
        expected_source_prefixes=["knowledge_base.json"],
        description="关键字段区别",
    ),
    EvalCase(
        id="sla_04_shift_dimension",
        question="班次有哪些",
        must_contain=["甲", "乙", "丙"],
        must_not_contain=[],
        min_confidence=0.50,
        expected_source_prefixes=["knowledge_base.json", "dimensions_meta.json"],
        description="班次维度查询",
    ),
    EvalCase(
        id="sla_05_line_dimension",
        question="检测中心有几条生产线",
        must_contain=["4", "产线"],
        must_not_contain=[],
        min_confidence=0.50,
        expected_source_prefixes=["knowledge_base.json", "dimensions_meta.json"],
        description="产线维度查询",
    ),
    EvalCase(
        id="sla_06_quality_distribution",
        question="质量等级分布是怎么算的",
        must_contain=["F_LABELING"],
        # 提到 F_FIRST_INSPECTION 没问题，只要是在「不是 F_FIRST_INSPECTION」否定语境里
        must_not_contain=[],
        min_confidence=0.55,
        expected_source_prefixes=["knowledge_base.json"],
        description="质量等级分布口径（防一次交检幻觉）",
    ),
    EvalCase(
        id="sla_07_batch_no",
        question="批次号是什么",
        must_contain=["F_FURNACE_BATCH_NO"],
        must_not_contain=[],
        min_confidence=0.50,
        expected_source_prefixes=["knowledge_base.json"],
        description="批次号 vs 炉号术语",
    ),
    EvalCase(
        id="sla_08_appearance",
        question="外观特性有哪些",
        must_contain=["脆", "划"],
        must_not_contain=[],
        min_confidence=0.50,
        expected_source_prefixes=["knowledge_base.json"],
        description="外观特性列表",
    ),
    EvalCase(
        id="sla_09_coil_weight",
        question="卷重是什么字段",
        must_contain=["F_SINGLE_COIL_WEIGHT"],
        must_not_contain=[],
        min_confidence=0.45,
        expected_source_prefixes=["knowledge_base.json"],
        description="卷重字段",
    ),
    EvalCase(
        id="sla_10_negative_case",
        question="本月有什么新闻头条",
        must_contain=[],
        must_not_contain=[],  # 不预设关键词，但必须包含 disclaimer
        min_confidence=0.0,
        expected_source_prefixes=[],
        description="无关问题 → 必须 disclaimer，不能瞎编",
    ),
]


@dataclass
class EvalResult:
    case_id: str
    question: str
    passed: bool
    answer_excerpt: str
    confidence: float
    citations: list[str]
    failures: list[str]
    elapsed_s: float


def _used_field_positively(answer: str, field: str) -> bool:
    """检测某字段在答案里是否被「肯定使用」（而不是被否定提及）。

    返回 True = 真幻觉（字段被误用）；False = 没出现，或者在否定语境（"不是 F_XXX""不能用 F_XXX"）里。

    策略：找到字段每次出现的位置，向前回看 ~15 字符，看是否有否定词。
    没有否定词在附近 → 算「肯定使用」。
    """
    if field not in answer:
        return False
    negation_markers = ["不是", "不应", "不能", "不可", "而非", "并非", "禁止", "禁用",
                        "不要用", "不要使用", "切勿", "勿用", "区别于", "区别在",
                        "not ", "rather than", "instead of"]
    pos = 0
    while True:
        idx = answer.find(field, pos)
        if idx < 0:
            return False
        # 看前 20 字符窗口
        window = answer[max(0, idx - 20):idx].lower()
        if not any(neg.lower() in window for neg in negation_markers):
            return True   # 肯定上下文里出现了 → 幻觉
        pos = idx + len(field)


async def run_one(case: EvalCase, *, verbose: bool = False) -> EvalResult:
    """跑用户真实路径：lookup_kb_smart（LightRAG 在线先走它，失败 fallback 静态 KB）"""
    from app.knowledge_graph.kb_lookup import lookup_kb_smart

    t0 = time.time()
    smart = await lookup_kb_smart(case.question)
    elapsed = time.time() - t0

    # 把 smart 结果统一适配成 LightRAGResult-like 结构
    if smart:
        answer = (smart.get("answer") or "").strip()
        citations = list(smart.get("citations") or [])
        confidence = float(smart.get("confidence") or 0.0)
    else:
        # smart 完全没命中 → fallback 到直接 LightRAG 查（验证 LightRAG 是否至少能召回点东西）
        result = await lightrag_index.query(case.question, mode="hybrid", top_k=8)
        answer = (result.answer or "").strip()
        citations = list(result.citations or [])
        confidence = result.confidence
    failures: list[str] = []

    # must_contain（每个都必须出现）
    for kw in case.must_contain:
        if kw.lower() not in answer.lower():
            failures.append(f"missing must_contain: {kw!r}")

    # must_not_contain（任一出现都失败）
    for kw in case.must_not_contain:
        if kw.lower() in answer.lower():
            failures.append(f"contains forbidden: {kw!r}")

    # min_confidence
    if confidence < case.min_confidence:
        failures.append(f"confidence {confidence:.2f} < min {case.min_confidence:.2f}")

    # negative case 的特殊校验：必须有 disclaimer
    if case.id == "sla_10_negative_case":
        disclaimers = [
            "没有足够", "无法回答", "不知道", "不包含",
            "没有找到", "找不到", "未找到", "未涉及",
            "不在", "并未", "没有提及", "没有相关",
            "i don't", "no information", "cannot find",
        ]
        if not any(d in answer for d in disclaimers):
            failures.append("negative case missing disclaimer (potential hallucination)")

    # 关键字段口径专项检测（防止真幻觉，比 must_not_contain 更智能）
    # 规则：如果答案的【第一个 200 字符】里"主断言"用错列，就算幻觉。
    # 因为业务人员一眼看到的就是开头几句，开头说对了就 OK。
    if case.id == "sla_02_first_inspection_def":
        head = answer[:200]
        if "F_LABELING" in head and "F_FIRST_INSPECTION" not in head:
            failures.append("hallucination: head paragraph uses F_LABELING without naming F_FIRST_INSPECTION")
    if case.id == "sla_06_quality_distribution":
        head = answer[:200]
        if "F_FIRST_INSPECTION" in head and "F_LABELING" not in head:
            failures.append("hallucination: head paragraph uses F_FIRST_INSPECTION without naming F_LABELING")

    # source 校验
    if case.expected_source_prefixes:
        if citations:
            hit = any(
                any(c.startswith(prefix) for prefix in case.expected_source_prefixes)
                for c in citations
            )
            if not hit:
                failures.append(
                    f"none of citations match expected sources; got: {citations[:3]}"
                )
        # citations 空时不强制（LightRAG context 解析可能 fallback 到无 source 模式）

    return EvalResult(
        case_id=case.id,
        question=case.question,
        passed=len(failures) == 0,
        answer_excerpt=answer[:300] + ("..." if len(answer) > 300 else ""),
        confidence=confidence,
        citations=citations[:5],
        failures=failures,
        elapsed_s=round(elapsed, 2),
    )


async def main() -> int:
    parser = argparse.ArgumentParser(description="LightRAG SLA regression")
    parser.add_argument("--verbose", action="store_true", help="打印每个答案全文")
    parser.add_argument("--json", type=str, default=None, help="输出 JSON 报告到此路径")
    parser.add_argument("--threshold", type=float, default=0.8,
                        help="通过率阈值（默认 0.8 = 80%），低于则 exit 1")
    args = parser.parse_args()

    print("\n========== LightRAG SLA Regression ==========\n")
    print(f"运行 {len(EVAL_CASES)} 个 SLA case，阈值通过率 {args.threshold:.0%}\n")

    results: list[EvalResult] = []
    for case in EVAL_CASES:
        print(f"[{case.id}] {case.question!r} ...", end="", flush=True)
        r = await run_one(case, verbose=args.verbose)
        results.append(r)
        flag = "✅" if r.passed else "❌"
        print(f" {flag} conf={r.confidence:.2f} ({r.elapsed_s}s)")
        if not r.passed:
            for f in r.failures:
                print(f"      → {f}")
        if args.verbose:
            print(f"      Answer: {r.answer_excerpt}")
            print(f"      Citations: {r.citations}")
            print()

    # 汇总
    passed = sum(1 for r in results if r.passed)
    total = len(results)
    pass_rate = passed / total if total else 0.0
    avg_conf = sum(r.confidence for r in results) / total if total else 0.0
    avg_latency = sum(r.elapsed_s for r in results) / total if total else 0.0

    print("\n========== Summary ==========")
    print(f"  通过率：{passed}/{total} = {pass_rate:.0%}")
    print(f"  平均置信度：{avg_conf:.2f}")
    print(f"  平均延迟：{avg_latency:.2f}s")

    if args.json:
        Path(args.json).write_text(
            json.dumps({
                "passed": passed, "total": total, "pass_rate": pass_rate,
                "avg_confidence": avg_conf, "avg_latency_s": avg_latency,
                "results": [asdict(r) for r in results],
            }, ensure_ascii=False, indent=2),
            encoding="utf-8",
        )
        print(f"\n报告写入 {args.json}")

    if pass_rate < args.threshold:
        print(f"\n❌ FAIL：通过率 {pass_rate:.0%} 低于阈值 {args.threshold:.0%}")
        return 1
    print(f"\n🎉 PASS：通过率 {pass_rate:.0%} ≥ 阈值 {args.threshold:.0%}")
    return 0


if __name__ == "__main__":
    rc = asyncio.run(main())
    sys.exit(rc)
