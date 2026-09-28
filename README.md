# User Management API

A backend REST API built with ASP.NET Core (.NET 8) for TechHive Solutions' HR and IT departments to manage user records (create, read, update, delete).

This project was built as part of a back-end development course, using Microsoft Copilot to scaffold, debug, and enhance the code across three activities.

## Project structure

```
UserManagementApp/
??? Controllers/
?   ??? UsersController.cs        # CRUD endpoints for users
??? Models/
?   ??? User.cs                   # User model with validation attributes
??? Middleware/
?   ??? ExceptionHandlingMiddleware.cs        # Global error handling
?   ??? TokenAuthenticationMiddleware.cs      # Bearer token authentication
?   ??? RequestResponseLoggingMiddleware.cs   # Request/response logging
??? appsettings.json               # Local configuration (gitignored - contains secrets)
??? appsettings.Example.json       # Template showing required configuration keys
??? Program.cs                     # App startup and middleware pipeline configuration
```

## Getting started

1. Copy `appsettings.Example.json` to `appsettings.json` (if not already present).
2. Set your own value for `Authentication:ApiToken` in `appsettings.json`.
3. Run the project:
   ```
   dotnet run
   ```
4. Open the Swagger UI at `https://localhost:<port>/swagger`.

### Authenticating in Swagger

1. Click **Authorize**.
2. Paste your raw token from `appsettings.json` (Swagger automatically adds the `Bearer` prefix — do not type it yourself).
3. Click **Authorize**, then **Close**.
4. All endpoint calls will now include the token.

## API Endpoints

| Method | Route              | Description                       |
|--------|--------------------|-----------------------------------|
| GET    | `/api/users`       | Get all users                     |
| GET    | `/api/users/{id}`  | Get a single user by ID           |
| POST   | `/api/users`       | Create a new user                 |
| PUT    | `/api/users/{id}`  | Update an existing user           |
| DELETE | `/api/users/{id}`  | Delete a user by ID               |

All endpoints (except Swagger routes) require a valid Bearer token in the `Authorization` header.

## Validation rules

- `FirstName` / `LastName`: required, 1-50 characters.
- `Email`: required, must be a valid email format.
- `Department`: required.
- Duplicate emails are rejected with `409 Conflict` on create/update.
- Invalid input returns `400 Bad Request` with validation details.

## Middleware pipeline

Configured in `Program.cs`, in this order:

1. **`ExceptionHandlingMiddleware`** — catches unhandled exceptions and returns a consistent JSON error (`{ "error": "Internal server error." }`) with a `500` status, instead of leaking stack traces.
2. **`TokenAuthenticationMiddleware`** — validates the `Authorization: Bearer <token>` header against the configured API token. Returns `401 Unauthorized` for missing/invalid tokens. Swagger routes (`/swagger`) are excluded so the docs remain browsable.
3. **`RequestResponseLoggingMiddleware`** — logs the HTTP method, path, response status code, and elapsed time for every authorized request.

This order ensures exceptions from auth/logging are still caught, unauthorized requests are rejected before reaching business logic, and only authorized traffic is logged in detail.

## Development history and Copilot usage

This project was built iteratively across three activities, with Microsoft Copilot assisting throughout:

1. **Scaffolding & CRUD generation** — Copilot helped scaffold the initial `UsersController` with GET/POST/PUT/DELETE endpoints and an in-memory data store.
2. **Debugging** — Copilot helped identify and fix real bugs:
   - Missing validation on user input (empty names, invalid emails).
   - No handling for non-existent user lookups.
   - No safeguards against unhandled exceptions crashing the API.
   Fixes included adding data annotations, `ModelState` validation checks, duplicate-email detection, and try/catch blocks with logging.
3. **Middleware implementation** — Copilot assisted in generating the logging, error-handling, and token-authentication middleware, and in configuring the pipeline order for correctness and security.

## Testing

Use Postman or the Swagger UI to test:

- Valid and invalid tokens (expect `401` for invalid/missing tokens).
- Valid and invalid user payloads (expect `400` for invalid data, `409` for duplicate emails).
- Non-existent user IDs on GET/PUT/DELETE (expect `404`).
- Triggering unexpected errors (expect a consistent `500` JSON response).
