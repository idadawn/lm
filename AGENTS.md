# AGENTS.md - Laboratory Data Analysis System

> This file provides comprehensive guidance for AI coding agents working with this codebase.
> 
> **Project Name:** 检测室数据分析系统 (Laboratory Data Analysis System)
> **Primary Language:** Chinese (UI, comments, documentation)
> **Last Updated:** 2026-01-29

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Project Structure](#project-structure)
4. [Development Environment Setup](#development-environment-setup)
5. [Build and Run Commands](#build-and-run-commands)
6. [Code Organization](#code-organization)
7. [Database Conventions](#database-conventions)
8. [Code Style Guidelines](#code-style-guidelines)
9. [Testing](#testing)
10. [Deployment](#deployment)
11. [AI Services Integration](#ai-services-integration)
12. [Common Tasks](#common-tasks)

---

## Project Overview

This is an enterprise Laboratory Information Management System (LIMS) with AI-powered features including:

- **Laboratory Data Management:** Raw data import, intermediate data processing, product specification management
- **AI-Powered Features:** Natural language to SQL queries, vector search capabilities
- **Workflow Engine:** Visual workflow design and process management
- **KPI Analytics:** Key performance indicator tracking and visualization
- **Code Generation:** Auto-generate CRUD code for new modules

The system follows a modular monolithic architecture with clear separation of concerns.

---

## Technology Stack

### Backend (api/)

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Core framework |
| SqlSugar | Latest | ORM for database access |
| Redis | 8.0+ | Caching and session storage |
| RabbitMQ | Latest | Message queue for event bus |
| JWT | - | Authentication |
| Serilog | - | Structured logging |
| Qdrant | v1.12.1+ | Vector database for AI features |
| TEI | 1.6+ | Text Embeddings Inference |
| vLLM | Latest | LLM inference engine |

### Frontend (web/)

| Technology | Version | Purpose |
|------------|---------|---------|
| Vue | 3.3.4 | UI framework |
| TypeScript | 5.2.2 | Type-safe JavaScript |
| Vite | 4.4.9 | Build tool |
| Ant Design Vue | 3.2.20 | UI component library |
| Pinia | 2.1.6 | State management |
| ECharts | 5.4.3 | Data visualization |
| TailwindCSS | 3.3.6 | Utility-first CSS |

### Database Support

- MySQL 8.0 (primary)
- SQL Server
- Oracle

---

## Project Structure

```
lm/
├── api/                          # Backend .NET Application
│   ├── framework/Poxiao/         # Core framework (DI, caching, auth, etc.)
│   │   ├── App/                  # Application bootstrap
│   │   ├── Authorization/        # JWT and permission handling
│   │   ├── Cache/                # Memory and Redis cache
│   │   ├── DependencyInjection/  # DI container extensions
│   │   ├── DynamicApiController/ # Auto API generation
│   │   ├── EventBus/             # Internal event system
│   │   ├── Logging/              # Serilog integration
│   │   ├── Schedule/             # Cron job scheduling
│   │   └── ...
│   ├── src/
│   │   ├── application/
│   │   │   └── Poxiao.API.Entry/ # API entry point (Program.cs)
│   │   ├── infrastructure/       # Infrastructure services
│   │   │   ├── Poxiao.Extras.CollectiveOAuth/  # OAuth integrations
│   │   │   ├── Poxiao.Extras.EventBus.RabbitMQ/
│   │   │   ├── Poxiao.Extras.Thirdparty/       # DingTalk, WeChat, Email
│   │   │   └── Poxiao.Extras.WebSockets/
│   │   └── modularity/           # Feature modules
│   │       ├── ai/               # AI services (NL2SQL, vector search)
│   │       ├── app/              # App management
│   │       ├── codegen/          # Code generation
│   │       ├── common/           # Shared entities and utilities
│   │       ├── engine/           # Visual development engine
│   │       ├── extend/           # Extended functionality
│   │       ├── kpi/              # KPI analytics module
│   │       ├── lab/              # Laboratory data (ACTIVE DEVELOPMENT)
│   │       ├── message/          # Message center
│   │       ├── oauth/            # OAuth authentication
│   │       ├── subdev/           # Sub-device management
│   │       ├── system/           # System management
│   │       ├── taskschedule/     # Task scheduling
│   │       ├── visualdata/       # Data visualization
│   │       ├── visualdev/        # Visual development
│   │       └── workflow/         # Workflow engine
│   ├── tests/                    # Test projects
│   │   ├── Poxiao.UnitTests/
│   │   ├── Poxiao.IntegrationTests/
│   │   └── Poxiao.TestProject/
│   └── resources/                # Templates and static resources
├── web/                          # Frontend Vue Application
│   ├── src/
│   │   ├── api/                  # API client services
│   │   ├── assets/               # Static assets
│   │   ├── components/           # Reusable Vue components
│   │   ├── design/               # Design system
│   │   ├── directives/           # Vue directives
│   │   ├── enums/                # TypeScript enums
│   │   ├── hooks/                # Vue composables
│   │   ├── layouts/              # Page layouts
│   │   ├── locales/              # i18n translations
│   │   ├── logics/               # Business logic
│   │   ├── router/               # Vue Router configuration
│   │   ├── settings/             # App settings
│   │   ├── store/                # Pinia stores
│   │   ├── utils/                # Utility functions
│   │   └── views/                # Page components by feature
│   │       ├── ai/               # AI configuration
│   │       ├── basic/            # Login, home, profile
│   │       ├── extend/           # Extended demos
│   │       ├── generator/        # Code generation UI
│   │       ├── kpi/              # KPI analytics
│   │       ├── lab/              # Laboratory data (ACTIVE)
│   │       │   ├── dashboard/    # 生产驾驶舱 (首页)
│   │       │   ├── rawData/      # 原始数据管理
│   │       │   ├── intermediateData/  # 中间数据管理
│   │       │   ├── appearance/   # 外观特征管理
│   │       │   ├── product/      # 产品规格管理
│   │       │   └── ...
│   │       ├── permission/       # User/role/organize management
│   │       ├── system/           # System settings
│   │       ├── systemData/       # Data dictionary, data source
│   │       └── workFlow/         # Workflow designer
│   ├── build/                    # Build configuration
│   ├── mock/                     # Mock server for development
│   └── deploy/                   # Deployment configuration
├── sql/                          # Database migration scripts
├── scripts/                      # Utility scripts
├── docker-compose.yml            # Docker compose for production
└── .env.example                  # Environment variables template
```

---

## Development Environment Setup

### Prerequisites

- **Backend:** .NET 10.0 SDK, Docker Desktop
- **Frontend:** Node.js >= 16.15.0, pnpm >= 8.1.0
- **Database:** MySQL 8.0 (via Docker recommended)

### Initial Setup

1. **Clone and enter the project directory**

2. **Start Infrastructure Services (Docker)**
   ```powershell
   docker-compose up -d
   ```
   This starts: MySQL, Redis, Qdrant (vector DB)

3. **Include AI services (optional)**
   ```powershell
   docker-compose --profile ai up -d
   ```
   This additionally starts: TEI (embeddings), vLLM (inference)

4. **Backend Setup**
   ```powershell
   cd api/src/application/Poxiao.API.Entry
   dotnet restore
   ```

5. **Frontend Setup**
   ```powershell
   cd web
   pnpm install
   ```

6. **Environment Configuration**
   - Copy `.env.example` to `.env.local` and configure your settings
   - Backend configs are in: `api/src/application/Poxiao.API.Entry/Configurations/`

---

## Build and Run Commands

### Backend Commands

```powershell
# Option 1: Use the provided batch script (Windows)
./run_api_dev.bat

# Option 2: Manual execution
cd api/src/application/Poxiao.API.Entry
set ASPNETCORE_ENVIRONMENT=dev
dotnet watch run --launch-profile dev
```

Default backend URL: `http://localhost:10089` (dev) or `http://localhost:9530` (production)

### Frontend Commands

```powershell
cd web

# Install dependencies
pnpm install

# Development server with hot reload
pnpm dev

# Production build
pnpm build

# Test environment build
pnpm build:test

# Type checking
pnpm type:check

# Linting
pnpm lint:eslint      # ESLint check and fix
pnpm lint:prettier    # Format code
pnpm lint:stylelint   # Style linting

# Clean cache
pnpm clean:cache
```

Default frontend URL: `http://localhost:3000`

### Mock Server (for frontend-only development)

```powershell
cd web/mock
npm install
npm run mock
```

---

## Code Organization

### Backend Module Structure

Each module in `api/src/modularity/` follows this structure:

```
ModuleName/
├── Poxiao.ModuleName/           # Implementation
│   ├── Service/                 # Business logic services
│   └── [other implementation]
├── Poxiao.ModuleName.Entity/    # Entities and DTOs
│   ├── Entity/                  # Database entities
│   └── Dto/                     # Data transfer objects
├── Poxiao.ModuleName.Interfaces/# Service interfaces
└── Poxiao.ModuleName.Web.Core/  # Controllers (if applicable)
    └── Controller/
```

### Frontend Structure

```
web/src/
├── api/           # API services - one file per module
├── components/    # Reusable components
│   ├── Application/   # App-level components
│   ├── Basic/         # Basic UI components
│   └── [feature]/     # Feature-specific components
├── views/         # Page components - mirror backend modules
│   ├── lab/           # Laboratory data pages
│   ├── kpi/           # KPI analytics
│   ├── permission/    # User management
│   └── ...
├── store/         # Pinia stores
│   └── modules/
└── router/        # Route definitions
    └── routes/
```

---

## Database Conventions

### ⚠️ CRITICAL: Entity Base Class Field Mapping

**ALWAYS read `.cursorrules` before creating/modifying database entities!**

All entities inherit from `CLDEntityBase`. The field naming is **inconsistent by design** - pay special attention:

#### Base Class Fields

**From OEntityBase (parent of CLDEntityBase):**
- `F_Id` - Primary key (**Note:** lowercase 'I' in 'Id', NOT `F_ID`)
- `F_TenantId` - Tenant ID

**From CLDEntityBase (ALL UPPERCASE, no underscores):**
- `F_CREATORTIME` - Creation time
- `F_CREATORUSERID` - Creator user ID
- `F_ENABLEDMARK` - Enable flag

**From CLDEntityBase (mixed case - F_ followed by PascalCase):**
- `F_LastModifyTime` - Last modification time
- `F_LastModifyUserId` - Last modifier user ID
- `F_DeleteMark` - Soft delete flag
- `F_DeleteTime` - Deletion time
- `F_DeleteUserId` - Deleter user ID

#### Entity Definition Rules

1. **DO NOT override base class properties** unless necessary
2. Use `[SugarColumn(ColumnName = "...")]` when table field names differ from base class
3. Use `[SugarColumn(IsIgnore = true)]` when table lacks certain base fields

#### Example Entity

```csharp
public class ProductSpecEntity : CLDEntityBase
{
    // Don't override Id - base class handles F_Id mapping
    
    [SugarColumn(ColumnName = "F_SPEC_CODE", Length = 50, IsNullable = false)]
    public string SpecCode { get; set; }
    
    [SugarColumn(ColumnName = "F_SPEC_NAME", Length = 100)]
    public string SpecName { get; set; }
}
```

#### SQL Script Template

```sql
CREATE TABLE IF NOT EXISTS `product_spec` (
    -- From OEntityBase
    `F_Id` VARCHAR(50) NOT NULL COMMENT '主键ID',
    `F_TenantId` VARCHAR(50) DEFAULT NULL COMMENT '租户ID',
    
    -- From CLDEntityBase (ALL UPPERCASE)
    `F_CREATORTIME` DATETIME DEFAULT NULL COMMENT '创建时间',
    `F_CREATORUSERID` VARCHAR(50) DEFAULT NULL COMMENT '创建用户ID',
    `F_ENABLEDMARK` INT DEFAULT 1 COMMENT '启用标识',
    
    -- From CLDEntityBase (mixed case)
    `F_LastModifyTime` DATETIME DEFAULT NULL COMMENT '修改时间',
    `F_LastModifyUserId` VARCHAR(50) DEFAULT NULL COMMENT '修改用户ID',
    `F_DeleteMark` INT DEFAULT 0 COMMENT '删除标志',
    `F_DeleteTime` DATETIME DEFAULT NULL COMMENT '删除时间',
    `F_DeleteUserId` VARCHAR(50) DEFAULT NULL COMMENT '删除用户ID',
    
    -- Business fields
    `F_SPEC_CODE` VARCHAR(50) NOT NULL COMMENT '规格编码',
    `F_SPEC_NAME` VARCHAR(100) COMMENT '规格名称',
    
    PRIMARY KEY (`F_Id`),
    KEY `IDX_TENANT_ID` (`F_TenantId`),
    KEY `IDX_DELETE_MARK` (`F_DeleteMark`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
```

---

## Code Style Guidelines

### Git Commit Conventions

Use conventional commits:

```
feat:     新功能/修改 (new feature)
fix:      修复 bug (bug fix)
docs:     文档修改 (documentation)
style:    格式调整 (formatting, no code change)
refactor: 重构 (refactoring)
perf:     性能优化 (performance)
test:     增加测试 (tests)
chore:    构建/工具变动 (build/tools)
revert:   回滚 (revert)
merge:    代码合并 (merge)
clean:    清理代码 (cleanup)
```

### C# Naming Conventions

- **Classes/Interfaces:** PascalCase (`ProductSpecService`, `IProductSpecService`)
- **Methods:** PascalCase (`GetProductSpecList`)
- **Properties:** PascalCase (`CreatedTime`)
- **Private fields:** camelCase with underscore (`_repository`)
- **Constants:** PascalCase (`MaxPageSize`)

### Vue/TypeScript Naming Conventions

- **Components:** PascalCase (`UserList.vue`)
- **Variables:** camelCase (`userList`)
- **CSS classes:** kebab-case (`user-list-page`)
- **No inline styles** - always use CSS classes

### Code Organization Rules

1. **Backend:**
   - One service interface per business domain
   - DTOs organized by feature in Entity projects
   - Controllers only in Web.Core projects

2. **Frontend:**
   - Views organized by module matching backend
   - Components in `components/` or co-located with views
   - API services mirror backend controllers

---

## Testing

### Backend Testing

Test projects are located in `api/tests/`:

```powershell
# Run all tests
dotnet test api/tests/

# Run specific test project
dotnet test api/tests/Poxiao.UnitTests/
dotnet test api/tests/Poxiao.IntegrationTests/
```

### Frontend Testing

```powershell
cd web

# Run unit tests
pnpm test

# Type checking
pnpm type:check
```

---

## Deployment

### Docker Deployment

1. **Build images**
   ```powershell
   docker build -t lm-api ./api
   docker build -t lm-web ./web
   ```

2. **Deploy with Docker Compose**
   ```powershell
   # Production deployment
   docker-compose up -d
   ```

### Manual Deployment

1. **Backend**
   ```powershell
   cd api/src/application/Poxiao.API.Entry
   dotnet publish -c Release -o ./publish
   ```

2. **Frontend**
   ```powershell
   cd web
   pnpm build
   # Output in web/dist/
   ```

---

## AI Services Integration

The system integrates with AI services for intelligent features:

### Required Services

| Service | Port | Purpose |
|---------|------|---------|
| Qdrant | 6333 | Vector database for similarity search |
| TEI | 8081 | Text embeddings (bge-m3 model) |
| vLLM | 8082 | LLM inference (Qwen2.5-7b model) |

### Configuration

AI settings are in `api/src/application/Poxiao.API.Entry/Configurations/AI.json`:

```json
{
  "AI": {
    "Qdrant": {
      "Host": "localhost",
      "Port": 6333
    },
    "TEI": {
      "Endpoint": "http://localhost:8081"
    },
    "VLLM": {
      "Endpoint": "http://localhost:8082"
    }
  }
}
```

### AI Features

- **Natural Language to SQL:** Users can query data using natural language
- **Vector Search:** Semantic search over documents and data
- **Appearance Feature Analysis:** AI-powered visual inspection analysis

---

## Common Tasks

### Adding a New Entity

1. Create entity class in `Poxiao.ModuleName.Entity/Entity/`
2. Inherit from `CLDEntityBase`
3. Add SQL migration script in `sql/`
4. Run migration against database
5. Use code generator or manually create DTOs, Service, Controller

### Adding a New API Endpoint

1. Define interface in `Poxiao.ModuleName.Interfaces/`
2. Implement in `Poxiao.ModuleName/Service/`
3. Create controller in `Poxiao.ModuleName.Web.Core/Controller/` (if needed)
4. Or use `[DynamicApiController]` attribute for auto-generation

### Adding a New Frontend Page

1. Create Vue component in `web/src/views/[module]/`
2. Add route in `web/src/router/routes/modules/[module].ts`
3. Create API service in `web/src/api/[module].ts`
4. Add menu item in system (if needed)

### Code Generation

The system has a powerful code generator:

1. Go to "代码生成" (Code Generation) in the UI
2. Select database table
3. Configure generation options
4. Download generated code
5. Copy to appropriate locations in project

### Lab Dashboard (生产驾驶舱)

The lab dashboard is the home page for the lab module, providing real-time production and quality monitoring:

**Location:** `web/src/views/lab/dashboard/`

**Components:**
- `KpiCards.vue` - KPI cards showing qualification rate, production output, lamination coefficient, and warnings
- `QualityDistribution.vue` - Donut chart for quality grade distribution
- `LaminationTrend.vue` - Line chart with confidence bands for lamination coefficient trends
- `DefectTop5.vue` - Horizontal bar chart for top 5 defect types
- `ProductionHeatmap.vue` - Heatmap showing quality by day/hour
- `ThicknessCorrelation.vue` - Scatter plot showing thickness vs lamination coefficient
- `AiAssistant.vue` - Floating AI assistant chat widget

**Features:**
- Responsive grid layout (4 → 2 → 1 columns based on screen width)
- Interactive charts with ECharts 5
- Real-time data refresh capability
- Date range filtering
- AI-powered assistant for data analysis

---

## Important Notes

### Multi-Database Compatibility

When writing SQL or using SqlSugar:
- Consider MySQL, SQL Server, and Oracle compatibility
- Use SqlSugar's cross-database methods when possible
- Test with target database before committing

### File Uploads

- Max request body size: 50MB (configured in Program.cs)
- Uploads stored in `api/src/application/Poxiao.API.Entry/uploads/`

### Logging

- Serilog is configured for structured logging
- Logs are written to both console and file
- File logs location: `api/src/application/Poxiao.API.Entry/logs/`

### Security Considerations

- JWT tokens for authentication
- Sensitive words filtering configured
- CORS configured for API access
- Tenant isolation for multi-tenant scenarios

---

## Resources

- **Documentation:** `doc/` directory
- **SQL Migrations:** `sql/` directory
- **Scripts:** `scripts/` directory
- **API Testing:** `api/test-color-api.http` (REST Client format)

---

## Getting Help

When working on this codebase:

1. Check `CLAUDE.md` for additional context
2. Review `.cursorrules` for database conventions
3. Look at existing implementations in `lab/` module for reference
4. Check `web/README.md` for frontend-specific guidance
