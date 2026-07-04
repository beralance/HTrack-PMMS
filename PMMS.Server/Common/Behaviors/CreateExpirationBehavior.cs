using PMMS.Server.Common.Helper;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Common.Behaviors;

public static class CreateExpirationBehavior
{
    public static IEnumerable<Expiration> CalculateExpirations(
        Project project,
        ProjectStatuses status,
        DateOnly dateOfCompletion,
        DateOnly? extensionOfTime)
    {
        var expirations = new List<Expiration>();
        
        bool isDone = status == ProjectStatuses.Completed || status == ProjectStatuses.FullyCompleted;
        bool isExtended = status == ProjectStatuses.Extended && project.IsExtended == true;
        bool isSrTerminated = status == ProjectStatuses.FullyCompleted;
        bool isPbTerminated = status == ProjectStatuses.Completed || status == ProjectStatuses.FullyCompleted;

        expirations.Add(new Expiration
        {
            Project = project,
            Type = ExpirationTypes.DateOfCompletion,
            Status = (isDone || isExtended)      
                ? ExpirationStatuses.Completed 
                : ExpirationStatuses.Ongoing,    
            CompletedAt = (isDone || isExtended) ? dateOfCompletion : null,         
            ExpiresOn = (isDone || isExtended) ? null : dateOfCompletion,             
            CreatedAt = DateTime.UtcNow,
            LastExpirationDate = (isDone || isExtended) ? dateOfCompletion : null,
            IsActive = true
        });

        if(extensionOfTime.HasValue) {
            expirations.Add(new Expiration
            {
                Project = project,
                Type = ExpirationTypes.ExtensionOfTime,                                         
                Status = isDone                                                    
                    ? ExpirationStatuses.Completed                                      
                    : ExpirationStatuses.Ongoing,
                CompletedAt = isDone ? extensionOfTime ?? default : null,                
                ExpiresOn = isDone ? null : extensionOfTime,                             
                CreatedAt = DateTime.UtcNow,
                LastExpirationDate = isDone ? extensionOfTime ?? default : null,
                IsActive = true
            });
        }


        var calculator = new ProjectTimelineCalculator(project.DateIssued);
        DateOnly finalTimelineTarget = extensionOfTime ?? dateOfCompletion;

        DateOnly? finalPbDate = null;
        DateOnly? upcomingPbDate = null;

        if (isPbTerminated)
        {
            finalPbDate = calculator.CalculateLastMilestoneBeforeCompletion(finalTimelineTarget, 12);
        }
        else
        {
            upcomingPbDate = calculator.CalculateUpcoming(12).NextDate;
        }

        expirations.Add(new Expiration
        {
            Project = project,
            Type = ExpirationTypes.PerformanceBond,
            Status = isPbTerminated 
                ? ExpirationStatuses.Completed 
                : ExpirationStatuses.Ongoing,
            CompletedAt = finalPbDate,
            ExpiresOn = upcomingPbDate,
            LastExpirationDate = finalPbDate,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        });


        DateOnly? finalSrDate = null;
        DateOnly? upcomingSrDate = null;

        if (isSrTerminated)
        {
            finalSrDate = calculator.CalculateLastMilestoneBeforeCompletion(finalTimelineTarget, 6);
        }
        else
        {
            upcomingSrDate = calculator.CalculateUpcoming(6).NextDate;
        }

        expirations.Add(new Expiration
        {
            Project = project,
            Type = ExpirationTypes.SemestralReport,
            Status = isSrTerminated 
                ? ExpirationStatuses.Completed 
                : ExpirationStatuses.Ongoing, 
            CompletedAt = finalSrDate, 
            ExpiresOn = upcomingSrDate,
            LastExpirationDate = finalSrDate,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        return expirations;
    }
}