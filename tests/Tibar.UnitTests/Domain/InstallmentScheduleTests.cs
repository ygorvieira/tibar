using Tibar.Domain.Helpers;
using Xunit;

namespace Tibar.UnitTests.Domain;

public class InstallmentScheduleTests
{
    [Fact]
    public void GetDates_SameMonthDay_ReturnsConsecutiveMonths()
    {
        var dates = InstallmentSchedule.GetDates(new DateOnly(2026, 1, 1), 5);

        Assert.Equal(5, dates.Length);
        Assert.Equal(
            new[] {
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 2, 1),
                new DateOnly(2026, 3, 1),
                new DateOnly(2026, 4, 1),
                new DateOnly(2026, 5, 1)
            },
            dates);
    }

    [Fact]
    public void GetDates_Day31_ClampsToLastDayOfMonth()
    {
        var dates = InstallmentSchedule.GetDates(new DateOnly(2026, 1, 31), 4);

        Assert.Equal(
            new[] {
                new DateOnly(2026, 1, 31),
                new DateOnly(2026, 2, 28),
                new DateOnly(2026, 3, 31),
                new DateOnly(2026, 4, 30)
            },
            dates);
    }

    [Fact]
    public void GetDates_Day31_LeapYear_UsesFeb29()
    {
        var dates = InstallmentSchedule.GetDates(new DateOnly(2024, 1, 31), 3);

        Assert.Equal(
            new[] {
                new DateOnly(2024, 1, 31),
                new DateOnly(2024, 2, 29),
                new DateOnly(2024, 3, 31)
            },
            dates);
    }

    [Fact]
    public void GetDates_Day30_ClampsIn30DayMonths()
    {
        var dates = InstallmentSchedule.GetDates(new DateOnly(2026, 1, 30), 4);

        Assert.Equal(
            new[] {
                new DateOnly(2026, 1, 30),
                new DateOnly(2026, 2, 28),
                new DateOnly(2026, 3, 30),
                new DateOnly(2026, 4, 30)
            },
            dates);
    }

    [Fact]
    public void GetDates_Day29_NonLeapFeb_ClampsToFeb28()
    {
        var dates = InstallmentSchedule.GetDates(new DateOnly(2025, 1, 29), 2);

        Assert.Equal(2, dates.Length);
        Assert.Equal(new DateOnly(2025, 1, 29), dates[0]);
        Assert.Equal(new DateOnly(2025, 2, 28), dates[1]);
    }

    [Fact]
    public void GetDates_December_WrapsToNextYear()
    {
        var dates = InstallmentSchedule.GetDates(new DateOnly(2026, 11, 30), 3);

        Assert.Equal(
            new[] {
                new DateOnly(2026, 11, 30),
                new DateOnly(2026, 12, 30),
                new DateOnly(2027, 1, 30)
            },
            dates);
    }

    [Fact]
    public void GetDates_SingleInstallment_ReturnsOnlyStart()
    {
        var dates = InstallmentSchedule.GetDates(new DateOnly(2026, 5, 27), 1);

        Assert.Single(dates);
        Assert.Equal(new DateOnly(2026, 5, 27), dates[0]);
    }
}
