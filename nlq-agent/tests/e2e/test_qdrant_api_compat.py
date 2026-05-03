"""
Regression test: qdrant-client ≥ 1.10 removed AsyncQdrantClient.search.

Two lanes:
  1. Smoke test (always runs): inspect-based method existence checks.
  2. Live Qdrant test (@pytest.mark.live_qdrant): round-trip create→upsert→query→delete.
"""

from __future__ import annotations

import hashlib
import socket
import struct
from typing import Any

import pytest

from qdrant_client import AsyncQdrantClient


# ── Helpers ────────────────────────────────────────────────────


def _fake_embedding(text: str, dim: int = 1024) -> list[float]:
    """Deterministic pseudo-embedding for testing (hash → dim floats).."""
    raw = hashlib.sha256(text.encode()).digest()
    vec: list[float] = []
    for i in range(dim):
        chunk = raw[i % len(raw) : (i % len(raw)) + 4] or raw[:4]
        vec.append(struct.unpack("<f", chunk)[0])
    # normalise so cosine similarity stays in [-1, 1]
    norm = sum(x * x for x in vec) ** 0.5 or 1.0
    return [x / norm for x in vec]


def _qdrant_reachable(host: str = "localhost", port: int = 6333) -> bool:
    """Check if a TCP connection to Qdrant is possible."""
    try:
        with socket.create_connection((host, port), timeout=2):
            return True
    except OSError:
        return False


# ── Lane 1: Smoke (always runs) ───────────────────────────────


class TestQdrantMethodSmoke:
    """Assert key methods exist on AsyncQdrantClient — catches removed APIs."""

    EXPECTED_METHODS = [
        "query_points",
        "upsert",
        "create_collection",
        "delete_collection",
        "collection_exists",
        "get_collections",
        "close",
    ]

    REMOVED_METHODS = [
        "search",
        "search_groups",
        "recommend",
    ]

    def test_expected_methods_exist(self) -> None:
        for method in self.EXPECTED_METHODS:
            assert hasattr(AsyncQdrantClient, method), (
                f"AsyncQdrantClient.{method}() missing — "
                f"qdrant-client may have been downgraded"
            )

    def test_removed_methods_are_gone(self) -> None:
        for method in self.REMOVED_METHODS:
            assert not hasattr(AsyncQdrantClient, method), (
                f"AsyncQdrantClient.{method}() still exists — "
                f"code may call the deprecated API without error"
            )


# ── Lane 2: Live Qdrant round-trip ────────────────────────────


@pytest.fixture
def _skip_if_no_qdrant():
    if not _qdrant_reachable():
        pytest.skip("Qdrant not reachable at localhost:6333")


@pytest.fixture
async def qdrant_client(_skip_if_no_qdrant) -> AsyncQdrantClient:
    client = AsyncQdrantClient(host="localhost", port=6333)
    yield client
    await client.close()


TEST_COLLECTION = "__test_api_compat"


@pytest.mark.live_qdrant
@pytest.mark.asyncio
async def test_query_points_round_trip(qdrant_client: AsyncQdrantClient) -> None:
    """Create → upsert → query_points → delete: full cycle without AttributeError."""
    from qdrant_client.models import Distance, PointStruct, VectorParams

    # cleanup if leftover from a prior run
    if await qdrant_client.collection_exists(TEST_COLLECTION):
        await qdrant_client.delete_collection(TEST_COLLECTION)

    await qdrant_client.create_collection(
        collection_name=TEST_COLLECTION,
        vectors_config=VectorParams(size=16, distance=Distance.COSINE),
    )

    vectors = [_fake_embedding(f"doc-{i}", dim=16) for i in range(3)]
    points = [
        PointStruct(id=i, vector=vectors[i], payload={"text": f"doc-{i}"})
        for i in range(3)
    ]
    await qdrant_client.upsert(collection_name=TEST_COLLECTION, points=points)

    # The critical assertion: query_points must exist and work
    response = await qdrant_client.query_points(
        collection_name=TEST_COLLECTION,
        query=vectors[0],
        limit=3,
    )
    assert len(response.points) >= 1
    assert response.points[0].payload is not None

    # Also assert the old API is truly gone
    assert not hasattr(qdrant_client, "search"), (
        "AsyncQdrantClient.search still present — qdrant-client < 1.10?"
    )

    await qdrant_client.delete_collection(TEST_COLLECTION)
