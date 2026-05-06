"use client";

import { useSearchParams } from "next/navigation";
import { NlqChatPanel } from "@/components/chat";

export default function ChatPage() {
  const searchParams = useSearchParams();
  const embedded = searchParams.get("embed") === "1";
  const mode = searchParams.get("mode") === "dock" ? "dock" : "fullscreen";

  return (
    <NlqChatPanel
      mode={mode}
      title={embedded ? "实验室智能问数" : "NLQ-Agent"}
      subtitle={embedded ? "已接入主系统登录态" : "工业质量数据智能查询"}
      showNavigation={!embedded}
    />
  );
}
