# Project Monitoring and Management System (PMMS)

> An On-the-Job Training (OJT) project developed for DHSUD-HREDRD as part of the BSIT OJT requirements at STI College Legazpi.

---

## 📌 Overview

The **Project Monitoring and Management System (PMMS)** is a centralized project storage and monitoring platform designed for **DHSUD-HREDRD**.

The system provides project tracking, expiration monitoring, automated notifications, and role-based access control.

### Main Objectives

- Reduce manual project tracking workload
- Organize workflows and responsibilities
- Minimize human errors
- Centralize project records and documentation
- Automate expiration monitoring and reporting

---

## 🚀 Key Features

- Centralized project repository
- Project expiration tracking
- Automated notification system
- Daily monitoring and reporting
- Role-based access control
- Draft management for temporary users
- Province-based project access restrictions
- Automatic expiration status updates

### Expiration Monitoring

The system monitors project expirations and:

- Detects projects nearing expiration (within 2 months)
- Sends daily notifications for upcoming expirations
- Tracks expired projects
- Generates automatic logs and reports
- Runs scheduled monitoring every day at **1:00 AM**

---

## 📋 Project Information

| Item | Value |
|--------|--------|
| Version | V0.1 |
| Development Duration | 2 Months |
| V1 Completion Date | June 9, 2026 |

---

## ⚠️ Current Status

The server is currently running on **Version 1 (V1)**.

Further updates, enhancements, and testing are required, including:

- Feature improvements
- Bug fixes
- Stress testing
- Intentional failure testing to identify system weaknesses
- Performance optimization

---

# 🛠 Technology Stack

## Core Framework

| Component | Technology |
|------------|-------------|
| Runtime | .NET 10 |
| Application Type | ASP.NET Core Web Application |

---

## Data & Persistence

| Component | Technology |
|------------|-------------|
| ORM | Entity Framework Core 10 |
| Database | PostgreSQL (Npgsql) |
| Identity | ASP.NET Core Identity |

---

## Architecture & Patterns

| Component | Technology |
|------------|-------------|
| Mediation | MediatR |
| Mapping | Mapster |
| Validation | FluentValidation |
| Background Jobs | Quartz.NET |

---

## Security & API

| Component | Technology |
|------------|-------------|
| Authentication | JWT Bearer Authentication |
| API Documentation | Scalar |

---

# 📂 Project Structure

### Features

```text
PMMS.Server
└── Features
```

### Domain Entities

```text
PMMS.Server
└── Domain
    └── Entities
```

### Entity Configurations

```text
PMMS.Server
└── Infrastructure
    └── Persistence
        └── Configurations
```

---

# 🔐 Sample Admin Account

For development purposes only:

```bash
cd PMMS.Server
dotnet user-secrets list
```

> ⚠️ Change credentials immediately and never expose development credentials during production deployment.

---

# 👥 User Roles

## Admin

Responsible for user and system management.

### Accessible Features

- Assignments
- Authentication
- Users

---

## Permanent User

Responsible for project management and monitoring.

### Accessible Features

- Authentication
- Central Project Network
- Expirations
- Municipalities
- Projects
- Project Types

---

## Temporary User

Responsible for drafting project entries before publication.

### Accessible Features

- Authentication
- Drafts
- Municipalities
- Project Types

---

# 📦 System Features

| Feature | Description |
|----------|-------------|
| Assignments | Assign provinces to permanent users |
| Authentication | User authentication and JWT generation |
| CentralProjectNetwork | Centralized project repository |
| Drafts | Draft workspace for temporary users |
| Expirations | Expiration monitoring and tracking |
| Municipalities | Retrieves municipality master data |
| Notifications | System-generated alerts and reports |
| Projects | Province-filtered project workspace |
| ProjectTypes | Retrieves project type master data |
| Users | User management module |

---

# 📚 Enums

## Expiration Types

