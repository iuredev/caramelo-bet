# Auth API Testing Guide with Insomnia

This guide tests every Auth endpoint and its meaningful success, validation, authentication, authorization, moderation, and business-rule cases.

## Recommended workflow

Use both tools for different jobs:

- Scalar is the live interactive API reference at `http://localhost:5057/scalar/v1`.
- OpenAPI is the machine-readable contract at `http://localhost:5057/openapi/v1.json`.
- Insomnia stores the requests, environment values, and test history.

Scalar and Insomnia read the same OpenAPI contract, so there is only one endpoint definition to maintain.

The OpenAPI and Scalar routes are available only when the API runs in the `Development` environment.

## 1. Start the local environment

### 1.1 Create local configuration

Copy `.env.example` to `.env` and replace every placeholder. Never commit `.env`.

The JWT secret must contain at least 32 bytes.

```bash
rtk cp .env.example .env
```

Load application secrets with user secrets or environment variables. These values must match the Docker credentials:

```bash
rtk dotnet user-secrets init --project src/CarameloBet.API
rtk dotnet user-secrets set "Jwt:Secret" "replace-with-at-least-32-random-bytes" --project src/CarameloBet.API
rtk dotnet user-secrets set "ConnectionStrings:PostgreSQL" "Host=localhost;Port=5433;Database=caramelo_bet;Username=YOUR_USER;Password=YOUR_PASSWORD" --project src/CarameloBet.API
rtk dotnet user-secrets set "RabbitMQ:Password" "YOUR_RABBITMQ_PASSWORD" --project src/CarameloBet.API
```

Optional password-reset email configuration:

```bash
rtk dotnet user-secrets set "Resend:ApiToken" "YOUR_RESEND_TOKEN" --project src/CarameloBet.API
```

### 1.2 Start infrastructure and migrate the clean database

```bash
rtk docker compose up -d postgres rabbitmq seq jaeger
rtk dotnet ef database update --project src/CarameloBet.Infrastructure --startup-project src/CarameloBet.API --context AuthDbContext
rtk dotnet ef database update --project src/CarameloBet.Infrastructure --startup-project src/CarameloBet.API --context WalletDbContext
rtk dotnet ef database update --project src/CarameloBet.Infrastructure --startup-project src/CarameloBet.API --context GameDbContext
rtk dotnet ef database update --project src/CarameloBet.Infrastructure --startup-project src/CarameloBet.API --context HistoryDbContext
```

### 1.3 Run the API

```bash
rtk dotnet run --project src/CarameloBet.API
```

Confirm these URLs:

- Health: `http://localhost:5057/health`
- Scalar: `http://localhost:5057/scalar/v1`
- OpenAPI JSON: `http://localhost:5057/openapi/v1.json`

If the launch profile selects a different port, use the URL printed by `dotnet run` everywhere below.

## 2. Configure Insomnia

### 2.1 Import the OpenAPI contract

1. Open Insomnia.
2. Create a project named `CarameloBet Local`.
3. Select Import and choose URL.
4. Enter `http://localhost:5057/openapi/v1.json`.
5. Import the generated request collection.

If your Insomnia version cannot import a local URL, save the JSON and import the file:

```bash
rtk curl http://localhost:5057/openapi/v1.json -o /tmp/caramelo-openapi.json
```

### 2.2 Create the base environment

Create an Insomnia environment with this JSON:

```json
{
  "base_url": "http://localhost:5057",
  "player_email": "player.insomnia@example.com",
  "player_password": "PlayerPass!2026",
  "player_new_password": "PlayerChanged!2026",
  "player_id": "",
  "player_access_token": "",
  "player_refresh_token": "",
  "admin_email": "admin.insomnia@example.com",
  "admin_password": "AdminPass!2026",
  "admin_id": "",
  "admin_access_token": "",
  "admin_refresh_token": "",
  "role_id": "",
  "reset_token": "insomnia-reset-token"
}
```

Use `{{ base_url }}` as the URL prefix. For protected requests, select Bearer Token authentication and use either:

- `{{ player_access_token }}` for `/api/users/*`
- `{{ admin_access_token }}` for `/api/admin/*`

When a response returns an ID or token, copy it into the environment. Insomnia response templates can automate this later, but manual values make the first run easier to understand.

## 3. Understand response formats

Successful responses use this envelope:

```json
{
  "success": true,
  "data": {}
}
```

Validation failures return HTTP 400 with a validation problem body containing an `errors` object.

