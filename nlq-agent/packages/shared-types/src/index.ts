/**
 * NLQ-Agent 共享类型定义
 *
 * 用于前后端类型共享，确保 API 契约一致性
 */

import type { ReasoningStep } from "./reasoning-protocol";

// ============================================================================
// API 类型定义
// ============================================================================

/** 聊天消息角色 */
export type MessageRole = "user" | "assistant" | "system";

/** 聊天消息 */
export interface ChatMessage {
  role: MessageRole;
  content: string;
}

/** 聊天请求 */
export interface ChatRequest {
  messages: ChatMessage[];
  session_id?: string;
  model_name?: string;
  auth_context?: AuthContext;
}

/** 鉴权上下文 */
export interface AuthContext {
  access_token?: string;
  token_type?: string;
  user_id?: string;
  account?: string;
  tenant_id?: string;
  origin?: "web" | "mobile" | "embedded";
  permissions?: string[];
}

/** 流事件类型 */
export type StreamEventType =
  | "text"
  | "tool_start"
  | "tool_end"
  | "chart"
  | "reasoning_step"
  | "response_metadata"
  | "error"
  | "done";

export interface CalculationExplanation {
  formula_source: string;
  data_fields: string[];
  natural_language: string;
}

export interface GradeJudgmentDetail {
  id?: number;
  name?: string;
  priority?: number;
  quality_status?: string;
  color?: string;
  is_default?: boolean;
  conditions?: Record<string, unknown>[];
}

export interface GradeJudgment {
  available: boolean;
  grade?: string | null;
  quality_status?: string | null;
  color?: string | null;
  metric_value?: number | null;
  matched_rule?: GradeJudgmentDetail | null;
  all_rules: GradeJudgmentDetail[];
  summary: string;
}

/** SSE 流事件 */
export interface StreamEvent {
  type: StreamEventType;
  content?: string;
  tool_name?: string;
  tool_input?: Record<string, unknown>;
  tool_output?: Record<string, unknown>;
  chart_spec?: ChartDescriptor;
  reasoning_step?: ReasoningStep;
  response_payload?: ChatResponse;
  error?: string;
}

/** 聊天响应 */
export interface ChatResponse {
  session_id: string;
  model_name?: string;
  response: string;
  chart_config?: ChartDescriptor;
  intent?: string;
  entities?: Record<string, unknown>;
  context?: Record<string, unknown>;
  calculation_explanation?: CalculationExplanation;
  grade_judgment?: GradeJudgment;
  reasoning_steps?: ReasoningStep[];
}

// ============================================================================
// 图表协议定义
// ============================================================================

/** 图表类型 */
export type ChartType =
  | "line"
  | "bar"
  | "pie"
  | "gauge"
  | "radar"
  | "scatter"
  | "area"
  | "column";

/** 等级阈值 */
export interface GradeThreshold {
  name: string;
  threshold: number;
  color: string;
}

/** 图表数据点 */
export interface ChartDataPoint {
  [key: string]: string | number | undefined;
  date?: string;
  value?: number;
  category?: string;
}

/** 图表标注 */
export interface ChartAnnotation {
  type: "line" | "region" | "text";
  value?: number;
  start?: number | string;
  end?: number | string;
  label?: string;
  color?: string;
}

/** 图表描述符（GPT-Vis 协议） */
export interface ChartDescriptor {
  type: ChartType;
  title: string;
  data: ChartDataPoint[];
  xField?: string;
  yField?: string;
  angleField?: string;
  colorField?: string;
  annotations?: ChartAnnotation[];
  meta?: {
    metricName: string;
    unit: string;
    aggregation?: string;
    gradeThresholds?: GradeThreshold[];
  };
}

// ============================================================================
// 指标类型定义
// ============================================================================

/** 指标定义 */
export interface MetricDefinition {
  id: number;
  name: string;
  columnName: string;
  formula: string;
  unit: string;
  formulaType: "CALC" | "JUDGE" | "NO";
  description?: string;
}

