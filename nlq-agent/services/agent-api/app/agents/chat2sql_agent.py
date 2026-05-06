"""Chat2SQL agent — KG-grounded NL → SQL with visible reasoning chain.

Pipeline (each step emits a ``reasoning_step`` SSE event):

  1. **schema_pick**   LLM picks ≤3 tables from the lab_* inventory
  2. **column_pick**   LLM picks columns from chosen tables
  3. **sql_draft**     LLM writes a SELECT, grounded by the picked schema
  4. **sql_validate**  whitelist + AST + parameter check; on failure go back to draft
  5. **execute_sql**   run via execute_safe_sql
  6. **result_summary** LLM phrases result in natural Chinese

Why an explicit chain instead of LangChain SQL Database Toolkit:
    - Show every reasoning step to the user (not just final answer)
    - Cap JOIN to 3 tables (industrial schema has 134-col fact table; LLM otherwise hallucinates)
    - Reuse existing SQL whitelist + parameterization safety
    - Fall back to fast paths (first_inspection / metric query / root_cause) when those win
"""

from __future__ import annotations

import json
import logging
import re
from datetime import date, timedelta
from typing import Any

from langchain_core.callbacks import adispatch_custom_event
from langchain_core.messages import HumanMessage

from app.core.llm_factory import get_llm
from app.knowledge_graph.schema_loader import (
    ColumnInfo,
    TableInfo,
    get_schema_cache,
    refresh_schema_cache,
)
from app.tools.sql_tools import execute_safe_sql, validate_sql

logger = logging.getLogger("nlq-agent")

MAX_JOIN_TABLES = 3
MAX_RESULT_ROWS = 50  # 给 LLM 总结时的上限


# --------------------------------------------------------------------------- #
# Helpers
# --------------------------------------------------------------------------- #


def _today_iso() -> str:
    return date.today().isoformat()


def _build_table_inventory_text(cache) -> str:
    """Compact one-line-per-table inventory for the LLM."""
    lines = []
    for tbl in cache.tables.values():
        comment = (tbl.comment or "").replace("\n", " ").strip()[:120]
        lines.append(f"- {tbl.name}：{comment or '（无注释）'}")
    return "\n".join(lines)


def _build_column_block(table: TableInfo, columns: list[ColumnInfo]) -> str:
    head = f"## {table.name}（{table.comment or '无注释'}）\n"
    lines = []
    for col in columns:
        flag = []
        if col.is_primary_key:
            flag.append("PK")
        if col.is_foreign_key_candidate:
            flag.append(f"FK→{col.fk_target}")
        flag_text = f" [{', '.join(flag)}]" if flag else ""
        cmt = (col.comment or "").replace("\n", " ").strip()[:60]
        lines.append(f"  - {col.name} ({col.data_type}){flag_text}  {cmt}")
    return head + "\n".join(lines)


def _extract_keywords(question: str) -> list[str]:
    """Naive split + Chinese-friendly token extraction for table/column matching."""
    cleaned = re.sub(r"[\s,，。？?！!；;()（）]+", " ", question)
    tokens = [t for t in cleaned.split(" ") if t]
    return tokens or [question]


async def _emit(kind: str, label: str, **extra: Any) -> dict[str, Any]:
    payload = {"kind": kind, "label": label, **extra}
    await adispatch_custom_event("reasoning_step", payload)
    return payload


def _strip_code_fence(s: str) -> str:
    s = s.strip()
    if s.startswith("```"):
        # remove first fence line
        s = re.sub(r"^```[a-zA-Z]*\n", "", s)
        if s.endswith("```"):
            s = s[:-3]
    return s.strip()


# --------------------------------------------------------------------------- #
# LLM prompts
# --------------------------------------------------------------------------- #


