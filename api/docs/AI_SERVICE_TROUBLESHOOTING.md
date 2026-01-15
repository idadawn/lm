# AI 服务故障排查指南

## 404 错误排查

当遇到 `System.ClientModel.ClientResultException: Service request failed. Status: 404 (Not Found)` 错误时，请按以下步骤排查：

### 1. 检查 vLLM 服务状态

确认 vLLM 服务是否正在运行：

```bash
# 检查容器状态
docker ps | grep vllm

# 检查服务日志
docker logs qwen_vllm
```

### 2. 验证端点配置

**当前配置：**
- 配置文件：`api/src/application/Poxiao.API.Entry/Configurations/AI.json`
- 端点：`http://47.105.59.151:8920/v1`
- 模型 ID：`qwen2.5-7b`

**注意：** docker-compose.yml 中 vLLM 服务映射到端口 8000，但配置中使用的是 8920。请确认：
- 如果 vLLM 直接运行在 8000 端口，应修改配置为：`http://47.105.59.151:8000/v1`
- 如果通过反向代理映射到 8920，请确认代理配置正确

### 3. 测试端点可访问性

使用 curl 测试端点：

```bash
curl -X POST http://47.105.59.151:8920/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer dummy-key" \
  -d '{
    "model": "qwen2.5-7b",
    "messages": [
      {"role": "user", "content": "Hello!"}
    ]
  }'
```

如果返回 404，请检查：
- vLLM 服务是否在指定端口运行
- 模型 ID 是否与 vLLM 服务中的模型名称匹配

### 4. 验证模型 ID

vLLM 服务可能使用不同的模型标识符。检查 vLLM 日志或使用以下命令查看可用模型：

```bash
curl http://47.105.59.151:8920/v1/models
```

确保配置的 `ModelId` 与 vLLM 服务返回的模型名称匹配。

### 5. 检查网络连接

确认应用服务器可以访问 vLLM 服务：

```bash
# 从应用服务器测试连接
telnet 47.105.59.151 8920
# 或
curl -v http://47.105.59.151:8920/v1/models
```

### 6. 常见问题

#### 问题 1：端口不匹配
- **症状**：404 错误
- **原因**：配置的端口与实际服务端口不一致
- **解决**：修改 `AI.json` 中的 `Endpoint` 配置

#### 问题 2：模型 ID 不匹配
- **症状**：404 错误
- **原因**：配置的模型 ID 与 vLLM 服务中的模型名称不一致
- **解决**：查询 vLLM 服务的可用模型列表，更新配置中的 `ModelId`

#### 问题 3：vLLM 服务未启动
- **症状**：连接超时或 404 错误
- **原因**：vLLM 容器未运行
- **解决**：启动 vLLM 服务：`docker-compose up -d vllm`

#### 问题 4：路径配置错误
- **症状**：404 错误
- **原因**：端点 URL 格式不正确
- **解决**：确保端点为 `http://host:port/v1` 格式（不要包含 `/chat/completions`，客户端会自动添加）

## 配置示例

### 正确的端点配置

```json
{
  "AI": {
    "Chat": {
      "Endpoint": "http://47.105.59.151:8920/v1",
      "ModelId": "qwen2.5-7b",
      "Key": "dummy-key"
    }
  }
}
```

### vLLM 服务启动命令参考

```bash
vllm serve /data/qwen2.5-7b \
  --host 0.0.0.0 \
  --port 8000 \
  --trust-remote-code \
  --gpu-memory-utilization 0.7 \
  --max-model-len 4096
```

## 日志查看

应用日志会包含详细的错误信息，包括：
- 端点 URL
- 模型 ID
- HTTP 状态码
- 错误详情

查看日志以获取更多诊断信息。
