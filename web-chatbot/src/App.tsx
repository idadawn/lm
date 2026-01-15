import React, { useState, useEffect } from 'react';
import { Bubble, Sender } from '@ant-design/x';
import { UserOutlined, RobotOutlined } from '@ant-design/icons';
import { ConfigProvider, theme } from 'antd';

// Interface for messages
interface Message {
    key: string;
    role: 'user' | 'ai';
    content: string;
}

const App: React.FC = () => {
    const [messages, setMessages] = useState<Message[]>([
        { key: 'init', role: 'ai', content: '您好，我是智能助手小美，有什么可以帮您？' }
    ]);
    const [value, setValue] = useState('');
    const [loading, setLoading] = useState(false);
    const [systemPrompt, setSystemPrompt] = useState('');

    // Listen for config from parent window
    useEffect(() => {
        const handleMessage = (event: MessageEvent) => {
            // Security check: ensure origin matches if deployed. 
            // For local dev with different ports, we skip strict origin check or allow specific.
            const data = event.data;
            if (data && data.type === 'INIT_CONFIG') {
                const { systemGreeting, userGreeting, systemPrompt } = data.payload;

                if (systemPrompt) setSystemPrompt(systemPrompt);

                const newMessages: Message[] = [];
                if (systemGreeting) {
                    newMessages.push({ key: 'sys-' + Date.now(), role: 'ai', content: systemGreeting });
                } else {
                    newMessages.push({ key: 'init', role: 'ai', content: '您好，我是智能助手小美，有什么可以帮您？' });
                }

                if (userGreeting) {
                    newMessages.push({ key: 'usr-' + Date.now(), role: 'user', content: userGreeting });
                }

                if (newMessages.length > 0) {
                    setMessages(newMessages);
                }
            }
        };

        window.addEventListener('message', handleMessage);
        // Notify parent framework that we are ready
        window.parent.postMessage({ type: 'CHATBOT_READY' }, '*');

        return () => {
            window.removeEventListener('message', handleMessage);
        };
    }, []);

    const handleSubmit = async () => {
        if (!value.trim()) return;

        const userMsg: Message = { key: `u-${Date.now()}`, role: 'user', content: value };
        setMessages(prev => [...prev, userMsg]);
        setLoading(true);
        const userInput = value;
        setValue('');

        try {
            // 获取API基础URL - 从父窗口的location获取，或使用相对路径
            const apiBaseUrl = window.parent.location.origin;
            const response = await fetch(`${apiBaseUrl}/api/ai/chat`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    message: userInput,
                    systemPrompt: systemPrompt || undefined
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            const aiResponse = data.response || data.Response || '抱歉，我无法生成回复。';
            
            const aiMsg: Message = {
                key: `a-${Date.now()}`,
                role: 'ai',
                content: aiResponse
            };
            setMessages(prev => [...prev, aiMsg]);
        } catch (error) {
            console.error('AI API调用失败:', error);
            const errorMsg: Message = {
                key: `e-${Date.now()}`,
                role: 'ai',
                content: `抱歉，处理您的请求时出现错误：${error instanceof Error ? error.message : '未知错误'}`
            };
            setMessages(prev => [...prev, errorMsg]);
        } finally {
            setLoading(false);
        }
    };

    return (
        <ConfigProvider theme={{ algorithm: theme.defaultAlgorithm }}>
            <div style={{ display: 'flex', flexDirection: 'column', height: '100vh', padding: '16px', background: '#f0f2f5' }}>
                <div style={{ flex: 1, overflowY: 'auto', paddingBottom: '16px' }}>
                    {messages.map(msg => (
                        <Bubble
                            key={msg.key}
                            placement={msg.role === 'user' ? 'end' : 'start'}
                            content={msg.content}
                            avatar={{ icon: msg.role === 'user' ? <UserOutlined /> : <RobotOutlined /> }}
                            style={{ marginBottom: '12px' }}
                        />
                    ))}
                    {loading && <Bubble placement="start" loading />}
                </div>
                <div style={{ marginTop: 'auto' }}>
                    <Sender
                        value={value}
                        onChange={setValue}
                        onSubmit={handleSubmit}
                        loading={loading}
                        placeholder="请输入您的问题..."
                    />
                </div>
            </div>
        </ConfigProvider>
    );
};

export default App;
