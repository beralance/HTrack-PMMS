using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PMMS.Server.Common.Hubs;
using PMMS.Server.Domain.Enums;
using PMMS.Server.Features.Drafts.SaveDraft;
using PMMS.Server.Infrastructure.Persistence;

namespace PMMS.Server.Features.Notification;

public class ProjectDraftCreatedEventHandler(PmmsDbContext context, IHubContext<NotificationHub> hubContext) 
    : INotificationHandler<DraftCreatedEvent>
{
    public async Task Handle(DraftCreatedEvent notification, CancellationToken ct)
    {
        // GOAL: gete all active users that are assigned to a province similar to the project
        // user that are not soft deleted, and verified to true
        //  is permanent should be true
        var targetUsers = await context.Assignments
            .Where(a => a.ProvinceId == notification.ProvinceId)
            .Select(p => p.UserId)
            .ToListAsync(ct);

        if (!targetUsers.Any()) return;

        var systemNotifications = targetUsers.Select(userId => new PMMS.Server.Domain.Entities.Notification
        {
            Title = "New Project Registered",
            Content = "You have a new Project to monitor.",
            Type = NotificationTypes.ProjectAddedReport,
            IsRead = false,
            IsDeleted = false,
            CreatedAt = DateTimeOffset.UtcNow,
            
            UserId = userId,
            ProjectId = notification.ProjectId,
            ProvinceId = notification.ProvinceId,
            SenderId = notification.TemporaryId
        }).ToList();

        context.Notifications.AddRange(systemNotifications);
        await context.SaveChangesAsync(ct);

        string targetingRoomName = $"Province_{notification.ProvinceId}";
        
        await hubContext.Clients.Group(targetingRoomName)
            .SendAsync("RefreshNotifications", ct);
    }
}