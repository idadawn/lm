"use client";

import { useEffect, useState } from "react";
import type {
  ProductSpec,
  MetricFormula,
  JudgmentRule,
  ProductSpecDetail,
  FirstInspectionConfig,
  AppearanceFeature,
  AppearanceFeatureCategory,
  AppearanceFeatureLevel,
  ReportConfig,
} from "@nlq-agent/shared-types";

// ============================================================================
// API 客户端
// ============================================================================

async function fetchAPI<T>(
  endpoint: string,
  options?: RequestInit,
): Promise<T> {
  const response = await fetch(`/api/kg${endpoint}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...options?.headers,
    },
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ detail: response.statusText }));
    throw new Error(error.detail || "API request failed");
  }

  return response.json();
}

// ============================================================================
// 知识图谱展示页面
// ============================================================================

export default function KnowledgeGraphPage() {
  const [health, setHealth] = useState<{
    ready: boolean;
    enabled?: boolean;
    backend: string;
    message?: string;
  } | null>(null);
  const [activeTab, setActiveTab] = useState<
    | "overview"
    | "specs"
    | "metrics"
    | "rules"
    | "config"
    | "appearance"
    | "report"
  >("overview");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    checkHealth();
  }, []);

  async function checkHealth() {
    try {
      const result = await fetchAPI<{
        ready: boolean;
        enabled?: boolean;
        backend: string;
        message?: string;
      }>(
        "/health",
      );
      setHealth(result);
    } catch (error) {
      console.error("Failed to check health:", error);
      setHealth({ ready: false, backend: "unknown" });
    } finally {
      setLoading(false);
    }
  }

  async function handleRefresh() {
    setLoading(true);
    try {
      const endpoint = health?.ready ? "/refresh" : "/init";
      await fetchAPI<{ message: string }>(endpoint, { method: "POST" });
      await checkHealth();
      alert(health?.ready ? "知识图谱刷新成功" : "知识图谱初始化成功");
    } catch (error) {
      alert(
        `${health?.ready ? "刷新" : "初始化"}失败: ${(error as Error).message}`,
      );
    } finally {
      setLoading(false);
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4" />
          <p className="text-gray-600">加载知识图谱...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      {/* 顶部导航 */}
      <header className="bg-white dark:bg-gray-800 border-b px-4 py-3">
        <div className="max-w-7xl mx-auto flex items-center justify-between">
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-3">
              <div className="w-8 h-8 bg-purple-600 rounded-lg flex items-center justify-center text-white font-bold">
                KG
              </div>
              <div>
                <h1 className="text-lg font-semibold">知识图谱浏览器</h1>
                <p className="text-xs text-gray-500">
                  {health?.ready ? (
                    <span className="text-green-600">
                      ● 就绪 ({health.backend})
                    </span>
                  ) : (
                    <span className="text-red-600">
                      ● 未初始化
                      {health?.enabled === false ? "（配置未启用）" : ""}
                    </span>
                  )}
                </p>
              </div>
            </div>
            <nav className="flex gap-2">
              <a
                href="/"
                className="text-sm text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-gray-100"
              >
                聊天
              </a>
              <a
                href="/dashboard"
                className="text-sm text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-gray-100"
              >
                指标看板
              </a>
              <a
                href="/kg"
                className="text-sm text-purple-600 hover:text-purple-700 font-medium"
              >
                知识图谱
              </a>
            </nav>
          </div>
          <button
            onClick={handleRefresh}
            disabled={!health?.ready || loading}
            className="px-4 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors"
          >
            刷新图谱
          </button>
        </div>
      </header>

      {/* 标签页导航 */}
      <div className="max-w-7xl mx-auto px-4 pt-6">
        <div className="bg-white dark:bg-gray-800 rounded-lg border">
          <div className="border-b">
            <nav className="flex gap-1 px-2">
              {[
                { id: "overview", label: "概览" },
                { id: "specs", label: "产品规格" },
                { id: "metrics", label: "指标公式" },
                { id: "rules", label: "判定规则" },
                { id: "appearance", label: "外观特性" },
                { id: "report", label: "报表统计" },
                { id: "config", label: "系统配置" },
              ].map((tab) => (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id as any)}
                  className={`px-4 py-3 text-sm font-medium transition-colors ${
                    activeTab === tab.id
                      ? "text-purple-600 border-b-2 border-purple-600"
                      : "text-gray-600 hover:text-gray-900 dark:hover:text-gray-300"
                  }`}
                >
                  {tab.label}
                </button>
              ))}
            </nav>
          </div>

          {/* 内容区域 */}
          <div className="p-6">
            {!health?.ready ? (
              <div className="text-center py-12">
                <p className="text-gray-600 mb-4">知识图谱未初始化</p>
                {health?.message && (
                  <p className="text-sm text-gray-500 mb-4">{health.message}</p>
                )}
                <button
                  onClick={handleRefresh}
                  className="px-6 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700"
                >
                  初始化知识图谱
                </button>
              </div>
            ) : (
              <>
                {activeTab === "overview" && <OverviewTab />}
                {activeTab === "specs" && <SpecsTab />}
                {activeTab === "metrics" && <MetricsTab />}
                {activeTab === "rules" && <RulesTab />}
                {activeTab === "appearance" && <AppearanceTab />}
                {activeTab === "report" && <ReportTab />}
                {activeTab === "config" && <ConfigTab />}
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

// ============================================================================
// 概览标签页
// ============================================================================

function OverviewTab() {
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold mb-2">知识图谱概览</h2>
        <p className="text-gray-600">查看系统中的产品规格、指标和判定规则</p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <QuickStatCard
          title="产品规格"
          description="查看所有产品规格及其属性"
          icon="📦"
          link="#specs"
        />
        <QuickStatCard
          title="指标公式"
          description="查看所有指标的计算公式"
          icon="📊"
          link="#metrics"
        />
        <QuickStatCard
          title="判定规则"
          description="查看质量判定规则"
          icon="✓"
          link="#rules"
        />
      </div>

      <div className="bg-blue-50 dark:bg-blue-900/20 border border-blue-200 dark:border-blue-800 rounded-lg p-4">
        <h3 className="font-semibold text-blue-900 dark:text-blue-100 mb-2">
          💡 使用提示
        </h3>
        <ul className="text-sm text-blue-800 dark:text-blue-200 space-y-1">
          <li>• 点击上方标签页切换不同数据视图</li>
          <li>• 在搜索框中输入关键词快速查找</li>
          <li>• 点击"刷新图谱"按钮从数据库重新加载最新数据</li>
        </ul>
      </div>
    </div>
  );
}

function QuickStatCard({
  title,
  description,
  icon,
  link,
}: {
  title: string;
  description: string;
  icon: string;
  link: string;
}) {
  return (
    <a
      href={link}
      className="block bg-gray-50 dark:bg-gray-700 rounded-lg p-6 hover:shadow-md transition-shadow"
    >
      <div className="text-3xl mb-2">{icon}</div>
      <h3 className="font-semibold text-lg mb-1">{title}</h3>
      <p className="text-sm text-gray-600 dark:text-gray-400">{description}</p>
    </a>
  );
}

// ============================================================================
// 产品规格标签页
// ============================================================================

function SpecsTab() {
  const [specs, setSpecs] = useState<ProductSpec[]>([]);
  const [selectedSpec, setSelectedSpec] = useState<string | null>(null);
  const [specDetail, setSpecDetail] = useState<ProductSpecDetail | null>(null);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");

  useEffect(() => {
    loadSpecs();
  }, []);

  async function loadSpecs() {
    setLoading(true);
    try {
      const data = await fetchAPI<ProductSpec[]>("/specs");
      setSpecs(data);
    } catch (error) {
      console.error("Failed to load specs:", error);
      alert("加载产品规格失败");
    } finally {
      setLoading(false);
    }
  }

  async function loadSpecDetail(specCode: string) {
    setSelectedSpec(specCode);
    try {
      const data = await fetchAPI<ProductSpecDetail>(`/specs/${specCode}`);
      setSpecDetail(data);
    } catch (error) {
      console.error("Failed to load spec detail:", error);
      alert("加载规格详情失败");
    }
  }

  const filteredSpecs = specs.filter(
    (spec) =>
      spec.code.toLowerCase().includes(searchQuery.toLowerCase()) ||
      spec.name.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">产品规格</h2>
        <input
          type="text"
          placeholder="搜索规格代码或名称..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
        />
      </div>

      {loading ? (
        <div className="text-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600 mx-auto" />
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* 规格列表 */}
          <div className="lg:col-span-1 space-y-2 max-h-96 overflow-y-auto">
            {filteredSpecs.map((spec) => (
              <button
                key={spec.id}
                onClick={() => loadSpecDetail(spec.code)}
                className={`w-full text-left px-4 py-3 rounded-lg border transition-colors ${
                  selectedSpec === spec.code
                    ? "bg-purple-100 dark:bg-purple-900/30 border-purple-500"
                    : "bg-white dark:bg-gray-800 hover:bg-gray-50 dark:hover:bg-gray-700"
                }`}
              >
                <div className="font-medium">{spec.code}</div>
                <div className="text-sm text-gray-600 dark:text-gray-400 truncate">
                  {spec.name}
                </div>
              </button>
            ))}
          </div>

          {/* 规格详情 */}
          <div className="lg:col-span-2">
            {specDetail ? (
              <div className="bg-white dark:bg-gray-800 rounded-lg border p-6">
                <h3 className="text-lg font-semibold mb-4">
                  规格代码: {specDetail.code}
                </h3>

                {/* 属性 */}
                <div className="mb-6">
                  <h4 className="font-medium mb-2">扩展属性</h4>
                  {specDetail.attributes.length > 0 ? (
                    <div className="grid grid-cols-2 gap-2">
                      {specDetail.attributes.map((attr, idx) => (
                        <div
                          key={idx}
                          className="bg-gray-50 dark:bg-gray-700 rounded px-3 py-2"
                        >
                          <div className="text-xs text-gray-600 dark:text-gray-400">
                            {attr.name}
                          </div>
                          <div className="font-medium">{attr.value}</div>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-sm text-gray-500">无扩展属性</p>
                  )}
                </div>

                {/* 判定类型 */}
                <div>
                  <h4 className="font-medium mb-2">判定类型</h4>
                  {specDetail.judgment_types.length > 0 ? (
                    <div className="flex flex-wrap gap-2">
                      {specDetail.judgment_types.map((jt) => (
                        <span
                          key={jt.formula_id}
                          className="px-3 py-1 bg-purple-100 dark:bg-purple-900/30 text-purple-700 dark:text-purple-300 rounded-full text-sm"
                        >
                          {jt.name} ({jt.rule_count}条规则)
                        </span>
                      ))}
                    </div>
                  ) : (
                    <p className="text-sm text-gray-500">无判定类型</p>
                  )}
                </div>
              </div>
            ) : (
              <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-8 text-center text-gray-500">
                选择一个产品规格查看详情
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

// ============================================================================
// 指标公式标签页
// ============================================================================

function MetricsTab() {
  const [metrics, setMetrics] = useState<MetricFormula[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");

  useEffect(() => {
    loadMetrics();
  }, []);

  async function loadMetrics() {
    setLoading(true);
    try {
      const data = await fetchAPI<MetricFormula[]>("/metrics");
      setMetrics(data);
    } catch (error) {
      console.error("Failed to load metrics:", error);
      alert("加载指标公式失败");
    } finally {
      setLoading(false);
    }
  }

  const filteredMetrics = metrics.filter(
    (metric) =>
      metric.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      metric.columnName.toLowerCase().includes(searchQuery.toLowerCase()),
  );

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">指标公式</h2>
        <input
          type="text"
          placeholder="搜索指标名称或列名..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
        />
      </div>

      {loading ? (
        <div className="text-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600 mx-auto" />
        </div>
      ) : (
        <div className="bg-white dark:bg-gray-800 rounded-lg border overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-medium">
                  指标名称
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium">
                  列名
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium">
                  公式
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium">
                  单位
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium">
                  类型
                </th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {filteredMetrics.map((metric) => (
                <tr
                  key={metric.id}
                  className="hover:bg-gray-50 dark:hover:bg-gray-700"
                >
                  <td className="px-4 py-3 font-medium">{metric.name}</td>
                  <td className="px-4 py-3 text-sm text-gray-600 dark:text-gray-400">
                    {metric.columnName}
                  </td>
                  <td className="px-4 py-3 text-sm font-mono">
                    {metric.formula}
                  </td>
                  <td className="px-4 py-3 text-sm">{metric.unit || "-"}</td>
                  <td className="px-4 py-3 text-sm">
                    <span className="px-2 py-1 bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 rounded text-xs">
                      {metric.formulaType}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

// ============================================================================
// 判定规则标签页
// ============================================================================

function RulesTab() {
  const [rules, setRules] = useState<JudgmentRule[]>([]);
  const [loading, setLoading] = useState(true);
  const [specCode, setSpecCode] = useState("");
  const [formulaId, setFormulaId] = useState("");
  const [specs, setSpecs] = useState<ProductSpec[]>([]);
  const [formulas, setFormulas] = useState<{ id: string; name: string }[]>([]);

  // 加载产品规格和判定公式列表
  useEffect(() => {
    loadSpecsAndFormulas();
  }, []);

  async function loadSpecsAndFormulas() {
    try {
      // 获取产品规格
      const specsData = await fetchAPI<ProductSpec[]>("/specs");
      setSpecs(specsData);

      // 获取判定公式列表（从指标中获取JUDGE类型的）
      const metricsData = await fetchAPI<MetricFormula[]>("/metrics");
      const judgeFormulas = metricsData
        .filter((m) => m.formulaType === "2" || m.formulaType === "JUDGE")
        .map((m) => ({ id: m.columnName || m.name, name: m.name }));
      setFormulas(judgeFormulas);
    } catch (error) {
      console.error("Failed to load specs/formulas:", error);
    }
  }

  async function loadRules() {
    if (!specCode) return;
    setLoading(true);
    try {
      const data = await fetchAPI<JudgmentRule[]>(`/specs/${specCode}/rules`);
      // 如果选择了判定公式，进行筛选
      const filtered = formulaId
        ? data.filter((r) => r.formulaId === formulaId)
        : data;
      setRules(filtered);
    } catch (error) {
      console.error("Failed to load rules:", error);
      alert("加载判定规则失败");
    } finally {
      setLoading(false);
    }
  }

  // 当规格或公式改变时自动加载
  useEffect(() => {
    if (specCode) {
      loadRules();
    }
  }, [specCode, formulaId]);

  return (
    <div className="space-y-6">
      {/* 查询区域 */}
      <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
        <h3 className="font-medium mb-3">查询判定规则</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div>
            <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">
              产品规格
            </label>
            <select
              value={specCode}
              onChange={(e) => setSpecCode(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
            >
              <option value="">选择产品规格</option>
              {specs.map((spec) => (
                <option key={spec.code} value={spec.code}>
                  {spec.code} - {spec.name}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm text-gray-600 dark:text-gray-400 mb-1">
              判定公式
            </label>
            <select
              value={formulaId}
              onChange={(e) => setFormulaId(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
            >
              <option value="">全部判定公式</option>
              {formulas.map((f) => (
                <option key={f.id} value={f.id}>
                  {f.name}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      {/* 规则列表 */}
      {loading ? (
        <div className="text-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600 mx-auto" />
        </div>
      ) : (
        <>
          {rules.length > 0 ? (
            <div>
              <h3 className="font-medium mb-3">
                {specCode} 的判定规则
                {formulaId &&
                  ` (${formulas.find((f) => f.id === formulaId)?.name || formulaId})`}
                <span className="text-sm text-gray-500 ml-2">
                  ({rules.length} 条)
                </span>
              </h3>
              <RulesList rules={rules} />
            </div>
          ) : specCode ? (
            <div className="text-center py-8 text-gray-500">
              该产品规格暂无判定规则
            </div>
          ) : (
            <div className="text-center py-8 text-gray-500">
              请选择产品规格查看判定规则
            </div>
          )}
        </>
      )}
    </div>
  );
}

function RulesList({ rules }: { rules: JudgmentRule[] }) {
  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg border overflow-hidden">
      <table className="w-full">
        <thead className="bg-gray-50 dark:bg-gray-700">
          <tr>
            <th className="px-4 py-3 text-left text-sm font-medium">
              规则名称
            </th>
            <th className="px-4 py-3 text-left text-sm font-medium">公式ID</th>
            <th className="px-4 py-3 text-left text-sm font-medium">优先级</th>
            <th className="px-4 py-3 text-left text-sm font-medium">
              质量状态
            </th>
            <th className="px-4 py-3 text-left text-sm font-medium">颜色</th>
            <th className="px-4 py-3 text-left text-sm font-medium">条件</th>
          </tr>
        </thead>
        <tbody className="divide-y">
          {rules.map((rule) => (
            <tr
              key={rule.id}
              className="hover:bg-gray-50 dark:hover:bg-gray-700"
            >
              <td className="px-4 py-3 font-medium">{rule.name}</td>
              <td className="px-4 py-3 text-sm">{rule.formulaId}</td>
              <td className="px-4 py-3 text-sm">{rule.priority}</td>
              <td className="px-4 py-3 text-sm">
                <span
                  className="px-2 py-1 rounded text-xs"
                  style={{
                    backgroundColor: rule.color,
                    color: "#fff",
                  }}
                >
                  {rule.qualityStatus}
                </span>
              </td>
              <td className="px-4 py-3 text-sm">
                <div className="flex items-center gap-2">
                  <div
                    className="w-4 h-4 rounded"
                    style={{ backgroundColor: rule.color }}
                  />
                  <span className="text-xs">{rule.color}</span>
                </div>
              </td>
              <td className="px-4 py-3 text-sm font-mono text-xs">
                {rule.conditionJson?.slice(0, 50)}...
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// ============================================================================
// 报表配置标签页
// ============================================================================

// ============================================================================
// 外观特性标签页
// ============================================================================

function AppearanceTab() {
  const [features, setFeatures] = useState<AppearanceFeature[]>([]);
  const [categories, setCategories] = useState<AppearanceFeatureCategory[]>([]);
  const [levels, setLevels] = useState<AppearanceFeatureLevel[]>([]);
  const [loading, setLoading] = useState(true);
  const [selectedCategory, setSelectedCategory] = useState<string>("all");

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    setLoading(true);
    try {
      const [featuresData, categoriesData, levelsData] = await Promise.all([
        fetchAPI<AppearanceFeature[]>("/appearance-features"),
        fetchAPI<AppearanceFeatureCategory[]>(
          "/appearance-features/categories",
        ),
        fetchAPI<AppearanceFeatureLevel[]>("/appearance-features/levels"),
      ]);
      setFeatures(featuresData);
      setCategories(categoriesData);
      setLevels(levelsData);
    } catch (error) {
      console.error("Failed to load appearance features:", error);
      alert("加载外观特性失败");
    } finally {
      setLoading(false);
    }
  }

  const filteredFeatures =
    selectedCategory === "all"
      ? features
      : features.filter((f) => f.category.id === selectedCategory);

  // 按大类分组
  const groupedFeatures = filteredFeatures.reduce(
    (acc, feature) => {
      const categoryName = feature.category.name;
      if (!acc[categoryName]) {
        acc[categoryName] = [];
      }
      acc[categoryName].push(feature);
      return acc;
    },
    {} as Record<string, AppearanceFeature[]>,
  );

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">外观特性</h2>
        <select
          value={selectedCategory}
          onChange={(e) => setSelectedCategory(e.target.value)}
          className="px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500"
        >
          <option value="all">全部大类</option>
          {categories.map((cat) => (
            <option key={cat.id} value={cat.id}>
              {cat.name}
            </option>
          ))}
        </select>
      </div>

      {/* 等级说明 */}
      <div className="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
        <h3 className="font-medium mb-2">等级说明</h3>
        <div className="flex flex-wrap gap-2">
          {levels.map((level) => (
            <span
              key={level.id}
              className="px-3 py-1 bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 rounded-full text-sm"
            >
              {level.name}
            </span>
          ))}
        </div>
      </div>

      {loading ? (
        <div className="text-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600 mx-auto" />
        </div>
      ) : (
        <div className="space-y-6">
          {Object.entries(groupedFeatures).map(
            ([categoryName, featureList]) => (
              <div
                key={categoryName}
                className="bg-white dark:bg-gray-800 rounded-lg border p-4"
              >
                <h3 className="font-semibold text-lg mb-3">{categoryName}</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-3">
                  {featureList.map((feature) => (
                    <div
                      key={feature.id}
                      className="bg-gray-50 dark:bg-gray-700 rounded-lg p-3"
                    >
                      <div className="flex items-center justify-between mb-2">
                        <span className="font-medium">{feature.name}</span>
                        <span
                          className={`px-2 py-0.5 rounded text-xs ${
                            feature.level.name === "严重" ||
                            feature.level.name === "超级"
                              ? "bg-red-100 dark:bg-red-900/30 text-red-700 dark:text-red-300"
                              : feature.level.name === "中等" ||
                                  feature.level.name === "中度"
                                ? "bg-yellow-100 dark:bg-yellow-900/30 text-yellow-700 dark:text-yellow-300"
                                : "bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300"
                          }`}
                        >
                          {feature.level.name}
                        </span>
                      </div>
                      {feature.keywords && (
                        <div className="text-xs text-gray-500 dark:text-gray-400">
                          关键字: {feature.keywords}
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              </div>
            ),
          )}
        </div>
      )}
    </div>
  );
}

// ============================================================================
// 报表统计标签页
// ============================================================================

function ReportTab() {
  const [configs, setConfigs] = useState<ReportConfig[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadData();
  }, []);

  async function loadData() {
    setLoading(true);
    try {
      // 获取报表配置
      const configsData = await fetchAPI<ReportConfig[]>("/report-configs");
      setConfigs(configsData);
    } catch (error) {
      console.error("Failed to load report configs:", error);
      alert("加载报表配置失败");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center">
        <h2 className="text-xl font-semibold">报表统计配置</h2>
      </div>

      {loading ? (
        <div className="text-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600 mx-auto" />
        </div>
      ) : (
        <div className="bg-white dark:bg-gray-800 rounded-lg border overflow-hidden">
          <table className="w-full">
            <thead className="bg-gray-50 dark:bg-gray-700">
              <tr>
                <th className="px-4 py-3 text-left text-sm font-medium">
                  统计名称
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium">
                  判定公式
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium">
                  包含等级
                </th>
                <th className="px-4 py-3 text-left text-sm font-medium">
                  显示设置
                </th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {configs.map((config) => (
                <tr
                  key={config.id}
                  className="hover:bg-gray-50 dark:hover:bg-gray-700"
                >
                  <td className="px-4 py-3">
                    <div className="font-medium">{config.name}</div>
                    {config.description && (
                      <div className="text-xs text-gray-500">
                        {config.description}
                      </div>
                    )}
                  </td>
                  <td className="px-4 py-3 text-sm">
                    {config.metric?.name || config.formulaId || "-"}
                  </td>
                  <td className="px-4 py-3 text-sm">
                    <div className="flex flex-wrap gap-1">
                      {config.levelNames?.map((level, idx) => (
                        <span
                          key={idx}
                          className="px-2 py-0.5 bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 rounded text-xs"
                        >
                          {level}
                        </span>
                      ))}
                    </div>
                  </td>
                  <td className="px-4 py-3 text-sm">
                    <div className="flex flex-wrap gap-1">
                      {config.isHeader && (
                        <span className="px-2 py-0.5 bg-gray-100 rounded text-xs">
                          表头
                        </span>
                      )}
                      {config.isShowInReport && (
                        <span className="px-2 py-0.5 bg-green-100 text-green-700 rounded text-xs">
                          报表显示
                        </span>
                      )}
                      {config.isShowRatio && (
                        <span className="px-2 py-0.5 bg-purple-100 text-purple-700 rounded text-xs">
                          显示比例
                        </span>
                      )}
                      {config.isPercentage && (
                        <span className="px-2 py-0.5 bg-yellow-100 text-yellow-700 rounded text-xs">
                          百分比
                        </span>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

function ConfigTab() {
  const [config, setConfig] = useState<FirstInspectionConfig | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadConfig();
  }, []);

  async function loadConfig() {
    setLoading(true);
    try {
      const data = await fetchAPI<FirstInspectionConfig>(
        "/first-inspection/config",
      );
      setConfig(data);
    } catch (error) {
      console.error("Failed to load config:", error);
      alert("加载配置失败");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-semibold">一次交检合格率配置</h2>

      {loading ? (
        <div className="text-center py-8">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-purple-600 mx-auto" />
        </div>
      ) : config ? (
        <div className="bg-white dark:bg-gray-800 rounded-lg border p-6">
          <div className="mb-4">
            <h3 className="font-medium mb-2">合格等级</h3>
            <div className="flex flex-wrap gap-2">
              {config.grades.map((grade) => (
                <span
                  key={grade}
                  className="px-3 py-1 bg-green-100 dark:bg-green-900/30 text-green-700 dark:text-green-300 rounded-full"
                >
                  {grade}
                </span>
              ))}
            </div>
          </div>

          {config.description && (
            <div>
              <h3 className="font-medium mb-2">说明</h3>
              <p className="text-gray-600 dark:text-gray-400">
                {config.description}
              </p>
            </div>
          )}
        </div>
      ) : (
        <div className="text-center py-8 text-gray-500">配置未找到</div>
      )}
    </div>
  );
}
