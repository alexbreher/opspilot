# OpsPilot

OpsPilot is a multi-tenant SaaS portfolio project focused on reliability engineering: incident management, runbooks, postmortems, and SLO tracking.

## Tech Stack
- Backend: .NET 8 (Web API) + .NET Worker
- Frontend: React + TypeScript (Vite)
- Messaging (planned): Azure Service Bus
- Data (planned): PostgreSQL
- Infra (planned): AKS + Terraform
- Observability (planned): OpenTelemetry + Azure Monitor / Application Insights

## Architecture Intent
Core principles:
- Multi-tenant architecture
- Role-based access control (RBAC)
- Event-driven workflows
- Operability-first design (health checks, logs, telemetry)
- Infrastructure as Code with Terraform
- Containerized workloads deployed to AKS

Planned bounded domains:
- Incidents
- Runbooks
- Postmortems
- SLO Tracking
- Audit Trail

---

## Local Development

### Run API
```bash
dotnet run --project src/api/OpsPilot.Api/OpsPilot.Api.csproj

Swagger

http://localhost:5033/swagger

Health

http://localhost:5033/health

Run Worker
dotnet run --project src/worker/OpsPilot.Worker/OpsPilot.Worker.csproj
Run Web UI
cd src/web/opspilot-web
npm install
npm run dev

opspilot/
├── src/
│   ├── api/            # .NET Web API
│   ├── worker/         # Background workers
│   └── web/            # React frontend
├── infra/
│   ├── aks/            # Kubernetes manifests
│   └── terraform/      # Infrastructure as Code
└── docs/
    ├── architecture/   # C4 diagrams
    └── adr/            # Architecture Decision Records