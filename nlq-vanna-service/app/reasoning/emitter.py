"""Thread-safe SSE step emitter.

关键设计 (Architect Round 1 A6 / Round 2 PASS)：
- asyncio.Queue 不是线程安全的（put_nowait 在工作线程里属 undefined behavior）
- Vanna generate_sql 跑在 asyncio.to_thread 包裹的工作线程里
- 所以工作线程调 put_step 时必须用 loop.call_soon_threadsafe(queue.put_nowait, step)
"""

from __future__ import annotations

import asyncio
import threading
from typing import AsyncIterator, Optional

from app.api.schemas import ReasoningStep


class ReasoningEmitter:
    """Thread-safe emitter for ReasoningStep events.

    Public API
    ----------
    put_step(step)          Thread-safe entry point — callable from any thread.
    _enqueue(step)          Same-loop (coroutine) entry point — no thread-safety overhead.
    snapshot()              Return all emitted steps (thread-safe read).
    aiter_until_done(task)  Async generator consumed by the SSE handler coroutine.
    close()                 Mark emitter as closed; subsequent put_step calls are no-ops.
    """

    def __init__(self, loop: Optional[asyncio.AbstractEventLoop] = None) -> None:
        self._loop: asyncio.AbstractEventLoop = loop or asyncio.get_running_loop()
        self._queue: asyncio.Queue[ReasoningStep] = asyncio.Queue()
        self._snapshot: list[ReasoningStep] = []
        self._snapshot_lock = threading.Lock()
        self._closed = False

    # ------------------------------------------------------------------
    # Cross-thread entry point (MUST use call_soon_threadsafe — A6)
    # ------------------------------------------------------------------

    def put_step(self, step: ReasoningStep) -> None:
        """Thread-safe; 可从工作线程或主协程调用。

        使用 loop.call_soon_threadsafe 将 _enqueue 调度到事件循环线程执行，
        确保 asyncio.Queue.put_nowait 始终在正确线程上调用。
        """
        if self._closed:
            return
        # A6 MUST: cross-thread emit via call_soon_threadsafe
        self._loop.call_soon_threadsafe(self._enqueue, step)  # line 48

    # ------------------------------------------------------------------
    # Same-loop entry point (used by coroutines directly)
    # ------------------------------------------------------------------

    def _enqueue(self, step: ReasoningStep) -> None:
        """Called on the event loop thread — safe to call put_nowait directly."""
        self._queue.put_nowait(step)
        with self._snapshot_lock:
            self._snapshot.append(step)

    # ------------------------------------------------------------------
    # Snapshot (thread-safe read for response_metadata)
    # ------------------------------------------------------------------

    def snapshot(self) -> list[ReasoningStep]:
        """返回所有已 emit 的 step，用于 response_metadata."""
        with self._snapshot_lock:
            return list(self._snapshot)

    # ------------------------------------------------------------------
    # Async generator consumed by chat_stream coroutine
    # ------------------------------------------------------------------

    async def aiter_until_done(
        self, sentinel_task: asyncio.Task  # type: ignore[type-arg]
    ) -> AsyncIterator[ReasoningStep]:
        """异步迭代 emitted steps，直到 sentinel_task 完成。

        使用 0.05s timeout 轮询（≤ 0.1s，ADR-3 A3 SHOULD 要求），避免在
        sentinel_task 已结束但 queue 已空时永久阻塞。
        """
        while not sentinel_task.done() or not self._queue.empty():
            try:
                step = await asyncio.wait_for(self._queue.get(), timeout=0.05)
                yield step
            except asyncio.TimeoutError:
                if sentinel_task.done():
                    break

    # ------------------------------------------------------------------
    # Lifecycle
    # ------------------------------------------------------------------

    def close(self) -> None:
        """Mark emitter closed; subsequent put_step calls become no-ops."""
        self._closed = True
