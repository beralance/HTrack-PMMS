using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Domain.Entities;

namespace PMMS.Server.Infrastructure.Persistence;

public class PmmsDbContext(DbContextOptions<PmmsDbContext> options) 
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Municipality> Municipalities => Set<Municipality>();
    public DbSet<ProjectType> ProjectTypes => Set<ProjectType>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Expiration> Expirations => Set<Expiration>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Province> Provinces => Set<Province>();
    public DbSet<Draft> Drafts => Set<Draft>();


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(PmmsDbContext).Assembly);
        builder.HasPostgresExtension("citext");

        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName()?.ToLower();
            entity.SetTableName(tableName);
        }
    }
}