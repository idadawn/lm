"""Thread-safety stress test for ReasoningEmitter.

A6 stress test (Architect Round 2 MUST):
  - 50 worker threads × 100 steps each = 5000 total steps
  - All steps must be collected by aiter_until_done with zero loss
  - Test has a 10s timeout to prevent silent deadlock hangs
"""

from __future__ import annotations

import asyncio
import threading

import pytest

from app.api.schemas import ReasoningStep
from app.reasoning.emitter import ReasoningEmitter


@pytest.mark.asyncio
@pytest.mark.timeout(10)
async def test_emitter_50_concurrent_no_loss():
    """50 threads × 100 steps = 5000 steps, zero loss."""
    loop = asyncio.get_running_loop()
    emitter = ReasoningEmitter(loop=loop)

    done_event = asyncio.Event()

    async def collect():
        collected: list[ReasoningStep] = []
        sentinel = asyncio.create_task(done_event.wait())
        async for step in emitter.aiter_until_done(sentinel):
            collected.append(step)
        return collected

    collect_task = asyncio.create_task(collect())

    def worker(idx: int) -> None:
        for i in range(100):
            step = ReasoningStep(kind="condition", label=f"w{idx}-i{i}")
            emitter.put_step(step)

    threads = [threading.Thread(target=worker, args=(i,)) for i in range(50)]
    for t in threads:
        t.start()
    for t in threads:
        t.join()

    # Wait for queue to drain, then signal done via Event
    await asyncio.sleep(0.3)
    loop.call_soon_threadsafe(done_event.set)

    collected = await collect_task
    assert len(collected) == 5000, (
        f"Expected 5000 steps, got {len(collected)}. "
        "Possible thread-safety regression in ReasoningEmitter.put_step()."
    )


@pytest.mark.asyncio
@pytest.mark.timeout(10)
async def test_emitter_all_steps_have_correct_kind():
    """All emitted steps retain their kind field after cross-thread delivery."""
    loop = asyncio.get_running_loop()
    emitter = ReasoningEmitter(loop=loop)

    async def collect():
        collected: list[ReasoningStep] = []
        sentinel = asyncio.create_task(asyncio.sleep(1.0))
        async for step in emitter.aiter_until_done(sentinel):
            collected.append(step)
        return collected

    collect_task = asyncio.create_task(collect())

    def worker(idx: int) -> None:
        for i in range(20):
            # Alternate kinds to verify no corruption
            kind = "condition" if i % 2 == 0 else "spec"
            step = ReasoningStep(kind=kind, label=f"w{idx}-i{i}")  # type: ignore[arg-type]
            emitter.put_step(step)

    threads = [threading.Thread(target=worker, args=(i,)) for i in range(10)]
    for t in threads:
        t.start()
    for t in threads:
        t.join()

    await asyncio.sleep(0.3)

    collected = await collect_task
    assert len(collected) == 200  # 10 threads × 20 steps
    kinds = {s.kind for s in collected}
    assert "condition" in kinds
    assert "spec" in kinds


@pytest.mark.asyncio
@pytest.mark.timeout(5)
async def test_emitter_closed_drops_steps():
    """Steps emitted after close() are silently dropped (no error)."""
    loop = asyncio.get_running_loop()
    emitter = ReasoningEmitter(loop=loop)

    # Emit one step before closing
    emitter.put_step(ReasoningStep(kind="condition", label="before-close"))
    await asyncio.sleep(0.1)

    emitter.close()

    # These should be no-ops — no error, no enqueue
    emitter.put_step(ReasoningStep(kind="condition", label="after-close-1"))
    emitter.put_step(ReasoningStep(kind="condition", label="after-close-2"))

    # snapshot only contains the pre-close step
    snap = emitter.snapshot()
    assert len(snap) == 1
    assert snap[0].label == "before-close"


@pytest.mark.asyncio
@pytest.mark.timeout(5)
async def test_emitter_snapshot_is_copy():
    """snapshot() returns a copy; mutations do not affect internal state."""
    loop = asyncio.get_running_loop()
    emitter = ReasoningEmitter(loop=loop)

    emitter.put_step(ReasoningStep(kind="condition", label="s1"))
    await asyncio.sleep(0.1)

    snap1 = emitter.snapshot()
    snap1.clear()  # mutate the returned copy

    snap2 = emitter.snapshot()
    assert len(snap2) == 1  # internal state unchanged
