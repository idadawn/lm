"use client";

import { useEffect, useMemo, useRef, useState, useCallback } from "react";
import ReactMarkdown from "react-markdown";
import { ChartRenderer } from "@/components/charts";
import {
  CalculationExplanationCard,
  GradeJudgmentCard,
} from "@/components/cards";
import { KgReasoningChain } from "@/components/chat/KgReasoningChain";
import type {
  AuthContext,
  CalculationExplanation,
  ChatResponse,
  GradeJudgment,
  ReasoningStep,
  StreamEvent,
} from "@nlq-agent/shared-types";

const SUGGESTED_QUESTIONS = [
  "最近7天叠片系数的平均值是多少？",
  "2026年1月甲班的Ps铁损趋势如何？",
  "上个月的厚度极差最大值是多少？",
  "最近三天矫顽力有什么变化？",
];

const MODEL_OPTIONS = [
  { value: "deepseek-chat", label: "DeepSeek Chat" },
  { value: "deepseek-reasoner", label: "DeepSeek Reasoner" },
  { value: "gpt-4o", label: "GPT-4o (LiteLLM only)" },
  { value: "gemini-2.5-flash", label: "Gemini 2.5 Flash (LiteLLM only)" },
];

interface ToolCallItem {
  id: string;
  name: string;
  displayName?: string;
  status: "running" | "completed" | "error";
  input?: Record<string, unknown>;
  output?: Record<string, unknown>;
  summary?: string;
  durationMs?: number;
}

interface ChatMessageItem {
  id: string;
  role: "user" | "assistant";
  content: string;
  reasoningSteps?: ReasoningStep[];
  toolCalls?: ToolCallItem[];
  chartConfig?: import("@nlq-agent/shared-types").ChartDescriptor | null;
  calculationExplanation?: CalculationExplanation | null;
  gradeJudgment?: GradeJudgment | null;
  status?: "running" | "completed" | "error" | "cancelled";
}

type ChatPanelMode = "fullscreen" | "dock";

interface NlqChatPanelProps {
  mode?: ChatPanelMode;
  authContext?: AuthContext;
  title?: string;
  subtitle?: string;
  showNavigation?: boolean;
  launcherLabel?: string;
}

function cn(...values: Array<string | false | null | undefined>) {
  return values.filter(Boolean).join(" ");
}

function isLatin1HeaderValue(value: string): boolean {
  for (let index = 0; index < value.length; index += 1) {
    if (value.charCodeAt(index) > 0xff) {
      return false;
    }
  }
  return true;
}

function assignSafeHeader(
  headers: Record<string, string>,
  key: string,
  value: string | null | undefined,
) {
  if (!value) return;
  if (!isLatin1HeaderValue(value)) return;
  headers[key] = value;
}