Business and authentication failures use Problem Details:

```json
{
  "title": "Unauthorized",
  "status": 401,
  "detail": "Invalid email or password"
}
```

Common status codes:

| Status | Meaning |
|---|---|
| 200 | Successful read or update |
| 201 | Resource created |
| 400 | Validation or business-rule failure |
| 401 | Missing, invalid, expired token, or invalid credentials |
| 403 | Authenticated user lacks the required role, or user is inactive |
| 404 | Resource or GUID route not found |
| 429 | Auth rate limit exceeded |
| 500 | Unexpected server failure, with a generic response detail |

## 4. Public Auth endpoints

All `/api/auth/*` routes are rate limited. The default limit is 10 requests per client per 60 seconds. Wait for the window to reset if you receive HTTP 429 while testing many cases.

### 4.1 POST `/api/auth/register`

Happy path request:

```json
{
  "name": "Insomnia Player",
  "email": "player.insomnia@example.com",
  "password": "PlayerPass!2026",
  "birthDate": "1995-06-15"
}
```

Expected result: HTTP 201. Copy `data.id` to `player_id`.

Database side effects:

- One `auth.users` row is created.
- The seeded `player` role is assigned.
- A zero-balance wallet is created in `wallet.wallets`.
- All three writes occur in one transaction.

Run these cases separately:

| Case | Change | Expected |
|---|---|---|
| Valid without birth date | Set `birthDate` to `null` | 201 |
| Duplicate email | Send the happy request again | 400, `Email already exists` |
| Empty name | `"name": ""` | 400 |
| Name too short | `"name": "AB"` | 400 |
| Name too long | More than 50 characters | 400 |
| Empty email | `"email": ""` | 400 |
| Invalid email | `"email": "not-an-email"` | 400 |
| Password under 12 characters | `"password": "Short!1A"` | 400 |
| Missing uppercase | `"password": "lowercase!2026"` | 400 |
| Missing lowercase | `"password": "UPPERCASE!2026"` | 400 |
| Missing number | `"password": "NoNumbersHere!"` | 400 |
| Missing symbol | `"password": "NoSymbols2026"` | 400 |
| Password over 128 characters | Use 129 characters | 400 |
| Malformed JSON | Remove the final `}` | 400 |

### 4.2 POST `/api/auth/login`

```json
{
  "email": "{{ player_email }}",
  "password": "{{ player_password }}"
}
```

Expected result: HTTP 200. Copy:

- `data.accessToken` to `player_access_token`
- `data.refreshToken` to `player_refresh_token`

Login also returns `data.user` with the user's ID, name, email, status, and roles. This supplies the initial frontend session context without a second request. `GET /api/users/me` remains the source for the latest profile data.

Run these cases:

| Case | Change | Expected |
|---|---|---|
| Valid credentials | Use the body above | 200 |
| Unknown email | Use `missing@example.com` | 401 |
| Wrong password | Use `WrongPass!2026` | 401 |
| Empty email | Use an empty string | 400 |
| Invalid email format | Use `invalid` | 400 |
| Empty password | Use an empty string | 400 |
| Deleted or blocked user | Delete or block with the dedicated admin endpoint, then log in | 403 |

The failure response intentionally does not reveal whether the email or password was incorrect.

### 4.3 POST `/api/auth/refresh`

```json
{
  "refreshToken": "{{ player_refresh_token }}"
}
```

Expected result: HTTP 200. The response data contains only `accessToken` and `refreshToken`. Replace both player tokens in the environment with the new values. Refresh tokens rotate and the old token becomes invalid.

Run these cases:

| Case | Action | Expected |
|---|---|---|
| Active refresh token | Send current token | 200 with new access and refresh tokens |
| Reuse old token | Send the pre-rotation token | 401 |
| Empty token | Send an empty string | 400 |
| Random token | Send `not-a-refresh-token` | 401 |
| Revoked token | Refresh after logout | 401 |
| Expired token | Set its `expires_at` in PostgreSQL to the past | 401 |
| Concurrent reuse | Send the same token from two requests at once | One 200 and one 401 |

The concurrent test verifies the database row lock used during rotation.

### 4.4 POST `/api/auth/logout`

```json
{
  "refreshToken": "{{ player_refresh_token }}"
}
```

Expected result: HTTP 200. The access token remains valid until its 15-minute expiry, but the refresh token can no longer create a new session.

Run these cases:

