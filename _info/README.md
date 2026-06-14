``` bash
    # This is an OJT project for DHSUD as part of our BSIT On-The-Job Training requirements on STI College Legazpi
```

## What is PMMS?
    Project Monitoring and Management System(PMMS) are designed to be a centralized
    project storage for DHSUD-HREDRD, it has 3 roles or actors which are ADMIN, PERMANENT, and TEMPORARY.
    Its main feature is to track the projects that are 2 months before its expiration.
    The system will send a notification everyday if an expiration entered the 2 months prior.
    The incoming expiration and expired projects will be tracked, update changes and send log automatically everyday at 1:00 am

## Whats the purpose of PMMS?
    To reduce the loads of tracking manually,
    to organize workflow and separations of concerns,
    to minimize human errors,
    and to centralize the collection of all projects

## Version:
    V 0.1

## Development Duration:
    2 months

## V1 Completion Date:
    June 9, 2026

## NOTE:
    the server is still on V1 and updates/changes should be done including a test which includes
    breaking the server on purpose to identify the bugs

## TECH STACK
**Core Framework**
    Runtime/Framework: 
        .NET 10.0
    Application Type: 
        ASP.NET Core Web Application

**Data & Persistence**
    ORM: 
        Entity Framework Core 10
    Database Provider:  
        PostgreSQL (via Npgsql)
    Identity: 
        ASP.NET Core Identity (integrated with EF Core for user management)

**Architecture & Patterns**
    Mediation: 
        MediatR (Implements the Mediator pattern, commonly used for CQRS)
    Mapping: 
        Mapster (High-performance object-to-object mapper)
    Validation: 
        FluentValidation (Strongly-typed validation rules)
    Background Jobs: 
        Quartz.NET (Job scheduling and background task management)

**Security & API**
    Authentication:    
        JWT Bearer Authentication (Microsoft.AspNetCore.Authentication.JwtBearer)
    API Documentation/UI: 
        Scalar (Used here as an alternative to Swagger/OpenAPI UI for API exploration)




For further informations about Features, navigate to:
    ~ PMMS.Server > Features

For further informations about Entities, navigate to:
    ~ PMMS.Server > Domain > Entities

For Entity configurations, navigate to:
    ~ PMMS.Server > Infrastructure > Persistence > Configurations

To Access the sample admin account (Change credential and do not include this _info during production/deployment):
    - open terminal
    - navigate to PMMS.Server
    - Run command: dotnet user-secrets list





## Roles:
- Admin: Handles user management
    **Features involved**
        - Assignments
        - Authentication
        - Users
- Permanent: Handles project management and tracking
    **Features involved**
        - Authentication
        - Central Project Network
        - Expirations
        - Municipalities
        - Projects
        - Project Types
- Temporary: Add projects as draft and save it in main project collection
    **Features involved**
        - Authentication
        - Drafts
        - Municipalities
        - ProjectTypes





## Features:
**Assignments**: assign a province to permanent user
**Authentication**: authenticate user credential and create JWT as user session
**CentralProjectNetwork**: centralized project collection where all projects are stored
**Drafts**: a drafting section for temporary users to add and verify projects before saving
**Expirations**: a monitoring/tracking features that tracks the projects expirations
**Municipalities**: fetches the seeded municipality data from database
**Notification**: system logs that are sent to user
**Projects**: a workspace for permanent users that are filtered based on the users current province assigned
**ProjectTypes**: fetches the seeded project tyeps data from database
**Users**: a user management that performs crud operation




## ENUMS
Expiration Types:
    Date of Completion
    Extension of Time
    Semestral Report
    Performance Bond

Expiration Status:
    None = 1,
    Ongoing = 2,            // expiration is running
    Completed = 3,          // expiraiton is completed
    Extended = 4,           // expiration is extended
    NearExpiration = 5,     // expiration is near expiration
    Expired = 6,            // expiraiton is expired
    Cancelled = 7           // expiration is cancelled

Notification Types:
    None = 1,
    ProjectAddedReport = 2,
    DailyReport = 3,
    Alert = 4 

Project Setup Status:
    None = 1,
    Active = 2,             // an expiration has been added
    InActive = 3,           // the project has been soft deleted 
    NotTracked = 4,         // the project is present but the expiration is not set

