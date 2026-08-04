namespace Tibar.Domain.Helpers;

public static class InstallmentSchedule
{
    public static DateOnly[] GetDates(DateOnly start, int count)
    {
        var dates = new DateOnly[count];
        for (var i = 0; i < count; i++)
            dates[i] = start.AddMonths(i);
        return dates;
    }
}
