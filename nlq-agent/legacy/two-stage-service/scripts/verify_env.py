#!/usr/bin/env python3
"""
.env 配置验证脚本

用法:
    python scripts/verify_env.py
    python scripts/verify_env.py --env-file .env.production
    python scripts/verify_env.py --no-network

退出码:
    0 — 全部通过
    1 — 有必填项缺失或格式错误
    2 — 仅网络连通性警告（无硬性错误）
"""

from __future__ import annotations

import argparse
import asyncio
import os
import socket
import sys
from pathlib import Path
from urllib.parse import urljoin


def _color(status: str, msg: str) -> str:
    """返回带 ANSI 颜色的状态行。"""
    codes = {"ok": "\033[32m", "missing": "\033[31m", "warn": "\033[33m", "reset": "\033[0m"}
    symbol = {"ok": "✅", "missing": "❌", "warn": "⚠️"}
    return f"{codes[status]}{symbol[status]} {msg}{codes['reset']}"


REQUIRED_VARS = [
    "LLM_BASE_URL",
    "LLM_API_KEY",
    "LLM_MODEL",
    "EMBEDDING_BASE_URL",
    "QDRANT_HOST",
    "QDRANT_PORT",
    "MYSQL_HOST",
    "MYSQL_PORT",
    "MYSQL_USER",
    "MYSQL_PASSWORD",
    "MYSQL_DATABASE",
]

PLACEHOLDER_PREFIXES = ("YOUR_", "sk-test")


class EnvChecker:
    """加载 .env 并执行验证。"""

    def __init__(self, env_file: str | None = None, no_network: bool = False) -> None:
        self.env_file = env_file
        self.no_network = no_network
        self.errors = 0
        self.warnings = 0

    def _load_env(self) -> dict[str, str]:
        """从文件或当前环境加载变量。"""
        if self.env_file:
            path = Path(self.env_file)
            if not path.exists():
                print(_color("missing", f"env file not found: {self.env_file}"))
                self.errors += 1
                return {}
            raw = path.read_text(encoding="utf-8")
            env: dict[str, str] = {}
            for line in raw.splitlines():
                line = line.strip()
                if not line or line.startswith("#"):
                    continue
                if "=" in line:
                    key, val = line.split("=", 1)
                    env[key.strip()] = val.strip()
            return env
        return dict(os.environ)

    def _check_required(self, env: dict[str, str]) -> None:
        """检查所有必填变量是否存在且非空。"""
        for var in REQUIRED_VARS:
            value = env.get(var)
            if not value:
                print(_color("missing", f"{var} missing or empty"))
                self.errors += 1
            else:
                print(_color("ok", f"{var} ok"))

    def _check_placeholder_key(self, env: dict[str, str]) -> None:
        """检查 LLM_API_KEY 不是 placeholder。"""
        key = env.get("LLM_API_KEY", "")
        if not key:
            return
        if any(key.startswith(p) or key == p for p in PLACEHOLDER_PREFIXES):
            print(_color("missing", f"LLM_API_KEY looks like a placeholder: '{key}'"))
            self.errors += 1

    async def _http_check(self, name: str, url: str, timeout: float = 5.0) -> bool:
        """对目标 URL 发起 HEAD 请求。"""
        try:
            import httpx
        except ImportError:
            # fallback: 使用标准库 urllib
            import urllib.request
            req = urllib.request.Request(url, method="HEAD")
            try:
                urllib.request.urlopen(req, timeout=timeout)
                return True
            except Exception as exc:
                print(_color("warn", f"{name} unreachable at {url} ({type(exc).__name__})"))
                return False

        try:
            async with httpx.AsyncClient(timeout=timeout) as client:
                resp = await client.head(url, follow_redirects=True)
                return resp.status_code < 500
        except Exception as exc:
            print(_color("warn", f"{name} unreachable at {url} ({type(exc).__name__})"))
            return False

    async def _tcp_check(self, host: str, port: int, timeout: float = 5.0) -> bool:
        """TCP 连通性测试。"""
        try:
            reader, writer = await asyncio.wait_for(
                asyncio.open_connection(host, port), timeout=timeout
            )
            writer.close()
            await writer.wait_closed()
            return True
        except Exception as exc:
            print(_color("warn", f"MySQL TCP {host}:{port} unreachable ({type(exc).__name__})"))
            return False

    async def _check_network(self, env: dict[str, str]) -> None:
        """实际 ping 各依赖服务。"""
        llm_base = env.get("LLM_BASE_URL", "")
        emb_base = env.get("EMBEDDING_BASE_URL", "")
        qdrant_host = env.get("QDRANT_HOST", "")
        qdrant_port = env.get("QDRANT_PORT", "")
        mysql_host = env.get("MYSQL_HOST", "")
        mysql_port = env.get("MYSQL_PORT", "")

        results: list[bool] = []

        if llm_base:
            url = urljoin(llm_base.rstrip("/") + "/", "models")
            results.append(await self._http_check("LLM", url))

        if emb_base:
            url = urljoin(emb_base.rstrip("/") + "/", "health")
            results.append(await self._http_check("Embedding", url))

        if qdrant_host and qdrant_port:
            try:
                port_int = int(qdrant_port)
            except ValueError:
                port_int = 6333
            url = f"http://{qdrant_host}:{port_int}/healthz"
            results.append(await self._http_check("Qdrant", url))

        if mysql_host and mysql_port:
            try:
                port_int = int(mysql_port)
            except ValueError:
                port_int = 3306
            results.append(await self._tcp_check(mysql_host, port_int))

        if results and not all(results):
            self.warnings += 1

    def run(self) -> int:
        """执行完整验证流程并返回退出码。"""
        env = self._load_env()
        if self.errors:
            return 1

        self._check_required(env)
        self._check_placeholder_key(env)

        if not self.no_network and env:
            asyncio.run(self._check_network(env))

        print()
        if self.errors:
            print(f"❌ {self.errors} error(s) found")
            return 1
        if self.warnings:
            print(f"⚠️  {self.warnings} warning(s) found (network only)")
            return 2
        print("✅ All checks passed")
        return 0


def main() -> None:
    parser = argparse.ArgumentParser(description="Verify nlq-agent environment configuration")
    parser.add_argument("--env-file", type=str, default=None, help="Path to .env file")
    parser.add_argument("--no-network", action="store_true", help="Skip network connectivity checks")
    args = parser.parse_args()

    checker = EnvChecker(env_file=args.env_file, no_network=args.no_network)
    sys.exit(checker.run())


if __name__ == "__main__":
    main()
