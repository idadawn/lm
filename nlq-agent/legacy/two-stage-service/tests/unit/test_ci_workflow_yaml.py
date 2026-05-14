"""Validate CI workflow YAML and dependabot config structure."""

from pathlib import Path

import yaml

REPO_ROOT = Path(__file__).resolve().parents[3]
CI_WORKFLOW = REPO_ROOT / ".github" / "workflows" / "nlq-agent-ci.yml"
DEPENDABOT = REPO_ROOT / ".github" / "dependabot.yml"


def _load_yaml(path: Path) -> dict:
    assert path.exists(), f"{path} not found"
    with open(path) as f:
        return yaml.safe_load(f)


class TestCIWorkflowYAML:
    def test_workflow_valid_yaml(self):
        data = _load_yaml(CI_WORKFLOW)
        assert "jobs" in data
        # YAML parses `on:` as Python True (boolean)
        assert True in data or "on" in data

    def test_lint_job_structure(self):
        data = _load_yaml(CI_WORKFLOW)
        job = data["jobs"]["lint"]
        assert "runs-on" in job
        assert "steps" in job
        assert len(job["steps"]) >= 1

    def test_test_job_structure(self):
        data = _load_yaml(CI_WORKFLOW)
        job = data["jobs"]["test"]
        assert "runs-on" in job
        assert "steps" in job
        assert len(job["steps"]) >= 1

    def test_docker_compose_validate_job_structure(self):
        data = _load_yaml(CI_WORKFLOW)
        job = data["jobs"]["docker-compose-validate"]
        assert "runs-on" in job
        assert "steps" in job
        assert len(job["steps"]) >= 1


class TestDependabotYAML:
    def test_dependabot_has_python_and_github_actions(self):
        data = _load_yaml(DEPENDABOT)
        assert "updates" in data
        ecosystems = {u["package-ecosystem"] for u in data["updates"]}
        assert "pip" in ecosystems, "Missing pip (python) dependabot entry"
        assert "github-actions" in ecosystems, "Missing github-actions dependabot entry"
