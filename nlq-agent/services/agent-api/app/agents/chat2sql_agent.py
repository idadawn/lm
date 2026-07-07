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
    SchemaCache,
    TableInfo,
    get_schema_cache,
    refresh_schema_cache,
)
from app.knowledge_graph.terms import expand_keywords
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


def _extract_keywords(question: str, cache: SchemaCache | None = None) -> list[str]:
    """朴素切分 + 业务词表 + glossary 扫描，供表/列匹配。

    中文问题往往整句无空格，朴素切分只得到一个长 token，列匹配几乎必失败；
    这里补两路召回：
    - terms.expand_keywords：aliases.json 同义词 → 规范名 + F_ 列名（「铁损」→ PsLoss/F_PS_LOSS）
    - cache.glossary：DB 公式表的中文术语 → 实际列名（「一次交检合格率」→ F_FIRST_INSPECTION）
    """
    cleaned = re.sub(r"[\s,，。？?！!；;()（）]+", " ", question)
    tokens = [t for t in cleaned.split(" ") if t]

    vocab = expand_keywords(question)

    glossary_terms: list[str] = []
    if cache is not None:
        for term, targets in cache.glossary.items():
            if term and term in question:
                glossary_terms.append(term)
                glossary_terms.extend(col for _tbl, col in targets)

    merged = [*tokens, *vocab, *glossary_terms]
    seen: set[str] = set()
    deduped = [k for k in merged if not (k in seen or seen.add(k))]
    return deduped or [question]


async def _emit(kind: str, label: str, **extra: Any) -> dict[str, Any]:
    payload = {"kind": kind, "label": label, **extra}
    await adispatch_custom_event("reasoning_step", payload)
    return payload


async def _emit_running(step_id: str, kind: str, label: str, **extra: Any) -> None:
    """推送占位"进行中"事件——同 step_id 的后续 _emit 会在前端原地替换。

    用法：在每个 LLM 调用前先调用一次，让前端立刻看到"正在 XXX"，
    LLM 返回后调用 _emit(kind=..., id=step_id, status="success", detail=...) 替换它。
    """
    payload = {"id": step_id, "kind": kind, "label": label, "status": "running", **extra}
    await adispatch_custom_event("reasoning_step", payload)


def _normalize_db_column(formula_col_name: str | None) -> str:
    """逻辑列名（FirstInspection / Labeling）→ 真实数据库列名（F_FIRST_INSPECTION）。

    lab_intermediate_data_formula.F_COLUMN_NAME 存的是逻辑驼峰名，需要转换。
    """
    if not formula_col_name:
        return ""
    s = str(formula_col_name).strip()
    if s.upper().startswith("F_"):
        return s.upper()
    # PascalCase → SNAKE_CASE，再加 F_ 前缀
    s = re.sub(r"([A-Z])", r"_\1", s).lstrip("_").upper()
    return f"F_{s}"


async def _load_metric_field_map() -> dict[str, dict[str, Any]]:
    """从 lab_report_config JOIN lab_intermediate_data_formula 加载所有指标的定义。

    每条 → {"name": "一次交检合格率", "column": "F_FIRST_INSPECTION", "levels": ["A"], "is_pct": True}

    LLM 通过这张表知道 A / B / 合格率 / 不合格 / 一次交检合格率 对应哪个判定列、
    哪几个合格等级。**未来在 lab/reportConfig 改了配置，这里自动跟进，不用改代码**。
    """
    sql = """
        SELECT
            r.F_NAME as name,
            r.F_LEVEL_NAMES as level_names,
            r.F_IS_PERCENTAGE as is_pct,
            f.F_COLUMN_NAME as col_logical
        FROM lab_report_config r
        LEFT JOIN lab_intermediate_data_formula f
            ON r.F_FORMULA_ID COLLATE utf8mb4_unicode_ci = f.F_Id COLLATE utf8mb4_unicode_ci
        WHERE (r.F_DeleteMark IS NULL OR r.F_DeleteMark = 0)
          AND r.F_ENABLEDMARK = 1
        ORDER BY r.F_SORT_ORDER ASC
    """
    rows = await execute_safe_sql(sql, {})
    out: dict[str, dict[str, Any]] = {}
    for row in rows:
        name = (row.get("name") or "").strip()
        if not name:
            continue
        # 解析 level_names JSON
        raw = row.get("level_names") or ""
        levels: list[str] = []
        if raw:
            try:
                parsed = json.loads(raw)
                if isinstance(parsed, list):
                    levels = [str(g) for g in parsed]
            except Exception:
                levels = [g.strip() for g in str(raw).split(",") if g.strip()]
        out[name] = {
            "column": _normalize_db_column(row.get("col_logical")),
            "levels": levels,
            "is_pct": bool(row.get("is_pct")),
        }
    return out


async def _load_unqualified_types() -> list[str]:
    """从 lab_intermediate_data 取近期出现过的不合格分类字符串（F_FIRST_INSPECTION 的字面值）。

    返回："性能不合"、"极差不合"、"叠片不合"、"毛边" 等真实出现过的标签，
    用来填到 LLM 提示里，避免它自创"F_THICK_ABNORMAL"之类的奇怪字段。
    """
    sql = """
        SELECT F_FIRST_INSPECTION AS v, COUNT(*) AS c
        FROM lab_intermediate_data
        WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)
          AND F_FIRST_INSPECTION IS NOT NULL
          AND TRIM(F_FIRST_INSPECTION) <> ''
          AND F_FIRST_INSPECTION NOT IN ('A', 'B')
          AND F_PROD_DATE >= DATE_SUB(CURDATE(), INTERVAL 6 MONTH)
        GROUP BY F_FIRST_INSPECTION
        ORDER BY c DESC
        LIMIT 30
    """
    try:
        rows = await execute_safe_sql(sql, {})
        return [str(r["v"]) for r in rows if r.get("v")]
    except Exception:
        return [
            "性能不合", "极差不合", "叠片不合", "带厚不合", "带宽不合",
            "毛边", "硬边", "网眼", "端面不良", "卷边", "脆边", "韧性不合",
        ]


