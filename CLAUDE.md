# Marketing Assistant MVP

Follow global code quality standards from ~/.claude/CLAUDE.md.

## Build & Run

```bash
# Backend
dotnet build                                          # Build solution
dotnet run --project src/MarketingAssistant.Api        # Run API (http://localhost:5000)
dotnet test                                           # Run all tests

# Frontend
cd client && npx ng serve                             # Angular dev server (http://localhost:4200)
cd client && npx ng build                             # Production build

# Database
dotnet ef migrations add <Name> --project src/MarketingAssistant.Infrastructure --startup-project src/MarketingAssistant.Infrastructure
dotnet ef database update --project src/MarketingAssistant.Infrastructure --startup-project src/MarketingAssistant.Infrastructure
```

## Architecture

- **Api** - ASP.NET Core Web API (entry point, controllers, SignalR hub, middleware)
- **Core** - Domain models, interfaces, enums, DTOs (no dependencies)
- **Infrastructure** - EF Core (SQLite), external API connectors, AI service, business services
- **Discord** - Discord bot as IHostedService
- **Scheduling** - Background jobs (briefings, alert checks)

Dependency graph: `Api -> Core, Infrastructure, Discord, Scheduling | Infrastructure -> Core | Discord -> Core, Infrastructure | Scheduling -> Core`

## Key Patterns

- Mock connectors toggled via `DevMode:UseMockConnectors` in appsettings.json
- Anthropic API via raw HttpClient (no SDK NuGet), configurable base_url for Max proxy
- All AI output in Dutch (Nederlands)
- EF Core enums stored as strings
- Angular uses standalone components, lazy-loaded routes, Angular Material
- SignalR hub at `/hubs/dashboard` for real-time updates
- Proxy config in `client/proxy.conf.json` maps `/api` and `/hubs` to .NET backend

## API Endpoints

- `GET /api/health` - Health check
- `GET /api/dashboard` - KPIs, alerts, pending action count
- `GET /api/briefings` - List briefings
- `GET /api/briefings/{id}` - Briefing detail with actions
- `POST /api/briefings/generate` - Generate new briefing
- `GET /api/actions` - Action queue (filter: ?status=Pending)
- `POST /api/actions/{id}/approve` - Approve action
- `POST /api/actions/{id}/reject` - Reject action
- `GET /api/settings` - Connector configuration
- `PUT /api/settings` - Update settings
