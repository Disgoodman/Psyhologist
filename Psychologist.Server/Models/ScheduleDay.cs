using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;

namespace Psychologist.Server.Models;

[Table("schedule")]
public class ScheduleDay
{
    [Column("date"), Key] public DateOnly Date { get; set; }
    [Column("start_time")] public TimeOnly StartTime { get; set; }
    [Column("end_time")] public TimeOnly EndTime { get; set; }
    [Column("break_time")] public TimeOnly? BreakTime { get; set; }
    //[Column("note"), DefaultValue("")] public string Note { get; set; } = ""; // TODO

    public ICollection<Consultation> Consultations { get; set; } = null!;
}

public class ScheduleDayPostModel
{
    public DateOnly? Date { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public TimeOnly? BreakTime { get; set; }
}

public class ScheduleDayPostModelValidator : AbstractValidator<ScheduleDayPostModel>
{
    public ScheduleDayPostModelValidator()
    {
        RuleFor(day => day.Date).NotNull();
        RuleFor(day => day.StartTime).NotNull();
        RuleFor(day => day.EndTime).NotNull().GreaterThanOrEqualTo(u => u.StartTime);
        RuleFor(day => day.BreakTime)
            .GreaterThan(u => u.StartTime)
            .LessThan(u => u.EndTime);
    }
}

public class ScheduleDayPutModel
{
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public TimeOnly? BreakTime { get; set; }
}

public class ScheduleDayPutModelValidator : AbstractValidator<ScheduleDayPutModel>
{
    public ScheduleDayPutModelValidator()
    {
        RuleFor(day => day.StartTime).NotNull();
        RuleFor(day => day.EndTime).NotNull().GreaterThanOrEqualTo(u => u.StartTime);
        RuleFor(day => day.BreakTime)
            .GreaterThan(u => u.StartTime)
            .LessThan(u => u.EndTime);
    }
}

public class ScheduleRangePostModel
{
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Dictionary<int, DayInfo?>? Weekdays { get; set; }

    public class DayInfo
    {
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? EndTime { get; set; }
        public TimeOnly? BreakTime { get; set; }
    }
}

public class ScheduleRangePostModelValidator : AbstractValidator<ScheduleRangePostModel>
{
    public ScheduleRangePostModelValidator()
    {
        RuleFor(range => range.StartDate).NotNull();
        RuleFor(range => range.EndDate).NotNull().GreaterThanOrEqualTo(u => u.StartDate);
        RuleFor(range => range.Weekdays).NotNull().NotEmpty();
        RuleForEach(range => range.Weekdays)
            .ChildRules(item =>
            {
                item.RuleFor(d => d.Key).InclusiveBetween(0, 6);
                item.RuleFor(d => d.Value)
                    .NotNull()
                    .ChildRules(schedule =>
                    {
                        schedule.RuleFor(s => s!.StartTime).NotNull();
                        schedule.RuleFor(s => s!.EndTime).NotNull().GreaterThanOrEqualTo(s => s!.StartTime);
                        schedule.RuleFor(s => s!.BreakTime)
                            .GreaterThan(s => s!.StartTime)
                            .LessThan(s => s!.EndTime);
                    });
            });
    }
}