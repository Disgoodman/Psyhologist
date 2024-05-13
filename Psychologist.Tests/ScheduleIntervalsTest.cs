using Psychologist.Server.Controllers;
using Psychologist.Server.Models;

namespace Psychologist.Tests;

public class ScheduleIntervalsTest
{
    [Fact]
    public void TestWithoutConsultations()
    {
        ScheduleDay day = new()
        {
            Date = new(2024, 01, 01),
            StartTime = new(10, 00),
            EndTime = new(18, 00),
            BreakTime = new(14, 00)
        };

        var actualIntervals = ScheduleController.GetIntervals(day);

        ScheduleController.ConsultationInterval[] expectedIntervals =
        [
            new(new(10, 00), new(11, 00)),
            new(new(11, 00), new(12, 00)),
            new(new(12, 00), new(13, 00)),
            new(new(13, 00), new(14, 00)),
            new(new(14, 00), new(15, 00), IsBreak: true),
            new(new(15, 00), new(16, 00)),
            new(new(16, 00), new(17, 00)),
            new(new(17, 00), new(18, 00)),
        ];

        Assert.Equal(expectedIntervals, actualIntervals);
    }

    [Fact]
    public void TestWithConsultations()
    {
        ScheduleDay day = new()
        {
            Date = new(2024, 01, 01),
            StartTime = new(10, 00),
            EndTime = new(18, 00),
            BreakTime = new(14, 00),
            Consultations = new List<Consultation>()
        };
        Consultation consultation = new() { Id = 1, Day = day, Time = new(11, 00) };
        day.Consultations.Add(consultation);

        var actualIntervals = ScheduleController.GetIntervals(day);

        ScheduleController.ConsultationInterval[] expectedIntervals =
        [
            new(new(10, 00), new(11, 00)),
            new(new(11, 00), new(12, 00), Consultation: consultation),
            new(new(12, 00), new(13, 00)),
            new(new(13, 00), new(14, 00)),
            new(new(14, 00), new(15, 00), IsBreak: true),
            new(new(15, 00), new(16, 00)),
            new(new(16, 00), new(17, 00)),
            new(new(17, 00), new(18, 00)),
        ];

        Assert.Equal(expectedIntervals, actualIntervals);
    }
}