def _system_prompt_step1_schema(today_iso: str) -> str:
    return (
        "你是工业实验室质量数据 Chat2SQL 助手。"
        f"今天是 {today_iso}。\n"
        "任务：根据用户问题，从给定的表清单里挑出最多 3 张相关的表，"
        "每张表给一句中文理由。\n"
        "输出严格 JSON：{\"tables\": [{\"name\":\"lab_xxx\",\"reason\":\"...\"}]}\n\n"
        "**业务事实关键提示**：\n"
        "- 缺陷标签（'叠片不合'、'性能不合'、'极差不合'、'毛边'、'硬边'、'网眼' 等）是 "
        "  `lab_intermediate_data.F_FIRST_INSPECTION` 列的字符串取值，不是独立的表。"
        "  问'本月哪些炉号是叠片不合' 应直接 SELECT FROM lab_intermediate_data WHERE F_FIRST_INSPECTION='叠片不合'。\n"
        "- 等级标签（A/B/C）以及人为打的等级 → `lab_intermediate_data.F_LABELING`。\n"
        "- 检测时间 → `lab_intermediate_data.F_DETECTION_DATE`。\n"
        "- 班次（甲/乙/丙）→ `lab_intermediate_data.F_SHIFT`。\n"
        "- 炉号/批次 → `F_FURNACE_NO` / `F_FURNACE_BATCH_NO`。\n"
        "- 卷重 → 优先 `lab_intermediate_data.F_SINGLE_COIL_WEIGHT`，缺失回落 `lab_raw_data.F_SINGLE_COIL_WEIGHT`（通过 F_RAW_DATA_ID JOIN）。\n"
        "- 产品规格代码 → `lab_product_spec.F_CODE`（通过 lab_intermediate_data.F_PRODUCT_SPEC_ID JOIN）。\n\n"
        "不要选业务无关的表（比如 import_log/import_session/excel_import_template 类表）；"
        "外观特性相关表（lab_appearance_feature*）只用在'外观/划痕/麻点'类外观问题上，"
        "不要用在缺陷分类聚合上。\n"
        "若问题与质量数据无关（如打招呼、问日期），返回 {\"tables\": []}。"
    )


def _system_prompt_step2_columns() -> str:
    return (
        "已选定 1-3 张表。请从给定列清单里挑出回答问题需要用到的列（SELECT/WHERE/GROUP BY/JOIN）。"
        "返回严格 JSON：\n"
        "{\"select_columns\": [\"table.col\"], \"where_hints\": [\"自然语言条件描述\"], "
        "\"group_by\": [\"table.col\"], \"order_by\": \"col DESC\", \"limit\": 100, "
        "\"reasoning\": \"一句中文说明为什么选这些列\"}\n"
        "限制：列名必须出现在给定的列清单里，不要凭空发明；JOIN 时优先用 FK→ 标记的列。"
    )


def _system_prompt_step3_sql(today_iso: str) -> str:
    return (
        f"今天是 {today_iso}。"
        "根据所选表/列写一个 MySQL **SELECT** 语句。\n"
        "硬规则：\n"
        "- 只允许 SELECT。禁止 INSERT/UPDATE/DELETE/DROP/CREATE/ALTER/UNION。\n"
        "- 最多 JOIN 3 张表。\n"
        "- 时间相对词必须用 MySQL 内置函数：CURDATE()、NOW()、YEARWEEK(...,1)、DATE_FORMAT(...,'%Y-%m-01')。\n"
        "- 用参数占位符 :name 形式（不直接拼接用户输入），但本次允许把字面量写在 SQL 里（用户输入里的炉号等已落盘，先简化）。\n"
        "- 必须带 LIMIT，最多 100 行。\n"
        "- 删除控制字符建议用 REPLACE(REPLACE(col, CHAR(13), ''), CHAR(10), '')。\n"
        "返回严格 JSON：{\"sql\": \"...\", \"reasoning\": \"一句中文说明 SQL 思路\"}\n"
        "不要返回 markdown 代码块。"
    )


def _system_prompt_step6_summary(today_iso: str) -> str:
    return (
        f"今天是 {today_iso}。"
        "你拿到 SQL 查询结果，请用 2-4 句中文向班组长汇报：先给核心数字，再说明上下文。\n"
        "若结果为空：说明可能原因（无数据 / 时间范围内无记录 / 列值都是 NULL）。\n"
        "禁止输出 JSON 或代码块。"
    )


# --------------------------------------------------------------------------- #
# LangGraph node
# --------------------------------------------------------------------------- #


