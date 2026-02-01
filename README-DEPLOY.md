# 部署使用说明（基础环境与应用分离）

## 适用场景
- 基础环境与应用服务部署在不同服务器
- 内网通信，AI 服务必须部署在基础环境服务器
- 前后端同机（同一应用服务器）

## 目录结构
```
├── infra/          # 基础环境服务器
│   ├── docker-compose.yml
│   └── .env.infra
└── apps/           # 应用服务器
    ├── docker-compose.yml
    └── .env.apps
```

---

## 一、基础环境服务器（infra）

### 1. 进入目录
```bash
cd infra
```

### 2. 配置环境变量
编辑 `.env.infra` 文件，重点配置：
- **数据库/Redis 端口**：`MYSQL_PORT`、`REDIS_PORT`
- **AI 模型路径**：`MODELS_HOST_PATH`（确保主机有该目录）
- **模型名称**：`TEI_MODEL_NAME`、`VLLM_MODEL_NAME`

### 3. 启动基础设施
```bash
docker compose up -d
```

### 4. 验证服务状态
```bash
docker compose ps
docker compose logs
```

### 包含服务
| 服务 | 说明 | 默认端口 |
|------|------|----------|
| MySQL | 数据库 | 3307 |
| Redis | 缓存 | 6380 |
| Qdrant | 向量数据库 | 6333 (HTTP), 6334 (gRPC) |
| TEI | 文本嵌入服务 | 8081 |
| vLLM | 大模型推理 | 8082 |

---

## 二、应用服务器（apps）

### 1. 进入目录
```bash
cd apps
```

### 2. 配置环境变量
编辑 `.env.apps` 文件，**必须配置**：
```bash
# 基础环境服务器内网地址
INFRA_HOST=10.0.0.5          # 修改为实际内网 IP

# 基础环境服务端口（默认配置，通常无需修改）
INFRA_MYSQL_PORT=3307
INFRA_REDIS_PORT=6380
INFRA_QDRANT_PORT=6333
INFRA_TEI_PORT=8081
INFRA_VLLM_PORT=8082
```

### 3. 配置 API 连接
在 `deploy/api/Configurations/` 目录下修改配置文件：

**ConnectionStrings.json**：
```json
{
  "ConnectionStrings": {
    "Default": "server=<INFRA_HOST>;port=<INFRA_MYSQL_PORT>;database=lumei;user=poxiao;password=<密码>;",
    "Redis": "<INFRA_HOST>:<INFRA_REDIS_PORT>,password=<密码>"
  }
}
```

**AI.json**：
```json
{
  "Qdrant": {
    "Host": "http://<INFRA_HOST>:<INFRA_QDRANT_PORT>"
  },
  "TEI": {
    "Url": "http://<INFRA_HOST>:<INFRA_TEI_PORT>"
  },
  "vLLM": {
    "Url": "http://<INFRA_HOST>:<INFRA_VLLM_PORT>"
  }
}
```

### 4. 启动应用服务
```bash
docker compose up -d
```

### 5. 验证服务状态
```bash
docker compose ps
docker compose logs -f api
```

### 包含服务
| 服务 | 说明 | 默认端口 |
|------|------|----------|
| API | 后端服务 | 9530 |
| Web | 前端静态资源 | 内部 |
| Nginx | 反向代理 | 8923 |

### 访问地址
```
http://<应用服务器IP>:8923
```

---

## 三、常见问题

### Q1: 内网 IP 每台机器不同怎么办？
只需修改对应服务器的 `.env.apps` 文件中的 `INFRA_HOST`，以及 API 配置文件中的地址。

### Q2: 不启用 RabbitMQ？
当前配置未包含 RabbitMQ。如需使用，请在 `infra/docker-compose.yml` 中添加 RabbitMQ 服务。

### Q3: 基础环境服务器无 GPU？
vLLM 需要 GPU 支持。选项：
- 将 vLLM 部署到有 GPU 的基础环境服务器
- 或使用 CPU 版本（性能较低）

### Q4: 如何查看服务日志？
```bash
# 基础环境服务器
cd infra && docker compose logs -f <服务名>

# 应用服务器
cd apps && docker compose logs -f <服务名>
```

### Q5: 如何停止所有服务？
```bash
# 基础环境服务器
cd infra && docker compose down

# 应用服务器
cd apps && docker compose down
```

---

## 四、网络说明

- **基础环境网络**：`lm-infra-network`
- **应用服务网络**：`lm-apps-network`

两个网络位于不同服务器，通过内网 IP 通信。

---

## 五、参考文档

- 单机部署参考：`docs/docker-compose-standalone.yml`
- 仅基础设施部署：`docs/docker-compose-infra.yml`
