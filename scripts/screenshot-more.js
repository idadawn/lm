const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const baseDir = path.join(__dirname, '../docs/screenshots');
const dataMgmtDir = path.join(baseDir, '数据管理');
const reportDir = path.join(baseDir, '报表分析');

async function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

async function screenshot(page, filePath) {
  await page.screenshot({ path: filePath, fullPage: false });
  console.log(`✓ ${path.basename(filePath)}`);
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1920, height: 1080 } });
  const page = await context.newPage();

  try {
    // 登录
    console.log('[登录] 访问系统...');
    await page.goto('http://47.105.59.151:8928/login', { waitUntil: 'networkidle' });
    await delay(2000);
    await page.locator('input').nth(0).fill('admin');
    await page.locator('input').nth(1).fill('Lm@2025');
    await page.locator('button').first().click();
    await delay(8000);

    // 1. 中间数据管理 - 排序编辑器
    console.log('[1] 04-10-中间数据管理-排序编辑器.png');
    await page.goto('http://47.105.59.151:8928/lab/subTable', { waitUntil: 'networkidle' });
    await delay(5000);
    const sortBtns = await page.locator('button:has-text("排序")').all();
    if (sortBtns.length > 0) {
      await sortBtns[0].click();
      await delay(2000);
      await screenshot(page, path.join(dataMgmtDir, '04-10-中间数据管理-排序编辑器.png'));
    }

    // 2. 中间数据管理 - 导出弹窗
    console.log('[2] 04-13-中间数据管理-导出弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/subTable', { waitUntil: 'networkidle' });
    await delay(5000);
    const exportBtns = await page.locator('button:has-text("导出")').all();
    if (exportBtns.length > 0) {
      await exportBtns[0].click();
      await delay(2000);
      await screenshot(page, path.join(dataMgmtDir, '04-13-中间数据管理-导出弹窗.png'));
    }

    // 3. 磁性数据导入弹窗
    console.log('[3] 04-14-磁性数据导入-弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/subTable', { waitUntil: 'networkidle' });
    await delay(5000);
    const magneticBtns = await page.locator('button:has-text("磁性数据")').all();
    if (magneticBtns.length > 0) {
      await magneticBtns[0].click();
      await delay(2000);
      await screenshot(page, path.join(dataMgmtDir, '04-14-磁性数据导入-弹窗.png'));
    }

    // 4. 生产驾驶舱各图表
    console.log('[4] 报表分析图表...');
    await page.goto('http://47.105.59.151:8928/lab/monthly-dashboard', { waitUntil: 'networkidle' });
    await delay(3000);
    
    // 滚动到不同区域截图
    // 叠片系数趋势
    await page.evaluate(() => window.scrollTo(0, 400));
    await delay(1000);
    await screenshot(page, path.join(reportDir, '05-03-生产驾驶舱-叠片系数趋势.png'));
    
    // 质量分布饼图
    await page.evaluate(() => window.scrollTo(0, 400));
    await delay(1000);
    await screenshot(page, path.join(reportDir, '05-04-生产驾驶舱-质量分布饼图.png'));
    
    // 滚动到下方图表
    await page.evaluate(() => window.scrollTo(0, 800));
    await delay(1000);
    await screenshot(page, path.join(reportDir, '05-05-生产驾驶舱-班次对比雷达图.png'));
    
    await page.evaluate(() => window.scrollTo(0, 1200));
    await delay(1000);
    await screenshot(page, path.join(reportDir, '05-06-生产驾驶舱-不合格原因Top5.png'));

    console.log('\n额外截图完成！');

  } catch (error) {
    console.error('\n✗ 错误:', error.message);
  } finally {
    await browser.close();
  }
})();
