# Contributing to nlq-agent

Thank you for contributing! This guide covers local development, commit conventions, and testing requirements.

## Local Development Setup

1. **Prerequisites**: Python 3.13+, [uv](https://docs.astral.sh/uv/) package manager, Docker & Docker Compose.

2. **Clone and install**:
   ```bash
   git clone <repo-url> && cd nlq-agent
   uv sync --frozen
   ```

3. **Start infrastructure** (MySQL, Qdrant) following `docs/RUNBOOK_NLQ_E2E.md` sections 0-3:
   ```bash
   docker compose -f docker-compose.yml up -d
   ```

4. **Run tests** to confirm everything works:
   ```bash
   uv run pytest tests/ -m "not live_llm and not live_qdrant" -v
   ```

5. **Lint** before committing:
   ```bash
   uv run ruff check src tests
   ```

## Commit Conventions

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
type(scope): short description

Optional body explaining WHY (not what).
```

Common types: `feat`, `fix`, `test`, `docs`, `ci`, `refactor`, `chore`.

If Claude or other AI tools generated code, add co-authorship:

```
Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

See `docs/decisions/` for design decision records.

## Testing Requirements

- All PRs must pass: `pytest -m "not live_llm"` — no exceptions.
- Tests marked `live_llm` or `live_qdrant` are advisory-only and run in nightly CI.
- Tests marked `load` are concurrent-load tests; they are **excluded** from the default run. Run them explicitly with `pytest -m load -v`.
- Write unit tests for new logic in `tests/unit/`.
- Write integration tests in `tests/integration/` when multiple components interact.
- Write load tests in `tests/load/` when testing concurrent behaviour or middleware under burst.

### Running Load Tests

```bash
# Run all load tests
uv run pytest tests/load/ -m load -v

# Default CI run excludes load tests automatically
uv run pytest tests/ -m "not live_llm and not live_qdrant and not load" -v
```

### Benchmark Script

A benchmark script is available for measuring throughput and latency percentiles
with mocked external services:

```bash
uv run python -m scripts.benchmark --concurrency 20 --requests 200 --output benchmark.md
```

## Branch Naming

| Pattern | Use case |
|---------|----------|
| `feat/<topic>` | New features |
| `fix/<topic>` | Bug fixes |
| `omc-team/r<N>/<worker>-<topic>` | Multi-agent round work |

Examples:
- `feat/add-cancellation-support`
- `fix/sql-injection-risk`
- `omc-team/r7/glm-github-actions-ci`

## Pull Requests

- Link to the relevant issue or task (if any).
- Include a summary of changes and testing performed.
- Ensure CI passes before requesting review.
- One logical change per PR — avoid mixing unrelated refactors with feature work.

## Code Style

We use [ruff](https://docs.astral.sh/ruff/) for linting and formatting. Configuration lives in `pyproject.toml` under `[tool.ruff]`.

Key rules:
- Line length: 100 characters
- Target: Python 3.13
- Rule set: E, F, W, I, UP, B (E501 excluded — formatter handles line length)

Run `ruff check src tests` before pushing. Fix warnings or explicitly document exceptions with `# noqa: <code>`.

## Questions?

## Production Readiness

Before deploying to production, review `PRODUCTION_CHECKLIST.md` and run the environment validator:

```bash
python scripts/verify_env.py --env-file .env.production
```

The checklist covers security, observability, performance, reliability, and operations. Each item includes status (✅/⚠️/❌) and remediation guidance.

Check `docs/RUNBOOK_NLQ_E2E.md` for end-to-end setup details, or open an issue.