- Date of Completion (DOC)
- Extension of Time (EOT)
- Semestral Report (SR)
- Performance Bond (PB)

---

## Expiration Status

| Value | Status |
|---------|---------|
| 1 | None |
| 2 | Ongoing |
| 3 | Completed |
| 4 | Extended |
| 5 | Near Expiration |
| 6 | Expired |
| 7 | Cancelled |

---

## Notification Types

| Value | Status |
|---------|---------|
| 1 | None |
| 2 | Project Added Report |
| 3 | Daily Report |
| 4 | Alert |

---

## Project Setup Status

| Value | Status |
|---------|---------|
| 1 | None |
| 2 | Active |
| 3 | Inactive |
| 4 | Not Tracked |

---

## Project Severities

| Value | Severity |
|---------|---------|
| 1 | None |
| 2 | Normal |
| 3 | Warning |
| 4 | Critical |
| 5 | Severe |

---

## Project Statuses

| Value | Status |
|---------|---------|
| 1 | None |
| 2 | Ongoing |
| 3 | Extended |
| 4 | Completed |
| 5 | Fully Completed |
| 6 | Cancelled |

---

# 📅 Expiration Rules

## Date of Completion (DOC)

- Accepts user input
- Cannot be earlier than `DateIssued`
- Cannot be modified if COC exists
- Cannot be modified if DOD exists
- Base expiration for every project
- Default status becomes **ONGOING**
- Terminates when EOT is added
- Terminates when project becomes COMPLETED

---

## Extension of Time (EOT)

- Accepts user input
- Must be later than DateIssued and DOC
- Cannot be added if COC exists
- Cannot be added if DOD exists
- Extends DOC
- Changes project status to **EXTENDED**
- Sets `IsExtended = true`
- Terminates when project becomes COMPLETED

---

## Semestral Report (SR)

- System-generated
- Generated every semester
- Uses DateIssued as baseline
- Continues until DOD exists
- Renewable through `handleSemestralReport`
- Stops when project becomes FULLYCOMPLETED

---

## Performance Bond (PB)

- System-generated
- Generated annually
- Uses DateIssued as baseline
- Continues until COC exists
- Renewable through `handlePerformanceBond`
- Stops when project becomes COMPLETED

---

## Certificate of Completion (COC)

- Accepts user input
- Requires DOC or EOT
- Cannot be earlier than DOC/EOT
- Changes project status to **COMPLETED**

---

## Deed of Donation (DOD)

- Accepts user input
- Requires COC
- Cannot be earlier than COC
- Changes project status to **FULLYCOMPLETED**

---

# 🏢 Business Rules

## General Project Rules

- Every project must belong to one municipality.
- Every project must belong to one project type.
- Users may only manage projects within their assigned province.
- Temporary users can only create drafts.
- Drafts must be published before becoming active projects.
- Deleted projects are treated as inactive.
- All projects must follow the expiration lifecycle.

---

## Project Lifecycle Rules

- DOC → ONGOING
- EOT → EXTENDED
- COC → COMPLETED
- DOD → FULLYCOMPLETED

Projects in terminal states must stop generating future expiration records.

---

## Expiration Logic Rules

- DOC is the base expiration for all projects.
- EOT requires an active DOC.
- SR is generated every 6 months.
- PB is generated annually.
- Expiration dates cannot be earlier than DateIssued.
- Expiration records must always reflect the latest project status.

---

## Status Transition Rules

```text
DOC → ONGOING
EOT → EXTENDED
COC → COMPLETED
DOD → FULLYCOMPLETED
```

Expiration processing stops once a project reaches its terminal state.

---

## Notification Rules

- Generate notifications for upcoming expirations.
- Generate notifications for expired records.
- Link notifications to affected projects.
- Execute monitoring automatically every day at 1:00 AM.

---

## 📄 License

This project was developed exclusively for DHSUD-HREDRD as part of the BSIT On-the-Job Training Program at STI College Legazpi.
