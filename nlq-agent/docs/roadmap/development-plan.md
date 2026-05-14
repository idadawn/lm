# NLQ-Agent Development Plan

## Executive Summary

This plan outlines the development roadmap for NLQ-Agent, a multi-agent Natural Language Query system for industrial quality data. The project is currently in MVP phase with QueryAgent fully implemented; remaining agents (RootCauseAgent, InsightAgent, HypothesisAgent) and supporting tools need development.

**Current Status:**
- ✅ QueryAgent: Fully functional with metric queries, judgment rules, product specs
- ✅ Core infrastructure: LangGraph, SQL safety, LLM factory, knowledge graph (Neo4j)
- ⚠️ Other Agents: Skeleton only, need full implementation
- ❌ Tools: Missing metric_tools, graph_tools, stats_tools, chart_tools
- ❌ Frontend: Only TrendLine chart exists, missing 3 chart types
- ❌ Mobile: Empty directory, needs full setup
- ⚠️ Tests: Backend unit tests exist, no agent flow or frontend tests

## Priority Phases

### Phase 1: Backend Foundation (Week 1-2)
**Goal:** Complete backend tools and agents for core functionality

**Dependencies:** None (can start immediately)

**Tasks:**
1. **metric_tools.py** (12h) - Advanced metric queries
2. **stats_tools.py** (10h) - Statistical analysis for InsightAgent/HypothesisAgent
3. **graph_tools.py** (14h) - Knowledge graph traversal for RootCauseAgent
4. **chart_tools.py** (8h) - Chart configuration and selection
5. **RootCauseAgent** (16h) - Root cause analysis using graph_tools

**Total Estimated Time:** 60 hours (~1.5 weeks)

**Deliverables:**
- 4 new tool modules with ≥80% test coverage
- RootCauseAgent implementation with ≥75% test coverage
- Integration tests for root cause analysis flows

**Blocking:** Phase 2 (InsightAgent depends on stats_tools)

### Phase 2: Advanced Analytics (Week 2-3)
**Goal:** Implement pattern detection and hypothesis testing

**Dependencies:** Phase 1 (stats_tools, metric_tools)

**Tasks:**
1. **InsightAgent** (16h) - Pattern detection and anomaly discovery
2. **HypothesisAgent** (14h) - What-if scenario analysis
3. **Agent Flow Tests** (12h) - Integration tests for all agents
4. **Chart Components** (12h) - MetricGauge, GradeDistribution, ComparisonBar

**Total Estimated Time:** 54 hours (~1.5 weeks)

**Deliverables:**
- InsightAgent and HypothesisAgent implementations
- 3 new chart components with unit tests
- Agent flow integration tests covering all scenarios

**Blocking:** Phase 3 (frontend depends on chart components)

### Phase 3: Frontend & Mobile (Week 4-5)
**Goal:** Complete UI and mobile app

**Dependencies:** Phase 2 (chart components, all agents)

**Tasks:**
1. **Frontend Component Tests** (10h) - Test all chart components
2. **Mobile App Setup** (24h) - uni-app x foundation, SSE client, chat UI
3. **API Documentation** (8h) - Comprehensive API docs

**Total Estimated Time:** 42 hours (~1 week)

**Deliverables:**
- Frontend test suite with ≥80% coverage
- Working mobile app with basic chat and chart display
- Complete API documentation

**Blocking:** None (final phase)

## Task Dependency Graph

```
Phase 1 (Foundation)
├── metric_tools.py ─────────────┐
├── stats_tools.py ──────────────┤
├── graph_tools.py ────┐         ├──┐
├── chart_tools.py ────┼─────────┤  │
└── RootCauseAgent ────┘         │  │
                                  │  │
Phase 2 (Analytics)               │  │
├── InsightAgent ─────────────────┘  │
│   (depends on stats_tools)        │
├── HypothesisAgent ─────────────────┘
│   (depends on stats_tools, metric_tools)
├── Agent Flow Tests ──────────────────┐
│   (depends on all agents)            │
└── Chart Components ──────────────────┤
    (depends on chart_tools)           │
                                        │
Phase 3 (UI & Mobile)                  │
├── Frontend Component Tests ──────────┘
│   (depends on chart components)
├── Mobile App Setup ──────────────┐
│   (independent)                   │
└── API Documentation ──────────────┘
    (depends on all above)
```

