const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const baseDir = path.join(__dirname, '../docs/screenshots');
const baseMgmtDir = path.join(baseDir, '基础管理');
const dataMgmtDir = path.join(baseDir, '数据管理');

async function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

async function screenshot(page, filePath) {
  await page.screenshot({ path: filePath, fullPage: false });
  console.log(`Screenshot saved: ${filePath}`);
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1920, height: 1080 } });
  const page = await context.newPage();

  try {
    // 登录
    console.log('Logging in...');
    await page.goto('http://47.105.59.151:8928/login');
    await delay(2000);
    await page.locator('input').first().fill('admin');
    await page.locator('input').nth(1).fill('lm@2025');
    await page.locator('button').first().click();
    await delay(5000);

    // 1. 产品规格定义 - 新增弹窗
    console.log('Screenshot: 产品规格定义-新增弹窗');
    await page.goto('http://47.105.59.151:8928/lab/product');
    await delay(5000);
    await page.waitForSelector('button', { timeout: 10000 });
    await page.screenshot({ path: path.join(baseMgmtDir, '03-01-产品规格定义-列表页-debug.png') });
    const buttons = await page.locator('button').all();
    console.log(`Found ${buttons.length} buttons`);
    for (let i = 0; i < Math.min(buttons.length, 5); i++) {
      const text = await buttons[i].textContent();
      console.log(`Button ${i}: ${text}`);
    }
    // 点击第一个包含"新增"的按钮
    for (const btn of buttons) {
      const text = await btn.textContent();
      if (text && text.includes('新增')) {
        await btn.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-02-产品规格定义-新增弹窗.png'));
    await page.keyboard.press('Escape');
    await delay(500);

    // 2. 单位管理 - 新增维度弹窗
    console.log('Screenshot: 单位管理-新增维度弹窗');
    await page.goto('http://47.105.59.151:8928/lab/unit');
    await delay(3000);
    await page.locator('button').filter({ hasText: /新增单位维度/ }).first().click();
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-04-单位管理-新增维度弹窗.png'));
    await page.keyboard.press('Escape');
    await delay(500);

    // 3. 特性等级管理 - 新增弹窗
    console.log('Screenshot: 特性等级管理-新增弹窗');
    await page.goto('http://47.105.59.151:8928/lab/severity-level');
    await delay(3000);
    await page.locator('button').filter({ hasText: /新增特性等级/ }).first().click();
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-08-特性等级管理-新增弹窗.png'));
    await page.keyboard.press('Escape');
    await delay(500);

    // 4. 公式维护 - 公式构建器
    console.log('Screenshot: 公式维护-公式构建器');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data-formula');
    await delay(3000);
    // 找到第一个有"编辑公式"按钮的行并点击
    const editButtons = await page.locator('button:has-text("编辑公式")').all();
    if (editButtons.length > 0) {
      await editButtons[0].click();
      await delay(3000);
      await screenshot(page, path.join(baseMgmtDir, '03-19-公式维护-公式构建器.png'));
      await page.keyboard.press('Escape');
      await delay(500);
    }

    // 5. 判定等级 - 新增弹窗
    console.log('Screenshot: 判定等级-新增弹窗');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data-judgment-level');
    await delay(3000);
    await page.locator('button').filter({ hasText: /新增等级/ }).first().click();
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-21-判定等级-新增弹窗.png'));
    await page.keyboard.press('Escape');
    await delay(500);

    // 6. 原始数据管理 - 其他页签
    console.log('Screenshot: 原始数据管理-其他页签');
    await page.goto('http://47.105.59.151:8928/lab/rawData');
    await delay(3000);
    
    // 原始数据页签
    await page.click('text=原始数据');
    await delay(2000);
    await screenshot(page, path.join(dataMgmtDir, '04-02-原始数据管理-原始数据.png'));
    
    // 导入与日志页签
    await page.click('text=导入与日志');
    await delay(2000);
    await screenshot(page, path.join(dataMgmtDir, '04-03-原始数据管理-导入与日志.png'));

    console.log('Modal screenshots completed!');

  } catch (error) {
    console.error('Error:', error);
  } finally {
    await browser.close();
  }
})();
