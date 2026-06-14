# PMMS Server Documentation Index

Welcome to the PMMS (Project Management & Monitoring System) comprehensive server documentation. This folder contains complete information about the server architecture, setup, features, and deployment.

## 📚 Documentation Files

### 1. **[Architecture Overview](01-ARCHITECTURE.md)**
Complete system architecture and design patterns used in PMMS server.

**Topics Covered:**
- Layered architecture (Common, Domain, Features, Infrastructure)
- Design patterns (CQRS, Vertical Slice, DI, AOP)
- Technology stack (ASP.NET Core 10.0, EF Core, PostgreSQL, MediatR)
- Communication flow
- Real-time features (SignalR)
- Background jobs (Quartz)

---

### 2. **[Server Setup & Configuration](02-SERVER_SETUP.md)**
Step-by-step guide to set up and run the PMMS server locally and in production.

**Topics Covered:**
- Prerequisites and environment setup
- Database configuration with PostgreSQL
- User Secrets setup for JWT and credentials
- CORS configuration
- Startup process and middleware
- Running in development mode
- API documentation access (Swagger/Scalar)
- Environment variables for production
- Troubleshooting common issues

---

### 3. **[API Endpoints](03-API_ENDPOINTS.md)**
Complete REST API documentation with all available endpoints and examples.

**Topics Covered:**
- Base URL and response format (AppResult wrapper)
- Authentication (JWT Bearer tokens)
- All endpoint categories:
  - Projects (CRUD, filtering)
  - Drafts (create, save, manage)
  - Assignments (user-project assignments)
  - Expirations (tracking, extension, reporting)
  - Notifications (real-time events)
  - Users (profile management)
  - Municipalities, Provinces, Project Types
  - CPN (Central Project Network integration)
- SignalR real-time events
- Error codes and handling
- Pagination support
- Rate limiting info

**Quick Start:**
```
POST /auth/login
GET /api/v1/projects
POST /api/v1/projects
```

---

### 4. **[Database Schema](04-DATABASE_SCHEMA.md)**
Detailed database structure, entities, relationships, and queries.

**Topics Covered:**
- Database system (PostgreSQL with EF Core)
- Entity relationship diagram
- All core entities:
  - ApplicationUser, Project, ProjectType
  - Province, Municipality
  - Assignment, Expiration, Draft
  - Notification
- Enums and status values
- Indexes for performance
- Database views
- Constraints and relationships
- Migration management
- Query optimization tips

---

### 5. **[Authentication & Authorization](05-AUTHENTICATION_AUTHORIZATION.md)**
Complete guide to JWT authentication and role-based access control.

**Topics Covered:**
- Authentication flow (login, token generation)
- JWT token structure and claims
- JWT configuration (secrets, issuer, audience, expiration)
- User Secrets setup
- Role hierarchy (Admin, Permanent, Temporary)
- Permission matrix
- Authorization attributes and policies
- Password policy requirements
- Token management and validation
- Common authentication issues and solutions
- Security best practices
- Default users created on startup

---

### 6. **[Features Overview](06-FEATURES_OVERVIEW.md)**
Comprehensive guide to all business features in PMMS.

**Topics Covered:**
- Projects feature (create, edit, track)
- Drafts feature (temporary storage)
- Expirations feature (performance bonds, reports)
- Assignments (user assignments)
- Municipalities and Provinces (geography)
- Project Types (categorization)
- Notifications (real-time alerts)
- Authentication & Authorization
- Users (profile management)
- CPN integration
- Background jobs (Quartz scheduling)
- Validation layer
- Error handling
- Result wrappers

---

### 7. **[Core Components](07-CORE_COMPONENTS.md)**
Deep dive into internal components, services, and utilities.

**Topics Covered:**
- Middleware & Behaviors:
  - Global Exception Handler
  - Validation Behavior
  - Expiration Behavior
- Hubs (SignalR NotificationHub)
- Result/Response Wrappers (AppResult<T>)
- Security Components (IUserContext)
- Helpers:
  - PerformanceBondCalculator
  - ProjectTimelineCalculator
  - StringFormatter