| Case | Action | Expected |
|---|---|---|
| Active token | Send current refresh token | 200 |
| Refresh after logout | Send the same token to `/refresh` | 401 |
| Logout twice | Send the same token again | 200 |
| Unknown token | Send a random non-empty value | 200 |
| Empty token | Send an empty string | 400 |

Logout is intentionally idempotent and does not reveal whether a token existed.

### 4.5 POST `/api/auth/forgot-password`

```json
{
  "email": "{{ player_email }}"
}
```

Expected result: HTTP 200 with the same message for existing, missing, blocked, and deleted accounts. This prevents account enumeration.

Run these cases:

| Case | Change | Expected |
|---|---|---|
| Active account | Use the player email | 200 and token stored |
| Unknown account | Use `missing@example.com` | 200 |
| Blocked or deleted account | Use an inactive account | 200 without token creation |
| Empty email | Use an empty string | 400 |
| Invalid email | Use `invalid` | 400 |

When Resend is configured, read the raw reset token from the email. Without Resend, use the local-only setup in section 4.6.

### 4.6 POST `/api/auth/reset-password`

#### Local-only reset token setup

The API stores only SHA-256 token hashes and never logs raw reset tokens. To test the success path without an email provider, insert a known token into the local database.

The raw token is:

```text
insomnia-reset-token
```

Its uppercase SHA-256 hash is:

```text
2CF5A772B40261592B2B08C87F9A40731CF55373FE56B0A31BEDF22BB8E6265F
```

Replace `YOUR_DB_USER` and the email if needed:

```bash
rtk docker exec -it caramelo-postgres psql -U YOUR_DB_USER -d caramelo_bet -c "INSERT INTO auth.password_reset_tokens (id, user_id, token_hash, expires_at, created_at) SELECT gen_random_uuid(), id, '2CF5A772B40261592B2B08C87F9A40731CF55373FE56B0A31BEDF22BB8E6265F', NOW() + INTERVAL '1 hour', NOW() FROM auth.users WHERE email = 'player.insomnia@example.com';"
```

This direct database setup is for local testing only.

Request:

```json
{
  "token": "{{ reset_token }}",
  "newPassword": "ResetPlayer!2026"
}
```

Expected result: HTTP 200. All active refresh tokens for the user are revoked.

Run these cases:

| Case | Action | Expected |
|---|---|---|
| Valid token and strong password | Use the request above | 200 |
| Reuse token | Send the same request again | 401 |
| Random token | Send an unknown token | 401 |
| Expired token | Insert or update a token with past `expires_at` | 401 |
| Empty token | Send an empty string | 400 |
| Weak new password | Violate any password rule | 400 |
| Old refresh token after reset | Send it to `/refresh` | 401 |
| Login with old password | Try the previous password | 401 |
| Login with new password | Use `ResetPlayer!2026` | 200 |

After the last case, update `player_password`, `player_access_token`, and `player_refresh_token` in Insomnia.

## 5. Current-user endpoints

These routes require `Authorization: Bearer {{ player_access_token }}`.

For each route, first test these shared authentication cases:

| Case | Authorization header | Expected |
|---|---|---|
| Missing token | No header | 401 |
| Malformed token | `Bearer invalid` | 401 |
| Expired access token | Use a token after expiry | 401 |
| Valid player token | Current access token | Continue to endpoint behavior |

### 5.1 GET `/api/users/me`

No request body.

Expected result: HTTP 200 with the current user profile.

Additional case:

| Case | Action | Expected |
|---|---|---|
| User becomes inactive after token issuance | Admin blocks or deletes the user, then reuse access token | 401, `Invalid user` |

### 5.2 PUT `/api/users/me`

```json
{
  "name": "Updated Insomnia Player",
  "email": "player.updated@example.com",
  "birthDate": "1995-06-15"
}
```

Expected result: HTTP 200. Update `player_email` in Insomnia after success.

Run these cases:

| Case | Change | Expected |
|---|---|---|
| Valid profile | Use the request above | 200 |
| Null birth date | Set `birthDate` to `null` | 200 |
| Same email | Keep the current email | 200 |
| Another user's email | Use an existing email | 400, `Email already exists` |
| Empty name | Use an empty string | 400 |
| Name under 3 characters | Use `AB` | 400 |
| Name over 50 characters | Use 51 characters | 400 |
| Empty or invalid email | Use invalid email data | 400 |
| Inactive current user | Block user after issuing token | 401 |

### 5.3 PUT `/api/users/me/password`