/** 等级条件 */
export interface GradeCondition {
  field: string;
  operator: ">" | ">=" | "<" | "<=" | "==" | "!=";
  value: number;
  logicOperator?: "AND" | "OR";
}

/** 等级规则 */
export interface GradeRule {
  id: number;
  formulaId: number;
  name: string;
  priority: number;
  qualityStatus: string;
  color: string;
  isDefault: boolean;
  conditions: GradeCondition[];
}

/** 查询结果 */
export interface QueryResult {
  metricName: string;
  columnName: string;
  value?: number;
  unit: string;
  aggregation: string;
  grade?: string;
  formulaDescription?: string;
  timeRangeLabel: string;
}

// ============================================================================
// Agent Conversation Timeline 类型（新增）
// ============================================================================

/** Agent 运行状态 */
export type AgentRunStatus =
  | "queued"
  | "thinking"
  | "calling_tool"
  | "answering"
  | "completed"
  | "error"
  | "cancelled";

/** Agent Block 类型 */
export type AgentBlockType =
  | "thinking"
  | "tool_call"
  | "artifact"
  | "final_answer"
  | "error";

/** 工具调用状态 */
export type ToolCallStatus = "running" | "success" | "error";

/** 推理/思考块 */
export interface ThinkingBlock {
  id: string;
  type: "thinking";
  title: string;
  steps: ReasoningStep[];
  collapsed: boolean;
}

/** 工具调用块 */
export interface ToolCallBlock {
  id: string;
  type: "tool_call";
  toolCallId: string;
  name: string;
  displayName?: string;
  status: ToolCallStatus;
  input?: Record<string, unknown>;
  output?: Record<string, unknown>;
  summary?: string;
  startedAt?: number;
  endedAt?: number;
  durationMs?: number;
}

/** 产物/结果块（图表、表格、知识图谱等） */
export interface ArtifactBlock {
  id: string;
  type: "artifact";
  kind: "chart" | "table" | "kg_subgraph" | "sql" | "file";
  title: string;
  payload: unknown;
}

/** 最终回答块 */
export interface FinalAnswerBlock {
  id: string;
  type: "final_answer";
  content: string;
}

/** 错误块 */
export interface ErrorBlock {
  id: string;
  type: "error";
  message: string;
  retryable: boolean;
}

/** Agent Block 联合类型 */
export type AgentBlock =
  | ThinkingBlock
  | ToolCallBlock
  | ArtifactBlock
  | FinalAnswerBlock
  | ErrorBlock;

/** 单轮 Agent 响应（Turn） */
export interface AgentTurn {
  id: string;
  userMessageId: string;
  assistantMessageId: string;
  status: AgentRunStatus;
  createdAt: number;
  updatedAt: number;
  blocks: AgentBlock[];
}

// ============================================================================
// 前端组件类型（兼容旧版）
// ============================================================================

/** 工具调用状态 */
export type ToolState = "loading" | "result" | "error";

/** 工具调用 */
export interface ToolInvocation {
  toolCallId: string;
  toolName: string;
  state: ToolState;
  input?: Record<string, unknown>;
  result?: Record<string, unknown>;
  error?: string;
}

/** AI 消息 */
export interface AIMessage {
  id: string;
  role: MessageRole;
  content: string;
  reasoning?: string;
  toolInvocations?: ToolInvocation[];
  sources?: DataSource[];
  chartConfig?: ChartDescriptor;
}

/** 数据来源 */
export interface DataSource {
  table: string;
  description: string;
  url?: string;
}

// ============================================================================
// 模型配置类型
// ============================================================================

/** 模型提供商 */
export type ModelProvider = "OpenAI" | "Google" | "Anthropic" | "Ollama" | "其他";

/** 可用模型 */
export interface AvailableModel {
  id: string;
  name: string;
  provider: ModelProvider;
  supportsFunctionCalling: boolean;
  description?: string;
}

// ============================================================================
// 知识图谱类型
// ============================================================================

export * from "./dashboard";
export * from "./knowledge-graph";
export * from "./reasoning-protocol";