export function NlqChatPanel({
  mode = "fullscreen",
  authContext,
  title = "NLQ-Agent",
  subtitle = "工业质量数据智能查询",
  showNavigation = true,
  launcherLabel = "AI问数",
}: NlqChatPanelProps) {
  const [sessionId, setSessionId] = useState<string | null>(null);
  const [selectedModel, setSelectedModel] = useState<string>("deepseek-chat");
  const [messages, setMessages] = useState<ChatMessageItem[]>([]);
  const [input, setInput] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [dockOpen, setDockOpen] = useState(mode === "fullscreen");
  const [runtimeAuthContext, setRuntimeAuthContext] = useState<AuthContext>(
    authContext ?? {},
  );

  // Abort controller for stopping generation
  const abortControllerRef = useRef<AbortController | null>(null);

  useEffect(() => {
    setRuntimeAuthContext(authContext ?? {});
  }, [authContext]);

  // 开发期注入：?token=<JWT>&account=<name> 直接灌入 runtimeAuthContext。
  useEffect(() => {
    if (typeof window === "undefined") return;
    const params = new URLSearchParams(window.location.search);
    const urlToken = params.get("token");
    if (!urlToken) return;
    setRuntimeAuthContext((prev) => ({
      ...prev,
      access_token: urlToken,
      token_type: "Bearer",
      origin: prev.origin ?? "web",
      account: params.get("account") ?? prev.account,
      user_id: params.get("user_id") ?? prev.user_id,
      tenant_id: params.get("tenant_id") ?? prev.tenant_id,
    }));
  }, []);

  useEffect(() => {
    const handleMessage = (event: MessageEvent) => {
      const data = event.data;
      if (
        !data ||
        typeof data !== "object" ||
        data.type !== "NLQ_AUTH_CONTEXT" ||
        !data.payload
      ) {
        return;
      }

      setRuntimeAuthContext((prev) => ({
        ...prev,
        ...(data.payload as AuthContext),
      }));
    };

    window.addEventListener("message", handleMessage);
    return () => window.removeEventListener("message", handleMessage);
  }, []);

  const requestHeaders = useMemo(() => {
    const headers: Record<string, string> = {
      "Content-Type": "application/json",
      "X-Request-Origin": runtimeAuthContext?.origin ?? "web",
    };

    if (runtimeAuthContext?.access_token) {
      headers.Authorization = runtimeAuthContext.token_type
        ? `${runtimeAuthContext.token_type} ${runtimeAuthContext.access_token}`
        : runtimeAuthContext.access_token;
    }
    assignSafeHeader(headers, "X-User-Id", runtimeAuthContext?.user_id);
    assignSafeHeader(headers, "X-User-Account", runtimeAuthContext?.account);
    assignSafeHeader(headers, "X-Tenant-Id", runtimeAuthContext?.tenant_id);
    if (runtimeAuthContext?.permissions?.length) {
      assignSafeHeader(
        headers,
        "X-User-Permissions",
        runtimeAuthContext.permissions.join(","),
      );
    }

    return headers;
  }, [runtimeAuthContext]);

  const stopGeneration = useCallback(() => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      abortControllerRef.current = null;
    }
    setIsLoading(false);
    setMessages((prev) => {
      const last = prev[prev.length - 1];
      if (last && last.role === "assistant" && last.status === "running") {
        const next = prev.slice();
        next[prev.length - 1] = { ...last, status: "cancelled" };
        return next;
      }
      return prev;
    });
  }, []);

  const submitMessage = async (content: string) => {
    const trimmedContent = content.trim();
    if (!trimmedContent || isLoading) return;

    setIsLoading(true);
    setInput("");

    const userMessage: ChatMessageItem = {
      id: crypto.randomUUID(),
      role: "user",
      content: trimmedContent,
    };
    const assistantMessageId = crypto.randomUUID();

    setMessages((prev) => [
      ...prev,
      userMessage,
      {
        id: assistantMessageId,
        role: "assistant",
        content: "",
        reasoningSteps: [],
        toolCalls: [],
        chartConfig: null,
        calculationExplanation: null,
        gradeJudgment: null,
        status: "running",
      },
    ]);

    const controller = new AbortController();
    abortControllerRef.current = controller;

    try {
      const response = await fetch("/api/chat", {
        method: "POST",
        headers: requestHeaders,
        body: JSON.stringify({
          messages: [{ role: "user", content: trimmedContent }],
          session_id: sessionId ?? undefined,
          model_name: undefined,
          auth_context: runtimeAuthContext,
        }),
        signal: controller.signal,
      });

      if (!response.ok || !response.body) {
        const errorText = await response.text();
        throw new Error(errorText || "请求失败");
      }

      const reader = response.body.getReader();
      const decoder = new TextDecoder();
      let buffer = "";

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        buffer += decoder.decode(value, { stream: true });
        const chunks = buffer.split("\n\n");
        buffer = chunks.pop() || "";

        for (const chunk of chunks) {
          const dataLine = chunk
            .split("\n")
            .find((line) => line.startsWith("data: "));

          if (!dataLine) continue;

          const payload = dataLine.slice(6).trim();
          if (payload === "[DONE]") continue;

          let eventData: StreamEvent;
          try {
            eventData = JSON.parse(payload) as StreamEvent;
          } catch {
            continue;
          }

          if (eventData.type === "text" && eventData.content) {
            setMessages((prev) =>
              prev.map((message) =>
                message.id === assistantMessageId
                  ? { ...message, content: message.content + eventData.content }
                  : message,
              ),
            );
          }

          if (eventData.type === "tool_start" && eventData.tool_input) {
            const toolInput = eventData.tool_input as Record<string, unknown>;
            const toolCallId = (toolInput.tool_call_id as string) || `${toolInput.name}-${Date.now()}`;
            setMessages((prev) =>
              prev.map((message) => {
                if (message.id !== assistantMessageId) return message;
                const existing = message.toolCalls ?? [];
                return {
                  ...message,
                  toolCalls: [
                    ...existing,
                    {
                      id: toolCallId,
                      name: (toolInput.name as string) || eventData.tool_name || "unknown",
                      displayName: (toolInput.display_name as string) || (toolInput.name as string) || eventData.tool_name || "unknown",
                      status: "running" as const,
                      input: toolInput.input as Record<string, unknown>,
                      summary: (toolInput.summary as string) || "",
                    },
                  ],
                };
              }),
            );
          }

          if (eventData.type === "tool_end" && eventData.tool_output) {
            const toolOutput = eventData.tool_output as Record<string, unknown>;
            const targetId = (toolOutput.tool_call_id as string) || "";
            const targetName = eventData.tool_name || "";
            setMessages((prev) =>
              prev.map((message) => {
                if (message.id !== assistantMessageId) return message;
                const updated = (message.toolCalls ?? []).map((tool) => {
                  if (targetId && tool.id === targetId) {
                    return {
                      ...tool,
                      status: (toolOutput.status as "completed" | "error") || "completed",
                      output: toolOutput.output as Record<string, unknown>,
                      durationMs: toolOutput.duration_ms as number,
                      summary: (toolOutput.summary as string) || tool.summary,
                    };
                  }
                  if (!targetId && tool.name === targetName && tool.status === "running") {
                    return {
                      ...tool,
                      status: (toolOutput.status as "completed" | "error") || "completed",
                      output: toolOutput.output as Record<string, unknown>,
                      durationMs: toolOutput.duration_ms as number,
                      summary: (toolOutput.summary as string) || tool.summary,
                    };
                  }
                  return tool;
                });
                return { ...message, toolCalls: updated };
              }),
            );
          }

          if (eventData.type === "chart" && eventData.chart_spec) {
            setMessages((prev) =>
              prev.map((message) =>
                message.id === assistantMessageId
                  ? { ...message, chartConfig: eventData.chart_spec }
                  : message,
              ),
            );
          }

          if (
            eventData.type === "reasoning_step" &&
            eventData.reasoning_step
          ) {
            const incoming = eventData.reasoning_step;
            setMessages((prev) =>
              prev.map((message) => {
                if (message.id !== assistantMessageId) return message;
                const steps = message.reasoningSteps ?? [];
                return { ...message, reasoningSteps: [...steps, incoming] };
              }),
            );
          }

          if (
            eventData.type === "response_metadata" &&
            eventData.response_payload
          ) {
            const responsePayload = eventData.response_payload as ChatResponse;
            if (responsePayload.session_id) {
              setSessionId(responsePayload.session_id);
            }
            if (responsePayload.model_name) {
              setSelectedModel(responsePayload.model_name);
            }
            if (
              typeof responsePayload.response === "string" &&
              responsePayload.response.length > 0
            ) {
              setMessages((prev) =>
                prev.map((message) => {
                  if (message.id !== assistantMessageId) return message;
                  if (message.content && message.content.length > 0) {
                    return message;
                  }
                  return { ...message, content: responsePayload.response };
                }),
              );
            }
            if (responsePayload.calculation_explanation) {
              setMessages((prev) =>
                prev.map((message) =>
                  message.id === assistantMessageId
                    ? { ...message, calculationExplanation: responsePayload.calculation_explanation }
                    : message,
                ),
              );
            }
            if (responsePayload.grade_judgment) {
              setMessages((prev) =>
                prev.map((message) =>
                  message.id === assistantMessageId
                    ? { ...message, gradeJudgment: responsePayload.grade_judgment }
                    : message,
                ),
              );
            }
            if (responsePayload.chart_config) {
              setMessages((prev) =>
                prev.map((message) =>
                  message.id === assistantMessageId
                    ? { ...message, chartConfig: responsePayload.chart_config }
                    : message,
                ),
              );
            }
            if (
              Array.isArray(responsePayload.reasoning_steps) &&
              responsePayload.reasoning_steps.length > 0
            ) {
              setMessages((prev) =>
                prev.map((message) =>
                  message.id === assistantMessageId
                    ? { ...message, reasoningSteps: responsePayload.reasoning_steps }
                    : message,
                ),
              );
            }
          }

          if (eventData.type === "error") {
            throw new Error(eventData.error || "流式响应失败");
          }
        }
      }

      setMessages((prev) =>
        prev.map((message) =>
          message.id === assistantMessageId && message.status === "running"
            ? { ...message, status: "completed" }
            : message,
        ),
      );
    } catch (error) {
      if ((error as Error).name === "AbortError") {
        // 用户主动停止，不显示错误
        return;
      }
      const message = error instanceof Error ? error.message : "请求失败";
      setMessages((prev) =>
        prev.map((item) =>
          item.id === assistantMessageId
            ? { ...item, content: `请求失败：${message}`, status: "error" }
            : item,
        ),
      );
    } finally {
      setIsLoading(false);
      abortControllerRef.current = null;
    }
  };

  const handleFormSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    void submitMessage(input);
  };

  const shellClassName =
    mode === "dock"
      ? cn(
          "fixed bottom-5 right-5 z-50 flex w-[min(420px,calc(100vw-24px))] flex-col overflow-hidden rounded-3xl border border-slate-200 bg-white shadow-[0_24px_80px_rgba(15,23,42,0.18)] transition-all",
          dockOpen ? "h-[min(760px,calc(100vh-40px))]" : "pointer-events-none h-0 border-0 shadow-none",
        )
      : "flex h-screen flex-col bg-gray-50 dark:bg-gray-900";

  return (
    <>
      {mode === "dock" && !dockOpen ? (
        <button
          onClick={() => setDockOpen(true)}
          className="fixed bottom-5 right-5 z-50 inline-flex items-center gap-2 rounded-full bg-sky-600 px-4 py-3 text-sm font-semibold text-white shadow-[0_14px_34px_rgba(2,132,199,0.35)] transition hover:bg-sky-700"
        >
          <span className="text-base">◌</span>
          {launcherLabel}
        </button>
      ) : null}

      <div className={shellClassName}>
        <header
          className={cn(
            "border-b bg-white px-4 py-3 dark:bg-gray-800",
            mode === "fullscreen"
              ? "flex items-center justify-between"
              : "flex items-start justify-between",
          )}
        >
          <div className="flex items-center gap-4">
            <div className="flex items-center gap-2">
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-600 font-bold text-white">
                N
              </div>
              <div>
                <h1 className="text-base font-semibold">{title}</h1>
                <p className="text-xs text-gray-500">{subtitle}</p>
              </div>
            </div>
            {showNavigation && mode === "fullscreen" ? (
              <nav className="flex gap-2">
                <a
                  href="/"
                  className="text-sm font-medium text-blue-600 hover:text-blue-700"
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
                  className="text-sm text-gray-600 hover:text-gray-900 dark:text-gray-400 dark:hover:text-gray-100"
                >
                  知识图谱
                </a>
              </nav>
            ) : null}
          </div>

          <div className="flex items-center gap-3">
            {sessionId ? (
              <div className="hidden text-xs text-gray-400 sm:block">
                Session: {sessionId.slice(0, 8)}
              </div>
            ) : null}
            {mode === "dock" ? (
              <button
                onClick={() => setDockOpen(false)}
                className="rounded-full p-2 text-gray-500 transition hover:bg-gray-100 hover:text-gray-800"
              >
                ×
              </button>
            ) : null}
          </div>
        </header>

        <main className="flex-1 overflow-y-auto px-4 py-6">
          <div
            className={cn(
              "mx-auto space-y-6",
              mode === "fullscreen" ? "max-w-3xl" : "max-w-none",
            )}
          >
            {messages.length === 0 ? (
              <div className="py-10 text-center">
                <h2 className="mb-2 text-2xl font-bold text-gray-800 dark:text-gray-200">
                  工业质量数据智能查询
                </h2>
                <p className="mb-8 text-gray-600 dark:text-gray-400">
                  用自然语言查询钢铁带材检测数据
                </p>

                <div className="mx-auto grid max-w-xl grid-cols-1 gap-3 sm:grid-cols-2">
                  {SUGGESTED_QUESTIONS.map((q) => (
                    <button
                      key={q}
                      onClick={() => void submitMessage(q)}
                      className="rounded-lg border bg-white p-3 text-left text-sm text-gray-700 transition-all hover:border-blue-500 hover:shadow-sm dark:bg-gray-800 dark:text-gray-300"
                    >
                      {q}
                    </button>
                  ))}
                </div>
              </div>
            ) : null}

            {messages.map((message, index) => (
              <div
                key={message.id || index}
                className={`flex ${
                  message.role === "user" ? "justify-end" : "justify-start"
                }`}
              >
                <div
                  className={`${
                    message.role === "user"
                      ? "max-w-[70%] rounded-2xl px-4 py-3 bg-blue-600 text-white"
                      : "w-full max-w-[90%]"
                  }`}
                >
                  {message.role === "assistant" ? (
                    <div className="space-y-3">
                      {/* 推理链 */}
                      {message.reasoningSteps && message.reasoningSteps.length > 0 ? (
                        <div className="rounded-lg border border-slate-200 bg-slate-50 p-3 dark:border-gray-700 dark:bg-gray-800">
                          <div className="mb-2 flex items-center gap-2">
                            <span className="h-2 w-2 rounded-full bg-blue-500" />
                            <span className="text-xs font-medium text-gray-500">
                              推理过程 · {message.reasoningSteps.length} 步
                            </span>
                          </div>
                          <KgReasoningChain steps={message.reasoningSteps} />
                        </div>
                      ) : null}

                      {/* 工具调用 */}
                      {message.toolCalls && message.toolCalls.length > 0 ? (
                        <div className="space-y-2">
                          {message.toolCalls.map((tool) => (
                            <div
                              key={tool.id}
                              className="rounded-lg border bg-white p-3 dark:border-gray-700 dark:bg-gray-800"
                            >
                              <div className="flex items-center gap-2">
                                <span
                                  className={`h-2 w-2 rounded-full ${
                                    tool.status === "running"
                                      ? "animate-pulse bg-yellow-500"
                                      : tool.status === "completed"
                                        ? "bg-green-500"
                                        : "bg-red-500"
                                  }`}
                                />
                                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">
                                  {tool.displayName || tool.name}
                                </span>
                                <span
                                  className={`ml-auto text-xs ${
                                    tool.status === "running"
                                      ? "text-yellow-600"
                                      : tool.status === "completed"
                                        ? "text-green-600"
                                        : "text-red-600"
                                  }`}
                                >
                                  {tool.status === "running"
                                    ? "执行中"
                                    : tool.status === "completed"
                                      ? "已完成"
                                      : "失败"}
                                </span>
                                {tool.durationMs ? (
                                  <span className="text-xs text-gray-400">
                                    {tool.durationMs}ms
                                  </span>
                                ) : null}
                              </div>
                              {tool.summary ? (
                                <p className="mt-1 text-xs text-gray-500">
                                  {tool.summary}
                                </p>
                              ) : null}
                            </div>
                          ))}
                        </div>
                      ) : null}

                      {/* 最终回答 */}
                      <div className="prose prose-sm max-w-none rounded-2xl border border-gray-200 bg-white px-4 py-3 dark:border-gray-700 dark:bg-gray-800 dark:prose-invert">
                        <ReactMarkdown>{message.content}</ReactMarkdown>
                      </div>

                      {/* 图表 */}
                      {message.chartConfig ? (
                        <ChartRenderer chartSpec={message.chartConfig} />
                      ) : null}

                      {/* 计算解释 */}
                      {message.calculationExplanation ? (
                        <CalculationExplanationCard
                          explanation={message.calculationExplanation}
                        />
                      ) : null}

                      {/* 等级判定 */}
                      {message.gradeJudgment && message.gradeJudgment.available ? (
                        <GradeJudgmentCard judgment={message.gradeJudgment} />
                      ) : null}
                    </div>
                  ) : (
                    <p className="whitespace-pre-wrap">{message.content}</p>
                  )}
                </div>
              </div>
            ))}

            {/* 执行中状态 */}
            {isLoading ? (
              <div className="flex justify-start">
                <div className="rounded-2xl border bg-white px-4 py-3 dark:bg-gray-800">
                  <div className="flex items-center gap-2 text-sm text-gray-500">
                    <div className="flex gap-1">
                      <span className="h-2 w-2 animate-bounce rounded-full bg-gray-400" />
                      <span
                        className="h-2 w-2 animate-bounce rounded-full bg-gray-400"
                        style={{ animationDelay: "0.1s" }}
                      />
                      <span
                        className="h-2 w-2 animate-bounce rounded-full bg-gray-400"
                        style={{ animationDelay: "0.2s" }}
                      />
                    </div>
                    <span>思考中…</span>
                  </div>
                </div>
              </div>
            ) : null}
          </div>
        </main>

        <footer className="border-t bg-white px-4 py-4 dark:bg-gray-800">
          <div
            className={cn(
              "mx-auto",
              mode === "fullscreen" ? "max-w-3xl" : "max-w-none",
            )}
          >
            <form onSubmit={handleFormSubmit} className="flex gap-2">
              <input
                type="text"
                value={input}
                onChange={(e) => setInput(e.target.value)}
                placeholder="请输入您的问题，例如：最近7天叠片系数的平均值是多少？"
                className="flex-1 rounded-lg border bg-gray-50 px-4 py-3 focus:border-transparent focus:outline-none focus:ring-2 focus:ring-blue-500 dark:border-gray-600 dark:bg-gray-700"
                disabled={isLoading}
              />
              {isLoading ? (
                <button
                  type="button"
                  onClick={stopGeneration}
                  className="rounded-lg bg-red-600 px-6 py-3 font-medium text-white transition-colors hover:bg-red-700"
                >
                  停止
                </button>
              ) : (
                <button
                  type="submit"
                  disabled={!input.trim()}
                  className="rounded-lg bg-blue-600 px-6 py-3 font-medium text-white transition-colors hover:bg-blue-700 disabled:cursor-not-allowed disabled:bg-gray-400"
                >
                  发送
                </button>
              )}
            </form>
            <p className="mt-2 text-center text-xs text-gray-500">
              NLQ-Agent Agent Conversation Timeline
            </p>
          </div>
        </footer>
      </div>
    </>
  );
}

export default NlqChatPanel;
