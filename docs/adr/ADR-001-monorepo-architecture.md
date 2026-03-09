# ADR-001: Monorepo Architecture

Date: 2026-03-05

## Status
Accepted

## Context

OpsPilot is a portfolio SaaS platform designed to simulate a real production environment.

The system contains multiple components:

- Web API (.NET 8)
- Background workers
- React frontend
- Infrastructure as Code
- Documentation

A decision must be made regarding repository structure.

Options considered:

1. Monorepo (single repository)
2. Polyrepo (multiple repositories)

## Decision

We will use a **monorepo** architecture.

All components will be stored in a single repository:
opspilot/
 ├── src/
 │   ├── api/
 │   ├── worker/
 │   └── web/
 │
 ├── infra/
 │   ├── aks/
 │   └── terraform/
 │
 └── docs/