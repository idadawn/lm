"""
Validate docker-compose.production.yml structure and required fields.

Skips when Docker is not available (e.g. CI without DinD).
"""

from __future__ import annotations

import shutil
import subprocess

import pytest
import yaml


DOCKER_AVAILABLE = shutil.which("docker") is not None


@pytest.mark.skipif(not DOCKER_AVAILABLE, reason="Docker not available")
def test_docker_compose_config_valid():
    """`docker compose config` exits 0 for the production file."""
    result = subprocess.run(
        [
            "docker",
            "compose",
            "-f",
            "docker-compose.production.yml",
            "config",
        ],
        capture_output=True,
        text=True,
    )
    assert result.returncode == 0, result.stderr


def test_compose_yaml_loads():
    """The file is valid YAML and contains the expected services."""
    with open("docker-compose.production.yml", encoding="utf-8") as f:
        data = yaml.safe_load(f)

    assert "services" in data
    services = data["services"]
    assert "nlq-agent" in services
    assert "qdrant" in services


def test_nlq_agent_healthcheck():
    """nlq-agent defines a curl-based healthcheck."""
    with open("docker-compose.production.yml", encoding="utf-8") as f:
        data = yaml.safe_load(f)

    svc = data["services"]["nlq-agent"]
    assert svc.get("healthcheck") is not None
    test_cmd = " ".join(svc["healthcheck"]["test"])
    assert "curl" in test_cmd
    assert "/health" in test_cmd


def test_nlq_agent_restart_policy():
    """nlq-agent uses unless-stopped restart policy."""
    with open("docker-compose.production.yml", encoding="utf-8") as f:
        data = yaml.safe_load(f)

    svc = data["services"]["nlq-agent"]
    assert svc.get("restart") == "unless-stopped"


def test_nlq_agent_resource_limits():
    """nlq-agent declares deploy.resources.limits."""
    with open("docker-compose.production.yml", encoding="utf-8") as f:
        data = yaml.safe_load(f)

    svc = data["services"]["nlq-agent"]
    limits = svc.get("deploy", {}).get("resources", {}).get("limits", {})
    assert limits.get("cpus") == "2.0"
    assert limits.get("memory") == "2G"


def test_nlq_agent_logging_options():
    """nlq-agent uses json-file logging with rotation."""
    with open("docker-compose.production.yml", encoding="utf-8") as f:
        data = yaml.safe_load(f)

    svc = data["services"]["nlq-agent"]
    logging = svc.get("logging", {})
    assert logging.get("driver") == "json-file"
    opts = logging.get("options", {})
    assert opts.get("max-size") == "50m"
    assert opts.get("max-file") == "3"


def test_nlq_agent_depends_on_conditions():
    """nlq-agent waits for qdrant and tei to be healthy."""
    with open("docker-compose.production.yml", encoding="utf-8") as f:
        data = yaml.safe_load(f)

    deps = data["services"]["nlq-agent"].get("depends_on", {})
    assert deps.get("qdrant", {}).get("condition") == "service_healthy"
    assert deps.get("tei", {}).get("condition") == "service_healthy"


def test_qdrant_volume_mount():
    """Qdrant mounts host path for persistence."""
    with open("docker-compose.production.yml", encoding="utf-8") as f:
        data = yaml.safe_load(f)

    volumes = data["services"]["qdrant"].get("volumes", [])
    assert any("/data/nlq-agent/qdrant/storage" in str(v) for v in volumes)


def test_tei_gpu_profile():
    """TEI GPU service has nvidia device reservations and empty default profile."""
    with open("docker-compose.production.yml", encoding="utf-8") as f:
        data = yaml.safe_load(f)

    tei = data["services"]["tei"]
    assert "" in tei.get("profiles", [])
    devices = (
        tei.get("deploy", {})
        .get("resources", {})
        .get("reservations", {})
        .get("devices", [])
    )
    assert any(d.get("driver") == "nvidia" for d in devices)


def test_tei_cpu_fallback_profile():
    """TEI CPU fallback is gated by --profile cpu."""
    with open("docker-compose.production.yml", encoding="utf-8") as f:
        data = yaml.safe_load(f)

    tei_cpu = data["services"]["tei-cpu"]
    assert "cpu" in tei_cpu.get("profiles", [])
