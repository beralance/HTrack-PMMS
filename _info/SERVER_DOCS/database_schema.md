## Database Schema

You can check:
    > PMMS.Server/domain/entities
    > for: Entities and Properties

You can check:
    > PMMS.Server/infrastructure/persistence/configuration
    > for: Schema related configurations
    
You can check:
    > PMMS.Server/infrastructure/persistence/PmmsDbContext.cs
    > for: database context

You can check:
    > PMMS.Server/infrastructure/persistence/PmmsDbSeeder.cs
    > for: seeding data syntax


If changes in entities/properties, configurations, context and seeder are changed:
    > Migrate before running
    > Commands: 
        - dotnet ef migrations add Identifier
        - dotnet ef database update
    > For further informations, check the link below
    *https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/?tabs=dotnet-core-cli*