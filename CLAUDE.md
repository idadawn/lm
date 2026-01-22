# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Laboratory Data Analysis System (检测室数据分析系统) - an enterprise application for laboratory information management with AI-powered features including natural language queries (NL to SQL) and vector search capabilities.

## Essential Commands

### Frontend (web/)
```bash
cd web
pnpm install          # Install dependencies (pnpm required, v8.1.0+)
pnpm dev              # Development server with hot reload
pnpm build            # Production build
pnpm build:test       # Test environment build
pnpm lint:eslint      # ESLint check and fix
pnpm lint:prettier    # Format code
pnpm type:check       # TypeScript type checking
```

### Backend (api/)
```bash
# Option 1: Use batch script (Windows)
run_api_dev.bat

# Option 2: Manual
cd api/src/application/Poxiao.API.Entry
set ASPNETCORE_ENVIRONMENT=dev
dotnet watch run --launch-profile dev
```

### Infrastructure (Docker)
```bash
docker-compose up -d                    # Start MySQL, Redis, Qdrant
docker-compose --profile ai up -d       # Include AI services (TEI, vLLM)
```

### Mock Server (for frontend-only development)
```bash
cd web/mock && npm install && npm run mock
```

## Architecture

### Backend Structure (api/)
Modular monolithic architecture using .NET 10.0 with SqlSugar ORM:

```
api/
├── framework/Poxiao/          # Core framework (DI, caching, auth, etc.)
├── src/application/           # API entry point
├── src/infrastructure/        # RabbitMQ, WebSockets, OAuth extras
└── src/modularity/            # Feature modules
    ├── lab/                   # Laboratory data (active development)
    ├── system/                # System management
    ├── workflow/              # Workflow engine
    ├── ai/                    # AI integration
    └── [other modules]/
```

Each module contains: `Entity/`, `Service/`, `Controller/`, `Dto/`

### Frontend Structure (web/)
Vue 3 + TypeScript with Ant Design Vue:

```
web/src/
├── api/           # API client services
├── components/    # Reusable components
├── views/         # Page components by feature
├── hooks/         # Composables
├── store/         # Pinia stores
└── router/        # Route definitions
```

## Database Entity Conventions

**Critical: Read .cursorrules before creating/modifying database entities.**

Entities inherit from `CLDEntityBase`. Field naming is inconsistent by design:

**From OEntityBase (parent):**
- `F_Id` - Primary key (note: lowercase 'I' in 'Id')
- `F_TenantId` - Tenant ID

**From CLDEntityBase (all uppercase, no underscores):**
- `F_CREATORTIME`, `F_CREATORUSERID`, `F_ENABLEDMARK`

**From CLDEntityBase (mixed case):**
- `F_LastModifyTime`, `F_LastModifyUserId`, `F_DeleteMark`, `F_DeleteTime`, `F_DeleteUserId`

**Rules:**
1. New tables should use standard field names - no need to override base properties
2. Legacy tables with different naming require `[SugarColumn(ColumnName = "...")]` overrides
3. Missing fields require `[SugarColumn(IsIgnore = true)]`

## Technology Stack

**Backend:** .NET 10.0, SqlSugar, Redis, RabbitMQ, JWT, Serilog
**Frontend:** Vue 3.3, TypeScript, Vite, Ant Design Vue 3.2, Pinia, TailwindCSS
**Database:** SQL Server (default), MySQL, Oracle
**AI Services:** Qdrant (vector DB), TEI (embeddings), vLLM (inference)

## Code Style

**Frontend naming:**
- Components: PascalCase (`UserList.vue`)
- Variables: camelCase
- CSS classes: kebab-case (`user-list-page`)
- No inline styles

**Git commits:** `feat:`, `fix:`, `refactor:`, `docs:`, `style:`, `perf:`, `test:`, `chore:`

## Configuration

**Frontend:** `.env.development`, `.env.production` - API proxy settings
**Backend:** `api/src/application/Poxiao.API.Entry/Configurations/` - AppSetting.json, ConnectionStrings.json, Cache.json

## Notes

- Primary language is Chinese (UI, comments, documentation)
- Multi-database queries must consider MySQL/SQL Server/Oracle compatibility
- AI services (Qdrant, TEI, vLLM) are optional but required for full functionality
