# DDL 补充注释（可选）

本文件可选——若需要给某些表补人工注释（覆盖 information_schema 的 COMMENT 不够具体的字段），写在这里。
启动时与自动 DDL 合并。当前为空。

> 提示：自动 DDL 由 `app/knowledge/ddl_loader.py` 从 `information_schema` 动态生成，
> 已包含表注释和列注释。仅在注释不足时才需要在此文件补充。