```json
{
  "currentPassword": "{{ player_password }}",
  "newPassword": "{{ player_new_password }}"
}
```

Expected result: HTTP 200. Update `player_password` after success.

Run these cases:

| Case | Change | Expected |
|---|---|---|
| Correct current and strong new password | Use the request above | 200 |
| Wrong current password | Use `WrongPass!2026` | 401 |
| Empty current password | Use an empty string | 400 |
| Weak new password | Violate each password rule | 400 |
| Inactive current user | Block user after issuing token | 401 |
| Login with old password after success | Use old password | 401 |
| Login with new password after success | Use new password | 200 |

Changing a password does not currently revoke refresh tokens. Password reset does revoke them.

## 6. Create a local admin session

There is no default admin account because committed credentials would be unsafe. Bootstrap an admin only in the local database.

### 6.1 Register the admin candidate

Use `POST /api/auth/register`:

```json
{
  "name": "Insomnia Admin",
  "email": "{{ admin_email }}",
  "password": "{{ admin_password }}",
  "birthDate": null
}
```

Copy `data.id` to `admin_id`.

### 6.2 Verify player access is forbidden

Log in as the admin candidate before assigning the role. Confirm that `data.user.roles` contains only `player`, then send `GET /api/admin/users` with that access token.

Expected result: HTTP 403. This proves that authentication alone is insufficient.

### 6.3 Assign the seeded admin role locally

Replace `YOUR_DB_USER` if needed:

```bash
rtk docker exec -it caramelo-postgres psql -U YOUR_DB_USER -d caramelo_bet -c "INSERT INTO auth.user_roles (user_id, role_id, created_at) SELECT u.id, r.id, NOW() FROM auth.users u CROSS JOIN auth.roles r WHERE u.email = 'admin.insomnia@example.com' AND r.name = 'admin' ON CONFLICT DO NOTHING;"
```

### 6.4 Log in again

JWT claims are created at login. The old token does not gain the new role. Log in again, confirm that `data.user.roles` contains `admin`, and copy the new `accessToken` and `refreshToken` to the admin environment values.

## 7. Admin endpoints

All routes require `Authorization: Bearer {{ admin_access_token }}` and the `admin` role.

For every admin route, test:

| Case | Token | Expected |
|---|---|---|
| Missing token | None | 401 |
| Invalid or expired token | Invalid bearer value | 401 |
| Valid player token | `player_access_token` | 403 |
| Valid admin token | `admin_access_token` | Continue to endpoint behavior |

### 7.1 GET `/api/admin/users`

No body. Expected result: HTTP 200 with every user and assigned roles.

Also verify that deleted users remain in the result for auditability.

### 7.2 GET `/api/admin/users/{id}`

Use `{{ player_id }}`.

| Case | Path value | Expected |
|---|---|---|
| Existing user | Valid `player_id` | 200 |
| Missing user | Random valid UUID | 404, `User not found` |
| Invalid UUID | `not-a-guid` | 404 because route constraint does not match |

### 7.3 PUT `/api/admin/users/{id}`

```json
{
  "name": "Admin Updated Player",
  "email": "admin-updated-player@example.com",
  "birthDate": null
}
```

| Case | Change | Expected |
|---|---|---|
| Valid update | Use request above | 200 |
| Empty or short name | Violate name rule | 400 |
| Invalid email | Use invalid email | 400 |
| Duplicate email | Use another user's email | 400 |
| Missing user | Use random UUID | 404 |

This endpoint cannot change account status. Use the dedicated endpoints below for blocking, unblocking, and deletion.

### 7.4 POST `/api/admin/users/{id}/block`

Permanent block:

```json
{
  "reason": "Chargeback investigation",
  "expiresAt": null
}
```

Temporary block dates must be in the future and should use UTC:

```json
{
  "reason": "Responsible gaming cooling-off period",
  "expiresAt": "2026-07-21T18:00:00Z"
}
```

Expected result: HTTP 200 with `status`, `blockedReason`, and `blockedUntil`. Active refresh tokens are revoked immediately. Existing access tokens receive HTTP 403 on their next protected request.

| Case | Action | Expected |
|---|---|---|
| Permanent block | Set `expiresAt` to `null` | 200 |
| Temporary block | Use a future UTC date | 200 |
| Empty reason | Use an empty string | 400 |
| Reason over 500 characters | Use 501 characters | 400 |
| Past expiration | Use a past date | 400 |
| Missing user | Use random UUID | 404 |
| Deleted user | Block a soft-deleted user | 403 |
| Login while blocked | Use correct credentials | 403 |
| Refresh while blocked | Use a token issued before blocking | 401 |
| Protected request with old access token | Call `/api/users/me` | 403 |
| Expired temporary block | Wait until expiration, then log in | 200 and block state is cleared |

