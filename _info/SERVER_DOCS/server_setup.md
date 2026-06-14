# Server Setup & Configuration

## Initial Setup Requirements

### Prerequisites
- **.NET 10.0 SDK** installed
- **PostgreSQL 13+** database server
- **Visual Studio Code** or **Visual Studio** 2022+
- **Git** for version control

### Environment Setup

#### 1. **Database Configuration**
Modify connection string in [appsettings.Development.json](../appsettings.Development.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=pmms_db;Username=postgres;Password=your_password"
  }
}
```

For production, use [appsettings.json](../appsettings.json) and environment variables.

#### 2. **User Secrets**
The project is configured with User Secrets (ID: `2a28cb6f-c5c7-4fce-812d-764a32aa3f34`):

  ```bash
  # Set admin account
  dotnet user-secrets set "Seed:Admin:Email" "adminemail@dhsud.hredrd.rv"
  dotnet user-secrets set "Seed:Admin:Password" "AdminPassword@123"

  # Set secrets for JWT configuration
  dotnet user-secrets set "JwtSettings:Secret" "your-secret-key"
  dotnet user-secrets set "JwtSettings:Issuer" "https://yourissuer.com"
  dotnet user-secrets set "JwtSettings:Audience" "https://youraudience.com"

  # Set database connection if needed
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
  ```

#### 3. **CORS Configuration**
Default CORS policy allows:
- Origin: `http://localhost:5173` (frontend)
- Methods: Any
- Headers: Any

Update in [Program.cs](../Program.cs) for production URLs:
```csharp
policy.WithOrigins("https://yourdomain.com")
```

## Startup Process

### 1. Service Registration (Program.cs)
```csharp
// Identity & Authentication
builder.Services.AddIdentityServices(builder.Configuration);

// Database & EF Core
builder.Services.AddInfrastructure(builder.Configuration);

// API Documentation
builder.Services.AddSwaggerDocumentation();

// Real-time Communication
builder.Services.AddSignalR();

// Scheduled Jobs
builder.Services.AddExpirationTrackingJobs();

// HTTP Context Access
builder.Services.AddHttpContextAccessor();
```

### 2. Middleware Configuration
```csharp
// Exception Handling
app.UseExceptionHandler();

// Path Base for all routes
app.UsePathBase("/api/v1");

// Development Tools
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();           // OpenAPI endpoint
    app.MapScalarApiReference(); // Scalar UI
}
```

### 3. Database Seeding
On startup, the application automatically:
- Creates database if not exists
- Applies migrations
- Seeds initial data (roles, default users, etc.)

```csharp
var seeder = scope.ServiceProvider.GetRequiredService<PmmsDbSeeder>();
await seeder.SeedAsync(dbContext, userManager, roleManager);
```

### 4. Endpoint Mapping
Routes are dynamically mapped from all `IEndpoint` implementations:
```csharp
app.MapEndpoints();
```

## Running the Server

### Development Mode
```bash
cd PMMS.Server
dotnet run
# Server runs on: http://localhost:5000 (or configured port)
# API Base: http://localhost:5000/api/v1
```

### With Watch Mode (auto-reload)
```bash
dotnet watch run
```

### Production Build
```bash
dotnet publish -c Release -o ./publish
# Deploy the ./publish folder
```

## Configuration Files

### `appsettings.Development.json`
- Development-specific settings
- Debug logging
- Local database connection

### `appsettings.json`
- Production settings (use environment variables)
- Default logging level
- Production database connection via env vars

### `launchSettings.json`
- Visual Studio launch profiles
- Debug configurations
- Environment variables per profile

## API Documentation

### Accessing API Docs

**OpenAPI Endpoint**
```
GET /api/v1/openapi/v1.json
```

**Scalar UI** (Development only)
```
GET /api/v1/scalar/v1
```
Browser-based, interactive API explorer.

**Swagger UI** (if enabled)
```
GET /api/v1/swagger/index.html
```

## Port Configuration

To change default, modify `launchSettings.json`:
```json
"applicationUrl": "https://localhost:5001;http://localhost:5000"
```

## Environment Variables

Key variables for deployment:

```bash
# Database
CONNECTIONSTRINGS__DEFAULTCONNECTION=Host=...

# JWT
JWTSETTINGS__SECRET=your-secret-key
JWTSETTINGS__ISSUER=https://issuer.com
JWTSETTINGS__AUDIENCE=https://audience.com
JWTSETTINGS__EXPIRATIONMINUTES=60

# CORS
CORS_ORIGINS=https://yourdomain.com

# Quartz Scheduler
QUARTZ__DATASTORE__CONNECTIONSTRING=...
```

## Health Checks

If implemented, health check endpoint:
```
GET /api/v1/health
```

## SignalR Configuration

### Connection URL
```
ws://localhost:5000/api/v1/notificationHub
```

### Re-connect Strategy
Auto-reconnect with exponential backoff (configured in client)

## Logging Configuration

Logging levels (appsettings.json):
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft": "Warning",
    "Microsoft.EntityFrameworkCore": "Information"
  }
}
```

## Database Migrations

### Apply Migrations
```bash
dotnet ef database update
```

### Create New Migration
```bash
dotnet ef migrations add MigrationName
```

### Remove Latest Migration
```bash
dotnet ef migrations remove
```

## Troubleshooting

| Issue | Solution |
|-------|----------|
| **Connection String Error** | Verify PostgreSQL is running, check connection string in appsettings |
| **Port Already in Use** | Change port in launchSettings.json or kill process on that port |
| **Database not seeding** | Check PmmsDbSeeder, verify roles exist |
| **CORS errors** | Update allowed origins in Program.cs for your frontend URL |
| **JWT token expired** | Adjust `JwtSettings:ExpirationMinutes` |
| **SignalR connection fails** | Verify WebSocket support, check CORS origin |
