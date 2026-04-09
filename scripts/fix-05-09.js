const { chromium } = require('playwright');
const BASE_URL = 'http://47.105.59.151:8928';
const SCREENSHOTS_DIR = './docs/screenshots';

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({ viewport: { width: 1920, height: 1080 } });
  
  await page.goto(BASE_URL + '/login', { waitUntil: 'domcontentloaded' });
  await page.waitForTimeout(2000);
  
  const u = await page.$('input[type="text"]');
  const p = await page.$('input[type="password"]');
  if (u) await u.fill('admin');
  if (p) await p.fill('Lm@2025');
  
  const b = await page.$('button[type="submit"]') || await page.$('button');
  if (b) await b.click();
  
  await page.waitForURL(/dashboard|home|monthly-dashboard|lab/, { timeout: 30000 });
  await page.waitForTimeout(2000);
  
  await page.goto(BASE_URL + '/lab/monthly-dashboard', { waitUntil: 'domcontentloaded', timeout: 30000 });
  await page.waitForTimeout(3000);
  
  await page.screenshot({ 
    path: SCREENSHOTS_DIR + '/报表分析/05-09-生产驾驶舱-质量趋势图.png', 
    clip: { x: 0, y: 700, width: 1920, height: 380 } 
  });
  console.log('✓ 05-09 已保存');
  await browser.close();
})();
