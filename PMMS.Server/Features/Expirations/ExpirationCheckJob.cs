using Microsoft.EntityFrameworkCore;
using PMMS.Server.Infrastructure.Persistence;
using Quartz;
using PMMS.Server.Domain.Enums;
using Microsoft.AspNetCore.SignalR;
using PMMS.Server.Common.Hubs;
using PMMS.Server.Common.Helper;

namespace PMMS.Server.Features.Expirations;

public class ExpirationCheckJob(
    PmmsDbContext context,
    IHubContext<NotificationHub> hubContext) : IJob
{
    public async Task Execute(IJobExecutionContext executionContext)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var sixtyDaysFromNow = today.AddDays(60);
        bool changesMade = false;
        
        // Track unique province IDs to aggregate targeted real-time UI refresh triggers
        var affectedProvinceIds = new HashSet<int>(); 

        // 1. Process upcoming warnings (Items entering the 60-day threshold window)
        var priorExpirations = await context.Expirations
            .Include(e => e.Project)
                .ThenInclude(p => p!.Municipality)
            .Where(e => 
                e.ExpiresOn <= sixtyDaysFromNow &&
                e.ExpiresOn >= today &&
                e.Status == ExpirationStatuses.Ongoing &&
                e.IsActive == true &&
                e.Project != null && 
                !e.Project.IsDeleted)
            .ToListAsync();
        
        foreach (var expiration in priorExpirations)
        {
            // Transition tracking status to NearExpiration warning block
            expiration.Status = ExpirationStatuses.NearExpiration;
            changesMade = true;

            if (expiration.Project?.Municipality != null)
            {
                affectedProvinceIds.Add(expiration.Project.Municipality.ProvinceId);
            }

            string readableType = (expiration.Type?.ToString() ?? "Unknown").SplitCamelCase();

            // Log Cron changes
            Console.WriteLine($"[ExpirationCheckJob] Entered 2 months priority window: \n'{readableType}' for project '{expiration.Project?.ProjectName}' is expiring on {expiration.ExpiresOn:MMMM dd, yyyy}.");

            // Record a persistent notification entry
            context.Notifications.Add(new Domain.Entities.Notification
            {
                Title = "Upcoming Expiration Warning!",
                Content = $"Expiration type '{readableType}' from {expiration.Project?.ProjectName} is expiring on {expiration.ExpiresOn:MMMM dd, yyyy}.",
                CreatedAt = DateTimeOffset.UtcNow,
                IsRead = false,
                ProjectId = expiration.ProjectId
            });
        }

        // 2. Process overdue items (Handles missed deadlines & prevents time-sync infrastructure drift)
        var expiredItems = await context.Expirations
            .Include(e => e.Project)
                .ThenInclude(p => p!.Municipality)
            .Where(e => 
                e.ExpiresOn < today && 
                (e.Status == ExpirationStatuses.NearExpiration || e.Status == ExpirationStatuses.Ongoing) && 
                e.IsActive &&
                e.Project != null && 
                !e.Project.IsDeleted)
            .ToListAsync();

        if (expiredItems.Count != 0)
        {
            // Gather distinct project profiles to pre-calculate severities efficiently
            var affectedProjectIds = expiredItems.Select(e => e.ProjectId).Distinct().ToList();

            var projectExpiredCounts = await context.Expirations
                .Where(e => affectedProjectIds.Contains(e.ProjectId) && e.Status == ExpirationStatuses.Expired)
                .GroupBy(e => e.ProjectId)
                .ToDictionaryAsync(g => g.Key!.Value, g => g.Count()); 

            foreach (var expiration in expiredItems)
            {
                expiration.Status = ExpirationStatuses.Expired;
                expiration.ExpiredAt = DateTimeOffset.UtcNow;
                changesMade = true;
                
                if (expiration.Project?.Municipality != null)
                {
                    affectedProvinceIds.Add(expiration.Project.Municipality.ProvinceId);
                }

                string readableType = (expiration.Type?.ToString() ?? "Unknown").SplitCamelCase();

                context.Notifications.Add(new Domain.Entities.Notification
                {
                    Title = $"{readableType} Expired",
                    Content = $"Expiration type '{readableType}' for project '{expiration.Project?.ProjectName}' has officially expired.",
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsRead = false,
                    ProjectId = expiration.ProjectId
                });

                // 3. Recalculate and scale regional project severity metrics in-memory
                if (expiration.Project != null && expiration.ProjectId.HasValue)
                {
                    projectExpiredCounts.TryGetValue(expiration.ProjectId.Value, out int currentDbCount);
                    currentDbCount++; 
                    projectExpiredCounts[expiration.ProjectId.Value] = currentDbCount; 

                    expiration.Project.Severity = currentDbCount switch
                    {
                        0 => ProjectSeverities.Normal,
                        1 => ProjectSeverities.Warning,
                        2 => ProjectSeverities.Critical,
                        _ => ProjectSeverities.Severe,
                    };
                }
            }
        }

        // 4. Conclude batch tracking cycle, save changes, and announce websocket push alerts
        if (changesMade)
        {
            await context.SaveChangesAsync();
            
            // Dispatch a real-time localized message broadcast strictly to impacted user groups
            foreach (var provinceId in affectedProvinceIds)
            {
                string groupName = $"Province_{provinceId}";
                await hubContext.Clients.Group(groupName)
                    .SendAsync("ReceiveNotification", "Project compliance statuses updated for your region.");
            }
        }
    }
}