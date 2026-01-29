# Repository Guidelines

## Project Structure & Module Organization
- Backend: `api/` (modular monolith). Feature modules live in `api/src/modularity/`, with the **lab** module under `api/src/modularity/lab/`.
- Frontend: `web/` (Vue 3). Feature views mirror backend modules under `web/src/views/` (e.g., `web/src/views/lab/`).
- Tests: `api/tests/` for .NET test projects; `web/` contains frontend tests.
- SQL and scripts: `sql/` for migrations, `scripts/` for utilities, `api/resources/` for templates.

## Build, Test, and Development Commands
Backend:
```powershell
cd api/src/application/Poxiao.API.Entry
dotnet restore
dotnet watch run --launch-profile dev
```
Frontend:
```powershell
cd web
pnpm install
pnpm dev
pnpm build
```
Tests:
```powershell
dotnet test api/tests/
pnpm test
```
Use `docker-compose up -d` to start MySQL/Redis/Qdrant, and `docker-compose --profile ai up -d` for TEI/vLLM.

## Coding Style & Naming Conventions
- C#: PascalCase for classes/methods/properties; private fields use `_camelCase`.
- Vue/TS: Components PascalCase, variables camelCase, CSS classes kebab-case; avoid inline styles.
- Entities must inherit `CLDEntityBase`; follow field naming rules in `.cursorrules` and use `[SugarColumn]` for column mapping.
- Prefer SqlSugar cross-database APIs for MySQL/SQL Server/Oracle compatibility.

## Testing Guidelines
- .NET tests run via `dotnet test`; keep tests colocated in `api/tests/*`.
- Frontend tests run via `pnpm test`. Name tests to match feature/module (e.g., `LabSampleList.spec.ts`).
- Add tests for new business logic and critical workflows.

## Commit & Pull Request Guidelines
- Use Conventional Commits: `feat:`, `fix:`, `docs:`, `style:`, `refactor:`, `perf:`, `test:`, `chore:`, `revert:`, `merge:`, `clean:`.
- PRs should include a clear description, linked issue (if any), and test commands run. Add screenshots for UI changes.

## Security & Configuration Tips
- Configure `.env.local` from `.env.example`.
- AI services are configured in `api/src/application/Poxiao.API.Entry/Configurations/AI.json`.
- Uploads go to `api/src/application/Poxiao.API.Entry/uploads/`; logs go to `api/src/application/Poxiao.API.Entry/logs/`.
