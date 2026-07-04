namespace PMMS.Server.Common.Helper;

public class ProjectTimelineCalculator(DateOnly dateIssued)
{
    public (DateOnly NextDate, int Count) CalculateUpcoming(int intervalMonths)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int count = 0;
        
        DateOnly pointer = dateIssued.AddMonths(intervalMonths);

        while (pointer <= today)
        {
            pointer = pointer.AddMonths(intervalMonths);
            count++;
        }

        return (pointer, count);
    }

    public DateOnly? CalculateLastMilestoneBeforeCompletion(DateOnly finalTimelineTarget, int intervalMonths)
    {
        DateOnly pointer = dateIssued;
        DateOnly? lastValidMilestone = null;

        while (pointer <= finalTimelineTarget)
        {
            lastValidMilestone = pointer;
            pointer = pointer.AddMonths(intervalMonths);
        }

        return lastValidMilestone;
    }
}