def _format_metric_field_map_for_prompt(metric_map: dict[str, dict[str, Any]]) -> str:
    """把指标定义表渲染成提示词里的多行 markdown 块。"""
    if not metric_map:
        return "  （报表配置暂未加载，请按业务术语自行匹配）"
    lines: list[str] = []
    for name, info in metric_map.items():
        col = info.get("column") or "?"
        levels = info.get("levels") or []
        pct = "占比指标" if info.get("is_pct") else "汇总指标"
        if levels:
            level_list = ",".join(f"'{lv}'" for lv in levels)
            lines.append(
                f"  - 「{name}」→ 判定列 `{col}`，合格等级 ({level_list})，{pct}。"
                f"按重量计：`SUM(CASE WHEN {col} IN ({level_list}) THEN COALESCE(F_SINGLE_COIL_WEIGHT,0) ELSE 0 END)`"
            )
        else:
            lines.append(f"  - 「{name}」→ 判定列 `{col}`，未配置合格等级，{pct}。")
    return "\n".join(lines)


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
        "- 【时间过滤一律用生产日期 `lab_intermediate_data.F_PROD_DATE`】，绝不能用 F_DETECTION_DATE。"
        "  这是与 /lab/monthlyReport、/lab/monthly-dashboard 月度报表保持一致的统计口径。\n"
        "- 班次（甲/乙/丙）→ `lab_intermediate_data.F_SHIFT`。\n"
        "- 炉号：**两列分工** —— "
        "`F_FURNACE_NO_FORMATTED`（物料唯一编码，剥离末尾 W/G/K 和汉字）用于匹配/JOIN/去重；"
        "`F_FURNACE_NO`（业务原始炉号，含末尾业务标签如 W/塌卷/毛边）用于展示给用户。"
        "**用户输入按 FORMATTED 匹配，结果展示给用户用 F_FURNACE_NO**。\n"
        "- 批次 → `F_FURNACE_BATCH_NO`。\n"
        "- 卷重 → 只使用 `lab_intermediate_data.F_SINGLE_COIL_WEIGHT`。\n"
        "- 产品规格代码 → `lab_product_spec.F_CODE`（通过 lab_intermediate_data.F_PRODUCT_SPEC_ID JOIN）。\n"
        "- 【任何聚合/统计都必须 WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0) 排除软删除记录】。\n"
        "- 【分布/占比/汇总/对比】类问题首选 SUM(F_SINGLE_COIL_WEIGHT) 而不是 COUNT(*)——业务系统口径都是按重量。\n\n"
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
        "\"reasoning\": \"...\"}\n"
        "限制：列名必须出现在给定的列清单里，不要凭空发明；JOIN 时优先用 FK→ 标记的列。\n"
        "**reasoning 必须是面向业务人员的中文话术**：只描述业务动作，不能出现表名/F_xxx 列名/SQL 关键字。"
        "  例（OK）：「按班次分组统计上月合格卷重，并按重量倒序」；"
        "  禁止例：「SELECT F_SHIFT, SUM(F_SINGLE_COIL_WEIGHT) GROUP BY F_SHIFT」。"
    )


def _system_prompt_step3_sql(
    today_iso: str,
    metric_map_md: str = "",
    unqualified_types: list[str] | None = None,
) -> str:
    types_line = (
        "、".join(f"'{t}'" for t in unqualified_types) if unqualified_types
        else "'性能不合','极差不合','叠片不合','带厚不合','带宽不合','毛边','硬边','网眼','端面不良','卷边','脆边'"
    )
    return (
        f"今天是 {today_iso}。"
        "根据所选表/列写一个 MySQL **SELECT** 语句。\n\n"
        "【报表配置指标 — 实时从 lab_report_config 加载，是判定列 / 合格等级的唯一真理来源】\n"
        f"{metric_map_md}\n\n"
        "**重要**：以上『判定列』和『合格等级』可能随业务方在 /lab/reportConfig 页面调整而改变。"
        "永远以这张表给出的 column / levels 为准，不要 hardcode F_LABELING / F_FIRST_INSPECTION 来匹配 A、B、合格率 等命名指标。\n\n"
        "【业务术语 → 数据库字段映射 — 通用部分】\n"
        "  - 班组 / 班次 / 三班 / 甲乙丙 → `lab_intermediate_data.F_SHIFT`（取值：甲、乙、丙）\n"
        "  - 生产量 / 检测量 / 总产量 / 卷重 → `SUM(COALESCE(F_SINGLE_COIL_WEIGHT, 0))`（单位 kg）\n"
        "  - 不合格分类（『极差不合 / 叠片不合 / 带厚不合 / 带宽不合 / 韧性不合 / 性能不合 / 毛边 / 硬边 / 网眼 / 端面不良 / 卷边 / 脆边 』等）"
        "    都是 `F_FIRST_INSPECTION` 这一列的【字符串字面值】，不是独立字段也不是 F_LABELING。"
        f"  实测出现过的取值：{types_line}。\n"
        "    例：『极差不合』→ `SUM(CASE WHEN F_FIRST_INSPECTION='极差不合' THEN COALESCE(F_SINGLE_COIL_WEIGHT,0) ELSE 0 END) AS \"极差不合(kg)\"`。\n"
        "  - 一次交检 / 一次交检结果 → `F_FIRST_INSPECTION`\n"
        "  - **质量分布 / 质量等级分布 / 等级分布 / 各等级占比 / 贴标分布 → 分组列必须是 `F_LABELING`**（贴标列，反映最终等级），\n"
        "    取值如 'A','B','性能不合','极差不合','未标注' 等。NULL/空值统一显示为 '未标注'。\n"
        "    例：`SELECT F_SHIFT AS \"班组\", COALESCE(NULLIF(TRIM(F_LABELING),''), '未标注') AS \"等级\", "
        "SUM(...) AS \"卷重(kg)\" FROM lab_intermediate_data GROUP BY F_SHIFT, F_LABELING`。\n"
        "  - **不要把『质量分布』误映射到 F_FIRST_INSPECTION**，那是「一次交检结果分布」，含义不同。\n"
        "  - 产品规格 / 规格 → `F_PRODUCT_SPEC_CODE` 或 `lab_product_spec.F_CODE`\n"
        "  - 炉号匹配/JOIN/去重 → `F_FURNACE_NO_FORMATTED`（物料唯一编码，剥离末尾 W/G/K 和汉字描述）；"
        "用 `=` 精确匹配，或 `LIKE` 模糊匹配；切勿把「班组(甲/乙/丙)」误当炉号。\n"
        "  - 炉号展示给业务人员 → 同时 SELECT `F_FURNACE_NO`（业务原始炉号，含末尾业务标签 W/塌卷/毛边 等）"
        "作为「炉号」列，让业务看到完整工艺标签。**F_FURNACE_NO_FORMATTED 只参与 WHERE/JOIN，不要 SELECT 给用户**。\n"
        "  - 产线 → `F_LINE_NO`\n"
        "  - 生产日期（统计时间口径）→ `F_PROD_DATE`\n\n"
        "【硬规则】\n"
        "- 只允许 SELECT。禁止 INSERT/UPDATE/DELETE/DROP/CREATE/ALTER/UNION。\n"
        "- 最多 JOIN 3 张表。\n"
        "- 时间相对词必须用 MySQL 内置函数：CURDATE()、NOW()、YEARWEEK(...,1)、DATE_FORMAT(...,'%Y-%m-01')。\n"
        "- **时间字段一律用 `F_PROD_DATE`（生产日期），绝不用 F_DETECTION_DATE**。\n"
        "- **所有 lab_* 表的查询必须带 `(F_DeleteMark IS NULL OR F_DeleteMark = 0)` 过滤软删除**。\n"
        "- **任何'统计/分布/汇总/对比/占比/明细'类问题，重量字段一律是 `F_SINGLE_COIL_WEIGHT`，不要用 COUNT(*)**。"
        "  如果用户明确要「件数 / 条数 / 记录数」才用 COUNT。\n"
        "- 跨表 JOIN 字符串 ID 时（lab_intermediate_data_judgment_level / lab_product_spec_attribute / lab_product_spec_version 与其他表），"
        "  两侧都加 `COLLATE utf8mb4_unicode_ci`。\n"
        "- 用参数占位符 :name 形式（不直接拼接用户输入），但本次允许把字面量写在 SQL 里（用户输入里的炉号等已落盘，先简化）。\n"
        "- 必须带 LIMIT，最多 100 行。\n"
        "- 列别名一律用**中文**（用 AS \"中文名\" 形式），表头要让业务人员看得懂。\n"
        "- 删除控制字符建议用 REPLACE(REPLACE(col, CHAR(13), ''), CHAR(10), '')。\n\n"
        "【完整示例 — 班组多列对比报表】\n"
        "用户问：『按班组查看上个月检测明细 生产量 A 类 A 类占比 B 类 B 类占比 极差不合 叠片不合 带厚不合 带宽不合』\n"
        "做法：\n"
        "  1. 命名指标（A / B / 一次交检合格率 / 合格率 / 不合格 等）必须查询上面"
        "「报表配置指标」清单，按 column / levels 字段拼 CASE WHEN。\n"
        "  2. 不合格分类（极差不合 / 叠片不合 / 带厚不合 / ...）按 F_FIRST_INSPECTION 字符串字面值过滤。\n"
        "  3. 占比 = SUM(命中卷重) / NULLIF(SUM(总卷重), 0) * 100，ROUND 2 位。\n"
        "理想 SQL 结构（假设报表配置中『A』指标的 column=F_FIRST_INSPECTION, levels=['A']）：\n"
        "  SELECT\n"
        "    F_SHIFT AS \"班组\",\n"
        "    SUM(COALESCE(F_SINGLE_COIL_WEIGHT,0)) AS \"生产量(kg)\",\n"
        "    SUM(CASE WHEN <A 类判定列> IN (<A 类合格等级>) THEN COALESCE(F_SINGLE_COIL_WEIGHT,0) ELSE 0 END) AS \"A 类(kg)\",\n"
        "    ROUND(SUM(CASE WHEN <A 类判定列> IN (<A 类合格等级>) THEN COALESCE(F_SINGLE_COIL_WEIGHT,0) ELSE 0 END)\n"
        "          / NULLIF(SUM(COALESCE(F_SINGLE_COIL_WEIGHT,0)),0) * 100, 2) AS \"A 类占比(%)\",\n"
        "    /* 同理 B 类 */\n"
        "    SUM(CASE WHEN F_FIRST_INSPECTION='极差不合' THEN COALESCE(F_SINGLE_COIL_WEIGHT,0) ELSE 0 END) AS \"极差不合(kg)\",\n"
        "    SUM(CASE WHEN F_FIRST_INSPECTION='叠片不合' THEN COALESCE(F_SINGLE_COIL_WEIGHT,0) ELSE 0 END) AS \"叠片不合(kg)\",\n"
        "    SUM(CASE WHEN F_FIRST_INSPECTION='带厚不合' THEN COALESCE(F_SINGLE_COIL_WEIGHT,0) ELSE 0 END) AS \"带厚不合(kg)\",\n"
        "    SUM(CASE WHEN F_FIRST_INSPECTION='带宽不合' THEN COALESCE(F_SINGLE_COIL_WEIGHT,0) ELSE 0 END) AS \"带宽不合(kg)\"\n"
        "  FROM lab_intermediate_data\n"
        "  WHERE (F_DeleteMark IS NULL OR F_DeleteMark = 0)\n"
        "    AND F_PROD_DATE >= DATE_FORMAT(DATE_SUB(CURDATE(), INTERVAL 1 MONTH), '%Y-%m-01')\n"
        "    AND F_PROD_DATE < DATE_FORMAT(CURDATE(), '%Y-%m-01')\n"
        "  GROUP BY F_SHIFT\n"
        "  ORDER BY F_SHIFT\n"
        "  LIMIT 100;\n"
        "把 `<A 类判定列>` 和 `<A 类合格等级>` 替换成「报表配置指标」清单里 A 当时配置的实际值。\n\n"
        "返回严格 JSON：{\"sql\": \"...\", \"reasoning\": \"...\"}\n"
        "**reasoning 必须是面向班组长/质检员（业务人员）的中文话术**：\n"
        "  - 只说做了什么业务动作，不说表名/列名/SQL 关键字\n"
        "  - 例子（OK）：「按班组（甲/乙/丙）汇总上个月每个班的生产量、A/B 类卷重和各项不合格分类」\n"
        "  - 反例（禁止）：「按 F_SHIFT 分组 SUM(F_SINGLE_COIL_WEIGHT) 且 F_DeleteMark=0」\n"
        "不要返回 markdown 代码块。"
    )


