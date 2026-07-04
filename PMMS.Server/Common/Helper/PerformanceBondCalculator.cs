namespace PMMS.Server.Common.Helper;

public class PerformanceBondCalculator(DateOnly dateIssued)
{
    private const int AnnualIntervalMonths = 12;

    public (DateOnly NextDate, int Count) Calculate()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);
        int count = 0;
        
        DateOnly pointer = dateIssued.AddMonths(AnnualIntervalMonths);

        while (pointer <= today)
        {
            pointer = pointer.AddMonths(AnnualIntervalMonths);
            count++;
        }

        return (pointer, count);
    }
}