# Legacy two-stage service

本目录保存 `nlq-agent` 早期的两阶段 Python 服务实现。它曾经以 `src.main:app` 作为 FastAPI 入口，并通过 Stage 1 语义/知识图谱检索与 Stage 2 数据/SQL 查询完成问答流程。当前项目主线已经切换到 `../../services/agent-api`，因此本目录不再作为默认开发入口。

## 归档内容

| 路径 | 内容 |
|---|---|
| `src/` | 旧 FastAPI 入口、两阶段编排器、旧服务层和旧模型定义。 |
| `tests/` | 旧实现对应的单元、E2E 和负载测试。 |
| `scripts/` | 旧语义层初始化、环境验证和 benchmark 脚本。 |
| `Dockerfile`、`docker-compose*.yml` | 旧服务部署和向量服务编排文件。 |
| `pyproject.toml`、`requirements.txt`、`uv.lock` | 旧 Python 项目依赖配置。 |
| `API.md`、`CONTRIBUTING.md`、`PRODUCTION_CHECKLIST.md` | 旧服务文档。 |

## 使用边界

本目录的代码仅用于历史参考、迁移对照和必要的兼容性排查。新增 NLQ、知识图谱、Chat2SQL、SSE 协议或工具调用能力时，应修改 `../../services/agent-api`。如果确实需要临时运行旧服务，请先进入本目录，再按旧文档中的命令执行，以免与主线服务混淆。

```bash
cd nlq-agent/legacy/two-stage-service
uv run uvicorn src.main:app --reload --port 18100
```

## 后续处理建议

当 `services/agent-api` 完成所有旧接口兼容并稳定运行一段时间后，可以将本目录迁移到单独归档分支，或在确认无引用后从主分支删除。当前保留它，是为了降低一次性清理造成历史逻辑不可追溯的风险。