### 7.5 DELETE `/api/admin/users/{id}/block`

No body. Expected result: HTTP 200 with an active user and cleared `blockedReason` and `blockedUntil`.

| Case | Action | Expected |
|---|---|---|
| Blocked user | Unblock by ID | 200 |
| Already active user | Repeat request | 200 |
| Missing user | Use random UUID | 404 |
| Deleted user | Attempt to unblock | 403 |

### 7.6 DELETE `/api/admin/users/{id}`

No body. This is a soft delete that changes status to `deleted`.

| Case | Path value | Expected |
|---|---|---|
| Existing user | Valid user ID | 200 |
| Delete same user again | Same ID | 200 |
| Missing user | Random UUID | 404 |

Use a disposable test user so the main player remains available.

### 7.7 GET `/api/admin/roles`

No body. Expected result: HTTP 200. Verify seeded roles such as `player` and `admin` are present.

### 7.8 POST `/api/admin/roles`

```json
{
  "name": "insomnia_reviewer",
  "description": "Role created through Insomnia"
}
```

Expected result: HTTP 201. Copy `data.id` to `role_id`.

| Case | Change | Expected |
|---|---|---|
| Valid role | Use request above | 201 |
| Duplicate name | Repeat request | 400, `Role already exists` |
| Uppercase name | Use `InsomniaReviewer` | 400 |
| Spaces or hyphens | Use `insomnia reviewer` or `insomnia-reviewer` | 400 |
| Empty name | Use an empty string | 400 |
| Name over 50 characters | Use 51 characters | 400 |
| Description over 255 characters | Use 256 characters | 400 |

Role names accept lowercase letters and underscores only.

### 7.9 PUT `/api/admin/roles/{id}`

```json
{
  "name": "insomnia_auditor",
  "description": "Updated through Insomnia"
}
```

| Case | Action | Expected |
|---|---|---|
| Valid update | Use `role_id` and request above | 200 |
| Same name | Repeat the current values | 200 |
| Duplicate other role name | Use `player` | 400 |
| Invalid name or description | Violate create rules | 400 |
| Missing role | Use random UUID | 404, `Role not found` |

### 7.10 POST `/api/admin/users/{userId}/roles`

```json
{
  "roleId": "{{ role_id }}"
}
```

| Case | Action | Expected |
|---|---|---|
| Existing user and role | Use player and created role | 200 |
| Assign same role twice | Repeat request | 200 without duplicate row |
| Empty role ID | Use `00000000-0000-0000-0000-000000000000` | 400 |
| Missing role | Use random UUID | 404, `Role not found` |
| Missing user | Use random user UUID | 404, `User not found` |
| Invalid user UUID in path | Use `not-a-guid` | 404 |

Log in as the player again after role assignment if you want to inspect the new role claim. Existing access tokens do not change.

## 8. Final regression sequence

Run this shorter sequence after any Auth change:

1. Register a new player and verify HTTP 201.
2. Register the same email and verify HTTP 400.
3. Log in and save both tokens.
4. Read and update `/api/users/me`.
5. Change password and verify old login fails while new login succeeds.
6. Refresh once and verify the old refresh token cannot be reused.
7. Log out and verify the refresh token is rejected.
8. Request password reset for existing and missing emails, verifying identical HTTP 200 responses.
9. Reset with a valid token, then verify reuse fails and earlier refresh tokens are revoked.
10. Call an admin route with no token, player token, and admin token, expecting 401, 403, and success.
11. Exercise admin user read, update, block, unblock, and soft delete.
12. Verify blocking immediately rejects access and refresh tokens, then verify temporary expiry.
13. Exercise role list, create, update, and assignment.
14. Send more than the configured Auth rate limit and verify HTTP 429.

## 9. Automated verification

Insomnia is excellent for exploration and debugging, but it should not replace automated tests.

Run the complete suite after manual testing:

```bash
rtk dotnet test CarameloBet.slnx
rtk dotnet build CarameloBet.slnx -c Release
```

The repository tests cover domain behavior and application use cases. The manual sequence covers HTTP serialization, middleware, JWT, RBAC, PostgreSQL, and the complete endpoint pipeline.
