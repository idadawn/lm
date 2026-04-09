const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const baseDir = path.join(__dirname, '../docs/screenshots');
const baseMgmtDir = path.join(baseDir, '基础管理');
const dataMgmtDir = path.join(baseDir, '数据管理');
const reportDir = path.join(baseDir, '报表分析');

[baseMgmtDir, dataMgmtDir, reportDir].forEach(dir => {
  if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
});

async function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

async function screenshot(page, filePath) {
  await page.screenshot({ path: filePath, fullPage: false });
  console.log(`✓ ${path.basename(filePath)}`);
}

async function waitForPageLoad(page) {
  await page.waitForLoadState('networkidle', { timeout: 60000 });
  await delay(2000);
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ 
    viewport: { width: 1920, height: 1080 },
    locale: 'zh-CN'
  });
  const page = await context.newPage();

  try {
    // 登录
    console.log('\n[登录] 访问登录页面...');
    await page.goto('http://47.105.59.151:8928/login', { waitUntil: 'networkidle' });
    await delay(2000);
    
    await page.locator('input').nth(0).fill('admin');
    await page.locator('input').nth(1).fill('Lm@2025');
    await delay(500);
    await page.locator('button').first().click();
    await delay(8000);
    
    console.log(`    当前URL: ${page.url()}`);

    // 1. 系统界面概览（使用生产驾驶舱）
    console.log('\n[1] 系统界面概览...');
    await screenshot(page, path.join(baseDir, '01-系统界面概览.png'));

    // 2. 产品规格定义 - 新增弹窗
    console.log('[2] 产品规格定义-新增弹窗...');
    await page.goto('http://47.105.59.151:8928/lab/product', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    const addButtons = await page.locator('button:has-text("新增")').all();
    if (addButtons.length > 0) {
      await addButtons[0].click();
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-02-产品规格定义-新增弹窗.png'));
      await page.keyboard.press('Escape');
      await delay(500);
    }

    // 3. 单位管理 - 单位定义页签 + 新增弹窗
    console.log('[3] 单位管理相关截图...');
    await page.goto('http://47.105.59.151:8928/lab/unit', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    
    // 切换到单位定义页签
    const tabs = await page.locator('.ant-tabs-tab:has-text("单位定义"), .ant-tabs-tab:has-text("定义")').all();
    if (tabs.length > 0) {
      await tabs[0].click();
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-05-单位管理-单位定义.png'));
      
      // 新增单位弹窗
      const addUnitBtn = await page.locator('button:has-text("新增单位")').first();
      if (addUnitBtn) {
        await addUnitBtn.click();
        await delay(2000);
        await screenshot(page, path.join(baseMgmtDir, '03-06-单位管理-新增单位弹窗.png'));
        await page.keyboard.press('Escape');
        await delay(500);
      }
    }
    
    // 先关闭当前弹窗
    await page.keyboard.press('Escape');
    await delay(1000);
    
    // 新增单位维度弹窗
    const addCatBtn = await page.locator('button:has-text("新增单位维度")').first();
    if (addCatBtn) {
      await addCatBtn.click();
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-04-单位管理-新增维度弹窗.png'));
      await page.keyboard.press('Escape');
      await delay(500);
    }

    // 4. 特性等级管理 - 新增弹窗
    console.log('[4] 特性等级管理-新增弹窗...');
    await page.goto('http://47.105.59.151:8928/lab/severity-level', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    const addLevelBtn = await page.locator('button:has-text("新增特性等级")').first();
    if (addLevelBtn) {
      await addLevelBtn.click();
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-08-特性等级管理-新增弹窗.png'));
      await page.keyboard.press('Escape');
      await delay(500);
    }

    // 5. 外观特性大类管理 - 新增弹窗
    console.log('[5] 外观特性大类管理-新增弹窗...');
    await page.goto('http://47.105.59.151:8928/lab/appearanceCategory', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    const addCatBtn2 = await page.locator('button:has-text("新增大类")').first();
    if (addCatBtn2) {
      await addCatBtn2.click();
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-10-外观特性大类管理-新增弹窗.png'));
      await page.keyboard.press('Escape');
      await delay(500);
    }

    // 6. 外观特性管理 - 新增弹窗 + 语义匹配测试
    console.log('[6] 外观特性管理相关截图...');
    await page.goto('http://47.105.59.151:8928/lab/appearance', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    
    // 展开语义匹配测试面板
    const testBtn = await page.locator('button:has-text("语义匹配测试")').first();
    if (testBtn) {
      await testBtn.click();
      await delay(2000);
      // 输入测试文本
      const testInput = await page.locator('input[placeholder*="测试"]').first();
      if (testInput) {
        await testInput.fill('微脆微划');
        await delay(500);
        const matchBtn = await page.locator('button:has-text("匹配")').first();
        if (matchBtn) await matchBtn.click();
        await delay(2000);
      }
      await screenshot(page, path.join(baseMgmtDir, '03-13-外观特性管理-语义匹配测试.png'));
    }
    
    // 新增特性弹窗
    const addFeatureBtn = await page.locator('button:has-text("新增特性")').first();
    if (addFeatureBtn) {
      await addFeatureBtn.click();
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-12-外观特性管理-新增弹窗.png'));
      await page.keyboard.press('Escape');
      await delay(500);
    }

    // 7. 公共维度管理 - 新增弹窗 + 版本管理
    console.log('[7] 公共维度管理相关截图...');
    await page.goto('http://47.105.59.151:8928/lab/publicDimension', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    
    const addDimBtn = await page.locator('button:has-text("新增公共维度")').first();
    if (addDimBtn) {
      await addDimBtn.click();
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-15-公共维度管理-新增弹窗.png'));
      await page.keyboard.press('Escape');
      await delay(500);
    }

    // 8. 公式维护 - 新增弹窗 + 公式构建器
    console.log('[8] 公式维护相关截图...');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data-formula', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    
    // 公式构建器
    const editFormulaBtns = await page.locator('button:has-text("编辑公式")').all();
    if (editFormulaBtns.length > 0) {
      await editFormulaBtns[0].click();
      await delay(3000);
      await screenshot(page, path.join(baseMgmtDir, '03-19-公式维护-公式构建器.png'));
      await page.keyboard.press('Escape');
      await delay(500);
    }

    // 9. 判定等级 - 新增弹窗 + 条件编辑器
    console.log('[9] 判定等级相关截图...');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data-judgment-level', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    
    // 先选择一个判定项目
    const formulaItems = await page.locator('.ant-list-item, [class*="formula-item"]').all();
    if (formulaItems.length > 0) {
      await formulaItems[0].click();
      await delay(2000);
    }
    
    const addLevelBtn2 = await page.locator('button:has-text("新增等级")').first();
    if (addLevelBtn2) {
      await addLevelBtn2.click();
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-21-判定等级-新增弹窗.png'));
      await page.keyboard.press('Escape');
      await delay(500);
    }

    // 10. 指标列表 - 新增弹窗
    console.log('[10] 指标列表-新增弹窗...');
    await page.goto('http://47.105.59.151:8928/lab/report-config', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    const addConfigBtn = await page.locator('button:has-text("新增配置")').first();
    if (addConfigBtn) {
      await addConfigBtn.click();
      await delay(2000);
      await screenshot(page, path.join(baseMgmtDir, '03-25-指标列表-新增弹窗.png'));
      await page.keyboard.press('Escape');
      await delay(500);
    }

    // 11. 原始数据管理 - 其他页签
    console.log('[11] 原始数据管理其他页签...');
    await page.goto('http://47.105.59.151:8928/lab/rawData', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    
    // 原始数据页签
    const rawDataTab = await page.locator('.ant-tabs-tab:has-text("原始数据")').first();
    if (rawDataTab) {
      await rawDataTab.click();
      await delay(2000);
      await screenshot(page, path.join(dataMgmtDir, '04-02-原始数据管理-原始数据.png'));
    }
    
    // 导入与日志页签
    const importTab = await page.locator('.ant-tabs-tab:has-text("导入与日志")').first();
    if (importTab) {
      await importTab.click();
      await delay(2000);
      await screenshot(page, path.join(dataMgmtDir, '04-03-原始数据管理-导入与日志.png'));
    }

    console.log('\n========================================');
    console.log('缺失截图补充完成！');
    console.log('========================================');

  } catch (error) {
    console.error('\n✗ 错误:', error.message);
    await page.screenshot({ path: path.join(baseDir, 'error-missing.png') });
  } finally {
    await browser.close();
  }
})();
