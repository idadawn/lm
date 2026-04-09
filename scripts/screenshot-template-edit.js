const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const SCREENSHOTS_DIR = './docs/screenshots';
const BASE_URL = 'http://47.105.59.151:8928';

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1920, height: 1080 } });
  const page = await context.newPage();

  // 登录
  console.log('正在登录...');
  await page.goto(`${BASE_URL}/login`, { waitUntil: 'domcontentloaded' });
  await page.waitForTimeout(2000);
  
  const usernameInput = await page.$('input[type="text"]');
  const passwordInput = await page.$('input[type="password"]');
  if (usernameInput) await usernameInput.fill('admin');
  if (passwordInput) await passwordInput.fill('Lm@2025');
  
  const submitBtn = await page.$('button[type="submit"]') || await page.$('button');
  if (submitBtn) await submitBtn.click();
  
  await page.waitForURL(/\/(dashboard|home|monthly-dashboard|lab)/, { timeout: 30000 });
  await page.waitForTimeout(2000);

  // 进入导入模板页面
  await page.goto(`${BASE_URL}/lab/template`, { waitUntil: 'networkidle', timeout: 30000 });
  await page.waitForTimeout(3000);

  // 截图1: 编辑弹窗 - 点击第一个卡片的编辑按钮
  console.log('\n正在截取: Excel导入模板-编辑弹窗');
  try {
    // 悬停在第一个卡片上（磁性数据导入模板）
    await page.mouse.move(450, 330);
    await page.waitForTimeout(800);
    
    // 点击编辑按钮（右上角）
    await page.mouse.click(825, 300);
    await page.waitForTimeout(1500);
    
    await page.screenshot({ 
      path: path.join(SCREENSHOTS_DIR, '基础管理', '03-27-Excel导入模板-编辑弹窗.png')
    });
    console.log('  ✓ 编辑弹窗已保存');
    
    // 截图2: 字段映射表格 - 滚动到表格区域
    console.log('\n正在截取: Excel导入模板-字段映射');
    
    await page.evaluate(() => {
      const tables = document.querySelectorAll('table');
      for (let table of tables) {
        if (table.textContent.includes('字段名称')) {
          table.scrollIntoView({ behavior: 'instant', block: 'start' });
          return true;
        }
      }
      // 如果找不到表格，滚动一屏
      window.scrollBy(0, 400);
      return false;
    });
    await page.waitForTimeout(500);
    
    await page.screenshot({ 
      path: path.join(SCREENSHOTS_DIR, '基础管理', '03-28-Excel导入模板-字段映射.png')
    });
    console.log('  ✓ 字段映射已保存');
    
  } catch (err) {
    console.log(`  ✗ 失败: ${err.message}`);
  }

  await browser.close();
  console.log('\n截图完成！');
}

main().catch(console.error);
