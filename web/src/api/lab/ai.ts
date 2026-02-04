import { defHttp } from '/@/utils/http/axios';

// AI 对话请求
export interface ChatRequest {
    message: string;
    systemPrompt?: string;
}

// AI 对话响应
export interface ChatResponse {
    response: string;
}

// 聊天消息
export interface ChatMessage {
    id: string;
    role: 'user' | 'assistant';
    content: string;
    createdAt: string;
    loading?: boolean;
}

// 聊天会话
export interface ChatSession {
    id: string;
    title: string;
    createdAt: string;
    updatedAt: string;
    messages: ChatMessage[];
}

// 创建会话请求
export interface CreateSessionRequest {
    title?: string;
}

// 发送消息到 AI
export function sendChatMessage(request: ChatRequest) {
    return defHttp.post<ChatResponse>({
        url: '/api/ai/chat',
        data: request,
    });
}

// 获取会话历史列表
export function getChatSessions() {
    return defHttp.get<ChatSession[]>({
        url: '/api/lab/chat/sessions',
    });
}

// 获取单个会话详情（包含消息）
export function getChatSession(sessionId: string) {
    return defHttp.get<ChatSession>({
        url: `/api/lab/chat/sessions/${sessionId}`,
    });
}

// 创建新会话
export function createChatSession(data?: CreateSessionRequest) {
    return defHttp.post<ChatSession>({
        url: '/api/lab/chat/sessions',
        data,
    });
}

// 删除会话
export function deleteChatSession(sessionId: string) {
    return defHttp.delete({
        url: `/api/lab/chat/sessions/${sessionId}`,
    });
}

// 添加消息到会话
export interface AddMessageRequest {
    role: 'user' | 'assistant';
    content: string;
}

export function addChatMessage(sessionId: string, data: AddMessageRequest) {
    return defHttp.post<ChatMessage>({
        url: `/api/lab/chat/sessions/${sessionId}/messages`,
        data,
    });
}
