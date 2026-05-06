"""Qdrant vector store mixin for Vanna.

ADR-6: 自管 payload subtype，绕过 Vanna 默认 add_documentation() 写入路径。
MUST-A: get_related_documentation() 严格返回 list[str]。

所有检索方法在有 emitter 时调用 emitter.put_step(ReasoningStep)，
emitter 接口由 Wave 1C 实现；此处仅调用，不定义。
"""

from __future__ import annotations

import uuid
from typing import TYPE_CHECKING, Any

from qdrant_client import QdrantClient
from qdrant_client.http import models as qmodels

# ReasoningStep 由 Wave 1A 在 app.api.schemas 中定义。
# 此处 TYPE_CHECKING 仅用于类型注解，运行时 import 延迟到方法内部
# 以避免循环依赖或 Wave 1A 尚未完成时的 ImportError。
if TYPE_CHECKING:
    from app.api.schemas import ReasoningStep

_COLLECTION = "nlq_vanna_knowledge"


def _new_id() -> str:
    """Generate a deterministic-ish string ID (UUID4 hex)."""
    return uuid.uuid4().hex


class QdrantStoreMixin:
    """Mixin that replaces Vanna's default vector store with Qdrant.

    Expected config keys:
        qdrant_url (str):    Qdrant HTTP URL, e.g. "http://localhost:6333"
        embedding_dim (int): Vector dimension; must match TEI model output.

    Payload schema
    --------------
    DDL point     : {"type": "documentation", "subtype": "ddl",           "content": str}
    Terminology   : {"type": "documentation", "subtype": "terminology",   "content": str}
    Judgment rule : {"type": "documentation", "subtype": "judgment_rule", "content": str}
    Q&A pair      : {"type": "qa", "question": str, "sql": str,           "content": str}
    """

    def __init__(self, config: dict) -> None:
        self._qdrant_url: str = config["qdrant_url"].rstrip("/")
        self._embedding_dim: int = int(config["embedding_dim"])
        self._client: QdrantClient = QdrantClient(url=self._qdrant_url)
        self._emitter: Any = None  # set via attach_emitter()
        self._ensure_collection()

    # ------------------------------------------------------------------
    # Internal helpers
    # ------------------------------------------------------------------

    def _ensure_collection(self) -> None:
        """Create the Qdrant collection if it does not yet exist."""
        existing = {c.name for c in self._client.get_collections().collections}
        if _COLLECTION not in existing:
            self._client.create_collection(
                collection_name=_COLLECTION,
                vectors_config=qmodels.VectorParams(
                    size=self._embedding_dim,
                    distance=qmodels.Distance.COSINE,
                ),
            )

    def _upsert(self, payload: dict, text_for_embed: str) -> str:
        """Embed text and upsert a single point; return the point ID."""
        point_id = _new_id()
        vector = self.generate_embedding(text_for_embed)  # type: ignore[attr-defined]
        self._client.upsert(
            collection_name=_COLLECTION,
            points=[
                qmodels.PointStruct(
                    id=point_id,
                    vector=vector,
                    payload=payload,
                )
            ],
        )
        return point_id

    def _search(
        self,
        question: str,
        filt: qmodels.Filter,
        limit: int,
    ) -> list[qmodels.ScoredPoint]:
        """Embed question and run a filtered vector search.

        qdrant-client 1.10+ deprecates `search()`; we use `query_points()`
        which returns QueryResponse whose `.points` is the ScoredPoint list.
        """
        vector = self.generate_embedding(question)  # type: ignore[attr-defined]
        result = self._client.query_points(
            collection_name=_COLLECTION,
            query=vector,
            query_filter=filt,
            limit=limit,
            with_payload=True,
        )
        return list(result.points)

    def _emit(self, step: ReasoningStep) -> None:
        """Silently emit a reasoning step if emitter is attached."""
        try:
            if self._emitter is not None:
                self._emitter.put_step(step)
        except AttributeError:
            pass

    def _make_step(self, **kwargs: Any) -> ReasoningStep:
        """Lazy-import ReasoningStep and construct an instance."""
        from app.api.schemas import ReasoningStep  # noqa: PLC0415

        return ReasoningStep(**kwargs)

    # ------------------------------------------------------------------
    # Emitter lifecycle
    # ------------------------------------------------------------------

    def attach_emitter(self, emitter: Any) -> None:
        """Attach a reasoning emitter (Wave 1C).

        The emitter must implement put_step(step: ReasoningStep) -> None.
        """
        self._emitter = emitter

    def detach_emitter(self) -> None:
        """Remove the attached emitter."""
        self._emitter = None

    # ------------------------------------------------------------------
    # Vanna write interface — override defaults with subtype-aware upserts
    # ------------------------------------------------------------------

    def add_ddl(self, ddl: str, **kwargs: Any) -> str:
        """Store a DDL string with subtype='ddl'.

        Args:
            ddl: A CREATE TABLE (or similar) DDL statement.

        Returns:
            The Qdrant point ID for the inserted vector.
        """
        payload = {"type": "documentation", "subtype": "ddl", "content": ddl}
        return self._upsert(payload, ddl)

    def add_documentation(self, documentation: str, **kwargs: Any) -> str:  # noqa: ARG002
        """Intentionally disabled.

        ADR-6: 不走 Vanna 默认文档路径。
        Use add_terminology() or add_judgment_rule() instead.

        Raises:
            NotImplementedError: Always.
        """
        raise NotImplementedError(
            "add_documentation() 已禁用（ADR-6）。"
            "请使用 add_terminology() 或 add_judgment_rule() 代替。"
        )

    def add_terminology(self, doc: str, **kwargs: Any) -> str:
        """Store a business terminology entry with subtype='terminology'.

        Args:
            doc: Natural-language description of the terminology.

        Returns:
            The Qdrant point ID.
        """
        payload = {"type": "documentation", "subtype": "terminology", "content": doc}
        return self._upsert(payload, doc)

    def add_judgment_rule(self, doc: str, **kwargs: Any) -> str:
        """Store a judgment rule with subtype='judgment_rule'.

        Args:
            doc: Serialized judgment rule text (may include JSON_EXTRACT examples).

        Returns:
            The Qdrant point ID.
        """
        payload = {"type": "documentation", "subtype": "judgment_rule", "content": doc}
        return self._upsert(payload, doc)

    def add_question_sql(self, question: str, sql: str, **kwargs: Any) -> str:
        """Store a Q&A pair with type='qa'.

        Args:
            question: The natural-language question.
            sql:      The corresponding SQL answer.

        Returns:
            The Qdrant point ID.
        """
        content = f"Q: {question}\nA: {sql}"
        payload = {
            "type": "qa",
            "question": question,
            "sql": sql,
            "content": content,
        }
        return self._upsert(payload, content)

    # ------------------------------------------------------------------
    # Vanna training_data interface — minimum implementations
    # to satisfy VannaBase abstract contract (ADR-6 self-managed)
    # ------------------------------------------------------------------

    def get_training_data(self, **kwargs: Any) -> "pd.DataFrame":  # noqa: ARG002
        """Return all training data as a DataFrame (Vanna abstract contract).

        Walks the Qdrant collection (excluding `__meta__` points) and returns
        a DataFrame with columns: id, training_data_type, question, content.
        """
        import pandas as pd  # noqa: PLC0415

        rows: list[dict] = []
        offset = None
        while True:
            points, next_offset = self._client.scroll(
                collection_name=self.COLLECTION,
                with_payload=True,
                with_vectors=False,
                limit=256,
                offset=offset,
            )
            for p in points:
                payload = p.payload or {}
                if payload.get("subtype") == "__meta__":
                    continue
                rows.append({
                    "id": str(p.id),
                    "training_data_type": payload.get("subtype")
                                          or payload.get("type", "unknown"),
                    "question": payload.get("question", ""),
                    "content": payload.get("content", ""),
                })
            if next_offset is None:
                break
            offset = next_offset
        return pd.DataFrame(rows)

    def remove_training_data(self, id: str, **kwargs: Any) -> bool:  # noqa: A002, ARG002
        """Remove a training-data point by Qdrant id (Vanna abstract contract)."""
        try:
            self._client.delete(
                collection_name=self.COLLECTION,
                points_selector=qmodels.PointIdsList(points=[id]),
            )
            return True
        except Exception:  # noqa: BLE001
            return False

    # ------------------------------------------------------------------
    # Vanna read interface — MUST-A: all doc methods return list[str]
    # ------------------------------------------------------------------

    def get_related_ddl(self, question: str, **kwargs: Any) -> list[str]:
        """Retrieve DDL entries most similar to the question.

        Args:
            question: The user's natural-language question.

        Returns:
            list[str]: Up to 5 DDL strings ordered by cosine similarity.
        """
        filt = qmodels.Filter(
            must=[
                qmodels.FieldCondition(
                    key="subtype",
                    match=qmodels.MatchValue(value="ddl"),
                )
            ]
        )
        # DeepSeek 64K 上下文足以放下完整 DDL；如切回小窗口模型可调小 limit
        hits = self._search(question, filt, limit=5)
        result: list[str] = [str(h.payload["content"]) for h in hits]  # type: ignore[index]

        if hits:
            try:
                step = self._make_step(
                    kind="spec",
                    label=(
                        f"匹配到表结构 (top-{len(hits)} "
                        f"score={hits[0].score:.3f})"
                    ),
                    detail=str(hits[0].payload["content"])[:200],  # type: ignore[index]
                    meta={"source": "ddl", "score": hits[0].score},
                )
                self._emit(step)
            except Exception:  # noqa: BLE001
                pass

        return result

    def get_similar_question_sql(self, question: str, **kwargs: Any) -> list[dict]:
        """Retrieve the most similar Q&A pairs for the question.

        Args:
            question: The user's natural-language question.

        Returns:
            list[dict]: Up to 3 dicts with keys "question" and "sql"
                        (Vanna default qa retrieval structure).
        """
        filt = qmodels.Filter(
            must=[
                qmodels.FieldCondition(
                    key="type",
                    match=qmodels.MatchValue(value="qa"),
                )
            ]
        )
        hits = self._search(question, filt, limit=3)
        result: list[dict] = [
            {
                "question": h.payload["question"],  # type: ignore[index]
                "sql": h.payload["sql"],  # type: ignore[index]
            }
            for h in hits
        ]

        if hits:
            try:
                step = self._make_step(
                    kind="spec",
                    label="命中相似历史问题",
                    meta={"source": "qa"},
                )
                self._emit(step)
            except Exception:  # noqa: BLE001
                pass

        return result

    def get_related_documentation(self, question: str, **kwargs: Any) -> list[str]:
        """Retrieve business documentation (terminology + judgment rules).

        MUST-A (ADR-6 / Architect Round 2): This method MUST return list[str].
        Each element is the raw payload content string; Vanna's VannaBase joins
        them with '\\n\\n' to build the prompt — returning anything other than
        str elements would produce '[object Object]' noise.

        Args:
            question: The user's natural-language question.

        Returns:
            list[str]: Up to 5 content strings from terminology and judgment_rule
                       subtypes, ordered by cosine similarity.
        """
        filt = qmodels.Filter(
            must=[
                qmodels.FieldCondition(
                    key="type",
                    match=qmodels.MatchValue(value="documentation"),
                ),
                qmodels.FieldCondition(
                    key="subtype",
                    match=qmodels.MatchAny(any=["terminology", "judgment_rule"]),
                ),
            ]
        )
        hits = self._search(question, filt, limit=5)

        # MUST-A: strictly list[str]
        result: list[str] = [str(h.payload["content"]) for h in hits]  # type: ignore[index]

        # Emit per-subtype reasoning steps
        for hit in hits:
            try:
                subtype = hit.payload.get("subtype", "")  # type: ignore[union-attr]
                content_preview = str(hit.payload.get("content", ""))[:80]  # type: ignore[union-attr]
                if subtype == "terminology":
                    step = self._make_step(
                        kind="rule",
                        label=f"应用业务术语：{content_preview}",
                        meta={"source": "terminology", "score": hit.score},
                    )
                elif subtype == "judgment_rule":
                    step = self._make_step(
                        kind="rule",
                        label=f"应用判级规则：{content_preview}",
                        meta={"source": "judgment", "score": hit.score},
                    )
                else:
                    continue
                self._emit(step)
            except Exception:  # noqa: BLE001
                pass

        return result