async def chat2sql_agent_node(state: dict[str, Any]) -> dict[str, Any]:
    """Run the 6-step Chat2SQL chain, streaming reasoning along the way."""
    cache = get_schema_cache()
    if not cache.loaded:
        try:
            cache = await refresh_schema_cache()
        except Exception as exc:  # noqa: BLE001
            await _emit("fallback", f"Schema 加载失败：{exc}")
            return {
                "response": "无法加载数据库 schema，请联系管理员。",
                "intent": "chat2sql",
                "reasoning_steps": [{"kind": "fallback", "label": f"Schema 加载失败：{exc}"}],
                "entities": state.get("entities", {}),
                "context": state.get("context", {}),
            }

    messages = state.get("messages", [])
    user_question = _last_human_text(messages)
    model_name = state.get("model_name") or None
    today = _today_iso()

    accumulated: list[dict[str, Any]] = []

    # ---------- Step 1: schema_pick ---------- #
    inventory = _build_table_inventory_text(cache)
    llm = get_llm(model_name)
    step1_resp = await llm.ainvoke(
        [
            {"role": "system", "content": _system_prompt_step1_schema(today)},
            {"role": "user", "content": f"用户问题：{user_question}\n\n可用表清单：\n{inventory}"},
        ]
    )
    step1_json = _safe_load_json(step1_resp.content)
    picked_tables: list[dict[str, str]] = step1_json.get("tables", [])[:MAX_JOIN_TABLES] if step1_json else []
    if not picked_tables:
        step = await _emit(
            "fallback",
            "Chat2SQL：未识别出与问题相关的表。建议把指标名/时间范围/规格说得更具体。",
        )
        accumulated.append(step)
        return {
            "response": "未在 lab_* 表里识别出与问题相关的表，请把要查的指标、时间范围或规格说得更具体。",
            "intent": "chat2sql",
            "reasoning_steps": accumulated,
            "entities": state.get("entities", {}),
            "context": state.get("context", {}),
        }

    schema_step = await _emit(
        "schema_pick",
        f"选定 {len(picked_tables)} 张表："
        + "、".join(t.get("name", "") for t in picked_tables),
        meta={"tables": picked_tables},
    )
    accumulated.append(schema_step)

    # ---------- Step 2: column_pick ---------- #
    keywords = _extract_keywords(user_question)
    table_blocks = []
    for entry in picked_tables:
        tname = entry.get("name", "")
        tbl = cache.tables.get(tname)
        if not tbl:
            continue
        cols = cache.relevant_columns(tname, keywords, max_cols=25)
        if not cols:
            cols = tbl.columns[:15]
        table_blocks.append(_build_column_block(tbl, cols))
    schema_doc = "\n\n".join(table_blocks)

    step2_resp = await llm.ainvoke(
        [
            {"role": "system", "content": _system_prompt_step2_columns()},
            {"role": "user", "content": f"用户问题：{user_question}\n\n候选列：\n{schema_doc}"},
        ]
    )
    step2_json = _safe_load_json(step2_resp.content) or {}
    selected_cols = step2_json.get("select_columns", [])
    column_step = await _emit(
        "column_pick",
        f"选定 {len(selected_cols)} 个字段：" + "、".join(selected_cols)[:120],
        meta={"plan": step2_json},
    )
    accumulated.append(column_step)

    # ---------- Step 3: sql_draft ---------- #
    step3_resp = await llm.ainvoke(
        [
            {"role": "system", "content": _system_prompt_step3_sql(today)},
            {
                "role": "user",
                "content": (
                    f"用户问题：{user_question}\n\n"
                    f"已选表/列：\n{schema_doc}\n\n"
                    f"列选取计划：\n{json.dumps(step2_json, ensure_ascii=False)}"
                ),
            },
        ]
    )
    step3_json = _safe_load_json(step3_resp.content) or {}
    draft_sql = (step3_json.get("sql") or "").strip().rstrip(";")
    sql_step = await _emit(
        "sql_draft",
        f"生成 SQL：{draft_sql[:140]}{'…' if len(draft_sql) > 140 else ''}",
        meta={"sql": draft_sql, "reasoning": step3_json.get("reasoning")},
    )
    accumulated.append(sql_step)

    # ---------- Step 4: sql_validate (with one retry) ---------- #
    final_sql = draft_sql
    last_error = None
    for attempt in range(2):
        try:
            validate_sql(final_sql)
            break
        except Exception as exc:  # noqa: BLE001
            last_error = str(exc)
            if attempt == 0:
                # 让 LLM 修正一次
                fix_resp = await llm.ainvoke(
                    [
                        {"role": "system", "content": _system_prompt_step3_sql(today)},
                        {
                            "role": "user",
                            "content": (
                                f"上次生成的 SQL 校验失败：{exc}\n"
                                f"原 SQL：{final_sql}\n"
                                "请修正后重新返回严格 JSON：{\"sql\":\"...\",\"reasoning\":\"...\"}"
                            ),
                        },
                    ]
                )
                fix_json = _safe_load_json(fix_resp.content) or {}
                final_sql = (fix_json.get("sql") or final_sql).strip().rstrip(";")
                continue
            # 第二次仍失败 → 放弃
            fail_step = await _emit(
                "fallback",
                f"SQL 校验两次仍失败：{exc}",
                meta={"sql": final_sql, "error": str(exc)},
            )
            accumulated.append(fail_step)
            return {
                "response": f"无法生成可执行的 SQL：{exc}",
                "intent": "chat2sql",
                "reasoning_steps": accumulated,
                "entities": state.get("entities", {}),
                "context": state.get("context", {}),
            }

    validate_step = await _emit(
        "sql_validate",
        "SQL 通过白名单与语法校验。" + ("（首次重试已修正）" if last_error else ""),
        meta={"final_sql": final_sql},
    )
    accumulated.append(validate_step)

    # ---------- Step 5: execute_sql (with one auto-fix retry) ---------- #
    rows: list[dict[str, Any]] | None = None
    exec_error: str | None = None
    for attempt in range(2):
        try:
            rows = await execute_safe_sql(final_sql, {})
            exec_error = None
            break
        except Exception as exc:  # noqa: BLE001
            exec_error = str(exc).split("\n")[0][:300]
            if attempt == 0:
                # 把执行报错喂回 LLM，让它修正 SQL（常见：列不存在/JOIN 字段类型不兼容）
                fix_resp = await llm.ainvoke(
                    [
                        {"role": "system", "content": _system_prompt_step3_sql(today)},
                        {
                            "role": "user",
                            "content": (
                                f"上次 SQL 执行失败：{exec_error}\n"
                                f"原 SQL：{final_sql}\n\n"
                                f"已选表/列：\n{schema_doc}\n\n"
                                "请基于上面的真实列名修正 SQL，"
                                "返回严格 JSON：{\"sql\":\"...\",\"reasoning\":\"修正了什么\"}"
                            ),
                        },
                    ]
                )
                fix_json = _safe_load_json(fix_resp.content) or {}
                fixed = (fix_json.get("sql") or "").strip().rstrip(";")
                if fixed:
                    final_sql = fixed
                    retry_step = await _emit(
                        "sql_validate",
                        f"执行报错→LLM 修正 SQL：{fix_json.get('reasoning', '')[:80]}",
                        meta={"error": exec_error, "fixed_sql": final_sql},
                    )
                    accumulated.append(retry_step)
                    # 二次验证 + 二次执行
                    try:
                        validate_sql(final_sql)
                    except Exception as v2:  # noqa: BLE001
                        exec_error = f"修正后白名单仍失败：{v2}"
                        break
                    continue
            # 第二次仍失败 → 放弃
            break

    if rows is None:
        exec_fail = await _emit(
            "fallback", f"SQL 执行失败（已尝试自动修正）：{exec_error}", meta={"sql": final_sql}
        )
        accumulated.append(exec_fail)
        return {
            "response": (
                f"很抱歉，无法生成正确的 SQL 来回答这个问题。\n\n"
                f"最后一次错误：{exec_error}\n\n"
                f"建议：换种说法（明确指标名/列名/表名），或拆成更小的问题。"
            ),
            "intent": "chat2sql",
            "reasoning_steps": accumulated,
            "entities": state.get("entities", {}),
            "context": state.get("context", {}),
        }

    truncated = rows[:MAX_RESULT_ROWS]
    exec_step = await _emit(
        "execute_sql",
        f"SQL 执行成功，返回 {len(rows)} 行（展示前 {len(truncated)} 行给 LLM 总结）",
        meta={"row_count": len(rows)},
    )
    accumulated.append(exec_step)

    # ---------- Step 6: result_summary ---------- #
    rows_for_llm = json.dumps(truncated, ensure_ascii=False, default=str)[:6000]
    summary_resp = await llm.ainvoke(
        [
            {"role": "system", "content": _system_prompt_step6_summary(today)},
            {
                "role": "user",
                "content": (
                    f"用户问题：{user_question}\n\n"
                    f"已执行 SQL：{final_sql}\n\n"
                    f"返回行数：{len(rows)}（预览前 {len(truncated)} 行）\n"
                    f"行内容：{rows_for_llm}"
                ),
            },
        ]
    )
    narrative = str(summary_resp.content).strip()

    summary_step = await _emit("result_summary", narrative[:120], meta={"narrative": narrative})
    accumulated.append(summary_step)

    chart_config = _maybe_build_chart_config(truncated, step2_json, user_question)

    response_md = (
        f"{narrative}\n\n"
        f"---\n\n"
        f"### 执行 SQL\n\n```sql\n{final_sql}\n```\n\n"
        f"### 结果（{len(rows)} 行，展示前 {min(10, len(rows))}）\n\n"
        + _rows_to_markdown(truncated[:10])
    )

    return {
        "response": response_md,
        "intent": "chat2sql",
        "reasoning_steps": accumulated,
        "chart_config": chart_config,
        "entities": state.get("entities", {}),
        "context": state.get("context", {}),
    }


