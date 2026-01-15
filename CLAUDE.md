# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This is a **Laboratory Data Analysis System** (检测室数据分析系统) - an enterprise-level application with AI-powered features for laboratory information management. The system supports natural language queries for data analysis (NL to SQL) and vector search capabilities for semantic matching.

## Technology Stack

### Frontend (`web/`)
- **Framework**: Vue 3.3.4 + TypeScript
- **Build Tool**: Vite 4.4.9
- **UI Library**: Ant Design Vue 3.2.20
- **State Management**: Pinia 2.1.6
- **Styling**: TailwindCSS + WindiCSS
- **Package Manager**: pnpm (required)

### Backend (`api/`)
- **Framework**: .NET 10.0 (ASP.NET Core)
- **Architecture**: Modular monolithic with clean architecture
- **ORM**: SqlSugar (with Dapper support)
- **Database**: SQL Server (primary), MySQL, Oracle support
- **Authentication**: JWT Bearer tokens
- **Additional**: Redis, RabbitMQ, Serilog, Mapster

### Infrastructure
- **AI Services**: Qdrant (vector DB), TEI (text embeddings), vLLM (LLM inference)
- **Containerization**: Docker with docker-compose
- **File Storage**: Local or MinIO/S3 compatible

## Essential Commands

### Frontend Development
```bash
cd web

# Install dependencies (requires pnpm)
pnpm install

# Development server with hot reload
pnpm dev

# Build for production
pnpm build

# Run linting
pnpm run lint:eslint
pnpm run lint:prettier
pnpm run lint:stylelint

# Type checking
pnpm run type:check
```

### Backend Development
```bash
# Method 1: Use batch script (recommended)
cd E:\project\2025\lm
run_api_dev.bat

# Method 2: Manual command
cd api\src\application\Poxiao.API.Entry
set ASPNETCORE_ENVIRONMENT=dev
dotnet watch run --launch-profile dev
```

### Infrastructure Services
```bash
# Start all infrastructure services (MySQL, Redis, RabbitMQ, AI services)
docker-compose up -d

# Individual services
docker-compose up -d qdrant      # Vector database
docker-compose up -d tei-embedding # Text embeddings service
docker-compose up -d vllm        # LLM inference service
```

### Mock Server (for frontend development)
```bash
cd web\mock
npm install
npm run mock
```

## Architecture Overview

### Backend Architecture (`api/`)
The backend follows a **modular monolithic** architecture with clean architecture principles:

```
api/src/modularity/
├── Poxiao.Core/           # Core shared components
├── Poxiao.OAuth/          # Authentication module
├── Poxiao.System/         # System management
├── Poxiao.Message/        # Message center
├── Poxiao.Task/           # Task scheduling
├── Poxiao.WorkFlow/       # Workflow engine
├── Poxiao.VisualDev/      # Low-code development
├── Poxiao.CodeGenerator/  # Code generation
├── Poxiao.VisualData/     # Visual data analysis
├── Poxiao.Lab/            # Laboratory management
└── [Other modules]/       # Additional business modules
```

Each module typically contains:
- `Entity/` - Domain entities
- `Service/` - Business logic
- `Controller/` - API endpoints
- `Dto/` - Data transfer objects

### Frontend Architecture (`web/`)
The frontend uses **feature-based organization** with Vue 3 Composition API:

```
web/src/
├── components/      # Reusable UI components
├── views/          # Page components (organized by feature)
├── hooks/          # Composable functions
├── utils/          # Utility functions
├── api/            # API client and endpoints
├── router/         # Route definitions
├── store/          # Pinia stores
└── assets/         # Static assets
```

Key frontend patterns:
- API integration via centralized HTTP client
- Component registration through auto-import
- Feature modules in `views/` directory
- Composables for shared logic

## Key Development Patterns

### Frontend Patterns
1. **API Calls**: Use the centralized API client in `src/api/`
2. **Component Registration**: Auto-import enabled, place components in `src/components/`
3. **Routing**: Define routes in `src/router/routes/modules/`
4. **State Management**: Use Pinia stores in `src/store/modules/`
5. **Icons**: Use `@ant-design/icons-vue` or Iconify

### Backend Patterns
1. **Controllers**: Inherit from base controller classes
2. **Services**: Implement interface contracts
3. **DTOs**: Use for API request/response objects
4. **Entities**: Define database models with SqlSugar attributes
5. **Module Registration**: Each module self-registers in the pipeline

## Common Issues and Solutions

### Modal Dialog Issues
The project has documented modal closing issues. When working with modals:
- Check `modal-close-solution.md` and `modal-close-fix.md` for specific solutions
- Ensure proper event handling and state management
- Test modal behavior across different browsers

### Custom Sorting Dialog
If the custom sorting dialog doesn't open:
1. Check browser console for errors
2. Verify `CustomSortControl` and `CustomSortEditor` components are properly imported
3. Ensure `debug-sort-modal.js` is loaded for debugging assistance
4. Check `DEBUG_INSTRUCTIONS.md` for detailed debugging steps

### Build Issues
- Frontend: Ensure pnpm is installed globally (`npm install pnpm -g`)
- Backend: Use Visual Studio 2022 or ensure .NET 6.0 SDK is installed
- Database: Configure connection strings in `appsettings.json`

## Testing Approach

Currently, the project has minimal automated testing:
- Unit test projects exist but are minimal
- Mock server available for frontend development
- Manual testing is primary method

When adding new features:
1. Test manually in development environment
2. Use mock server for API development
3. Follow existing patterns for consistency

## Environment Configuration

### Frontend Environment Files
- `.env.development` - Development configuration
- `.env.production` - Production configuration
- Configure API proxy and other settings here

### Backend Configuration
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- Configure database connections, Redis, RabbitMQ here

## Important Notes

1. **Chinese Primary**: The system is primarily designed for Chinese users - UI text, comments, and documentation are mainly in Chinese
2. **Multi-database Support**: Always consider compatibility with MySQL, SQL Server, and Oracle when writing database queries
3. **AI Integration**: The system integrates with AI services (Qdrant, TEI, vLLM) - ensure these services are running for full functionality
4. **File Storage**: Supports both local file system and MinIO/S3 - configure appropriately for deployment
5. **Module System**: New features should follow the modular architecture pattern established in the codebase