- Extension Methods (DI setup)
- Endpoint Pattern (IEndpoint interface)
- Domain Exceptions
- Database Seeder
- Dependency Injection Container

---

### 8. **[Deployment Guide](08-DEPLOYMENT_GUIDE.md)**
Complete guide to deploying PMMS to production environments.

**Topics Covered:**
- Pre-deployment checklist
- Environment variables required
- Docker deployment (Dockerfile, Docker Compose)
- Kubernetes deployment (manifests, ConfigMaps, Secrets)
- Manual server deployment (Systemd, Nginx)
- Database migrations
- Monitoring setup
- Backup strategy
- Scaling options
- Performance optimization
- Troubleshooting
- Post-deployment verification

---

## 🚀 Quick Start

### Development Environment
```bash
# 1. Clone repository
git clone <repo-url>
cd PMMS_MAIN/PMMS.Server

# 2. Set up User Secrets
dotnet user-secrets set "JwtSettings:Secret" "your-secret-key-min-32-characters"
dotnet user-secrets set "JwtSettings:Issuer" "https://localhost:5001"
dotnet user-secrets set "JwtSettings:Audience" "https://localhost:5173"

# 3. Update connection string
# Edit appsettings.Development.json with your PostgreSQL connection

# 4. Run the server
dotnet run

# 5. Access API documentation
# Swagger: http://localhost:5000/api/v1/swagger/index.html
# Scalar: http://localhost:5000/api/v1/scalar/v1
```

### Login with Default Credentials
```
Email: admin@pmms.com
Password: [Set during database seeding]
```

---

## 📁 File Structure

```
_info/
├── SERVER_DOCS/                          (This comprehensive documentation)
│   ├── 01-ARCHITECTURE.md               (System design)
│   ├── 02-SERVER_SETUP.md               (Installation & configuration)
│   ├── 03-API_ENDPOINTS.md              (REST API reference)
│   ├── 04-DATABASE_SCHEMA.md            (Database structure)
│   ├── 05-AUTHENTICATION_AUTHORIZATION.md (Auth & roles)
│   ├── 06-FEATURES_OVERVIEW.md          (Features guide)
│   ├── 07-CORE_COMPONENTS.md            (Internal components)
│   ├── 08-DEPLOYMENT_GUIDE.md           (Production deployment)
│   └── INDEX.md                         (This file)
├── installed.md                         (Installed packages info)
├── new_connection_string.md             (Connection string guide)
├── README.md                            (Original project info)
└── run.md                               (Run instructions)

PMMS.Server/
├── Program.cs                           (Main entry point)
├── Common/                              (Shared utilities & middleware)
├── Domain/                              (Business entities)
├── Features/                            (Business logic by feature)
├── Infrastructure/                      (Data access & persistence)
└── Properties/                          (Project properties)
```

---

## 🔍 Find Information By Topic

### "How do I...?"