# --------------------------------------------------------------------------- #
# Chart auto-build
# --------------------------------------------------------------------------- #


def _maybe_build_chart_config(
    rows: list[dict[str, Any]],
    plan: dict[str, Any],
    user_question: str,
) -> dict[str, Any] | None:
    """Heuristically build ChartDescriptor when result shape supports it.

    规则：
    - ≥2 行，2-3 列
    - 1 列是字符串/日期（category/x-axis），1 列是数值 → bar 或 line
    - 列数大于 3 或全是字符串 → 不画图（让 markdown 表格展示）
    """
    if not rows or len(rows) < 2:
        return None
    cols = list(rows[0].keys())
    if not cols or len(cols) > 3:
        return None

    from decimal import Decimal as _Decimal
    from datetime import date as _Date, datetime as _Datetime

    # 找数值列 + 类别列
    numeric_cols: list[str] = []
    category_cols: list[str] = []
    date_cols: list[str] = []
    for c in cols:
        sample = next((r[c] for r in rows if r.get(c) is not None), None)
        if sample is None:
            continue
        if isinstance(sample, (int, float, _Decimal)) and not isinstance(sample, bool):
            numeric_cols.append(c)
        elif isinstance(sample, (_Date, _Datetime)):
            date_cols.append(c)
        else:
            sample_str = str(sample)
            if (
                len(sample_str) >= 8
                and sample_str[:4].isdigit()
                and ("-" in sample_str or "/" in sample_str)
            ):
                date_cols.append(c)
            else:
                category_cols.append(c)

    if not numeric_cols:
        return None

    # 选 x：日期 > 类别
    x_field = (date_cols + category_cols)[0] if (date_cols or category_cols) else None
    if not x_field:
        return None
    y_field = numeric_cols[0]

    chart_type = "line" if x_field in date_cols else "bar"

    title_hint = plan.get("reasoning") if isinstance(plan, dict) else None
    title = (title_hint or user_question or "数据可视化")[:60]

    chart_data = []
    for r in rows:
        x_val = r.get(x_field)
        y_val = r.get(y_field)
        if x_val is None or y_val is None:
            continue
        try:
            y_num = float(y_val)
        except (TypeError, ValueError):
            y_num = 0.0
        chart_data.append(
            {
                "date": str(x_val),
                "value": y_num,
                "category": str(x_val),
            }
        )
    if not chart_data:
        return None

    return {
        "type": chart_type,
        "title": title,
        "data": chart_data,
        "xField": "date" if chart_type == "line" else "category",
        "yField": "value",
        "meta": {
            "metricName": y_field,
            "unit": "",
            "aggregation": "raw",
        },
    }


