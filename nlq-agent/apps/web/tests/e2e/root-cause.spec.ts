/**
 * E2E test for the KG-augmented RootCause reasoning chain.
 *
 * 该测试假设以下服务已经启动：
 *   - docker compose up -d  （含 Neo4j + MySQL）
 *   - cd nlq-agent/services/agent-api && uv run uvicorn app.main:app --port 18100
 *   - cd nlq-agent/apps/web && pnpm dev (port 3000)
 *
 * 通过命令行运行：
 *   cd nlq-agent && pnpm --filter web test:e2e -- root-cause.spec.ts
 *
 * 验收点（plan AC-1）：
 *   1. 输入示例 query 后，DevTools 能观察到 ≥3 条 reasoning_step SSE 事件
 *   2. 页面渲染包含 KgReasoningChain 折叠块（[data-testid="kg-reasoning-chain"]）
 *   3. 折叠块内含中文文本
 */

import { test, expect } from "@playwright/test";

const SAMPLE_QUERY = "为什么炉号 1丙20260110-1 是 C 级？";

test.describe("KG reasoning chain e2e", () => {
  test("test_reasoning_block_renders @root-cause", async ({ page }) => {
    // Track reasoning_step events from the SSE stream.
    const reasoningEvents: unknown[] = [];
    page.on("response", async (response) => {
      const url = response.url();
      if (!url.includes("/api/chat") && !url.includes("/chat/stream")) return;
      try {
        const headers = response.headers();
        if (!headers["content-type"]?.includes("text/event-stream")) return;
        const body = await response.text();
        for (const line of body.split("\n")) {
          if (!line.startsWith("data:")) continue;
          const payload = line.slice(5).trim();
          if (!payload || payload === "[DONE]") continue;
          try {
            const evt = JSON.parse(payload);
            if (evt.type === "reasoning_step") reasoningEvents.push(evt);
          } catch {
            // ignore malformed
          }
        }
      } catch {
        // ignore — non-SSE responses
      }
    });

    await page.goto("/");

    // Submit the sample query through the input form.
    const input = page.getByPlaceholder(/输入|提问|消息/i).first();
    await input.fill(SAMPLE_QUERY);
    await input.press("Enter");

    // Wait for the reasoning block to render.
    const reasoning = page.getByTestId("kg-reasoning-chain");
    await expect(reasoning).toBeVisible({ timeout: 30_000 });

    // Assert it contains Chinese summary text.
    const text = await reasoning.textContent();
    expect(text).not.toBeNull();
    expect(text!.length).toBeGreaterThan(10);
    expect(/[一-鿿]/.test(text!)).toBe(true);

    // AC-1 requires ≥3 reasoning_step events on the wire.
    expect(reasoningEvents.length).toBeGreaterThanOrEqual(3);
  });
});