def _system_prompt_step23_combined(
    today_iso: str,
    metric_map_md: str = "",
    unqualified_types: list[str] | None = None,
) -> str:
    """合并 step2 (column_pick) + step3 (sql_draft) 的 system prompt。

    DeepSeek 单次调用比两次串行少一个 round-trip 的延迟（实测 column_pick 单独要 30-50s），
    把"选列计划 + 实际 SQL"都在一次 JSON 里返回。

    输出 JSON：
        {
            "select_columns": ["table.col", ...],
            "group_by": ["..."],
            "where_hints": ["..."],
            "order_by": "...",
            "limit": 100,
            "reasoning_columns": "中文话术：为什么选这些维度",
            "sql": "SELECT ...",
            "reasoning_sql": "中文话术：SQL 思路（业务动作，不能出现 SQL 关键字 / F_xxx）"
        }
    """
    # 复用 step3 的全部业务规则，只在末尾加入 column plan 要求
    step3_body = _system_prompt_step3_sql(today_iso, metric_map_md, unqualified_types)
    # 把 step3 原本要求的 ``{"sql":"...","reasoning":"..."}`` 输出契约替换为合并版
    step3_body = step3_body.replace(
        "返回严格 JSON：{\"sql\": \"...\", \"reasoning\": \"...\"}\n",
        "",
    ).replace(
        "**reasoning 必须是面向班组长/质检员（业务人员）的中文话术**：\n"
        "  - 只说做了什么业务动作，不说表名/列名/SQL 关键字\n"
        "  - 例子（OK）：「按班组（甲/乙/丙）汇总上个月每个班的生产量、A/B 类卷重和各项不合格分类」\n"
        "  - 反例（禁止）：「按 F_SHIFT 分组 SUM(F_SINGLE_COIL_WEIGHT) 且 F_DeleteMark=0」\n"
        "不要返回 markdown 代码块。",
        "",
    )
    return (
        step3_body
        + "\n\n"
        + "===  输出契约（合并选列 + SQL）  ===\n"
        + "请一次性返回严格 JSON（不要 markdown 代码块）：\n"
        + "{\n"
        + "  \"select_columns\": [\"table.col\", ...],   // 用到的列\n"
        + "  \"group_by\": [\"table.col\", ...],          // 分组列；没有就空数组\n"
        + "  \"where_hints\": [\"自然语言条件\"],          // 时间窗口、过滤条件中文描述\n"
        + "  \"order_by\": \"col DESC\",                   // 没有就空字符串\n"
        + "  \"limit\": 100,                              // 必须有，最多 100\n"
        + "  \"reasoning_columns\": \"...\",              // 业务话术：选了哪些维度\n"
        + "  \"sql\": \"SELECT ...\",                     // 完整可执行 MySQL SELECT\n"
        + "  \"reasoning_sql\": \"...\"                   // 业务话术：SQL 大致思路\n"
        + "}\n"
        + "两段 reasoning 都用业务人话（不要 SQL 关键字 / F_xxx 列名）。"
    )