**Setup & Configuration**
- Set up the development environment? → [Server Setup](02-SERVER_SETUP.md)
- Configure database connection? → [Server Setup - Database Configuration](02-SERVER_SETUP.md#1-database-configuration)
- Set up JWT authentication? → [Authentication - JWT Configuration](05-AUTHENTICATION_AUTHORIZATION.md#jwt-configuration)
- Deploy to production? → [Deployment Guide](08-DEPLOYMENT_GUIDE.md)

**API Usage**
- Call an endpoint? → [API Endpoints](03-API_ENDPOINTS.md)
- Authenticate requests? → [API Endpoints - Authentication](03-API_ENDPOINTS.md#authentication)
- Handle paginated results? → [API Endpoints - Pagination](03-API_ENDPOINTS.md#pagination)
- Use real-time updates? → [API Endpoints - Real-time Events](03-API_ENDPOINTS.md#real-time-events-signalr)

**Development**
- Understand the architecture? → [Architecture Overview](01-ARCHITECTURE.md)
- Add a new feature? → [Features Overview](06-FEATURES_OVERVIEW.md)
- Create a database entity? → [Database Schema](04-DATABASE_SCHEMA.md)
- Implement authentication? → [Authentication & Authorization](05-AUTHENTICATION_AUTHORIZATION.md)

**Operations**
- Monitor the application? → [Deployment Guide - Monitoring](08-DEPLOYMENT_GUIDE.md#monitoring)
- Back up the database? → [Deployment Guide - Backup Strategy](08-DEPLOYMENT_GUIDE.md#backup-strategy)
- Scale horizontally? → [Deployment Guide - Scaling](08-DEPLOYMENT_GUIDE.md#scaling)
- Fix a deployment issue? → [Deployment Guide - Troubleshooting](08-DEPLOYMENT_GUIDE.md#troubleshooting)

---

## 🛠️ Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Framework** | ASP.NET Core | 10.0 |
| **Language** | C# | Latest |
| **Database** | PostgreSQL | 13+ |
| **ORM** | Entity Framework Core | 10.0.8 |
| **CQRS** | MediatR | 14.1.0 |
| **Validation** | FluentValidation | 12.1.1 |
| **Mapping** | Mapster | 10.0.7 |
| **Auth** | JWT Bearer | ASP.NET Core 10 |
| **Real-time** | SignalR | ASP.NET Core 10 |
| **Jobs** | Quartz.NET | 3.18.1 |
| **API Docs** | Swagger/Scalar | Latest |
| **Identity** | ASP.NET Core Identity | 10.0.8 |

---

## 🔐 Security

- ✅ JWT token-based authentication
- ✅ Role-based authorization (RBAC)
- ✅ Strong password policies
- ✅ Global exception handling
- ✅ Secure database connections (PostgreSQL)
- ✅ CORS protection
- ✅ Input validation & sanitization
- ✅ Claims-based authorization

---

## 📊 Database

- **System**: PostgreSQL 13+
- **Approach**: Code-First with Migrations
- **Auto-migrate**: Yes (on startup)
- **Seed Data**: Yes (default roles, users, provinces)
- **Backup**: Regular backups recommended
- **Connection Pool**: Enabled

---

## 🔄 Key Workflows

### User Login & Access
```
1. User provides email/password
2. Server validates credentials
3. JWT token generated
4. Token sent to client
5. Client includes token in all requests
6. Server validates token & claims
7. Request authorized/denied based on roles
```

### Project Lifecycle
```
1. Create Draft
2. Save Draft as Project (status: Draft)
3. Activate Project (status: Active)
4. Add Expiration tracking
5. Handle Expirations (bonds, reports)
6. Complete Project (status: Completed)
7. Archive Project
```

### Background Processing
```
1. Quartz Job scheduled
2. Job runs at interval
3. Checks for expiring projects
4. Creates notifications
5. Sends real-time updates via SignalR
6. Updates database records
```

---

## 📞 Support & Troubleshooting

**Common Issues:**
- Connection string errors → [Server Setup - Troubleshooting](02-SERVER_SETUP.md#troubleshooting)
- Authentication failures → [Authentication - Common Issues](05-AUTHENTICATION_AUTHORIZATION.md#common-issues)
- API errors → [API Endpoints - Error Codes](03-API_ENDPOINTS.md#error-codes)
- Deployment problems → [Deployment Guide - Troubleshooting](08-DEPLOYMENT_GUIDE.md#troubleshooting)

---

## 📝 Documentation Standards

All documentation includes:
- Clear overview and purpose
- Step-by-step instructions
- Code examples
- Configuration samples
- Common issues & solutions
- Best practices
- Security considerations

---

## 🔄 Last Updated
**2026-06-06**

All documentation is maintained together and kept in sync with the codebase.

---

## 📋 Navigation Tips

- **Table of Contents**: Each document has a detailed TOC at the top
- **Links**: Use internal links (e.g., `[link](file.md#section)`) to navigate between documents
- **Code Examples**: All examples are ready to copy and use
- **Search**: Use browser search (Ctrl+F / Cmd+F) within documents
- **Version**: Refer to "[Last Updated]" date to check if information is current

---

**Ready to get started?** → Start with [Server Setup & Configuration](02-SERVER_SETUP.md) or [Architecture Overview](01-ARCHITECTURE.md)
