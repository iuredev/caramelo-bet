# CarameloBet Project Learning Notes

This document explains what has been built so far, why the main decisions were made, and how the project structure should help you learn backend architecture instead of only following implementation steps.

## Current Architecture

CarameloBet is being built as a modular monolith.

That means the backend stays in one main codebase, but each layer has a clear responsibility.

```text
Domain <- Application <- Infrastructure <- API
                         <- Workers

Gateway is separate
```

The dependency direction matters.

Domain does not depend on anything.

Application depends on Domain.

Infrastructure depends on Application and Domain.

API and Workers depend on Application and Infrastructure.

Gateway stays separate because it has a different job: receive traffic, validate access, rate limit requests, and route calls.

## Why Modular Monolith

A modular monolith gives you service boundaries without the operational complexity of microservices.

For this project, that is a good decision because you can learn:

- Clean architecture
- Dependency direction
- Use cases
- Database separation
- Auth flows
- Background workers
- Messaging
- Observability

At the same time, you avoid managing many backend services too early.

## Project Structure

The current source structure is:

```text
src/
  CarameloBet.Domain
  CarameloBet.Application
  CarameloBet.Infrastructure
  CarameloBet.API
  CarameloBet.Gateway
  CarameloBet.Workers

tests/
  CarameloBet.Domain.Tests
  CarameloBet.Application.Tests
  CarameloBet.Architecture.Tests

infra/
  prometheus/
  jaeger/
  nginx/
```

Each project has a separate purpose.

## Domain

Path:

```text
src/CarameloBet.Domain
```

Domain contains the core business objects.

Examples:

```text
Entities/Auth/User.cs
Entities/Wallet/Wallet.cs
Entities/Game/Round.cs
Entities/Game/Bet.cs
Entities/Transaction/Transaction.cs
Entities/History/HistoryEvent.cs
```

Domain should not know about:

- EF Core
- PostgreSQL
- BCrypt
- Redis
- RabbitMQ
- HTTP
- Controllers
- DTOs

The Domain project should focus on business concepts and business rules.

Example:

```csharp
User.Create(...)
```

That factory is useful because object creation rules stay inside the entity instead of being spread across the application.

## Application

Path:

```text
src/CarameloBet.Application
```

Application contains what the system can do.

Examples:

```text
UseCases/Auth/RegisterUseCase.cs
DTOs/Auth/RegisterRequest.cs
Validators/Auth/RegisterRequestValidator.cs
```

The register use case currently represents this flow:

```text
Receive request
Check if email exists
Hash password
Create user
Save user
Return response
```

Application should not know how PostgreSQL works.

Application should not know how BCrypt works.

Instead, it depends on interfaces:

```csharp
IUserRepository
IPasswordHasher
```

This is one of the most important ideas in the project.

Application says what it needs.

Infrastructure decides how to do it.

## Infrastructure

Path:

```text
src/CarameloBet.Infrastructure
```

Infrastructure contains concrete technical implementations.

Examples:

```text
Persistence/Auth/AuthDbContext.cs
Repositories/Auth/UserRepository.cs
Services/Auth/PasswordHasher.cs
```

`UserRepository` uses EF Core.

`PasswordHasher` uses BCrypt.

That is correct because EF Core and BCrypt are technical details.

The decision is:

```text
Application uses IPasswordHasher
Infrastructure implements PasswordHasher
```

This keeps use cases cleaner and easier to test.

## API

Path:

```text
src/CarameloBet.API
```

The API project is the HTTP application.

It currently contains:

```text
Program.cs
Middlewares/GlobalExceptionHandler.cs
Models/ApiResponse.cs
```

The API configures:

- CORS
- OpenAPI
- Serilog
- Prometheus metrics
- OpenTelemetry tracing
- MassTransit
- EF Core database contexts
- Exception handling
- Basic health routes

Current simple routes include:

```text
/
/api
/health
/test-error
```

The next missing route is:

```text
POST /auth/register
```

That endpoint should call `RegisterUseCase`.

## Gateway

Path:

```text
src/CarameloBet.Gateway
```

Gateway is the front door of the backend.

It currently has:

- YARP reverse proxy
- JWT middleware
- Redis rate limiting
- Prometheus metrics
- OpenTelemetry tracing
- Serilog

Gateway should stay separate from API because they have different responsibilities.

Gateway protects and routes traffic.

API runs business endpoints.

## Workers

Path:

```text
src/CarameloBet.Workers
```

Workers are for background processes.

Right now the worker project is mostly scaffolded.

Later it should handle things like:

- Roulette round lifecycle
- Aviator round lifecycle
- Queue consumers
- Payout processing

Workers should not expose HTTP endpoints.

They run background business processes.

## Database Structure

The project currently has four EF Core contexts:

```text
AuthDbContext
WalletDbContext
GameDbContext
HistoryDbContext
```

These match the planned database schemas:

```text
auth
wallet
game
history
```

This is a good modular monolith decision.

The system is one backend, but each business area has its own database boundary.

## Password Hashing Decision

The password hashing flow is:

```text
User sends plain password
Application calls IPasswordHasher
Infrastructure hashes with BCrypt
Only PasswordHash is stored
Plain password is never stored
```

BCrypt is a good password hashing choice because:

- It is slow on purpose
- It includes a salt
- It is designed for passwords
- It can verify a password later without decrypting anything

The implementation lives in:

```text
src/CarameloBet.Infrastructure/Services/Auth/PasswordHasher.cs
```

The use case depends only on:

```csharp
IPasswordHasher
```

That keeps BCrypt out of the Application layer.

## Current Status

The solution currently builds successfully.

```text
dotnet build CarameloBet.slnx
10 projects, 0 errors, 0 warnings
```

Completed or started:

- Solution structure exists
- Domain entities exist
- EF Core contexts exist
- Initial migrations exist
- Gateway exists
- API startup exists
- Auth registration use case started
- Password hashing exists
- User repository exists
- Test projects exist

Still missing:

- `POST /auth/register` endpoint
- Login use case
- JWT generation
- Refresh token flow
- Wallet use cases
- Game use cases
- Round workers
- Queue consumers
- Real tests

## Current Git Status

There are uncommitted changes related to the auth work:

```text
Modified:
src/CarameloBet.API/Program.cs

Untracked:
src/CarameloBet.Application/Abstractions/
src/CarameloBet.Application/DependencyInjection.cs
src/CarameloBet.Application/DTOs/
src/CarameloBet.Application/UseCases/
src/CarameloBet.Application/Validators/
src/CarameloBet.Infrastructure/DependencyInjection.cs
src/CarameloBet.Infrastructure/Repositories/
src/CarameloBet.Infrastructure/Services/
```

## Best Next Learning Step

The next thing to learn deeply is the full request flow:

```text
HTTP request
DTO
Validation
Use case
Repository
DbContext
Entity
Response
```

The next implementation task should be:

```text
POST /auth/register
```

Recommended learning workflow:

1. Understand what the endpoint must do.
2. Write the route yourself.
3. Call the validator.
4. Call `RegisterUseCase`.
5. Return `ApiResponse`.
6. Build the project.
7. Review the code and improve it.

This way the project becomes a learning tool, not just a list of instructions to follow.
