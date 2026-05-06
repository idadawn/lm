import { test, expect } from "@playwright/test";

test.describe("知识图谱页面", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/kg");
  });

  test("页面应该正确加载", async ({ page }) => {
    // 等待页面加载完成
    await page.waitForLoadState("networkidle");

    // 检查标题
    await expect(page.locator("h1")).toContainText("知识图谱浏览器");

    // 检查导航链接
    await expect(page.locator('a[href="/"]')).toContainText("聊天");
    await expect(page.locator('a[href="/kg"]')).toContainText("知识图谱");
  });

  test("应该显示知识图谱健康状态", async ({ page }) => {
    await page.waitForLoadState("networkidle");

    // 等待健康状态加载
    await page.waitForSelector("text=/就绪|未初始化/", { timeout: 10000 });

    // 检查健康状态显示
    const healthStatus = page.locator("p").filter({ hasText: /就绪|未初始化/ });
    await expect(healthStatus).toBeVisible();
  });

  test("应该显示所有标签页", async ({ page }) => {
    await page.waitForLoadState("networkidle");

    // 检查标签页导航
    const tabs = ["概览", "产品规格", "指标公式", "判定规则", "报表配置"];
    for (const tab of tabs) {
      await expect(page.getByRole("button", { name: tab })).toBeVisible();
    }
  });

  test("概览标签页应该显示快捷卡片", async ({ page }) => {
    await page.waitForLoadState("networkidle");

    // 点击概览标签
    await page.getByRole("button", { name: "概览" }).click();

    // 等待内容加载
    await page.waitForTimeout(1000);

    // 检查快捷卡片
    await expect(page.locator("text=产品规格")).toBeVisible();
    await expect(page.locator("text=指标公式")).toBeVisible();
    await expect(page.locator("text=判定规则")).toBeVisible();
  });

  test("应该能够切换标签页", async ({ page }) => {
    await page.waitForLoadState("networkidle");

    // 切换到产品规格标签
    await page.getByRole("button", { name: "产品规格" }).click();
    await page.waitForTimeout(1000);
    await expect(page.locator("text=产品规格")).toBeVisible();

    // 切换到指标公式标签
    await page.getByRole("button", { name: "指标公式" }).click();
    await page.waitForTimeout(1000);

    // 检查是否有搜索框
    const searchInput = page.locator('input[placeholder*="搜索"]');
    await expect(searchInput).toBeVisible();
  });

  test("应该有刷新图谱按钮", async ({ page }) => {
    await page.waitForLoadState("networkidle");

    const refreshButton = page.getByRole("button", { name: "刷新图谱" });
    await expect(refreshButton).toBeVisible();
  });

  test("未初始化时应该显示初始化按钮", async ({ page }) => {
    await page.waitForLoadState("networkidle");

    // 如果健康状态显示未初始化，应该有初始化按钮
    const notReadyText = page.locator("text=未初始化");
    if (await notReadyText.isVisible()) {
      const initButton = page.getByRole("button", { name: "初始化知识图谱" });
      await expect(initButton).toBeVisible();
    }
  });

  test("页面应该响应式显示", async ({ page }) => {
    await page.waitForLoadState("networkidle");

    // 测试桌面视图
    await page.setViewportSize({ width: 1280, height: 720 });
    await page.waitForTimeout(500);

    // 测试移动端视图
    await page.setViewportSize({ width: 375, height: 667 });
    await page.waitForTimeout(500);

    // 检查导航栏仍然可见
    await expect(page.locator("h1")).toBeVisible();
  });

  test("应该有使用提示", async ({ page }) => {
    await page.waitForLoadState("networkidle");

    // 点击概览标签
    await page.getByRole("button", { name: "概览" }).click();
    await page.waitForTimeout(1000);

    // 检查使用提示
    await expect(page.locator("text=使用提示")).toBeVisible();
    await expect(
      page.locator("text=点击上方标签页切换不同数据视图"),
    ).toBeVisible();
  });
});

test.describe("知识图谱 API 集成测试", () => {
  test("API 健康检查应该返回正确格式", async ({ request }) => {
    const response = await request.get(
      "http://localhost:8000/api/v1/kg/health",
    );
    expect(response.status()).toBe(200);

    const data = await response.json();
    expect(data).toHaveProperty("ready");
    expect(data).toHaveProperty("backend");
  });

  test("API 应该返回指标列表", async ({ request }) => {
    const response = await request.get(
      "http://localhost:8000/api/v1/kg/metrics",
    );
    expect(response.status()).toBe(200);

    const data = await response.json();
    expect(Array.isArray(data)).toBe(true);

    if (data.length > 0) {
      expect(data[0]).toHaveProperty("name");
      expect(data[0]).toHaveProperty("columnName");
      expect(data[0]).toHaveProperty("unit");
    }
  });

  test("API 应该返回产品规格列表", async ({ request }) => {
    const response = await request.get("http://localhost:8000/api/v1/kg/specs");
    expect(response.status()).toBe(200);

    const data = await response.json();
    expect(Array.isArray(data)).toBe(true);
  });

  test("API 应该返回判定规则", async ({ request }) => {
    const response = await request.get(
      "http://localhost:8000/api/v1/kg/specs/120/rules",
    );
    expect(response.status()).toBe(200);

    const data = await response.json();
    expect(Array.isArray(data)).toBe(true);

    if (data.length > 0) {
      expect(data[0]).toHaveProperty("name");
      expect(data[0]).toHaveProperty("formulaId");
      expect(data[0]).toHaveProperty("priority");
    }
  });

  test("API 应该支持刷新知识图谱", async ({ request }) => {
    const response = await request.post(
      "http://localhost:8000/api/v1/kg/refresh",
    );
    expect(response.status()).toBe(200);

    const data = await response.json();
    expect(data).toHaveProperty("message");
  });
});
