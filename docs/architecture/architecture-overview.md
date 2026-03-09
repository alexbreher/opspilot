# OpsPilot Architecture Overview

OpsPilot follows a cloud-native architecture.

## Design Goals

- High observability
- Reliability-first design
- Infrastructure as Code
- Event-driven communication
- Containerized workloads

## Core Architecture Principles

### Modular Services
Backend services are modularized by domain.

### Event-Driven Workflows
Asynchronous processing via Service Bus.

### Infrastructure as Code
All cloud infrastructure defined using Terraform.

### Containerization
Applications run inside containers deployed to AKS.

### Observability
OpenTelemetry instrumentation provides:

- Traces
- Metrics
- Logs

Exported to Azure Monitor / Application Insights.

## Deployment Model

Services are containerized and deployed to:

Azure Kubernetes Service (AKS)

Deployment pipelines will be implemented using GitHub Actions.

## Future Enhancements

- Multi-tenant RBAC
- Incident management APIs
- Runbook automation
- SLO tracking