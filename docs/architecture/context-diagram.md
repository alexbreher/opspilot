```markdown
# OpsPilot System Context

OpsPilot is a multi-tenant SaaS platform designed to manage operational reliability processes.

## Actors

User
- Engineers
- SRE teams
- Platform teams

## External Systems

Azure Kubernetes Service
- Hosts application services

Azure Service Bus
- Event-driven communication

PostgreSQL
- Application data storage

Azure Monitor / Application Insights
- Observability

## System Components

Frontend
- React + TypeScript UI

API
- .NET 8 Web API

Worker Services
- Background processing
- Event handling

Infrastructure
- Terraform
- Kubernetes

## High-Level Flow

User → React UI → API → Database

Events → Service Bus → Worker → Database

Telemetry → Application Insights