Project Severities:
    None = 1,
    Normal = 2,             // No expired expiration
    Warning = 3,            // 1 expiration type expired
    Critical = 4,           // 2 Expiration type expired
    Severe = 5              // 3 or more expiration type expired

Project Statuses:
    None = 1,
    OnGoing = 2,            // project is running with an expiration of DOC
    Extended  = 3,          // project is running with an expiration of EOT
    Completed  = 4,         // project is completed: COC is given
    FullyCompleted = 5,     // project is completed: DOD is given
    Cancelled = 6,          // Project is cancelled within ongoing and extended status


## KEY DATAS
**Date of Completion(DOC) Rules:**
    - will accept user iput
    - should not accept input less than DateIssued
    - should not accept input if COC is given
    - should not accept input if DOD is given
    - base expiration for every project
    - default project status will be ONGOING
    - should be terminated if EOT is given
    - should be terminated if project status is COMPLETED

**Extension of Time(EOT) Rules:**
    - will accept user input
    - should not accept input less than DateIssued and DOC
    - should not accept input if COC is given
    - should not accept input if DOD is given
    - extends DOC
    - accepts data if a project expiration should be extended
    - should mark the project as EXTENDED
    - isExtended = true
    - should be terminated if project status is COMPLETED

**Semestral Report(SR) Rules:**
    - will not accept user input
    - calculated by system
    - expiration per semester
    - will calculate future expiration as long as DOD is not given
    - renewed by user using handleSemestralReport slice
    - should be terminated or end its expiration calculation if project status is FULLYCOMPLETED
    - will depend on DateIssued for the semester calculation
    - will depend on DOD for the expiration completion

**Performance Bond(PB) Rules:**
    - will not accept user input
    - calculated by system
    - expiration per year
    - will calculate future expiration as long as COC is not given
    - renewed by user using handlePerformanceBond slice
    - should be terminated or end its expiration calculation if project status is COMPLETED
    - will depend on DateIssued for the expiration calculation
    - will depend on COC for the expiration completion

**Certificate of Completion(COC) Rules:**
    - will accept user input
    - should only accept input if DOC or EOT have data
    - should not accept input less than DOC or EOT
    - should mark the project as COMPLETED

**Deed of Donation(DOD) Rules:**
    - will accept user input
    - should only accept input if COC is given
    - should not accept input less than COC
    - should mark the project as FULLYCOMPLETED



## BUSINESS RULES
**General Project Rules:**
    - A project must belong to one municipality and one project type.
    - A project can only be managed by users assigned to the same province as the project.
    - Temporary users can create draft projects only; drafts must be published before becoming active projects.
    - Deleted projects must be treated as inactive and excluded from active tracking views.
    - Every project must follow the expiration lifecycle defined by the system.

**Project Lifecycle Rules:**
    - A new project starts as ONGOING once its Date of Completion is set.
    - A project becomes EXTENDED when an Extension of Time is added.
    - A project becomes COMPLETED when a Certificate of Completion is recorded.
    - A project becomes FULLYCOMPLETED when a Deed of Donation is recorded.
    - A project that is COMPLETED or FULLYCOMPLETED must stop generating future expiration records that no longer apply.

**Expiration Logic Rules:**
    - Date of Completion is the base expiration for every project.
    - Extension of Time is only valid when the project is still active and has a valid Date of Completion.
    - Semestral Report is system-generated every six months until the project becomes FULLYCOMPLETED.
    - Performance Bond is system-generated every year until the project becomes COMPLETED or FULLYCOMPLETED.
    - Expiration dates must never be earlier than the project issuance date.
    - Expiration records must reflect the latest valid project status.

**Status Transition Rules:**
    - DOC creates or maintains an ONGOING project state.
    - EOT changes the project state to EXTENDED.
    - COC changes the project state to COMPLETED.
    - DOD changes the project state to FULLYCOMPLETED.
    - Expiration processing must stop when the project reaches a terminal state.

**Notification Rules:**
    - The system must generate notifications for upcoming expirations.
    - The system must generate notifications for expired records.
    - Notifications must be tied to the affected project.
    - Daily monitoring must run automatically at 1:00 AM.
