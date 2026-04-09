const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const baseDir = path.join(__dirname, '../docs/screenshots');
const baseMgmtDir = path.join(baseDir, '基础管理');
const dataMgmtDir = path.join(baseDir, '数据管理');

[baseMgmtDir, dataMgmtDir].forEach(dir => {
  if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
});

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
    console.log('\n[登录] 访问系统...');
    await page.goto('http://47.105.59.151:8928/login', { waitUntil: 'networkidle' });
    await delay(2000);
    await page.locator('input').nth(0).fill('admin');
    await page.locator('input').nth(1).fill('Lm@2025');
    await page.locator('button').first().click();
    await delay(8000);

    // 1. 系统界面概览
    console.log('[1] 01-系统界面概览.png');
    await screenshot(page, path.join(baseDir, '01-系统界面概览.png'));

    // 2. 产品规格定义-新增弹窗
    console.log('[2] 03-02-产品规格定义-新增弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/product', { waitUntil: 'networkidle' });
    await delay(3000);
    const btns = await page.locator('button').all();
    for (const btn of btns) {
      const text = await btn.textContent();
      if (text && text.includes('新增')) {
        await btn.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-02-产品规格定义-新增弹窗.png'));
    await page.goto('http://47.105.59.151:8928/lab/product', { waitUntil: 'networkidle' });
    await delay(2000);

    // 3. 单位管理-单位定义页签
    console.log('[3] 03-05-单位管理-单位定义.png');
    await page.goto('http://47.105.59.151:8928/lab/unit', { waitUntil: 'networkidle' });
    await delay(3000);
    const tabs = await page.locator('.ant-tabs-tab').all();
    for (const tab of tabs) {
      const text = await tab.textContent();
      if (text && text.includes('单位定义')) {
        await tab.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-05-单位管理-单位定义.png'));

    // 4. 单位管理-新增单位弹窗
    console.log('[4] 03-06-单位管理-新增单位弹窗.png');
    const unitBtns = await page.locator('button').all();
    for (const btn of unitBtns) {
      const text = await btn.textContent();
      if (text && text.includes('新增单位') && !text.includes('维度')) {
        await btn.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-06-单位管理-新增单位弹窗.png'));

    // 5. 单位管理-新增维度弹窗
    console.log('[5] 03-04-单位管理-新增维度弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/unit', { waitUntil: 'networkidle' });
    await delay(3000);
    const catBtns = await page.locator('button').all();
    for (const btn of catBtns) {
      const text = await btn.textContent();
      if (text && text.includes('新增单位维度')) {
        await btn.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-04-单位管理-新增维度弹窗.png'));

    // 6. 特性等级管理-新增弹窗
    console.log('[6] 03-08-特性等级管理-新增弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/severity-level', { waitUntil: 'networkidle' });
    await delay(3000);
    const levelBtns = await page.locator('button').all();
    for (const btn of levelBtns) {
      const text = await btn.textContent();
      if (text && text.includes('新增特性等级')) {
        await btn.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-08-特性等级管理-新增弹窗.png'));

    // 7. 外观特性大类管理-新增弹窗
    console.log('[7] 03-10-外观特性大类管理-新增弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/appearanceCategory', { waitUntil: 'networkidle' });
    await delay(3000);
    const catBtns2 = await page.locator('button').all();
    for (const btn of catBtns2) {
      const text = await btn.textContent();
      if (text && text.includes('新增大类')) {
        await btn.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-10-外观特性大类管理-新增弹窗.png'));

    // 8. 外观特性管理-新增弹窗
    console.log('[8] 03-12-外观特性管理-新增弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/appearance', { waitUntil: 'networkidle' });
    await delay(3000);
    const featureBtns = await page.locator('button').all();
    for (const btn of featureBtns) {
      const text = await btn.textContent();
      if (text && text.includes('新增特性')) {
        await btn.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-12-外观特性管理-新增弹窗.png'));

    // 9. 外观特性管理-语义匹配测试
    console.log('[9] 03-13-外观特性管理-语义匹配测试.png');
    await page.goto('http://47.105.59.151:8928/lab/appearance', { waitUntil: 'networkidle' });
    await delay(3000);
    const testBtn = await page.locator('button:has-text("语义匹配测试")').first();
    if (testBtn) {
      await testBtn.click();
      await delay(2000);
      const inputs = await page.locator('input').all();
      for (const input of inputs) {
        const placeholder = await input.getAttribute('placeholder');
        if (placeholder && placeholder.includes('测试')) {
          await input.fill('微脆微划');
          break;
        }
      }
      await delay(500);
      const matchBtns = await page.locator('button').all();
      for (const btn of matchBtns) {
        const text = await btn.textContent();
        if (text && text.includes('匹配')) {
          await btn.click();
          break;
        }
      }
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-13-外观特性管理-语义匹配测试.png'));
    }

    // 10. 公共维度管理-新增弹窗
    console.log('[10] 03-15-公共维度管理-新增弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/publicDimension', { waitUntil: 'networkidle' });
    await delay(3000);
    const dimBtns = await page.locator('button').all();
    for (const btn of dimBtns) {
      const text = await btn.textContent();
      if (text && text.includes('新增公共维度')) {
        await btn.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-15-公共维度管理-新增弹窗.png'));

    // 11. 公共维度管理-版本管理
    console.log('[11] 03-16-公共维度管理-版本管理.png');
    await page.goto('http://47.105.59.151:8928/lab/publicDimension', { waitUntil: 'networkidle' });
    await delay(3000);
    // 点击第一个卡片的版本按钮
    const versionBtns = await page.locator('button:has-text("版本")').all();
    if (versionBtns.length > 0) {
      await versionBtns[0].click();
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-16-公共维度管理-版本管理.png'));
    }

    // 12. 公式维护-新增弹窗
    console.log('[12] 03-18-公式维护-新增弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data-formula', { waitUntil: 'networkidle' });
    await delay(3000);
    const formulaBtns = await page.locator('button').all();
    for (const btn of formulaBtns) {
      const text = await btn.textContent();
      if (text && text.includes('新增公式')) {
        await btn.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-18-公式维护-新增弹窗.png'));

    // 13. 公式维护-公式构建器
    console.log('[13] 03-19-公式维护-公式构建器.png');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data-formula', { waitUntil: 'networkidle' });
    await delay(3000);
    const editBtns = await page.locator('button:has-text("编辑公式")').all();
    if (editBtns.length > 0) {
      await editBtns[0].click();
      await delay(3000);
      await screenshot(page, path.join(baseMgmtDir, '03-19-公式维护-公式构建器.png'));
    }

    // 14. 判定等级-新增弹窗
    console.log('[14] 03-21-判定等级-新增弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data-judgment-level', { waitUntil: 'networkidle' });
    await delay(3000);
    const judgeBtns = await page.locator('button').all();
    for (const btn of judgeBtns) {
      const text = await btn.textContent();
      if (text && text.includes('新增等级')) {
        await btn.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-21-判定等级-新增弹窗.png'));

    // 15. 判定等级-条件编辑器
    console.log('[15] 03-22-判定等级-条件编辑器.png');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data-judgment-level', { waitUntil: 'networkidle' });
    await delay(3000);
    const conditionBtns = await page.locator('button:has-text("条件")').all();
    if (conditionBtns.length > 0) {
      await conditionBtns[0].click();
      await delay(3000);
      await screenshot(page, path.join(baseMgmtDir, '03-22-判定等级-条件编辑器.png'));
    }

    // 16. 判定等级-批量复制弹窗
    console.log('[16] 03-23-判定等级-批量复制弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data-judgment-level', { waitUntil: 'networkidle' });
    await delay(3000);
    const copyBtns = await page.locator('button:has-text("批量复制")').all();
    if (copyBtns.length > 0) {
      await copyBtns[0].click();
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-23-判定等级-批量复制弹窗.png'));
    }

    // 17. 指标列表-新增弹窗
    console.log('[17] 03-25-指标列表-新增弹窗.png');
    await page.goto('http://47.105.59.151:8928/lab/report-config', { waitUntil: 'networkidle' });
    await delay(3000);
    const configBtns = await page.locator('button').all();
    for (const btn of configBtns) {
      const text = await btn.textContent();
      if (text && text.includes('新增配置')) {
        await btn.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(baseMgmtDir, '03-25-指标列表-新增弹窗.png'));

    // 18. 原始数据管理-原始数据页签
    console.log('[18] 04-02-原始数据管理-原始数据.png');
    await page.goto('http://47.105.59.151:8928/lab/rawData', { waitUntil: 'networkidle' });
    await delay(3000);
    const rawTabs = await page.locator('.ant-tabs-tab').all();
    for (const tab of rawTabs) {
      const text = await tab.textContent();
      if (text && text.includes('原始数据')) {
        await tab.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(dataMgmtDir, '04-02-原始数据管理-原始数据.png'));

    // 19. 原始数据管理-导入与日志页签
    console.log('[19] 04-03-原始数据管理-导入与日志.png');
    await page.goto('http://47.105.59.151:8928/lab/rawData', { waitUntil: 'networkidle' });
    await delay(3000);
    const importTabs = await page.locator('.ant-tabs-tab').all();
    for (const tab of importTabs) {
      const text = await tab.textContent();
      if (text && text.includes('导入与日志')) {
        await tab.click();
        break;
      }
    }
    await delay(2000);
    await screenshot(page, path.join(dataMgmtDir, '04-03-原始数据管理-导入与日志.png'));

    console.log('\n========================================');
    console.log('所有缺失截图补充完成！');
    console.log('========================================');

  } catch (error) {
    console.error('\n✗ 错误:', error.message);
  } finally {
    await browser.close();
  }
})();
