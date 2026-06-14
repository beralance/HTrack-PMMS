namespace PMMS.Server.Common.Helper;

public class ProjectTimelineCalculator(DateOnly dateIssued)
{
    /// <summary>
    /// Calculates the next upcoming tracking date relative to today's current date.
    /// Used when a project tracking metric is LIVE (Ongoing, Extended, etc.).
    /// </summary>
    public (DateOnly NextDate, int Count) CalculateUpcoming(int intervalMonths)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int count = 0;
        
        // Start from the first cycle boundary (e.g., DateIssued + 6 months)
        DateOnly pointer = dateIssued.AddMonths(intervalMonths);

        // Keep stepping forward by the interval until we surpass the current real-world date
        while (pointer <= today)
        {
            pointer = pointer.AddMonths(intervalMonths);
            count++;
        }

        return (pointer, count);
    }

    /// <summary>
    /// Calculates the final valid tracking date milestone that occurred before the project timeline concluded.
    /// Used when a project tracking metric is TERMINATED (Completed or FullyCompleted).
    /// </summary>
    public DateOnly? CalculateLastMilestoneBeforeCompletion(DateOnly finalTimelineTarget, int intervalMonths)
    {
        DateOnly pointer = dateIssued;
        DateOnly? lastValidMilestone = null;

        // Step forward through time from Date Issued until we cross the project's final target line
        while (pointer <= finalTimelineTarget)
        {
            lastValidMilestone = pointer;
            pointer = pointer.AddMonths(intervalMonths);
        }

        // Returns the last calculated date that was safely <= finalTimelineTarget
        return lastValidMilestone;
    }
}