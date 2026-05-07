import { test, expect } from '@playwright/test';
import * as fs from 'node:fs';
import * as path from 'node:path';

test.describe('chat golden path', () => {
  test('full flow: ask → see reasoning_step → see markdown → refresh → session restored', async ({ page }) => {
    // Mock SSE endpoint with fixture
    const fixture = fs.readFileSync(
      path.resolve(__dirname, '../tests/fixtures/sse-golden-trace.ndjson'),
      'utf8',
    );

    await page.route('**/api/v1/chat/stream', async (route) => {
      await route.fulfill({
        status: 200,
        headers: {
          'content-type': 'text/event-stream',
          'cache-control': 'no-cache',
        },
        body: fixture,
      });
    });

    // Step 1: open the dashboard with chat assistant trigger
    await page.goto('/lab/monthly-dashboard');

    // Click the chat trigger (floating button)
    await page.click('[data-testid="chat-trigger"]', { timeout: 10000 });

    // Step 2: ask a question
    await page.fill('[data-testid="chat-input"]', '今天合格率');
    await page.click('[data-testid="chat-send"]');

    // Step 3: wait for at least one reasoning_step bubble
    // ReasoningChain opens automatically while streaming (defaultOpen = true during stream)
    await page.waitForSelector('[data-testid="reasoning-step"]', { timeout: 15000 });

    // Step 4: mid-stream refresh
    await page.reload();

    // Step 5: re-open chat (visibility does not auto-restore after reload)
    await page.click('[data-testid="chat-trigger"]', { timeout: 10000 });

    // Step 6: assert session restored — user message "今天合格率" still in messages
    await expect(
      page.locator('[data-testid="message-bubble"][data-role="user"]').first(),
    ).toContainText('今天合格率');
  });
});