def _system_prompt_step6_summary(today_iso: str) -> str:
    return (
        f"今天是 {today_iso}。"
        "你拿到 SQL 查询结果，请用 2-4 句中文向班组长汇报：先给核心数字，再说明上下文。\n"
        "若结果为空：说明可能原因（无数据 / 时间范围内无记录 / 列值都是 NULL）。\n"
        "禁止输出 JSON 或代码块。\n\n"
        "【❗严禁幻觉指标 — 这是硬性红线】\n"
        "1. 『一次交检合格率』『合格率』『不合格率』『A 类占比』『B 类占比』 等都是**报表配置里有严格口径的命名指标**，\n"
        "   只在用户的问题里明确出现且 SQL 真的按 lab_report_config 定义口径计算时才能用这些词。\n"
        "2. 如果 SQL 是按 F_LABELING 分组统计了各等级卷重（如 A / B / 性能不合 / 极差不合 …），\n"
        "   这是【质量等级分布】，**不是**一次交检合格率——绝对不要把『A 类卷重 / 总卷重』算出来叫它『一次交检合格率』！\n"
        "   一次交检合格率走的是 F_FIRST_INSPECTION 列（一次检验列），口径完全不同。\n"
        "3. 不要把『合格 / 不合』两字随意往结果上套。除非 SQL 明确用了 F_FIRST_INSPECTION IN ('A',...)，\n"
        "   否则只能描述『A 类卷重 X kg、B 类卷重 Y kg、其余等级 Z kg』这种事实，不能编造一个合格率百分比。\n"
        "4. 如果用户问的是『分布』『占比』『对比』——只描述具体等级的卷重和它在该组（班组/规格…）的占比，\n"
        "   不要算『合格率』，也不要给出统一的『合格率 = xx%』这种结论。\n\n"
        "【输出风格】\n"
        "- 直接用业务话术，不出现表名/字段名（F_xxx、lab_xxx）。\n"
        "- 引用具体数字时保留单位（kg / 吨 / 条），不要四舍五入到没意义的精度。\n"
        "- 2-4 句简短汇报，重点突出最大值/最小值/异常项，不要逐行复读。\n"
        "- **空值显示**：SQL 结果里 null / 空字符串 / NULL 一律说成『未标注』或『未填』，绝不要在答案里出现 `(null)`、`NULL`、`(NULL)`、`None` 这种调试痕迹。"
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
            await _emit("fallback", f"数据库连接异常：{str(exc)[:80]}")
            return {
                "response": "无法加载数据库 schema，请联系管理员。",
                "intent": "chat2sql",
                "reasoning_steps": [{"kind": "fallback", "label": f"Schema 加载失败：{exc}"}],
                "entities": state.get("entities", {}),
                "context": state.get("context", {}),
            }

    messages = state.get("messages", [])
    # 优先用意图分类阶段改写出的"自包含问题"（多轮追问承接上文）；缺失时回退到最新一条消息
    user_question = str(state.get("resolved_question") or "").strip() or _last_human_text(messages)
    model_name = state.get("model_name") or None
    today = _today_iso()

    accumulated: list[dict[str, Any]] = []

    # ---------- Step 1: schema_pick ---------- #
    inventory = _build_table_inventory_text(cache)
    llm = get_llm(model_name)
    await _emit_running("step-schema-pick", "schema_pick", "正在挑选相关数据表…")
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
            "没能理解你想查的指标，请把指标名 / 时间范围 / 规格说得更具体一点。",
        )
        accumulated.append(step)
        return {
            "response": "未在 lab_* 表里识别出与问题相关的表，请把要查的指标、时间范围或规格说得更具体。",
            "intent": "chat2sql",
            "reasoning_steps": accumulated,
            "entities": state.get("entities", {}),
            "context": state.get("context", {}),
        }

    # 业务人话标签 + 详情：把选中的表用业务说法列出来，方便业务人员核对
    _TABLE_ALIAS = {
        "lab_intermediate_data": "中间检测数据（单卷主表）",
        "lab_raw_data": "原始叠片数据",
        "lab_magnetic_raw_data": "原始磁性能数据",
        "lab_product_spec": "产品规格",
        "lab_product_spec_attribute": "产品规格扩展属性",
        "lab_intermediate_data_judgment_level": "判定规则",
        "lab_intermediate_data_formula": "指标公式",
        "lab_report_config": "报表配置（命名指标）",
    }
    tbl_names = [t.get("name", "") for t in picked_tables]
    tbl_zh = "、".join(_TABLE_ALIAS.get(n, n) for n in tbl_names) or "（未识别）"
    schema_step = await _emit(
        "schema_pick",
        f"选定 {len(picked_tables)} 张数据表",
        id="step-schema-pick",
        status="success",
        detail=f"涉及：{tbl_zh}",
        meta={"tables": picked_tables, "table_names": tbl_names},
    )
    accumulated.append(schema_step)

    # ---------- Step 2: column_pick ---------- #
    keywords = _extract_keywords(user_question, cache)
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

    # ★ 方案 B：合并 column_pick + sql_draft 为一次 LLM 调用（省掉 ~40s round-trip）
    # 先并行计算 LightRAG 上下文和指标定义，下面合并调用一次性把它们都喂进去。
    try:
        metric_map = await _load_metric_field_map()
    except Exception as exc:  # noqa: BLE001
        logger.warning("[chat2sql] load metric map failed: %s", exc)
        metric_map = {}
    try:
        unqualified_types = await _load_unqualified_types()
    except Exception:
        unqualified_types = []
    metric_map_md = _format_metric_field_map_for_prompt(metric_map)

    lightrag_ctx_md = ""
    lightrag_sources: list[str] = []
    try:
        from app.knowledge_graph.kb_lookup import fetch_lightrag_context
        chunks = await fetch_lightrag_context(user_question, top_k=5)
        if chunks:
            lightrag_ctx_md = "\n\n【相关业务知识（来自知识图谱，必须遵守）】\n"
            for ch in chunks:
                content = (ch.get("content") or "").strip()
                source = ch.get("source") or ""
                if not content:
                    continue
                snippet = content[:400] + ("..." if len(content) > 400 else "")
                lightrag_ctx_md += f"- {snippet}\n  来源：{source}\n"
                if source:
                    lightrag_sources.append(source)
            lightrag_ctx_md += "\n请基于上面的业务知识写 SQL；特别注意：F_LABELING / F_FIRST_INSPECTION 等字段口径以图谱为准。\n"
            kg_step = await _emit(
                "knowledge_lookup",
                f"已从知识图谱取到 {len(chunks)} 条相关业务知识",
                detail=("来源：" + "、".join(s for s in lightrag_sources[:3]) +
                        (f" …" if len(lightrag_sources) > 3 else "")),
                meta={"sources": lightrag_sources, "top_k": len(chunks)},
            )
            accumulated.append(kg_step)
    except Exception:
        logger.debug("[chat2sql] LightRAG context fetch failed; continuing", exc_info=True)

    combined_system_prompt = _system_prompt_step23_combined(today, metric_map_md, unqualified_types)
    # 同时给前端推「正在选维度」和「正在写 SQL」两个 running 占位，
    # LLM 返回后两条会被对应的 success 事件原地替换。
    await _emit_running("step-column-pick", "column_pick", "正在选择分析维度…")
    await _emit_running("step-sql-draft", "sql_draft", "正在编写查询脚本…")
    combined_resp = await llm.ainvoke(
        [
            {"role": "system", "content": combined_system_prompt},
            {
                "role": "user",
                "content": (
                    f"用户问题：{user_question}\n\n"
                    f"候选列：\n{schema_doc}"
                    f"{lightrag_ctx_md}"
                ),
            },
        ]
    )
    combined_json = _safe_load_json(combined_resp.content) or {}
    # 拆分为 step2_json / step3_json 保持下游逻辑（chart_config / smart_table）原样能用
    step2_json = {
        "select_columns": combined_json.get("select_columns", []),
        "group_by": combined_json.get("group_by", []),
        "where_hints": combined_json.get("where_hints", []),
        "order_by": combined_json.get("order_by", ""),
        "limit": combined_json.get("limit", 100),
        "reasoning": combined_json.get("reasoning_columns") or "",
    }
    step3_json = {
        "sql": combined_json.get("sql", ""),
        "reasoning": combined_json.get("reasoning_sql") or "",
    }
    selected_cols = step2_json.get("select_columns", [])
    # 业务化标签：用人类语言总结要分析的列，把 F_xxx 翻译成中文
    _COL_ALIAS = {
        "F_SHIFT": "班组", "F_SHIFT_NUMERIC": "班组(数字)",
        "F_PROD_DATE": "生产日期", "F_DETECTION_DATE": "检测日期",
        "F_LINE_NO": "产线", "F_PRODUCT_SPEC_CODE": "产品规格", "F_PRODUCT_SPEC_NAME": "规格名称",
        # 展示用 F_FURNACE_NO（业务原始炉号，含末尾业务标签如 W/塌卷/毛边等）
        # 唯一性判定用 F_FURNACE_NO_FORMATTED（物料编码，剥离末尾 W/G/K 和汉字）
        "F_FURNACE_NO": "炉号", "F_FURNACE_NO_FORMATTED": "炉号编码", "F_FURNACE_BATCH_NO": "批次号",
        "F_COIL_NO": "卷号", "F_SUBCOIL_NO": "分卷号",
        "F_SINGLE_COIL_WEIGHT": "单卷重(kg)", "F_COIL_WEIGHT": "卷重(kg)",
        "F_FIRST_INSPECTION": "一次检验结果", "F_LABELING": "贴标/最终等级",
        "F_MAGNETIC_RES": "磁性能判定", "F_THICK_RES": "厚度判定", "F_LAM_FACTOR_RES": "叠片系数判定",
        "F_PS_LOSS": "Ps铁损", "F_SS_POWER": "激磁功率", "F_HC": "矫顽力",
        "F_THICKNESS": "厚度", "F_WIDTH": "带宽", "F_LAM_FACTOR": "叠片系数", "F_DENSITY": "密度",
    }
    def _humanize(col_ref):
        if isinstance(col_ref, dict):
            name = col_ref.get("column") or col_ref.get("name") or ""
        else:
            name = str(col_ref or "")
        # 去掉 表名. 前缀
        if "." in name:
            name = name.split(".", 1)[1]
        return _COL_ALIAS.get(name.upper(), name)
    selected_zh = [_humanize(c) for c in selected_cols][:8]
    detail = ("分析维度：" + "、".join(selected_zh)) if selected_zh else "（未选出明确列）"
    column_step = await _emit(
        "column_pick",
        f"选定 {len(selected_cols)} 个分析维度",
        id="step-column-pick",
        status="success",
        detail=detail,
        meta={"plan": step2_json, "selected_columns_zh": selected_zh},
    )
    accumulated.append(column_step)

    # ---------- Step 3: sql_draft ---------- #
    # 注：column_pick + sql_draft 已在上面 ★合并 LLM 调用 ★ 中一次产出；
    # 此处只需把 combined_json 拆出来的 step3_json 接到既有的 detail/校验/重试流程。
    # step3_system_prompt 仍然按原 step3 prompt 构造，用于后续 SQL 校验失败 / 执行失败时的"二次修正"重试，
    # 那时不需要重新选列，所以单独保留较小的 step3-only prompt。
    step3_system_prompt = _system_prompt_step3_sql(today, metric_map_md, unqualified_types)
    draft_sql = (step3_json.get("sql") or "").strip().rstrip(";")
    # SQL 生成完毕：把 LLM 的中文 reasoning 作为 detail（业务人话），完整 SQL 留在 meta
    llm_reasoning = (step3_json.get("reasoning") or "").strip()
    if not llm_reasoning:
        # reasoning 缺失时，从 SQL 萃取出关键操作（GROUP BY / WHERE 时间范围）作为兜底
        bits = []
        m = re.search(r"GROUP\s+BY\s+([\w_,.()\s]+?)(?:ORDER|LIMIT|$)", draft_sql, re.IGNORECASE)
        if m:
            bits.append(f"按 {m.group(1).strip()} 分组")
        m = re.search(r"F_PROD_DATE\s*[<>=]+[\s\S]{0,80}?(?:CURDATE|DATE_FORMAT|INTERVAL)", draft_sql, re.IGNORECASE)
        if m:
            bits.append("含时间窗口过滤")
        llm_reasoning = "；".join(bits) or "已生成查询脚本"
    sql_preview = (draft_sql[:140] + " …") if len(draft_sql) > 140 else draft_sql
    sql_step = await _emit(
        "sql_draft",
        "已编写好查询脚本",
        id="step-sql-draft",
        status="success",
        detail=llm_reasoning,
        meta={
            "sql": draft_sql,
            "reasoning": step3_json.get("reasoning"),
            "sql_preview": sql_preview,
            "sql_length": len(draft_sql),
        },
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
                        {"role": "system", "content": step3_system_prompt},
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
                "查询脚本两次校验都没通过，请把问题说得更清楚一点重试。",
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

    # 校验通过：把安全检查的具体项作为 detail（业务人员可以看到我们都防了哪些攻击 / 错误）
    _checks = [
        "仅允许 SELECT 语句",
        "JOIN ≤ 3 张表",
        "包含 LIMIT 限流",
        "时间字段统一用 F_PROD_DATE",
        "软删除记录已过滤",
    ]
    validate_step = await _emit(
        "sql_validate",
        "查询脚本已通过安全校验" + ("（自动修正了 1 处问题）" if last_error else ""),
        id="step-sql-validate",
        status="success",
        detail=f"已检查：{ '、'.join(_checks) }",
        meta={
            "final_sql": final_sql,
            "auto_fixed": bool(last_error),
            "checks_passed": _checks,
        },
    )
    accumulated.append(validate_step)

    # ---------- Step 5: execute_sql (with one auto-fix retry) ---------- #
    await _emit_running("step-execute-sql", "execute_sql", "正在执行查询…")
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
                        {"role": "system", "content": step3_system_prompt},
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
                        "查询执行报错，已自动尝试修正",
                        meta={"error": exec_error, "fixed_sql": final_sql, "reasoning": fix_json.get("reasoning", "")},
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
            "fallback",
            "数据查询失败，已经尝试自动修正还是没成功；请换种说法再试一次。",
            meta={"sql": final_sql, "error": exec_error},
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
    # 把结果的"形状"信息也推到推理链 detail，业务人员能感知到数据有几列、覆盖多少行、第一行长啥样
    _shape_detail = ""
    if truncated:
        cols_list = list(truncated[0].keys())
        col_zh = "、".join(cols_list[:6]) + (" …" if len(cols_list) > 6 else "")
        # 抽样：第一行的简短表示
        first = truncated[0]
        sample_parts = []
        for c in cols_list[:3]:
            v = first.get(c)
            if v is None:
                sample_parts.append(f"{c}=未填")
            else:
                vs = str(v)
                sample_parts.append(f"{c}={vs[:20]}")
        _shape_detail = f"列：{col_zh}；示例：{ ' / '.join(sample_parts) }"
    exec_step = await _emit(
        "execute_sql",
        f"数据已拿到，共 {len(rows)} 条记录（取前 {len(truncated)} 行）",
        id="step-execute-sql",
        status="success",
        detail=_shape_detail or "（空结果集）",
        meta={
            "row_count": len(rows),
            "preview_count": len(truncated),
            "columns": list(truncated[0].keys()) if truncated else [],
        },
    )
    accumulated.append(exec_step)

    # ---------- Step 6: result_summary (token streaming) ---------- #
    # 给前端先推一个"正在总结"的占位，然后逐 token 流式输出 narrative，
    # 利用 LangChain 的 ``tags=["narrative"]`` 让 chat.py 把这一次 LLM stream
    # 的 chunk 漏到正文（其他 chat2sql 内部 LLM 调用仍被 INTERNAL_LLM_NODES 屏蔽）。
    await _emit_running("step-result-summary", "result_summary", "正在总结结论…")
    rows_for_llm = json.dumps(truncated, ensure_ascii=False, default=str)[:6000]
    narrative_chunks: list[str] = []
    async for piece in llm.astream(
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
        ],
        config={"tags": ["narrative"]},
    ):
        text = getattr(piece, "content", None)
        if text:
            narrative_chunks.append(str(text))
    narrative = "".join(narrative_chunks).strip()

    # 把 narrative 前 100 字作为推理链最后一步的 detail，方便业务人员收起推理链也能瞄到结论概要
    narr_excerpt = narrative.replace("\n", " ").strip()
    if len(narr_excerpt) > 100:
        narr_excerpt = narr_excerpt[:100] + "…"
    summary_step = await _emit(
        "result_summary",
        "答案已就绪",
        id="step-result-summary",
        status="success",
        detail=narr_excerpt or "（结论已整理）",
        meta={"narrative": narrative},
    )
    accumulated.append(summary_step)

    chart_config = _maybe_build_chart_config(truncated, step2_json, user_question)

    # 统一表格 dispatcher：自动按 shape 选 pivot / 扁平
    result_table, table_summary = _build_smart_table(truncated, max_rows=10)
    result_title = f"### 结果（{table_summary}）"

    # SQL 默认折叠（<details>），用户想看实现细节时手动展开；
    # 业务人员一般只看 narrative + 结果表，不希望被一长段 SQL 占满屏幕。
    # 注意：用纯 HTML 而非 markdown 嵌套，避免 Showdown 不解析 details 块内的 markdown。
    import html as _html
    sql_escaped = _html.escape(final_sql)
    response_md = (
        f"{narrative}\n\n"
        f"---\n\n"
        f"<details class=\"sql-block\"><summary>📄 查看执行的 SQL 语句</summary>"
        f"<pre><code class=\"language-sql\">{sql_escaped}</code></pre>"
        f"</details>\n\n"
        f"{result_title}\n\n"
        + result_table
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


def _analyze_columns(
    rows: list[dict[str, Any]],
    cols: list[str],
) -> dict[str, str]:
    """分析每列的类型：'numeric' | 'time' | 'categorical'。

    用于后续基于 shape 的图表/表格选择。判断逻辑要稳健：
    - 整型/浮点/Decimal → numeric
    - date/datetime/形如 YYYY-MM-DD 的字符串 → time
    - 看着像纯数字的字符串（"1234"）也算 numeric，但要排除日期
    - 其他 → categorical
    """
    from decimal import Decimal as _D
    from datetime import date as _Date, datetime as _Datetime

    out: dict[str, str] = {}
    for c in cols:
        sample = next((r[c] for r in rows if r.get(c) is not None), None)
        if sample is None:
            out[c] = "categorical"
            continue
        if isinstance(sample, bool):
            out[c] = "categorical"
            continue
        if isinstance(sample, (int, float, _D)):
            out[c] = "numeric"
            continue
        if isinstance(sample, (_Date, _Datetime)):
            out[c] = "time"
            continue
        s = str(sample)
        # 形如 YYYY-MM-DD / YYYY/MM/DD 的字符串先认作时间
        if len(s) >= 8 and s[:4].isdigit() and ("-" in s[:10] or "/" in s[:10]):
            out[c] = "time"
            continue
        # 否则尝试当数字
        try:
            float(s)
            out[c] = "numeric"
            continue
        except (ValueError, TypeError):
            pass
        out[c] = "categorical"
    return out


def _classify_shape(
    rows: list[dict[str, Any]],
    cols: list[str],
) -> dict[str, Any]:
    """返回 shape 描述：{cat: [...], num: [...], time: [...], fingerprint: 'cat:2+num:1+time:0'}"""
    kinds = _analyze_columns(rows, cols)
    cat = [c for c in cols if kinds[c] == "categorical"]
    num = [c for c in cols if kinds[c] == "numeric"]
    time = [c for c in cols if kinds[c] == "time"]
    return {
        "cat": cat,
        "num": num,
        "time": time,
        "fingerprint": f"cat:{len(cat)}+num:{len(num)}+time:{len(time)}",
    }


def _pivot_2cat(
    rows: list[dict[str, Any]],
    row_col: str,
    col_col: str,
    num_col: str,
) -> dict[str, Any] | None:
    """把 (row_col, col_col, num_col) 三列数据透视成矩阵结构。

    返回：
      groups: row_col 的所有唯一值（首次出现顺序）
      series_names: col_col 的所有唯一值
      pivot: {(group, series): value}
      row_col / col_col / num_col：原列名（供 meta 使用）
    若维度太稀疏（<2 group 或 <2 series 或几乎全 0）返回 None。
    """
    groups: list[str] = []
    series_names: list[str] = []
    pivot: dict[tuple[str, str], float] = {}
    for r in rows:
        g = str(r.get(row_col) or "").strip() or "未标注"
        s = str(r.get(col_col) or "").strip() or "未标注"
        try:
            v = float(r.get(num_col) or 0)
        except (TypeError, ValueError):
            v = 0.0
        if g not in groups:
            groups.append(g)
        if s not in series_names:
            series_names.append(s)
        pivot[(g, s)] = v

    if len(groups) < 2 or len(series_names) < 2:
        return None
    non_zero_cells = sum(1 for v in pivot.values() if v > 0)
    if non_zero_cells < 2:
        return None

    return {
        "groups": groups,
        "series_names": series_names,
        "pivot": pivot,
        "row_col": row_col,
        "col_col": col_col,
        "num_col": num_col,
    }


def _maybe_build_chart_config(
    rows: list[dict[str, Any]],
    plan: dict[str, Any],
    user_question: str,
) -> dict[str, Any] | None:
    """Build ChartDescriptor based on result shape (multi-dim aware).

    Shape-based dispatch（按 fingerprint 选最佳图表）：
      - 1 time + 1 num                  → line
      - 1 cat + 1 num                   → bar / donut
      - 1 cat + 1 time + 1 num          → multi_line（每个 cat 一条线，x = time）
      - 2 cat + 1 num                   → grouped_bar（pivot to matrix）
      - 1 cat + N num                   → grouped_bar（series = 每个数值列）
      - 3+ cat + 1 num                  → grouped_bar（折叠：cat[0]+cat[1] 合并到 group label）
      - 其他                              → None（留给表格展示）
    """
    if not rows or len(rows) < 2:
        return None
    cols = list(rows[0].keys())
    if not cols or len(cols) > 6:
        return None

    shape = _classify_shape(rows, cols)
    cat, num, time = shape["cat"], shape["num"], shape["time"]
    if not num:
        return None  # 没有数值列，无法画图

    title_hint = plan.get("reasoning") if isinstance(plan, dict) else None
    title = (title_hint or user_question or "数据可视化")[:60]

    # ── 1 time + 1 cat + 1 num → multi_line ─────────────────
    if len(time) == 1 and len(cat) == 1 and len(num) == 1:
        return _build_multi_line(rows, time[0], cat[0], num[0], title)

    # ── 1 time + 1 num → line ───────────────────────────────
    if len(time) == 1 and len(num) == 1 and not cat:
        return _build_line_or_bar(rows, time[0], num[0], title, force_line=True)

    # ── 2 cat + 1 num → grouped_bar ─────────────────────────
    if len(cat) == 2 and len(num) == 1 and not time:
        return _build_grouped_bar(rows, cat[0], cat[1], num[0], title)

    # ── 2 cat + N num → grouped_bar（按主数值列建堆叠图，副数值列忽略；表格那边出 N 份矩阵）
    if len(cat) == 2 and len(num) >= 2 and not time:
        return _build_grouped_bar(rows, cat[0], cat[1], num[0], title)

    # ── 1 cat + N num → grouped_bar (series = 每个数值列) ──
    if len(cat) == 1 and len(num) >= 2 and not time:
        return _build_multi_metric_bar(rows, cat[0], num, title)

    # ── 1 cat + 1 num → bar / donut ─────────────────────────
    if len(cat) == 1 and len(num) == 1 and not time:
        return _build_line_or_bar(rows, cat[0], num[0], title, force_line=False)

    # ── 3+ cat + 1 num → 折叠 grouped_bar ──────────────────
    if len(cat) >= 3 and len(num) == 1:
        return _build_grouped_bar_collapsed(rows, cat, num[0], title)

    return None


# --------------------------------------------------------------------------- #
# Chart builders（按 shape 分别构造 chart_config）
# --------------------------------------------------------------------------- #


def _safe_num(v: Any) -> float:
    try:
        return float(v or 0)
    except (TypeError, ValueError):
        return 0.0


def _build_line_or_bar(
    rows: list[dict[str, Any]],
    x_col: str,
    y_col: str,
    title: str,
    force_line: bool,
) -> dict[str, Any] | None:
    """1 维 line / bar。 force_line=True 时强制 line（x 是时间）。"""
    data: list[dict[str, Any]] = []
    for r in rows:
        x_val = r.get(x_col)
        y_val = r.get(y_col)
        if x_val is None or y_val is None:
            continue
        data.append({"date": str(x_val), "category": str(x_val), "value": _safe_num(y_val)})
    if not data:
        return None
    chart_type = "line" if force_line else "bar"
    return {
        "type": chart_type,
        "title": title,
        "data": data,
        "xField": "date" if chart_type == "line" else "category",
        "yField": "value",
        "meta": {"metricName": y_col, "unit": "", "aggregation": "raw"},
    }


def _build_grouped_bar(
    rows: list[dict[str, Any]],
    cat1_col: str,
    cat2_col: str,
    num_col: str,
    title: str,
) -> dict[str, Any] | None:
    """2 cat + 1 num → grouped_bar（pivot 成矩阵：行=cat1, series=cat2）。"""
    p = _pivot_2cat(rows, cat1_col, cat2_col, num_col)
    if not p:
        return None
    series = []
    for sname in p["series_names"]:
        data = [round(p["pivot"].get((g, sname), 0.0), 2) for g in p["groups"]]
        series.append({"name": sname, "data": data})
    return {
        "type": "grouped_bar",
        "title": title,
        "groups": p["groups"],
        "series": series,
        "meta": {
            "groupLabel": cat1_col,
            "seriesLabel": cat2_col,
            "valueLabel": num_col,
        },
    }


def _build_multi_metric_bar(
    rows: list[dict[str, Any]],
    cat_col: str,
    num_cols: list[str],
    title: str,
) -> dict[str, Any] | None:
    """1 cat + N num → grouped_bar，每个数值列作为一个 series。"""
    groups: list[str] = []
    metric_data: dict[str, dict[str, float]] = {n: {} for n in num_cols}
    for r in rows:
        g = str(r.get(cat_col) or "").strip() or "—"
        if g not in groups:
            groups.append(g)
        for n in num_cols:
            metric_data[n][g] = _safe_num(r.get(n))
    if len(groups) < 2:
        return None
    series = [
        {"name": n, "data": [round(metric_data[n].get(g, 0.0), 2) for g in groups]}
        for n in num_cols
    ]
    return {
        "type": "grouped_bar",
        "title": title,
        "groups": groups,
        "series": series,
        "meta": {
            "groupLabel": cat_col,
            "seriesLabel": "指标",
            "valueLabel": "",
        },
    }


def _build_multi_line(
    rows: list[dict[str, Any]],
    time_col: str,
    cat_col: str,
    num_col: str,
    title: str,
) -> dict[str, Any] | None:
    """1 time + 1 cat + 1 num → multi_line：x = time，每个 cat 一条线。"""
    times: list[str] = []
    cats: list[str] = []
    pivot: dict[tuple[str, str], float] = {}
    for r in rows:
        t = str(r.get(time_col) or "").strip()
        c = str(r.get(cat_col) or "").strip() or "—"
        if not t:
            continue
        if t not in times:
            times.append(t)
        if c not in cats:
            cats.append(c)
        pivot[(t, c)] = _safe_num(r.get(num_col))
    times.sort()  # 时间维度按字典序（ISO 字符串够用）
    if len(times) < 2 or not cats:
        return None
    series = [
        {"name": c, "data": [round(pivot.get((t, c), 0.0), 2) for t in times]}
        for c in cats
    ]
    return {
        "type": "multi_line",
        "title": title,
        "xCategories": times,
        "series": series,
        "meta": {
            "xLabel": time_col,
            "seriesLabel": cat_col,
            "valueLabel": num_col,
        },
    }


def _build_grouped_bar_collapsed(
    rows: list[dict[str, Any]],
    cat_cols: list[str],
    num_col: str,
    title: str,
) -> dict[str, Any] | None:
    """3+ cat + 1 num → 折叠成 grouped_bar：
       - 用 cat[1] 当 series（横向比较的维度）
       - 把 cat[0] + cat[2..] 拼成 group label（如 "甲班 · 04-15"），落到行上
    """
    if len(cat_cols) < 3:
        return None
    series_col = cat_cols[1]
    row_cols = [cat_cols[0]] + cat_cols[2:]

    groups: list[str] = []
    series_names: list[str] = []
    pivot: dict[tuple[str, str], float] = {}
    for r in rows:
        g_parts = [str(r.get(c) or "").strip() or "—" for c in row_cols]
        g = " · ".join(g_parts)
        s = str(r.get(series_col) or "").strip() or "—"
        if g not in groups:
            groups.append(g)
        if s not in series_names:
            series_names.append(s)
        pivot[(g, s)] = _safe_num(r.get(num_col))
    if len(groups) < 2 or len(series_names) < 2:
        return None
    series = [
        {"name": sn, "data": [round(pivot.get((g, sn), 0.0), 2) for g in groups]}
        for sn in series_names
    ]
    return {
        "type": "grouped_bar",
        "title": title,
        "groups": groups,
        "series": series,
        "meta": {
            "groupLabel": " · ".join(row_cols),
            "seriesLabel": series_col,
            "valueLabel": num_col,
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


def _fmt_num_smart(v: float) -> str:
    """数字格式化：整数不带小数；带小数保留 2 位；大数自带千分位。"""
    if v is None:
        return ""
    n = float(v)
    if n == int(n):
        return f"{int(n):,}"
    return f"{n:,.2f}"


def _rows_to_markdown(rows: list[dict[str, Any]]) -> str:
    if not rows:
        return "_无数据_"
    cols = list(rows[0].keys())
    # 按首行类型判断每列是 numeric 还是 categorical：null 在前者显示 0、后者显示「未标注」
    kinds = _analyze_columns(rows, cols)
    header = "| " + " | ".join(cols) + " |"
    sep = "| " + " | ".join("---" for _ in cols) + " |"
    body = []
    for row in rows:
        cells: list[str] = []
        for c in cols:
            v = row.get(c)
            if v is None or (isinstance(v, str) and not v.strip()):
                cells.append("0" if kinds.get(c) == "numeric" else "未标注")
            else:
                cells.append(str(v))
        body.append("| " + " | ".join(cells) + " |")
    return "\n".join([header, sep, *body])


def _rows_to_markdown_pivot(pivot_info: dict[str, Any]) -> str:
    """2D pivot 表格：行=group，列=series，含小计列和合计行。

    兼容两种 key 命名：
      - 新版（_pivot_2cat）：row_col / col_col / num_col
      - 旧版（_try_pivot_2d，已删除但保留兼容）：cat1_col / cat2_col / num_col
    """
    groups = pivot_info["groups"]
    series_names = pivot_info["series_names"]
    pivot = pivot_info["pivot"]
    g_label = pivot_info.get("row_col") or pivot_info.get("cat1_col") or "类别"

    headers = [g_label] + list(series_names) + ["合计"]
    header_row = "| " + " | ".join(headers) + " |"
    sep_row = "| " + " | ".join("---" for _ in headers) + " |"

    body: list[str] = []
    col_totals = {s: 0.0 for s in series_names}
    grand_total = 0.0
    for g in groups:
        row_total = 0.0
        cells = [str(g)]
        for s in series_names:
            v = pivot.get((g, s), 0.0)
            cells.append(_fmt_num_smart(v))
            row_total += v
            col_totals[s] += v
        cells.append(_fmt_num_smart(row_total))
        body.append("| " + " | ".join(cells) + " |")
        grand_total += row_total

    # 合计行
    total_cells = ["**合计**"] + [_fmt_num_smart(col_totals[s]) for s in series_names] + [_fmt_num_smart(grand_total)]
    body.append("| " + " | ".join(total_cells) + " |")
    return "\n".join([header_row, sep_row, *body])


def _build_smart_table(
    rows: list[dict[str, Any]],
    *,
    max_rows: int = 20,
) -> tuple[str, str]:
    """统一表格 dispatcher。根据 shape 选最易读的表格形式。

    返回 (markdown_text, table_summary_line)。
    table_summary_line 是给 result_title 用的说明，如 "按 班组 × 等级 透视，共 4 个 班组"。
    """
    if not rows:
        return "_无数据_", "（暂无数据）"
    cols = list(rows[0].keys())
    shape = _classify_shape(rows, cols)
    cat, num, time = shape["cat"], shape["num"], shape["time"]

    # 2 cat + 1 num → 单矩阵 pivot
    if len(cat) == 2 and len(num) == 1 and not time:
        p = _pivot_2cat(rows, cat[0], cat[1], num[0])
        if p:
            md = _rows_to_markdown_pivot(p)
            return md, f"按 {cat[0]} × {cat[1]} 透视，共 {len(p['groups'])} 个 {cat[0]}"

    # ★ 2 cat + N num → 每个数值列一份 pivot，顺序拼接
    # 例：[班组, 一次交检结果, 卷重(kg), 卷数] → 出 2 个 pivot（卷重矩阵 + 卷数矩阵）
    if len(cat) == 2 and len(num) >= 2 and not time:
        parts: list[str] = []
        groups_count = 0
        for n in num:
            p = _pivot_2cat(rows, cat[0], cat[1], n)
            if not p:
                continue
            groups_count = max(groups_count, len(p["groups"]))
            parts.append(f"#### 按「{n}」聚合\n\n" + _rows_to_markdown_pivot(p))
        if parts:
            md = "\n\n".join(parts)
            return md, f"按 {cat[0]} × {cat[1]} 透视，{len(num)} 个指标分别成表，共 {groups_count} 个 {cat[0]}"

    # 1 cat + 1 time + 1 num → pivot（cat 行，time 列）
    if len(cat) == 1 and len(time) == 1 and len(num) == 1:
        p = _pivot_2cat(rows, cat[0], time[0], num[0])
        if p:
            md = _rows_to_markdown_pivot(p)
            return md, f"按 {cat[0]} × {time[0]} 透视，共 {len(p['groups'])} 个 {cat[0]}"

    # ★ 1 cat + 1 time + N num → 每个数值列一份 pivot
    if len(cat) == 1 and len(time) == 1 and len(num) >= 2:
        parts: list[str] = []
        groups_count = 0
        for n in num:
            p = _pivot_2cat(rows, cat[0], time[0], n)
            if not p:
                continue
            groups_count = max(groups_count, len(p["groups"]))
            parts.append(f"#### 按「{n}」聚合\n\n" + _rows_to_markdown_pivot(p))
        if parts:
            md = "\n\n".join(parts)
            return md, f"按 {cat[0]} × {time[0]} 透视，{len(num)} 个指标分别成表"

    # 3+ cat + 1 num → 折叠 pivot（cat[0]+cat[2..] 合并到行，cat[1] 作为列）
    if len(cat) >= 3 and len(num) == 1:
        row_cols = [cat[0]] + cat[2:]
        col_col = cat[1]
        num_col = num[0]
        groups: list[str] = []
        series_names: list[str] = []
        pivot: dict[tuple[str, str], float] = {}
        for r in rows:
            g_parts = [str(r.get(c) or "").strip() or "—" for c in row_cols]
            g = " · ".join(g_parts)
            s = str(r.get(col_col) or "").strip() or "—"
            if g not in groups:
                groups.append(g)
            if s not in series_names:
                series_names.append(s)
            pivot[(g, s)] = _safe_num(r.get(num_col))
        if len(groups) >= 2 and len(series_names) >= 2:
            p = {
                "groups": groups,
                "series_names": series_names,
                "pivot": pivot,
                "row_col": " · ".join(row_cols),
                "col_col": col_col,
                "num_col": num_col,
            }
            md = _rows_to_markdown_pivot(p)
            return md, f"按 {' · '.join(row_cols)} × {col_col} 透视，共 {len(groups)} 行"

    # 默认扁平
    md = _rows_to_markdown(rows[:max_rows])
    summary = f"{len(rows)} 行，展示前 {min(max_rows, len(rows))}"
    return md, summary