# --------------------------------------------------------------------------- #
# Helpers (cont.)
# --------------------------------------------------------------------------- #


def _last_human_text(messages: list[Any]) -> str:
    for msg in reversed(messages or []):
        if isinstance(msg, HumanMessage):
            return str(msg.content)
        if isinstance(msg, dict) and (msg.get("type") == "human" or msg.get("role") == "user"):
            return str(msg.get("content", ""))
    return ""


def _safe_load_json(content: Any) -> dict | None:
    text = _strip_code_fence(str(content))
    try:
        return json.loads(text)
    except json.JSONDecodeError:
        # 容错：截取首个 { ... } 段
        match = re.search(r"\{[\s\S]*\}", text)
        if match:
            try:
                return json.loads(match.group(0))
            except json.JSONDecodeError:
                return None
        return None


def _rows_to_markdown(rows: list[dict[str, Any]]) -> str:
    if not rows:
        return "_无数据_"
    cols = list(rows[0].keys())
    header = "| " + " | ".join(cols) + " |"
    sep = "| " + " | ".join("---" for _ in cols) + " |"
    body = []
    for row in rows:
        body.append(
            "| " + " | ".join(str(row.get(c, "")) if row.get(c) is not None else "" for c in cols) + " |"
        )
    return "\n".join([header, sep, *body])
