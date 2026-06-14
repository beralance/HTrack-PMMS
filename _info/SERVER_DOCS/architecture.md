# PMMS Server Architecture

## Overview
The PMMS (Project Management & Monitoring System) Server is built on a clean, layered architecture using ASP.NET Core 10.0 with a focus on maintainability, scalability, and separation of concerns.

## Architecture Layers

### 1. **Common Layer** (`/Common`)
Shared infrastructure and utilities used across the application.

#### Sub-components:
- **Behaviors**: MediatR pipeline behaviors for cross-cutting concerns
  - `ValidationBehavior.cs`: Validates commands/queries using FluentValidation
  - `CreateExpirationBehavior.cs`: Handles expiration creation workflow

- **Endpoints**: Generic endpoint pattern implementation
  - `IEndpoint.cs`: Interface for endpoint definition
  - `EndpointsExtension.cs`: Extension methods for mapping endpoints

- **Extensions**: Dependency injection and service configuration
  - `IdentityServiceExtension.cs`: Identity/Authentication setup
  - `InfrastructureServiceExtension.cs`: Database and EF Core configuration
  - `OpenApiExtensions.cs`: Swagger/OpenAPI documentation setup
  - `QuartzExpirationReportExtension.cs`: Scheduled job configuration

- **Exceptions**: Global error handling
  - `DomainException.cs`: Custom domain exceptions
  - `GlobalExceptionHandler.cs`: Middleware for exception handling

- **Helper**: Business logic utilities
  - `PerformanceBondCalculator.cs`: Calculations for performance bonds
  - `ProjectTimelineCalculator.cs`: Project timeline computations
  - `StringFormatter.cs`: String manipulation utilities

- **Hubs**: Real-time communication
  - `NotificationHub.cs`: SignalR hub for push notifications

- **Result**: Standardized response wrapper
  - `AppResult.cs`: Generic result container
  - `AppResultExtension.cs`: Extension methods
  - `ErrorType.cs`: Error type enumeration

- **Security**: User context and authorization
  - `IUserContext.cs`: Current user information interface

### 2. **Domain Layer** (`/Domain`)
Core business entities and enums representing the problem domain.

#### Entities:
- `ApplicationUser.cs`: User accounts with identity integration
- `Project.cs`: Main project entity
- `ProjectType.cs`: Project classification
- `Assignment.cs`: User project assignments
- `Draft.cs`: Temporary project drafts
- `Expiration.cs`: Project expiration tracking
- `Notification.cs`: User notifications
- `Province.cs`: Geographic location
- `Municipality.cs`: Geographic subdivision

#### Enums:
- `ProjectStatuses.cs`: Draft, Active, Completed, OnHold, etc.
- `ProjectSetupStatus.cs`: Setup progression states
- `ExpirationStatus.cs`: Expiration tracking states
- `ExpirationTypes.cs`: Types of expirations (Performance Bond, Report, etc.)
- `NotificationTypes.cs`: Notification categories
- `ProjectSeverities.cs`: Project severity levels

### 3. **Features Layer** (`/Features`)
Business logic organized by feature using a vertical slice approach with MediatR.

#### Feature Groups:
- **Authentication**: Login and user authentication
- **Users**: User management
- **Projects**: Project CRUD and operations
- **Assignments**: Province/project assignments to users
- **Drafts**: Draft project management
- **Expirations**: Expiration tracking and management
- **Municipalities**: Municipality data management
- **Notifications**: Notification system
- **CentralProjectNetwork**: Integration with external CPN system
- **ProjectTypes**: Project type management

Each feature contains:
- Commands (Create, Update, Delete)
- Queries (Read, Filter, List)
- Validators
- MediatR handlers

### 4. **Infrastructure Layer** (`/Infrastructure`)
Data access and external service integration.

#### Components:
- **Persistence**: Entity Framework Core and database context
  - `PmmsDbContext.cs`: Main database context
  - `PmmsDbSeeder.cs`: Database seeding
  - Entity configurations and migrations

## Design Patterns Used

### **CQRS (Command Query Responsibility Segregation)**
- Commands for write operations
- Queries for read operations
- Separated via MediatR

### **Vertical Slice Architecture**
Features are organized by business capability, not by technical layers. Each feature is self-contained with its own commands, queries, and handlers.

### **Dependency Injection (DI)**
Service registration through extension methods in Common/Extensions for clean startup configuration.

### **Generic Endpoint Pattern**
Standardized endpoint handling through `IEndpoint` interface for consistent routing and response handling.

### **Pipeline Behaviors (AOP)**
MediatR behaviors for:
- Validation
- Logging
- Expiration handling
- Error handling

## Technology Stack

- **Framework**: ASP.NET Core 10.0
- **ORM**: Entity Framework Core 10.0.8
- **Database**: PostgreSQL (via Npgsql)
- **API Documentation**: Swagger/OpenAPI with Scalar
- **Validation**: FluentValidation
- **Mapping**: Mapster
- **CQRS**: MediatR
- **Real-time Communication**: SignalR
- **Authentication**: JWT Bearer + Identity
- **Background Jobs**: Quartz.NET

*For further information: Check PMMS.Server/PMMS.Server.csproj*

## Communication Flow

```
Client Request
    ↓
[CORS Middleware]
    ↓
[Path Base: /api/v1]
    ↓
[Authentication/Authorization]
    ↓
[Endpoint Router]
    ↓
[MediatR Handler]
    ↓
[Validation Behavior]
    ↓
[Business Logic]
    ↓
[Infrastructure/Database]
    ↓
[Response via AppResult]
    ↓
Client Response
```

## Real-time Features

- **SignalR Hub**: `NotificationHub` handles real-time notifications
- Connected to endpoint: `/notificationHub`
- Used for: Push notifications, live updates

## Background Jobs

- **Quartz.NET Integration**: Scheduled expiration tracking and reporting
- Runs independently in the background
- Configured in `QuartzExpirationReportExtension`

## Database Strategy

- Entity Framework Core with Code-First approach
- PostgreSQL as primary database
- Automatic migrations and seeding on startup
- User identity integration


