# Security Review

Date: 2026-07-08

## Status

The security issues found on 2026-07-08 were fixed in the current workspace.

## Fixed Items

### Hardcoded JWT secret

Status: fixed.

Changes:
- Removed committed JWT secrets from API and Gateway appsettings.
- Removed fallback JWT secrets from token generation and token validation.
- API and Gateway now fail startup if `Jwt:Secret`, `Jwt:Issuer`, or `Jwt:Audience` is missing.
- `Jwt:Secret` must be at least 32 bytes.

Refs:
- `src/CarameloBet.API/appsettings.json`
- `src/CarameloBet.Gateway/appsettings.json`
- `src/CarameloBet.Infrastructure/Services/Auth/JwtService.cs`
- `src/CarameloBet.API/Middlewares/JwtMiddleware.cs`
- `src/CarameloBet.Gateway/Middlewares/JwtMiddleware.cs`

### Development credentials and exposed infrastructure

Status: fixed for local development safety.

Changes:
- Removed committed default service passwords from Docker Compose.
- Docker Compose now requires local environment variables for service credentials.
- Infrastructure ports are bound to `127.0.0.1` instead of all interfaces.
- Added `.env.example` with placeholders.
- Added `.env` and `.env.*` to `.gitignore`, while keeping `.env.example` tracked.
- Removed stale password examples from `CARAMELOBET-CONTEXT.md`.

Refs:
- `docker-compose.yml`
- `.env.example`
- `.gitignore`
- `CARAMELOBET-CONTEXT.md`

### Password reset token leakage

Status: fixed.

Changes:
- Stopped logging password reset URLs when Resend is not configured.
- Missing Resend configuration now logs only a generic warning.

Ref:
- `src/CarameloBet.Infrastructure/Services/Auth/PasswordResetEmailSender.cs`

### Direct API auth rate-limit bypass

Status: fixed.

Changes:
- Added API-side fixed-window rate limiting for `/api/auth/*`.
- Gateway rate limiting remains in place as an outer layer.
- `/test-error` is now mapped only in development.

Refs:
- `src/CarameloBet.API/Program.cs`
- `src/CarameloBet.API/Endpoints/AuthEndpoints.cs`

### Refresh token rotation race

Status: fixed.

Changes:
- Refresh rotation now uses a repository-level transaction.
- The old refresh token row is locked with `SELECT FOR UPDATE`.
- A concurrent refresh request using the same token can no longer issue a second valid refresh token.

Refs:
- `src/CarameloBet.Application/UseCases/Auth/RefreshTokenUseCase.cs`
- `src/CarameloBet.Application/Abstractions/IRefreshTokenRepository.cs`
- `src/CarameloBet.Infrastructure/Repositories/Auth/RefreshTokenRepository.cs`

### Password reset session revocation

Status: fixed.

Changes:
- Password reset now revokes active refresh tokens for the user.
- Tests assert that active refresh tokens are revoked after a successful password reset.

Refs:
- `src/CarameloBet.Application/UseCases/Auth/ResetPasswordUseCase.cs`
- `tests/CarameloBet.Application.Tests/AuthUseCaseTests.cs`

### Unexpected exception message leakage

Status: fixed.

Changes:
- 500 responses now return a generic error detail.
- Expected client errors still return their specific validation or auth messages.

Ref:
- `src/CarameloBet.API/Middlewares/GlobalExceptionHandler.cs`

### Weak password policy

Status: fixed.

Changes:
- Passwords now require 12 to 128 characters.
- Passwords must include uppercase, lowercase, numeric, and symbol characters.
- The policy is shared across registration, reset password, and change password.

Refs:
- `src/CarameloBet.Application/Validators/Auth/PasswordRules.cs`
- `src/CarameloBet.Application/Validators/Auth/RegisterRequestValidator.cs`
- `src/CarameloBet.Application/Validators/Auth/ResetPasswordRequestValidator.cs`
- `src/CarameloBet.Application/Validators/Auth/ChangeCurrentUserPasswordRequestValidator.cs`

## Verification

Commands run:

```bash
dotnet test CarameloBet.slnx
rg -n "super-secret|caramelo123|Reset URL|Jwt:Secret.*\?\?|Password=caramelo|RABBITMQ_DEFAULT_PASS: [^$]|POSTGRES_PASSWORD: [^$]|GF_SECURITY_ADMIN_PASSWORD: [^$]" . -g '!docs/security-review.md'
```

Results:
- `40 tests passed`
- Clean database migration passed for all four DbContexts
- Auth smoke test passed with 22 assertions across all 17 endpoints, JWT, RBAC, refresh rotation, logout, password reset, and registration side effects
- secret-pattern scan returned no matches outside this review file

## Operational Notes

- Local development now needs `.env` or user secrets for `Jwt__Secret`, `ConnectionStrings__PostgreSQL`, `RabbitMQ__Password`, and Docker Compose service credentials.
- Do not deploy Docker Compose observability/admin tools publicly without authentication and network controls.
- Consider adding a real secret manager before any non-local deployment.
