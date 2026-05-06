import { test, expect } from '@playwright/test';

test('site loads', async ({ page }) => {
  await page.goto('/');
  // very loose smoke: assert title is non-empty
  await expect(page).toHaveTitle(/.+/);
});
