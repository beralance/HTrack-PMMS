# Project Monitoring and Management System (PMMS)

An On-the-Job Training (OJT) project developed for DHSUD-HREDRD as part of the BSIT OJT requirements at STI College Legazpi.

---

## What is the project?
PMMS is a centralized web-based platform developed for the Department of Human Settlements and Urban Development (DHSUD) – Housing and Real Estate Development Regulation Division (HREDRD) during my internship.

The system was designed to reduce the administrative workload involved in monitoring government projects by replacing manual, paper-based processes with a centralized digital platform. It provides project management, automated expiration monitoring, role-based access control, and project tracking to improve operational efficiency and data accessibility.

As the primary backend developer, I designed and implemented the application's backend architecture, REST APIs, authentication and authorization, database design, and automated background processing. The project was originally intended to be developed as a full-stack application within the internship period.

---

## The Problem
Prior to PMMS, the HREDRD division managed projects using a combination of spreadsheets and physical documents. While this approach allowed project information to be stored digitally, monitoring and managing projects remained largely manual and time-consuming.

One of the biggest challenges was expiration monitoring. Staff had to manually inspect spreadsheet records and compare project dates to determine which projects were nearing expiration or had already expired. As the number of projects grew, this process became increasingly difficult and prone to oversight.

The existing workflow also introduced several operational challenges:

- **Human Error** – Spreadsheet cells could be modified freely, making records susceptible to typographical mistakes and inconsistent data.
- **Fragmented Information** – Project records were split between physical documents and spreadsheets, making information difficult to organize and maintain.
- **Limited Collaboration** – Staff relied on general-purpose productivity tools rather than a system designed specifically for their workflow, resulting in disconnected processes.
- **Lack of Workflow Integration** – Project monitoring, reporting, and record management required switching between multiple tools instead of working within a single, centralized environment.
- **Administrative Overhead** – Maintaining project records digitally was only one part of the overall workflow, adding to the division's operational workload.

These challenges highlighted the need for a dedicated project management platform that could centralize project information, automate repetitive monitoring tasks, and support the division's day-to-day operations through a workflow tailored to their specific business processes.

---

## Technical Highlights

- **Workflow-Driven Design** – Built the system around the division's existing workflow, allowing staff to transition from manual processes without changing how they work.
- **Automatic Expiration Tracking** – A single project entry automatically calculates and stores all applicable expiration schedules, enabling continuous project monitoring.
- **Automated Background Monitoring** – The system checks project expirations every day and automatically performs monitoring tasks without user intervention.
- **Permission-Based Notifications** – Notifications are only sent to users responsible for the affected project's assigned province, ensuring relevant updates reach the right personnel.
- **Draft Approval Workflow** – Internship personnel can only create draft projects, which must be reviewed before becoming part of the main project repository.
- **Centralized Project Repository** – All projects are stored in one centralized system, giving authorized users a complete and organized view of project information.
- **Role-Oriented Workspaces** – Each user is provided with a workspace tailored to their assigned responsibilities, reducing unnecessary information and improving focus.
- **Project Severity Classification** – Projects are automatically categorized by expiration status, helping users quickly identify records that require immediate attention.
- **Scalable Backend Architecture** – Built using a modular, feature-based architecture to simplify maintenance and support future expansion.

---

## My Contributions

As the primary backend developer, I was responsible for designing and implementing the backend of the Project Monitoring and Management System (PMMS). I analyzed business requirements, designed the system architecture, and translated manual workflows into a scalable and maintainable software solution.

### My Responsibilities

- **Designed the Backend Architecture** – Planned the overall system structure, feature organization, and project architecture with scalability, maintainability, and future development in mind.

- **Designed the Database** – Created the relational database structure and relationships to support project management, expiration monitoring, notifications, and user permissions.

- **Translated Business Workflows into Software** – Worked closely with the division's requirements to digitize existing processes without disrupting their established workflow.

- **Engineered the Expiration System** – Designed the logic that automatically calculates and stores project expiration schedules from a single project entry, providing the foundation for automated monitoring and notifications.

- **Built Automated Monitoring** – Implemented scheduled background processes that continuously monitor project expirations and generate notifications for the appropriate users.

- **Designed Permission-Based Workflows** – Implemented role-based authorization, dedicated workspaces, draft approval, and permission-based notifications to ensure users only interact with information relevant to their responsibilities.

- **Developed Flexible Business Logic** – Designed the project workflow to support multiple project states and independent expiration lifecycles through a single project entry process.

- **Engineered for Maintainability** – Organized the project using a modular architecture that simplifies future maintenance, feature expansion, and onboarding for new developers.

- **Performed Continuous Testing** – Regularly tested features throughout development to validate business rules, verify expected behavior, and identify potential edge cases before deployment.

- **Selected the Technology Stack** – Evaluated and integrated technologies that best matched the project's functional requirements, scalability goals, and long-term maintainability.

---

## Key Engineering Decisions

Throughout the development of PMMS, I focused on building a solution that aligned with the division's existing workflow while remaining scalable, maintainable, and practical for long-term use.

### Preserve the Existing Workflow

Instead of forcing users to adapt to a completely new process, the system was designed around the division's current workflow. This reduced the learning curve and allowed staff to transition naturally from manual operations to a digital environment.

### Design Around Business Rules

Rather than building generic CRUD operations, the system was designed to reflect the division's actual business processes. Project states, expiration lifecycles, notifications, permissions, and approvals were implemented according to real operational requirements.

### Automate Repetitive Tasks

Recurring administrative work such as expiration monitoring and notification generation was automated through scheduled background processing, allowing users to focus on project management instead of repetitive manual tasks.

### Keep User Responsibilities Separate

Different users perform different responsibilities within the division. The system provides dedicated workspaces, permissions, and access restrictions so each user only interacts with information and features relevant to their role.

### Build for Future Developers

The backend was structured using a modular architecture with clear separation of concerns to simplify maintenance, encourage scalability, and make future development easier.

### Validate Business Logic Continuously

Development was driven by continuous testing and validation to ensure that business rules behaved as expected across different project scenarios. Particular attention was given to edge cases and logical consistency rather than simply producing functional output.

### Prioritize Practical Solutions

Every major feature was evaluated not only from a technical perspective but also from an operational one. The goal was to deliver solutions that improved the division's daily workflow while remaining realistic, maintainable, and easy to adopt.

---

## Key Features

- Centralized project repository
- Project expiration tracking
- Automated notification system
- Daily monitoring and reporting
- Role-based access control
- Draft management for temporary users
- Regional access restrictions
- Automatic expiration status updates

# Technology Stack

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

# Project Structure

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

# User Roles

## Admin
Responsible for user management.
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

## Why This Project Matters
PMMS was built to solve real operational challenges within a government division by digitizing existing workflows while minimizing disruption to daily operations.

---

## Note
This project was developed as part of a BSIT On-the-Job Training Program.
