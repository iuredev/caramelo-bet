# 🎰 CarameloBet

> Multiplayer iGaming platform featuring real-time European Roulette, built with ASP.NET Core .NET 10 and a production-grade architecture.

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat-square&logo=dotnet)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat-square&logo=postgresql)
![Redis](https://img.shields.io/badge/Redis-7-DC382D?style=flat-square&logo=redis)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.13-FF6600?style=flat-square&logo=rabbitmq)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square&logo=docker)

---

## Overview

CarameloBet is a portfolio project that demonstrates a production-grade iGaming platform. It features a multiplayer European Roulette game where players connect in real-time, place bets, and watch the wheel spin together.

The project was designed with a full system design process — requirements gathering, high-level design, low-level design — before a single line of code was written. Every architectural decision has a documented reason behind it.

---

## Features

- 🎡 **Multiplayer European Roulette** — real-time, multiple players per table
- 💰 **Virtual Wallet** — credits system with full transaction history
- 🔐 **Authentication** — JWT with refresh tokens and RBAC
- ⚡ **Real-time** — SignalR WebSocket for live game events
- 📊 **Observability** — structured logging, metrics, and distributed tracing
- 🛡️ **Rate Limiting** — Redis-backed request throttling
- 📋 **Audit Trail** — every bet, round, and transaction permanently recorded

---

## Architecture

CarameloBet follows a **Modular Monolith** architecture — clear service boundaries without the operational overhead of microservices.

```
Client (Next.js)
    ↕ HTTP / WebSocket
Nginx (Load Balancer)
    ↕
YARP (API Gateway) — JWT validation, Rate limiting
    ↕ HTTP
┌─────────────────────────────────────────┐
│  Auth Service  │  Wallet Service         │
│  Game Service  │  History Service        │
└─────────────────────────────────────────┘
    ↕                    ↕
  Redis              RabbitMQ
(Round state,      (Async events)
 Rate limits)           ↕
                  Wallet Service
                  History Service
    ↕
PostgreSQL (Primary + Replica)
```

### Services

| Service | Responsibility |
|---|---|
| **Auth Service** | Registration, login, JWT, RBAC |
| **Wallet Service** | Balance, deposits, withdrawals, payouts |
| **Game Service** | Tables, rounds, bet placement |
| **Round Worker** | Autonomous round lifecycle engine |
| **History Service** | Audit trail, reports, bet history |

### Round Lifecycle

Every table runs a Round Worker — an autonomous background service that drives the round lifecycle independently of player actions:

```
Create round → Open bets (30s) → Close bets → Spin (RNG) →
Calculate winners → Pay via queue → Save to DB → Broadcast → Repeat
```

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | Next.js 15 + React |
| Backend | ASP.NET Core (.NET 10) |
| API Gateway | YARP |
| Load Balancer | Nginx |
| Realtime | SignalR |
| Database | PostgreSQL 16 |
| ORM | Entity Framework Core 10 |
| Cache | Redis + StackExchange.Redis |
| Message Queue | RabbitMQ + MassTransit |
| Auth | JWT + BCrypt |
| Validation | FluentValidation |
| Mapping | Mapster |
| Logs | Serilog + Seq |
| Metrics | Prometheus + Grafana |
| Traces | OpenTelemetry + Jaeger |
| Testing | xUnit + Testcontainers |
| Email | Resend |
| Containers | Docker + Docker Compose |

---

## Database Schema

Four PostgreSQL schemas with clean separation:

```
auth.*     → users, roles, permissions, tokens (6 tables)
wallet.*   → wallets, transactions (2 tables)
game.*     → games, tables, rounds, bets (4 tables)
history.*  → events — event store pattern (1 table)
```

**Total: 13 tables**

Key design decisions:
- UUID primary keys — no sequential ID enumeration
- `decimal(18,2)` for all monetary values — never float
- Pessimistic locking on wallet debits — prevents race conditions
- Idempotency keys on transactions — prevents duplicate credits
- Soft delete with status fields — preserves audit trail
- RBAC — role-based access control with granular permissions

---

## API Contracts

| Service | Endpoints |
|---|---|
| Auth Service | 17 endpoints |
| Wallet Service | 4 endpoints + queue consumer |
| Game Service | 9 endpoints + WebSocket |
| History Service | 10 endpoints |

### Message Queue Events

```
Round Worker  → round.finished, payout.process
Wallet Service → wallet.debited, wallet.credited
Game Service  → bet.placed

History Service ← consumes all events
Wallet Service  ← consumes payout.process
```

---

## Getting Started

### Prerequisites

- Docker + Docker Compose
- .NET 10 SDK
- Node.js 20+

### Run locally

```bash
# Clone the repository
git clone https://github.com/iuredev/CarameloBet.git
cd CarameloBet

# Start all infrastructure
docker compose up -d

# Apply database migrations
dotnet ef database update --project src/CarameloBet.Infrastructure

# Run the API
dotnet run --project src/CarameloBet.API

```

### Access the services

| Service | URL |
|---|---|
| API | http://localhost:5000 |
| RabbitMQ Dashboard | http://localhost:15672 |
| Seq (Logs) | http://localhost:5341 |
| Grafana (Metrics) | http://localhost:3001 |
| Jaeger (Traces) | http://localhost:16686 |

---

## Project Structure

```
CarameloBet/
├── src/
│   ├── CarameloBet.Domain          # Entities, value objects
│   ├── CarameloBet.Application     # Use cases, CQRS, DTOs
│   ├── CarameloBet.Infrastructure  # EF Core, repos, external services
│   ├── CarameloBet.API             # ASP.NET Core, endpoints
│   ├── CarameloBet.Gateway         # YARP API Gateway
│   └── CarameloBet.Workers         # Round Worker background services
├── tests/
│   ├── CarameloBet.Domain.Tests
│   ├── CarameloBet.Application.Tests
│   └── CarameloBet.Architecture.Tests
├── frontend/                       # Next.js application
├── docker-compose.yml
└── README.md
```

---

## Key Design Decisions

**Why Modular Monolith over Microservices?**
Clean service boundaries without the operational overhead of managing multiple deployments, networks, and databases as a solo developer. The architecture is designed to extract services when needed.

**Why Redis for round state?**
The round state changes every second and is read by hundreds of players simultaneously. PostgreSQL would be overwhelmed by this frequency. Redis serves the hot data; PostgreSQL stores the permanent record after each round ends.

**Why queue for payouts instead of direct HTTP?**
If the server crashes mid-payout, HTTP calls are lost. Queue messages survive failures and are redelivered. Combined with idempotency keys, this guarantees every winner is paid exactly once.

**Why pessimistic locking on wallet debits?**
Two concurrent bet requests could both read the same balance and both succeed, resulting in a negative balance. Pessimistic locking serializes access to the wallet row, preventing race conditions on financial data.

---

## Roadmap

- [ ] Slots game (solo, instant result)
- [ ] Aviator crash game
- [ ] Real payment provider (Stripe / Pix)
- [ ] Multi-currency support
- [ ] Kubernetes deployment
- [ ] Admin analytics dashboard

---

## Author

**Iure** — Full Stack Developer  
[GitHub](https://github.com/iuredev) · [LinkedIn](https://linkedin.com/in/iure-silva) · [Portfolio](https://iure.dev)

---

*Built as a portfolio project to demonstrate production-grade system design and .NET development for the European iGaming market.*
