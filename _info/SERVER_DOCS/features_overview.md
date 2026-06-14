# Features Overview

## Feature Categories

### 1. Projects
Complete project lifecycle management from creation to completion.

**Sub-features:**
- Create new projects
- Edit project details
- View all projects with filtering
- Delete projects (soft delete)

**Key Entities:**
- `Project.cs`: Main project entity
- `ProjectType.cs`: Project categorization

**Handlers:**
- Create: `Features/Projects/CreateProject.cs`
- Read: `Features/Projects/GetProjects.cs`, `Features/Projects/GetProjectById.cs`
- Update: `Features/Projects/UpdateProject.cs`
- Delete: `Features/Projects/DeleteProject.cs`


### 2. Drafts
Temporary project storage before finalization.

**Sub-features:**
- Create project drafts
- Save draft with project details
- List all drafts
- Delete drafts
- Edit draft information

**Key Entities:**
- `Draft.cs`: Temporary project entity

**Handlers:**
- Create: `Features/Drafts/CreateDraft.cs`
- Save: `Features/Drafts/SaveDraft/`
- List: `Features/Drafts/GetDrafts.cs`
- Read: `Features/Drafts/GetDraftById.cs`
- Delete: `Features/Drafts/DeleteDraft.cs`
- Update: `Features/Drafts/UpdateDraft.cs`


### 3. Expirations
Track and manage project expiration dates and deadlines.

**Sub-features:**
- Add expiration dates to projects
- Track expiration status
- Extend expiration dates
- Reset expiration
- Handle performance bonds
- Handle semestral reports
- Automatic expiration checking (Quartz job)
- Expiration alerts and notifications

**Key Entities:**
- `Expiration.cs`: Expiration records
- `ExpirationStatus.cs`: Active, Extended, Expired, Handled
- `ExpirationTypes.cs`: PerformanceBond, Report, Submission, Extension

**Handlers:**
- Add: `Features/Expirations/AddExpirations/`
- Get: `Features/Expirations/GetExpirations.cs`
- Get by Project: `Features/Expirations/GetExpirationsByProjectId.cs`
- Get by ID: `Features/Expirations/GetExpirationById.cs`
- Extend: `Features/Expirations/ExtendProjectExpiration.cs`
- Reset: `Features/Expirations/ResetExpiration/`
- Performance Bond: `Features/Expirations/HandlePerformanceBond.cs`
- Semestral Report: `Features/Expirations/HandleSemestralReport.cs`
- Check Job: `Features/Expirations/ExpirationCheckJob.cs` (Background)

**Expiration Types:**

#### Performance Bond
- Expiration tracked every year (6 months)
- Depends on Date issued

#### Semestral Report
- Expiration tracked every semester (6 months)
- Depends on Date issued

#### Date of Completion
- Default/First Submission deadline

#### Extension of Time
- Extend project expiration
- Depends on Date Date of Completion

### 4. Assignments
User assignments to provinces and projects.

**Sub-features:**
- Assign users to provinces
- View user assignments
- Change assignment status
- Remove assignments
- Track assignment dates

**Key Entities:**
- `Assignment.cs`: User to province/project mapping

**Handlers:**
- Assign Province: `Features/Assignments/AssignProvince.cs`
- Get Assignments: `Features/Assignments/GetAssignments.cs`
- Get by User: `Features/Assignments/GetAssignmentsByUser.cs`


### 5. Municipalities
Geographic data management for projects.

**Sub-features:**
- List all municipalities
- Get municipality details
- Filter by province
- Get projects in municipality

**Key Entities:**
- `Municipality.cs`: Subdivision entity
- Relationship: Province has many Municipalities

**Handlers:**
- Get All: `Features/Municipalities/GetMunicipalities.cs`
- Get by ID: `Features/Municipalities/GetMunicipalityById.cs`

---

### 6. Provinces
Province/region management.

**Sub-features:**
- List all provinces
- Get province details
- View province projects
- Geographic filtering

**Key Entities:**
- `Province.cs`: Region entity

**Handlers:**
- Get All: `Features/Provinces/GetProvinces.cs`
- Get by ID: `Features/Provinces/GetProvinceById.cs`

---

### 7. Project Types
Project categorization and classification.

**Sub-features:**
- List project types
- Get type details
- View projects by type

**Key Entities:**
- `ProjectType.cs`: Project category

**Handlers:**
- Get All: `Features/ProjectTypes/GetProjectTypes.cs`
- Get by ID: `Features/ProjectTypes/GetProjectTypeById.cs`

---

### 8. Notifications
User notification system for events and alerts.

**Sub-features:**
- Create notifications
- Mark as read
- Delete notifications
- Real-time push via SignalR
- Notification filtering

**Key Entities:**
- `Notification.cs`: Notification record
- `NotificationTypes.cs`: ProjectUpdate, ExpirationAlert, AssignmentChange, SystemAlert

**Handlers:**
- Get: `Features/Notifications/GetNotifications.cs`
- Mark Read: `Features/Notifications/MarkNotificationAsRead.cs`
- Delete: `Features/Notifications/DeleteNotification.cs`

**SignalR Events:**
- `NewNotification`: Real-time notification push
- `ProjectUpdated`: Project change broadcast
- `ExpirationAlert`: Expiration alerts

---

### 9. Authentication & Authorization
User login and role-based access control.

**Sub-features:**
- User login
- JWT token generation
- Role-based authorization
- Permission checking
- Secure password management

**Key Components:**
- `Login.cs`: Authentication handler
- `ApplicationUser.cs`: Extended identity user
- Roles: Admin, Permanent, Temporary

**Handlers:**
- Login: `Features/Authentication/Login.cs`

---

### 10. Users
User management and profile.

**Sub-features:**
- Get user profile
- Update profile
- Get user assignments
- View user permissions
- List all users (Admin)
- Manage user roles (Admin)
- Deactivate/activate users (Admin)

**Key Entities:**
- `ApplicationUser.cs`: User entity

**Handlers:**
- Get Profile: `Features/Users/GetUserProfile.cs`
- Update Profile: `Features/Users/UpdateUserProfile.cs`
- Get All: `Features/Users/GetAllUsers.cs` (Admin)

---

### 11. Central Project Network (CPN)
Integration with external CPN system.

**Sub-features:**
- Fetch CPN projects
- Get CPN project details
- View CPN data

**Handlers:**
- Get Projects: `Features/CentralProjectNetwork/GetCpnProjects.cs`
- Get by ID: `Features/CentralProjectNetwork/GetCpnProjectById.cs`

## Background Jobs (Quartz)

### Expiration Check Job
- **Frequency**: Configured interval (typically hourly)
- **Purpose**: Check for expiring projects
- **Actions**:
  - Mark expirations as expired
  - Create alerts
  - Trigger notifications
  - Generate reports

**Handler**: `Features/Expirations/ExpirationCheckJob.cs`

## Validation

All features include input validation through FluentValidation:

**Behaviors**: `Common/Behaviors/ValidationBehavior.cs`

Example validations:
- Required fields
- Email format
- Date range
- Numeric ranges
- String length
- Unique constraints

## Error Handling

Centralized exception handling through:
- `GlobalExceptionHandler.cs`: Middleware for unhandled exceptions
- `DomainException.cs`: Domain-specific exceptions
- MediatR pipeline behaviors

## Result Wrapper

All responses wrapped in `AppResult`:

```csharp
{
  "success": bool,
  "statusCode": int,
  "message": string,
  "data": T,
  "errors": List<ErrorDetail>
}
