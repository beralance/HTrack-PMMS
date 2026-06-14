namespace PMMS.Server.Common.Helper;

/// <summary>
/// 
/// Calculate Performance Bond based on  date issued
/// Performance bond due = every 12 months
/// 
/// </summary>
/// <param name="dateIssued"></param>


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