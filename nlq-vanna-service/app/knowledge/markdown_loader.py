"""加载 knowledge/ 目录下的 Markdown 文件与 YAML 示例。

提供三个公共函数：
  - load_terminology():    读取 terminology.md，按段落拆分
  - load_judgment_rules(): 读取 judgment_rules.md，按段落拆分
  - load_qa_examples():    读取 qa_examples.yaml，返回 [{question, sql}]
"""

from __future__ import annotations

import logging
from pathlib import Path

logger = logging.getLogger(__name__)

# 段落分隔符：按二级标题 "## " 拆分（保留标题行）
_H2_SEPARATOR = "\n\n## "


def _split_by_h2(text: str) -> list[str]:
    """按 '\\n\\n## ' 将 Markdown 文本拆成多个段落 chunk。

    每个 chunk 包含标题行及其后续内容。
    开头第一段（文件头，# 标题之前或之间）也作为独立 chunk 保留。

    Args:
        text: 原始 Markdown 文本。

    Returns:
        非空字符串列表，每项对应一个语义段落。
    """
    parts = text.split(_H2_SEPARATOR)
    chunks: list[str] = []
    for i, part in enumerate(parts):
        part = part.strip()
        if not part:
            continue
        # 非第一段重新加回 "## " 前缀，保持 Markdown 结构完整
        if i > 0:
            part = "## " + part
        chunks.append(part)
    return chunks


def load_terminology(knowledge_dir: Path) -> list[str]:
    """读取 terminology.md 并按 '\\n\\n## ' 分段。

    Args:
        knowledge_dir: knowledge/ 目录的 Path 对象。

    Returns:
        每个语义段落对应一个字符串的列表。
        文件不存在时返回空列表并记录 warning。
    """
    path = knowledge_dir / "terminology.md"
    if not path.exists():
        logger.warning("load_terminology: 文件不存在 path=%s", path)
        return []

    text = path.read_text(encoding="utf-8")
    chunks = _split_by_h2(text)
    logger.info("load_terminology: 加载 %d 个段落 from %s", len(chunks), path)
    return chunks


def load_judgment_rules(knowledge_dir: Path) -> list[str]:
    """读取 judgment_rules.md 并按 '\\n\\n## ' 分段。

    Args:
        knowledge_dir: knowledge/ 目录的 Path 对象。

    Returns:
        每个语义段落对应一个字符串的列表。
        文件不存在时返回空列表并记录 warning。
    """
    path = knowledge_dir / "judgment_rules.md"
    if not path.exists():
        logger.warning("load_judgment_rules: 文件不存在 path=%s", path)
        return []

    text = path.read_text(encoding="utf-8")
    chunks = _split_by_h2(text)
    logger.info("load_judgment_rules: 加载 %d 个段落 from %s", len(chunks), path)
    return chunks


def load_qa_examples(knowledge_dir: Path) -> list[dict]:
    """读取 qa_examples.yaml，返回 [{question, sql}] 列表。

    Args:
        knowledge_dir: knowledge/ 目录的 Path 对象。

    Returns:
        包含 "question" 和 "sql" 键的 dict 列表。
        文件不存在或解析失败时返回空列表并记录 warning。
    """
    path = knowledge_dir / "qa_examples.yaml"
    if not path.exists():
        logger.warning("load_qa_examples: 文件不存在 path=%s", path)
        return []

    try:
        import yaml  # noqa: PLC0415

        data = yaml.safe_load(path.read_text(encoding="utf-8"))
    except Exception as exc:  # noqa: BLE001
        logger.warning("load_qa_examples: 解析失败 path=%s error=%s", path, exc)
        return []

    if not isinstance(data, list):
        logger.warning(
            "load_qa_examples: 期望列表，实际 %s path=%s", type(data).__name__, path
        )
        return []

    examples: list[dict] = []
    for i, item in enumerate(data):
        if not isinstance(item, dict):
            logger.warning("load_qa_examples: 第 %d 项不是 dict，跳过", i)
            continue
        if "question" not in item or "sql" not in item:
            logger.warning(
                "load_qa_examples: 第 %d 项缺少 question/sql 字段，跳过", i
            )
            continue
        examples.append({"question": str(item["question"]), "sql": str(item["sql"])})

    logger.info("load_qa_examples: 加载 %d 条示例 from %s", len(examples), path)
    return examples
