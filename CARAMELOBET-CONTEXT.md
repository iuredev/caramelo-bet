# CarameloBet — Complete Project Context

> Handoff document with all context to continue work in any chat session. Contains everything: project context, completed Phases 1-2, and detailed plans for Phases 3-8.

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Developer Profile](#developer-profile)
3. [Architecture](#architecture)
4. [Tech Stack](#tech-stack)
5. [Database Schema](#database-schema)
6. [Project Structure](#project-structure)
7. [API Contracts](#api-contracts)
8. [Reliability Patterns](#reliability-patterns)
9. [Phase 1 — Foundation (COMPLETE)](#phase-1--foundation-complete)
10. [Phase 2 — Auth Service (COMPLETE)](#phase-2--auth-service--complete-)
11. [Phase 3 — Wallet Service](#phase-3--wallet-service)
12. [Phase 4 — Game Service](#phase-4--game-service)
13. [Phase 5 — Round Worker + Rust RNG](#phase-5--round-worker--rust-rng)
14. [Phase 6 — History Service](#phase-6--history-service)
15. [Phase 7 — Frontend + Go WebSocket Server](#phase-7--frontend--go-websocket-server)
16. [Phase 8 — Polish + Tests + Deploy](#phase-8--polish--tests--deploy)
17. [Critical Gotchas](#critical-gotchas)
18. [Quick Commands Reference](#quick-commands-reference)
19. [Continuation Prompt](#continuation-prompt)

---

## Project Overview

**CarameloBet** is a multiplayer iGaming portfolio project targeting senior .NET / iGaming roles in Europe (specifically Betsson Malaga). Built to demonstrate production-grade system design, distributed systems, and polyglot architecture.

**Two games:**
- **European Roulette** — multiplayer, even-chance bets only (red/black, odd/even, low/high, all 1:1)
- **Aviator** — multiplayer crash game, cash out before crash

**Goal:** Portfolio piece to land a senior .NET role in EU iGaming. Aligns with Spain relocation plan (2-year naturalization via Ibero-American treaty).

---

## Developer Profile

- **Name:** Iure (GitHub: `iuredev`)
- **Background:** 6 years dev experience, Node.js/TypeScript background, transitioning to .NET
- **English level:** B1/B2, daily 1-to-1 classes
- **Important:** Responds in English ONLY. Never answer if asked in Portuguese.
- **Learning style:** Wants to understand deeply, not copy-paste. Prefers minimal folder complexity. Types code manually.
- **Preferences:** Strict no-em-dash/no-dash rule on outputs. No emojis unless asked.

---

## Architecture

**Modular Monolith for .NET backend + polyglot microservices for RNG and WebSocket.**

```
Client (Next.js)
    ↕ HTTP                    ↕ WebSocket
Nginx (Load Balancer, port 80)
    ↕                              ↕
YARP API Gateway          Go WebSocket Server
JWT + Redis Rate Limiting   (CarameloBet-WS)
    ↕                              ↕
┌────────────────────┐        RabbitMQ
│  Auth Service      │   (consumes events)
│  Wallet Service    │
│  Game Service      │
│  History Service   │
└────────────────────┘
    ↕ gRPC                    ↕
Rust RNG Service          RabbitMQ
(CarameloBet-RNG)    (publishes events)
    ↕
PostgreSQL + Redis
```

### Repositories (4 total)

```
CarameloBet         → .NET 10 backend (in progress, private)
CarameloBet-Web     → Next.js 15 frontend (not started)
CarameloBet-RNG     → Rust + gRPC RNG service (not started)
CarameloBet-WS      → Go WebSocket server (not started, REPLACES SignalR)
```

**Important decision:** Go WebSocket server REPLACES SignalR. .NET backend only publishes events to RabbitMQ; Go server consumes and broadcasts to players.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core .NET 10 |
| API Gateway | YARP |
| Load Balancer | Nginx |
| WebSocket Server | Go (separate repo) |
| RNG Service | Rust + gRPC (separate repo) |
| Frontend | Next.js 15 + React (separate repo) |
| Database | PostgreSQL (latest) |
| ORM | EF Core 10 + EFCore.NamingConventions (snake_case) |
| Cache | Redis 7 + StackExchange.Redis |
| Message Queue | RabbitMQ + MassTransit v8 (NOT v9 — paid license) |
| Auth | JWT + BCrypt |
| Validation | FluentValidation |
| Mapping | Mapster (NO MediatR) |
| Logs | Serilog + Seq |
| Metrics | Prometheus + Grafana |
| Traces | OpenTelemetry + Jaeger (SPM enabled) |
| Testing | xUnit + Testcontainers |
| Email | Resend |
| Containers | Docker + Docker Compose |

---

## Database Schema — 14 tables across 4 schemas

```
auth.*     → users, roles, permissions, user_roles, role_permissions,
              refresh_tokens, password_reset_tokens (7 tables)
wallet.*   → wallets, transactions (2 tables)
game.*     → games, tables, rounds, bets (4 tables)
history.*  → events (1 table)
```

### Key design decisions

- UUID primary keys
- `decimal(18,2)` for all monetary values
- Pessimistic locking on wallet debits (SELECT FOR UPDATE)
- Idempotency keys on transactions (prevents duplicate credits)
- Outbox pattern planned for Phases 3/4/5
- Soft delete via status fields
- `player_id` in wallet/game (only players), `user_id` in history (any user)
- Wallet starts at 0 balance
- snake_case column naming via EFCore.NamingConventions

### Bet types

```
Roulette: red, black, odd, even, low, high  (all pay 1:1)
Aviator:  cashout                            (pays current multiplier)
```

### Round Lifecycle

**Roulette:**
```
Create round (PostgreSQL)
→ Open bets 30s (Redis + RabbitMQ broadcast to Go WS)
→ Close bets
→ Call Rust RNG via gRPC → result 0-36
→ Calculate winners (even chance bets only)
→ Publish payout.process to RabbitMQ → Wallet Service credits winners
→ Save to PostgreSQL
→ Publish round.finished → Go WS broadcasts result
→ Wait 5s → repeat
```

**Aviator:** Similar with multiplier streaming via RabbitMQ → Go WS → players.

---

## Project Structure

```
CarameloBet/
├── src/
│   ├── CarameloBet.Domain          # Entities, enums, exceptions
│   ├── CarameloBet.Application     # Use cases, DTOs, validators, mappings
│   ├── CarameloBet.Infrastructure  # EF Core, repositories, Redis, messaging
│   ├── CarameloBet.API             # ASP.NET Core endpoints
│   ├── CarameloBet.Gateway         # YARP, JWT, Redis rate limiting
│   └── CarameloBet.Workers         # Round Workers
├── tests/
│   ├── CarameloBet.Domain.Tests
│   ├── CarameloBet.Application.Tests
│   └── CarameloBet.Architecture.Tests
├── infra/
│   ├── prometheus/prometheus.yml
│   ├── jaeger/jaeger-config.yml
│   └── nginx/nginx.conf
├── docker-compose.yml
├── .editorconfig
└── CarameloBet.slnx
```

### Dependency chain
```
Domain ← Application ← Infrastructure ← API/Workers
Gateway → no internal dependencies
```

### Folder organization (max 2 levels)

```
Domain/Entities/Auth/User.cs
Domain/Entities/Auth/Role.cs       (Role + UserRole + RolePermission)
Domain/Entities/Auth/Tokens.cs     (RefreshToken + PasswordResetToken)
Domain/Entities/Wallet/Wallet.cs
Domain/Entities/Game/Game.cs       (Game + Table)
Domain/Entities/History/HistoryEvent.cs

Infrastructure/Persistence/Auth/AuthDbContext.cs
Infrastructure/Persistence/Auth/Configurations/UserConfiguration.cs
```

### Entity pattern — Factory + Private Constructor

```csharp
public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    // all properties private set

    private User() { }  // EF Core uses reflection

    public static User Create(string name, string email, string passwordHash)
    {
        return new User { /* ... */ };
    }
}
```

---

## API Contracts

| Service | Endpoints |
|---|---|
| Auth Service | 19 endpoints |
| Wallet Service | 4 endpoints + queue consumer |
| Game Service | 9 endpoints |
| History Service | 10 endpoints |

### Status codes
- 401 — bad credentials
- 403 — banned/no permission
- 422 — business rule violation

### Message Queue Events

```
Round Worker   → round.finished, payout.process, multiplier.update
Wallet Service → wallet.debited, wallet.credited
Game Service   → bet.placed

History Service     ← all events
Wallet Service      ← payout.process
Go WebSocket Server ← round.finished, multiplier.update
```

### Response envelope (all endpoints)

```json
{
  "success": true,
  "data": { /* actual data */ },
  "error": null
}
```

---

## Reliability Patterns

### Retry (MassTransit)
```
3 incremental retries: 1s, 3s, 5s
```

### Dead Letter Queue (MassTransit)
```
Delayed redelivery: 5m, 15m, 30m → DLQ
```

### Idempotency
```
Unique constraint on idempotency_key in wallet.transactions
```

### Outbox pattern (Phases 3/4/5)
```
wallet.outbox and game.outbox tables
Events written in same transaction as business data
Background process publishes to RabbitMQ
```

---

## Phase 1 — Foundation — COMPLETE ✅

### What's done

```
✅ Solution structure with .slnx
✅ 6 src projects + 3 test projects with proper references
✅ Docker Compose with 9 containers running
✅ NuGet packages installed
✅ Folder structure organized by domain
✅ Serilog + Seq (port 8081)
✅ OpenTelemetry + Jaeger v2.6.0 with SPM (port 16686)
✅ Prometheus (9090) + Grafana (3001, dashboards created)
✅ MassTransit v8 + RabbitMQ + Retry + DLQ
✅ FluentValidation + Mapster
✅ Global exception handler (IExceptionHandler)
✅ Base API response envelope (ApiResponse<T>)
✅ YARP Gateway (port 5186) routing to API (port 5057)
✅ JWT middleware (extension method pattern, secret in User Secrets)
✅ Redis rate limiting (custom middleware, IP-based, Redis-backed)
✅ Nginx load balancer (port 80, WebSocket sticky sessions)
✅ CORS policy
✅ .editorconfig
✅ 14 Domain entities created
✅ 4 DbContexts (one per schema)
✅ All entity configurations with indexes
✅ Database migrations created and applied
✅ snake_case naming convention
✅ Seed data (4 roles, 8 permissions, 2 games, 3 tables)
```

### Local Secret Configuration

```
Copy `.env.example` to `.env` and provide local-only values.
Set `Jwt__Secret` with at least 32 random bytes.
Set `ConnectionStrings__PostgreSQL` with the local PostgreSQL credentials.
Never commit `.env`, real passwords, or JWT secrets.
```

### Service URLs (local)

```
http://localhost          → Nginx → Gateway → API
http://localhost:5186     → Gateway directly
http://localhost:5057     → API directly
http://localhost:15672    → RabbitMQ Dashboard
http://localhost:8081     → Seq Logs
http://localhost:3001     → Grafana
http://localhost:9090     → Prometheus
http://localhost:16686    → Jaeger UI
http://localhost:5540     → RedisInsight
```

---

## Phase 2 — Auth Service — COMPLETE ✅

**Estimated time: 1-2 weeks**

The Auth Service establishes patterns all other services follow. Build this carefully.

### Endpoints implemented (19 total)

```
POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/refresh
POST   /api/auth/logout
POST   /api/auth/forgot-password
POST   /api/auth/reset-password

GET    /api/users/me
PUT    /api/users/me
PUT    /api/users/me/password

GET    /api/admin/users
GET    /api/admin/users/{id}
PUT    /api/admin/users/{id}
DELETE /api/admin/users/{id}
POST   /api/admin/users/{id}/block
DELETE /api/admin/users/{id}/block

GET    /api/admin/roles
POST   /api/admin/roles
PUT    /api/admin/roles/{id}
POST   /api/admin/users/{userId}/roles
```

### Tasks in order

1. **Build user registration**
   - DTO: `RegisterRequest { Name, Email, Password, Birthdate? }`
   - FluentValidator: email format, password strength (min 8 chars, etc)
   - Use case: `RegisterUserUseCase`
     - Check email not taken
     - Hash password with BCrypt
     - Create User entity
     - Assign "player" role (create UserRole)
     - Create Wallet (balance 0)
     - Save all in transaction
   - Repository: `IUserRepository` in Domain, `UserRepository` in Infrastructure
   - Endpoint: returns user data wrapped in ApiResponse<T>

2. **Build JWT generation service**
   - `IJwtService` in Application
   - `JwtService` in Infrastructure
   - Generates access token (15 min expiry) + refresh token (7 days)
   - Includes claims: sub (user id), email, roles, permissions

3. **Build login**
   - DTO: `LoginRequest { Email, Password }`
   - Use case: `LoginUseCase`
     - Find user by email
     - Verify password with BCrypt
     - Generate access + refresh tokens
     - Store refresh token hash in DB
   - Returns: `LoginResponse { AccessToken, RefreshToken, User }`

4. **Build refresh tokens**
   - Use case: `RefreshTokenUseCase`
     - Validate refresh token (not expired, not revoked)
     - Generate new access token
     - Rotate refresh token (revoke old, issue new)
   - Important: refresh tokens are stored hashed in DB

5. **Build logout**
   - Revoke refresh token (set revoked_at)

6. **Build forgot password flow**
   - `POST /forgot-password`: generate password reset token, send email via Resend
   - `POST /reset-password`: verify token, update password, mark token as used

7. **Build /me endpoints**
   - GET — return current user info (from JWT claims)
   - PUT — update name, email, birthdate
   - PUT password — change password

8. **Build RBAC management (admin endpoints)**
   - List/get/update/delete users
   - Block users with a reason and optional expiration; unblock users
   - List/create/update roles
   - Assign roles to users

### Patterns to establish in Phase 2

Use case pattern in Application:
```csharp
public class RegisterUserUseCase(
    IUserRepository userRepo,
    IWalletRepository walletRepo,
    IPasswordHasher hasher,
    IUnitOfWork uow)
{
    public async Task<UserResponse> ExecuteAsync(RegisterRequest request)
    {
        // 1. Validate (FluentValidation already ran)
        // 2. Business logic
        // 3. Create entities
        // 4. Save via repos
        // 5. Map to response with Mapster
        // 6. Return
    }
}
```

Repository pattern:
```csharp
// Domain/Interfaces/IUserRepository.cs
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task<bool> EmailExistsAsync(string email);
}

// Infrastructure/Repositories/Auth/UserRepository.cs
public class UserRepository(AuthDbContext context) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
        => await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    // ...
}
```

Endpoint pattern (Minimal API):
```csharp
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            RegisterUserUseCase useCase) =>
        {
            var result = await useCase.ExecuteAsync(request);
            return Results.Ok(ApiResponse<UserResponse>.Ok(result));
        });
    }
}
```

### Phase 2 deliverables

- [x] All 19 endpoints working, including dedicated block and unblock moderation
- [x] JWT validation on protected endpoints in the API and Gateway
- [x] Password reset email flow
- [x] Admin endpoints with proper RBAC checks
- [x] Registration atomically assigns the player role and creates a zero-balance wallet
- [x] Unit tests for Domain entities
- [x] Unit tests for Application use cases with fake repositories

### Phase 2 verification

Completed on 2026-07-13:

- Clean PostgreSQL database migrated successfully for all four DbContexts
- 22 smoke assertions passed across all 17 endpoints, JWT, RBAC, refresh rotation, logout, password reset, and registration side effects
- Full solution test suite passed
- Secret-pattern scan and `git diff --check` passed

---

## Phase 3 — Wallet Service

**Estimated time: 1 week**

The Wallet Service handles all money operations. Most critical for correctness.

### Endpoints to build (4 + consumer)

```
GET    /api/wallets/me/balance
POST   /api/wallets/me/deposit
POST   /api/wallets/me/withdraw
GET    /api/wallets/me/transactions
```

### Tasks in order

1. **Add Outbox table to schema**
   ```sql
   wallet.outbox
   ├── id (uuid, PK)
   ├── event_type (varchar)
   ├── payload (jsonb)
   ├── created_at (timestamp)
   ├── published_at (timestamp, nullable)
   └── attempts (int)
   ```
   Create migration and apply.

2. **Build IWalletRepository with pessimistic locking**
   ```csharp
   public async Task<Wallet?> GetForUpdateAsync(Guid playerId)
   {
       return await context.Wallets
           .FromSqlRaw("SELECT * FROM wallet.wallets WHERE player_id = {0} FOR UPDATE", playerId)
           .FirstOrDefaultAsync();
   }
   ```

3. **Build deposit endpoint**
   - DTO: `DepositRequest { Amount, IdempotencyKey }`
   - Use case: `DepositUseCase`
     - Get wallet with FOR UPDATE lock
     - Check idempotency key not used
     - Credit wallet
     - Create transaction record
     - Write event to outbox
     - Save in single transaction
   - Returns new balance

4. **Build withdraw endpoint**
   - Same pattern as deposit but with balance check
   - Throws `InsufficientBalanceException` → 422 response

5. **Build balance endpoint**
   - Simple read, returns current balance + currency

6. **Build transactions endpoint**
   - Paginated list of transactions
   - Filter by date range, type
   - Order by created_at DESC

7. **Build payout queue consumer**
   - `PayoutConsumer : IConsumer<PayoutProcessEvent>`
   - When Round Worker publishes payout.process:
     - Get wallet FOR UPDATE
     - Check idempotency
     - Credit winner
     - Save transaction
     - Write wallet.credited event to outbox
   - Idempotency key is the bet_id — same bet can't pay twice

8. **Build Outbox publisher background service**
   - `OutboxPublisherWorker : BackgroundService`
   - Polls outbox table every second
   - Publishes unpublished events to RabbitMQ
   - Marks as published_at on success
   - Retries failed publishes (increment attempts)

### Phase 3 deliverables

- All wallet endpoints working
- Pessimistic locking proven (test with concurrent requests)
- Payout consumer processing queue messages
- Outbox publisher running and publishing events
- Idempotency tested with duplicate messages

---

## Phase 4 — Game Service

**Estimated time: 1-2 weeks**

The Game Service handles tables, rounds, and bet placement.

### Endpoints to build (9 total)

```
GET    /api/games
GET    /api/tables
GET    /api/tables/{id}
GET    /api/tables/{id}/current-round

POST   /api/rounds/{roundId}/bets        # place a bet
GET    /api/rounds/{roundId}/bets        # bets on this round
GET    /api/players/me/bets              # my bet history

# Admin
POST   /api/admin/tables
PUT    /api/admin/tables/{id}
DELETE /api/admin/tables/{id}
```

### Tasks in order

1. **Add Outbox table for game schema**
   ```sql
   game.outbox (same structure as wallet.outbox)
   ```

2. **Add Redis state repository**
   ```csharp
   public interface IRoundStateRepository
   {
       Task SetRoundStateAsync(Guid roundId, RoundState state, TimeSpan ttl);
       Task<RoundState?> GetRoundStateAsync(Guid roundId);
       Task AddBetAsync(Guid roundId, Bet bet);
       Task<List<Bet>> GetBetsAsync(Guid roundId);
   }
   ```
   Implements with `IDatabase` from StackExchange.Redis.

3. **Build game listing endpoints**
   - List games
   - List tables (with current status)
   - Get table details

4. **Build current round endpoint**
   - Returns current round for a table
   - Reads from Redis (live state)
   - Includes time remaining, current phase, total bets

5. **Build place bet endpoint**
   - DTO: `PlaceBetRequest { Amount, BetType, BetValue }`
   - FluentValidator: amount > 0, bet_type valid, bet_value matches type
   - Use case: `PlaceBetUseCase`
     - Get current round from Redis
     - Check round status is "betting"
     - Check player has sufficient balance (call Wallet Service or check directly)
     - Debit wallet (with idempotency key = bet_id)
     - Create Bet entity
     - Save to PostgreSQL
     - Add to Redis round state
     - Write bet.placed event to outbox
   - Returns: bet confirmation

6. **Build bet history endpoints**
   - Player's own bet history (paginated)
   - Bets for a specific round
   - Admin: any player's bet history

7. **Build admin table management**
   - Create table (assigns to game)
   - Update table (activate/deactivate/maintenance)
   - Delete table (soft delete via status)

### Phase 4 deliverables

- All game endpoints working
- Bet placement integrated with Wallet Service
- Redis state read/write working
- Idempotent bet placement
- Admin table management

---

## Phase 5 — Round Worker + Rust RNG Service

**Estimated time: 2-3 weeks**

The most complex phase. Round Worker drives the game lifecycle autonomously. Rust RNG service provides cryptographically secure random numbers.

### Sub-phase 5A — Rust RNG Service

Build the standalone Rust service first since the Worker depends on it.

1. **Create CarameloBet-RNG repository**
   ```
   CarameloBet-RNG/
   ├── src/
   │   └── main.rs
   ├── proto/
   │   └── rng.proto
   ├── Cargo.toml
   └── Dockerfile
   ```

2. **Define gRPC contract (rng.proto)**
   ```protobuf
   syntax = "proto3";
   package rng;

   service RngService {
     rpc GenerateRouletteResult (RouletteRequest) returns (RouletteResponse);
     rpc GenerateCrashPoint (CrashRequest) returns (CrashResponse);
   }

   message RouletteRequest {
     string round_id = 1;
     string table_id = 2;
   }

   message RouletteResponse {
     int32 result = 1;   // 0-36
     string seed = 2;
     string seed_hash = 3;
   }

   message CrashRequest {
     string round_id = 1;
   }

   message CrashResponse {
     double multiplier = 1;  // e.g. 2.45
     string seed = 2;
     string seed_hash = 3;
   }
   ```

3. **Implement Rust service**
   - Use `tonic` crate for gRPC
   - Use `rand` crate with `OsRng` for cryptographic randomness
   - For Aviator: use exponential distribution for crash point (house edge configurable)
   - Generate seed, compute SHA256 hash, return both
   - Listen on port 50051

4. **Create Dockerfile and add to docker-compose.yml**
   ```yaml
   rng-service:
     build: ../CarameloBet-RNG
     container_name: caramelo-rng
     ports:
       - "50051:50051"
   ```

### Sub-phase 5B — gRPC Client in .NET Workers

1. **Add proto file to CarameloBet.Workers**
   - Copy `rng.proto` to `src/CarameloBet.Workers/Protos/rng.proto`
   - Configure `.csproj` to generate client code

2. **Build RNG client wrapper**
   ```csharp
   public interface IRngClient
   {
       Task<RouletteResult> GenerateRouletteResultAsync(Guid roundId, Guid tableId);
       Task<CrashResult> GenerateCrashPointAsync(Guid roundId);
   }
   ```

3. **Register gRPC client in Workers**
   ```csharp
   builder.Services.AddGrpcClient<RngService.RngServiceClient>(o =>
   {
       o.Address = new Uri("http://localhost:50051");
   });
   ```

### Sub-phase 5C — Round Worker Implementation

1. **Add seed/seed_hash columns to game.rounds**
   - Migration to add columns
   - Update Round entity

2. **Build RouletteRoundWorker**
   ```csharp
   public class RouletteRoundWorker(
       Guid tableId,
       IRoundRepository roundRepo,
       IBetRepository betRepo,
       IRoundStateRepository stateRepo,
       IRngClient rngClient,
       IPublishEndpoint publisher,
       ILogger<RouletteRoundWorker> logger) : BackgroundService
   {
       protected override async Task ExecuteAsync(CancellationToken ct)
       {
           while (!ct.IsCancellationRequested)
           {
               // 1. Create round (commit seed_hash, hide seed)
               // 2. Open betting (30s) — publish round.started
               // 3. Close betting — publish betting.closed
               // 4. Call RNG → get result + seed
               // 5. Calculate winners (red/black/odd/even/low/high)
               // 6. For each winner: publish payout.process to RabbitMQ
               // 7. Update round in DB (status, result, reveal seed)
               // 8. Publish round.finished with seed revealed
               // 9. Clear Redis state
               // 10. Wait 5s
           }
       }
   }
   ```

3. **Build AviatorRoundWorker**
   ```csharp
   public class AviatorRoundWorker(...) : BackgroundService
   {
       protected override async Task ExecuteAsync(CancellationToken ct)
       {
           while (!ct.IsCancellationRequested)
           {
               // 1. Create round, get crash point from RNG
               // 2. Open betting (10s)
               // 3. Close betting
               // 4. Stream multiplier (1.00 → up to crash_point)
               //    - Every 100ms publish multiplier.update
               //    - Check for cashout messages from players
               // 5. Crash — pay survivors
               // 6. Publish round.finished
               // 7. Wait 5s
           }
       }
   }
   ```

4. **Worker registration**
   - On startup, query all active tables from DB
   - Start one worker per table
   - Workers run independently

5. **Provably Fair implementation**
   - Before round starts: generate seed, publish hash(seed) to players
   - After round ends: reveal seed
   - Players can verify: hash(revealed_seed) == published_hash

### Phase 5 deliverables

- Rust RNG service running and responding to gRPC calls
- Round Workers driving rounds for all active tables
- Roulette rounds completing end-to-end
- Aviator rounds with multiplier streaming
- Payouts published to RabbitMQ
- Seed/seed_hash for provably fair verification
- Graceful shutdown handling

---

## Phase 6 — History Service

**Estimated time: 3-5 days**

The History Service is the audit trail and reporting layer. Read-heavy, consumes events from all other services.

### Endpoints to build (10 total)

```
GET /api/history/bets
GET /api/history/bets/{id}
GET /api/history/rounds
GET /api/history/rounds/{id}
GET /api/history/transactions
GET /api/history/events

# Admin reports
GET /api/admin/reports/ggr            # Gross Gaming Revenue
GET /api/admin/reports/active-players
GET /api/admin/reports/top-players
GET /api/admin/reports/table-stats
```

### Tasks in order

1. **Build consumers for all events**
   ```csharp
   public class BetPlacedConsumer : IConsumer<BetPlacedEvent>
   {
       public async Task Consume(ConsumeContext<BetPlacedEvent> context)
       {
           var historyEvent = HistoryEvent.Create(
               eventType: "bet.placed",
               payload: context.Message,
               userId: context.Message.PlayerId,
               roundId: context.Message.RoundId,
               betId: context.Message.BetId
           );
           await _historyRepo.AddAsync(historyEvent);
           await _historyRepo.SaveAsync();
       }
   }
   ```

   Similar consumers for:
   - `RoundFinishedConsumer`
   - `WalletDebitedConsumer`
   - `WalletCreditedConsumer`

2. **Build query endpoints**
   - Paginated, filterable
   - Optimized queries (use indexes)

3. **Build reports**
   - GGR: total bets - total payouts
   - Active players: unique players in last 24h
   - Top players: by total bet amount
   - Table stats: rounds per hour, average bet, etc

### Phase 6 deliverables

- All consumers running and persisting events
- Query endpoints with pagination
- Admin reports working
- Event store searchable by user, round, type, date

---

## Phase 7 — Frontend + Go WebSocket Server

**Estimated time: 2-3 weeks (combined)**

### Sub-phase 7A — Go WebSocket Server

Build first since frontend depends on it.

1. **Create CarameloBet-WS repository**
   ```
   CarameloBet-WS/
   ├── main.go
   ├── handlers/
   │   ├── connection.go
   │   └── room.go
   ├── consumers/
   │   ├── round_consumer.go
   │   └── multiplier_consumer.go
   ├── go.mod
   └── Dockerfile
   ```

2. **Implement WebSocket server**
   - Use `gorilla/websocket` or `nhooyr.io/websocket`
   - Validate JWT token on connection
   - Maintain map of `tableId → []Connection`
   - Heartbeat / ping-pong for connection health

3. **Implement RabbitMQ consumer**
   - Use `amqp091-go`
   - Consume `round.finished`, `multiplier.update`
   - Route messages to correct table room
   - Broadcast to all connections in that room

4. **Implement connection rooms**
   - On connect: client sends `{ tableId }`
   - Server adds to room
   - On disconnect: remove from room
   - On broadcast: iterate room, send to all

5. **Dockerfile and docker-compose**
   ```yaml
   ws-server:
     build: ../CarameloBet-WS
     container_name: caramelo-ws
     ports:
       - "8080:8080"
   ```

### Sub-phase 7B — Next.js Frontend

1. **Create CarameloBet-Web repository**
   ```
   CarameloBet-Web/
   ├── app/
   │   ├── (auth)/
   │   │   ├── login/
   │   │   └── register/
   │   ├── (game)/
   │   │   ├── lobby/
   │   │   ├── roulette/[tableId]/
   │   │   └── aviator/[tableId]/
   │   ├── (user)/
   │   │   ├── wallet/
   │   │   ├── history/
   │   │   └── profile/
   │   └── layout.tsx
   ├── components/
   │   ├── ui/
   │   ├── game/
   │   └── wallet/
   ├── lib/
   │   ├── api.ts
   │   ├── websocket.ts
   │   └── auth.ts
   └── package.json
   ```

2. **Setup core libraries**
   - Tailwind CSS
   - shadcn/ui components
   - React Query or SWR for API calls
   - Zustand or Jotai for state
   - JWT token management (localStorage/cookies)

3. **Build pages in order**
   - Login + Register
   - Game Lobby (list of tables)
   - Roulette Table (betting board + WebSocket)
   - Aviator Table (multiplier + cashout)
   - Wallet (balance + transactions)
   - Bet History
   - Profile

4. **WebSocket integration**
   - Connect on table page mount
   - Subscribe to table room
   - Handle `round.started`, `multiplier.update`, `round.finished`
   - Reconnect on disconnect
   - Show live timer, current bets, results

5. **Game-specific UIs**
   - Roulette: betting board with red/black/odd/even/low/high buttons, chip selector
   - Aviator: multiplier display, cashout button (active during round)
   - Both: live results history, players list

### Phase 7 deliverables

- Go WebSocket server handling connections
- Next.js frontend with all pages
- Real-time game updates working
- Full user journey: register → login → place bet → see result

---

## Phase 8 — Polish + Tests + Deploy

**Estimated time: 1-2 weeks**

### Sub-phase 8A — Testing

1. **Domain tests (xUnit)**
   - Test all entity factories
   - Test entity business rules (e.g., Wallet.Debit throws InsufficientBalanceException)
   - Test value objects

2. **Application tests (xUnit + Moq)**
   - Test use cases with mocked dependencies
   - Test happy path + error paths
   - Test validators

3. **Integration tests (Testcontainers)**
   - Spin up real PostgreSQL, Redis, RabbitMQ in tests
   - Test end-to-end flows (register, login, place bet)

4. **Load tests (k6)**
   - Test bet placement under concurrent load
   - Test wallet operations for race conditions
   - Measure latency p50/p95/p99

5. **Architecture tests (NetArchTest)**
   - Domain doesn't reference Infrastructure
   - Application doesn't reference API
   - etc

### Sub-phase 8B — Deployment

1. **Choose deployment platform**
   - Option A: Self-hosted on Proxmox homelab
   - Option B: Cloud (Railway, Render, AWS EC2)
   - Option C: Hybrid (homelab for demo, video for portfolio)

2. **Production configuration**
   - Production `docker-compose.prod.yml`
   - Environment variables (not files)
   - HTTPS via Let's Encrypt + Certbot
   - Domain DNS setup

3. **CI/CD pipeline (optional)**
   - GitHub Actions
   - Run tests on PR
   - Deploy on main branch push

### Sub-phase 8C — Polish

1. **README improvements**
   - Add live demo link (or video)
   - Add screenshots
   - Add badges
   - Polish language

2. **Code cleanup**
   - Remove TODOs
   - Add XML doc comments on public APIs
   - Refactor any rushed code

3. **Performance**
   - Add database indexes where queries are slow
   - Cache hot reads (table list, game configs)
   - Profile and optimize bottlenecks

4. **Security review**
   - JWT secret rotation
   - SQL injection check
   - Rate limit tuning
   - HTTPS everywhere

5. **Documentation**
   - API documentation via Scalar (already configured)
   - Architecture decision records (ADRs)
   - Setup guide for developers

### Phase 8 deliverables

- Test coverage >70%
- Load tested with realistic traffic
- Deployed to production environment
- Public demo or video
- Polished README
- Project ready for portfolio submission

---

## Critical Gotchas

### 1. MassTransit v9 requires paid license
Use v8.3.6 specifically.

### 2. PostgreSQL 18+ data directory issue
Use `PGDATA: /var/lib/postgresql/data/pgdata` in docker-compose.

### 3. Seq requires admin password
Set the `SEQ_FIRSTRUN_ADMINPASSWORD` environment variable locally.

### 4. Jaeger v2 SPM configuration
Requires complex YAML config with `jaeger_storage`, `jaeger_query`, `spanmetrics` connector.

### 5. ApplyConfigurationsFromAssembly bug
Scans ALL configurations from ALL DbContexts. Use explicit `ApplyConfiguration(new XConfiguration())` per DbContext.

### 6. EF Core + Npgsql version mismatch warnings
Npgsql lags behind EF Core. Keep aligned. Warnings often unavoidable but harmless.

### 7. Wallet/Game namespace conflicts with class names
Use aliases:
```csharp
using WalletEntity = CarameloBet.Domain.Entities.Wallet.Wallet;
```

### 8. SignalR removed from stack
Replaced by Go WebSocket server.

### 9. dotnet user-secrets multi-line issue
Don't paste multiple commands at once — they get concatenated into the secret value.

### 10. Migrations need separate folders per DbContext
Use `--output-dir Persistence/{Domain}/Migrations`.

### 11. snake_case naming convention
Use `EFCore.NamingConventions` package. Apply `.UseSnakeCaseNamingConvention()` per DbContext.

### 12. Seed must run after migrations
In `Program.cs` after `app.Build()`, before `app.Run()`.

---

## Quick Commands Reference

### Building & running
```bash
dotnet build
dotnet run --project src/CarameloBet.API
dotnet watch --project src/CarameloBet.Gateway
docker compose up -d
docker compose ps
docker compose down
```

### EF Core migrations (per DbContext)
```bash
dotnet ef migrations add MigrationName \
  --context AuthDbContext \
  --project src/CarameloBet.Infrastructure \
  --startup-project src/CarameloBet.API \
  --output-dir Persistence/Auth/Migrations

dotnet ef database update \
  --context AuthDbContext \
  --project src/CarameloBet.Infrastructure \
  --startup-project src/CarameloBet.API

dotnet ef migrations remove \
  --context AuthDbContext \
  --project src/CarameloBet.Infrastructure \
  --startup-project src/CarameloBet.API
```

### Verify database
```bash
docker exec -it caramelo-postgres psql -U caramelo -d caramelo_bet -c "\dn"
docker exec -it caramelo-postgres psql -U caramelo -d caramelo_bet -c "\dt auth.*"
docker exec -it caramelo-postgres psql -U caramelo -d caramelo_bet -c 'SELECT "name" FROM auth.roles;'
```

### User Secrets
```bash
dotnet user-secrets init --project src/CarameloBet.Gateway
dotnet user-secrets set "Jwt:Secret" "your-secret" --project src/CarameloBet.Gateway
dotnet user-secrets list --project src/CarameloBet.Gateway
```

### Drop all schemas (for clean reset)
```bash
docker exec -it caramelo-postgres psql -U caramelo -d caramelo_bet -c "
DROP SCHEMA auth CASCADE;
DROP SCHEMA wallet CASCADE;
DROP SCHEMA game CASCADE;
DROP SCHEMA history CASCADE;
"
```

---

## Memory / User Edits (saved across sessions)

1. **CarameloBet gaps to address:** Outbox pattern (Phase 3/4/5), Redis TTL/cleanup strategy, Redis failure/recovery strategy. Phase 1 gaps closed.

2. **CarameloBet additional repositories:** RNG Service in Rust + gRPC (CarameloBet-RNG), WebSocket server in Go (CarameloBet-WS). Both separate from main .NET backend.

---

## Key Architecture Decisions (with rationale)

**Why Modular Monolith?** Lower operational overhead for solo dev. Services can be extracted later.

**Why Go for WebSocket?** Goroutines handle 10k+ concurrent connections efficiently.

**Why Rust for RNG?** Cryptographic safety, auditability, can be certified for iGaming compliance.

**Why Redis for round state?** Changes every second, read by all players. PostgreSQL would be overwhelmed.

**Why queue for payouts?** Survives crashes. With idempotency keys, guarantees exactly-once payout.

**Why pessimistic locking on wallets?** Financial data — no race conditions acceptable.

**Why Outbox pattern?** Prevents lost events between DB write and queue publish.

**Why one Round Worker per table?** Isolation + scalability.

**Why simplified roulette?** Ship working game faster. Extensible to add more bet types.

---

## User Preferences (for AI assistants)

1. **Language:** English ONLY. Do not answer Portuguese questions.
2. **Style:** No em-dashes, no excessive emojis, clean prose.
3. **Code:** Show, don't tell. Complete files. Modern C# 12 syntax (primary constructors, `init`, expression bodies).
4. **Teaching:** Explain concepts when asked, don't over-explain.
5. **Pace:** Step-by-step, verify each step before moving on.
6. **Format:** Code in fenced blocks, file paths as headers, terminal commands separated.
7. **Decisions:** Always explain trade-offs when proposing options.

---

## Continuation Prompt

When starting a new chat, paste this:

> I'm continuing work on CarameloBet, a multiplayer iGaming platform built with .NET 10, Go, and Rust. Phases 1 and 2 are complete, including infrastructure, observability, authentication, JWT, refresh tokens, password reset, user profiles, RBAC administration, security hardening, migrations, and tests. I'm starting Phase 3, the Wallet Service. I have the full context document attached with all 8 phases detailed. Please respond in English only, use no em-dashes, and follow the patterns established in the completed phases.

Then attach this file.

---

## Time Estimates

| Phase | Estimated time |
|---|---|
| Phase 2 — Auth | Complete |
| Phase 3 — Wallet | 1 week |
| Phase 4 — Game | 1-2 weeks |
| Phase 5 — Worker + Rust RNG | 2-3 weeks |
| Phase 6 — History | 3-5 days |
| Phase 7 — Frontend + Go WS | 2-3 weeks |
| Phase 8 — Polish + Tests + Deploy | 1-2 weeks |

**Total remaining: ~8-13 weeks of consistent work**

---

*Last updated: 2026-07-13. Phase 2 Auth complete and verified. Next step: Phase 3 Wallet Service.*
