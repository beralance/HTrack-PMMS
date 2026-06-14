using PMMS.Server.Common.Helper;
using PMMS.Server.Domain.Entities;
using PMMS.Server.Domain.Enums;

namespace PMMS.Server.Common.Behaviors;


/// <summary>
/// EXTENSION RULES
/// 
/// 
/// Date of completion rules:
/// - will accept a value as long as project-status != COMPLETED || FULLYCompleted
/// - required
/// - depends on user DOC input
/// - value should be directly from user
/// - will set expiration-status to "COMPLETED" if EOT value is present
/// - DOC value should not be below DATE ISSUED
/// 
/// Extension of time rules:
/// - will accept a value as long as project-status != COMPLETED || FULLYCOMPLETED
/// - optional
/// - depends on user EOT input
/// - value should be directly from user
/// - EOT value should not be below date of completion date
/// - should be marked as completed if project-status == COMPLETED || FULLYCOMPLETED
/// - should be marked as on-going if project-status != COMPLETED || FULLYCOMPLETED
/// - will not be store in DB if value was not added
/// - if value was added, DOC shouls always be marked as COMPLETED
/// 
/// Performance bond rules:
/// - value will come from date issued + 12 months
/// - will calculate value as long as project-status != COMPLETED || FULLYCOMPLETED
/// - SR will be marked as COMPLETED if projects-status == COMPLETED || FULLYCOMPLETED
/// - required
/// - depends on DATE ISSUED
/// - calculated/handled by system
/// - no direct 'PERFORMANCE BOND date' input needed from user
/// - will calculate and store PERFORMANCE BOND till project-status == COMPLETED || FULLYCOMPLETED
/// 
/// Semetral report rules:
/// - value will come from date issued + 6 months
/// - will calculate value as long as project-status != FULLYCOMPLETED
/// - SR will be marked as COMPLETED if projects-status == FULLYCOMPLETED
/// - required
/// - depends on DATE ISSUED
/// - calculated/handled by system
/// - no direct 'SEMESTRAL REPORT date' input needed from user
/// - will calculate and store SEMESTRAL REPORT till project-status == FULLYCOMPLETED
///  
/// </summary>


public static class CreateExpirationBehavior
{
    public static IEnumerable<Expiration> CalculateExpirations(
        Project project,
        ProjectStatuses status,
        DateOnly dateOfCompletion,
        DateOnly? extensionOfTime)
    {
        var expirations = new List<Expiration>();
        
        // Condition variables
        bool isDone = status == ProjectStatuses.Completed || status == ProjectStatuses.FullyCompleted;
        bool isExtended = status == ProjectStatuses.Extended && project.IsExtended == true;
        bool isSrTerminated = status == ProjectStatuses.FullyCompleted;
        bool isPbTerminated = status == ProjectStatuses.Completed || status == ProjectStatuses.FullyCompleted;

        // Add Date of Completion (DOC)
        expirations.Add(new Expiration
        {
            Project = project,
            Type = ExpirationTypes.DateOfCompletion,
            Status = (isDone || isExtended)                                                 // if done, mark as completed, else on-going
                ? ExpirationStatuses.Completed 
                : ExpirationStatuses.Ongoing,    
            CompletedAt = (isDone || isExtended) ? dateOfCompletion : null,                 // if done, store date of completion, else null
            ExpiresOn = (isDone || isExtended) ? null : dateOfCompletion,                   // if done, store null, else store date of completion
            CreatedAt = DateTime.UtcNow,
            LastExpirationDate = (isDone || isExtended) ? dateOfCompletion : null,
            IsActive = true
        });

        // Add Extension of Time (EOT)
        if(extensionOfTime.HasValue) {
            expirations.Add(new Expiration
            {
                Project = project,
                Type = ExpirationTypes.ExtensionOfTime,                                         
                Status = isDone                                                             // if done, mark as completed, else on-going
                    ? ExpirationStatuses.Completed                                      
                    : ExpirationStatuses.Ongoing,
                CompletedAt = isDone ? extensionOfTime ?? default : null,                   // if done, store extension of time, else null
                ExpiresOn = isDone ? null : extensionOfTime,                                // if done, store null, else store extension of time
                CreatedAt = DateTime.UtcNow,
                LastExpirationDate = isDone ? extensionOfTime ?? default : null,                   // if done, store extension of time, else null
                IsActive = true
            });
        }


        // Pass date issued to calculator helper
        var calculator = new ProjectTimelineCalculator(project.DateIssued);
        DateOnly finalTimelineTarget = extensionOfTime ?? dateOfCompletion;

        DateOnly? finalPbDate = null;
        DateOnly? upcomingPbDate = null;

        // Check if Performance bond is terminated based on project status
        if (isPbTerminated)
        {
            // Project Completed; calculate the final milestone before it ended
            finalPbDate = calculator.CalculateLastMilestoneBeforeCompletion(finalTimelineTarget, 12);
        }
        else
        {
            // Project On-going; calculate the next upcoming milestone relative to today
            upcomingPbDate = calculator.CalculateUpcoming(12).NextDate;
        }

        // Add Performance Bond (PB)
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

        // Check if Semestral report is terminated based on project status
        if (isSrTerminated)
        {
            finalSrDate = calculator.CalculateLastMilestoneBeforeCompletion(finalTimelineTarget, 6);
        }
        else
        {
            upcomingSrDate = calculator.CalculateUpcoming(6).NextDate;
        }

        // Add Semestral Report (SR)
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