## Parallel Execution Opportunities

**Maximum Parallelism (3+ developers):**
- Dev 1: metric_tools, stats_tools (Phase 1)
- Dev 2: graph_tools, RootCauseAgent (Phase 1)
- Dev 3: chart_tools, Chart Components (can start Phase 1)

**Medium Parallelism (2 developers):**
- Dev 1: Backend tools (Phase 1)
- Dev 2: Frontend components (can start in parallel with Phase 2)

**Single Developer:**
- Follow Phase 1 → Phase 2 → Phase 3 sequence
- Chart components can be done in parallel with agents (Phase 2)

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|------------|
| Neo4j knowledge graph incomplete | High | Mock graph_tools initially, integrate real KG later |
| Statistical analysis complexity | Medium | Start with simple outlier detection, add advanced stats iteratively |
| uni-app x learning curve | Medium | Allocate extra time for mobile app, consider web-only MVP |
| Test coverage requirements | Low | Use pytest/vitest effectively, aim for 80%+ |

## Testing Strategy

**Backend:**
- Unit tests: ≥80% for tools, ≥75% for agents
- Integration tests: Agent flow tests for all scenarios
- Use pytest with asyncio support
- Mock LLM and database for deterministic tests

**Frontend:**
- Unit tests: ≥80% for components
- Use Vitest with @testing-library/react
- Mock @ant-design/charts to avoid Canvas rendering in tests
- Test ChartDescriptor handling and error cases

**Coverage Tracking:**
```bash
# Backend
cd services/agent-api
uv run pytest --cov=app --cov-report=term-missing

# Frontend
pnpm --filter web test:coverage
```

## Success Criteria

**Phase 1 Complete When:**
- ✅ All 4 tool modules implemented and tested (≥80% coverage)
- ✅ RootCauseAgent handles "why" questions with causal chains
- ✅ Integration tests pass for root cause analysis

**Phase 2 Complete When:**
- ✅ InsightAgent detects anomalies and patterns
- ✅ HypothesisAgent performs what-if analysis
- ✅ All 4 chart components rendering correctly
- ✅ Agent flow tests cover all scenarios

**Phase 3 Complete When:**
- ✅ Frontend test suite passes (≥80% coverage)
- ✅ Mobile app can connect and display chat/charts
- ✅ API documentation is comprehensive and accurate

## Resource Allocation

**Total Estimated Effort:** 156 hours (~4 weeks for 1 developer)

**With 3 Developers:** ~1.5 weeks (parallel execution)
**With 2 Developers:** ~2.5 weeks
**With 1 Developer:** ~4 weeks

**Skill Requirements:**
- Backend Dev: Python, LangGraph, Neo4j, statistics
- Frontend Dev: Next.js, React, @ant-design/charts, Vitest
- Mobile Dev: uni-app x, UTS (optional, can be same as frontend)

## Next Steps

1. **Review and Prioritize:** Confirm this plan aligns with project goals
2. **Assign Tasks:** Allocate tasks to developers based on skills
3. **Set Up Tracking:** Use GitHub Projects or similar to track progress
4. **Start Phase 1:** Begin with metric_tools and stats_tools (no dependencies)

## Maintenance & Future Enhancements

**Post-MVP Enhancements:**
- Real-time streaming of tool execution progress
- Advanced visualization (heatmaps, scatter plots)
- Multi-language support (beyond Chinese)
- User feedback collection and learning
- Performance optimization (caching, query optimization)

**Technical Debt to Address:**
- Add comprehensive error handling
- Implement request rate limiting
- Add request/response logging for debugging
- Optimize database queries with proper indexing
- Add E2E tests with Playwright

---

**Document Version:** 1.0
**Last Updated:** 2026-03-17
**Status:** Ready for Review
