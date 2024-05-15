using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psychologist.Server.Database;
using Psychologist.Server.Models;

namespace Psychologist.Server.Controllers;

[ApiController, Route("schedule")]
public class ScheduleController : ControllerBase
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly ApplicationDbContext _context;

    public ScheduleController(ILogger<ScheduleController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("{specialistId:int?}"), Authorize(Roles.Visitor)]
    public async Task<IActionResult> GetAll(int? specialistId)
    {
        if (specialistId == null && User.IsInRole(Roles.Employee))
        {
            int userId = User.GetUserId()!.Value;
            var specialist = await _context.Specialists.FirstAsync(v => v.UserId == userId);
            specialistId = specialist.Id;
        }
        
        var schedule = await _context.ScheduleDays.AsNoTracking()
            .Where(d => d.SpecialistId == specialistId)
            .ToListAsync();
        return Ok(schedule);
    }

    public record struct ConsultationInterval(
        TimeOnly Start,
        TimeOnly End,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] bool IsBreak = false,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Consultation? Consultation = null
    );

    public static List<ConsultationInterval> GetIntervals(ScheduleDay day)
    {
        var intervalsCount = day.EndTime.Hour - day.StartTime.Hour - (day.BreakTime != null ? 1 : 0);

        List<ConsultationInterval> consultationIntervals = new(intervalsCount);
        for (var time = day.StartTime; time < day.EndTime; time = time.AddHours(1))
        {
            consultationIntervals.Add(new(
                time, time.AddHours(1),
                time >= day.BreakTime && time < day.BreakTime.Value.AddHours(1),
                day.Consultations?.FirstOrDefault(c => c.Time == time)
            ));
        }

        return consultationIntervals;
    }

    [HttpGet(@"{specialistId:int}/{date:regex(\d\d\d\d-\d\d-\d\d)}"), Authorize(Roles.Employee)]
    public async Task<IActionResult> Get(int specialistId, DateOnly date)
    {
        var day = await _context.ScheduleDays.AsNoTracking()
            .Include(d => d.Consultations)
            .ThenInclude(c => c.Visitor)
            .Where(d => d.SpecialistId == specialistId)
            .FirstOrDefaultAsync(d => d.Date == date);

        if (day == null) return NotFound();

        var consultationIntervals = GetIntervals(day);

        if (consultationIntervals.Count(i => i.Consultation != null) != day.Consultations.Count)
            _logger.LogWarning("Invalid time of one of the consultations {Date}", date.ToString("O"));

        return Ok(new
        {
            day.Date, day.StartTime, day.EndTime, day.BreakTime,
            Consultations = consultationIntervals
        });
    }

    [HttpPost("{specialistId:int}"), Authorize(Roles.Employee)]
    public async Task<IActionResult> Post(int specialistId, [FromBody] ScheduleDayPostModel model)
    {
        var existedDay = await _context.ScheduleDays
            .FirstOrDefaultAsync(d => d.Date == model.Date!.Value && d.SpecialistId == specialistId);
        if (existedDay != null) return Problem(title: "Day already exists.", statusCode: StatusCodes.Status409Conflict);

        ScheduleDay day = new()
        {
            SpecialistId = specialistId,
            Date = model.Date!.Value,
            StartTime = model.StartTime!.Value.TruncateToHours(),
            EndTime = model.EndTime!.Value.TruncateToHours(),
            BreakTime = model.BreakTime!.Value.TruncateToHours()
        };
        _context.ScheduleDays.Add(day);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new
        {
            specialistId, date = day.Date.ToString("O")
        }, day);
    }

    [HttpPost("{specialistId:int}/range"), Authorize(Roles.Employee)]
    public async Task<IActionResult> Post(int specialistId, [FromBody] ScheduleRangePostModel model)
    {
        var existedDay = await _context.ScheduleDays.FirstOrDefaultAsync(d =>
            d.Date >= model.StartDate && d.Date <= model.EndDate
                                      && d.SpecialistId == specialistId);
        if (existedDay != null) return Conflict($"Schedule for date {existedDay.Date:dd.MM.yyyy} already exists.");

        int GetDayOfWeekNumber(DayOfWeek day) => day == DayOfWeek.Sunday ? 6 : (int)day - 1;

        List<ScheduleDay> days = new();
        for (DateOnly date = model.StartDate!.Value; date <= model.EndDate; date = date.AddDays(1))
        {
            var dayOfWeek = GetDayOfWeekNumber(date.DayOfWeek);
            if (model.Weekdays!.TryGetValue(dayOfWeek, out var schedule))
            {
                days.Add(new()
                {
                    SpecialistId = specialistId,
                    Date = date,
                    StartTime = schedule!.StartTime!.Value.TruncateToHours(),
                    EndTime = schedule!.EndTime!.Value.TruncateToHours(),
                    BreakTime = schedule!.BreakTime!.Value.TruncateToHours()
                });
            }
        }

        _context.ScheduleDays.AddRange(days);
        await _context.SaveChangesAsync();

        return Ok(days);
    }

    [HttpPut("{specialistId:int}/{date:regex(\\d\\d\\d\\d-\\d\\d-\\d\\d)}"), Authorize(Roles.Employee)]
    [Produces<ScheduleDay>]
    public async Task<IActionResult> Put(int specialistId, DateOnly date, [FromBody] ScheduleDayPutModel model)
    {
        var day = await _context.ScheduleDays.FirstOrDefaultAsync(d => d.Date == date && d.SpecialistId == specialistId);
        if (day == null) return NotFound();

        day.StartTime = model.StartTime!.Value.TruncateToHours();
        day.EndTime = model.EndTime!.Value.TruncateToHours();
        day.BreakTime = model.BreakTime!.Value.TruncateToHours();

        await _context.SaveChangesAsync();
        return Ok(day);
    }

    [HttpDelete("{specialistId:int}/{date:regex(\\d\\d\\d\\d-\\d\\d-\\d\\d)}"), Authorize(Roles.Employee)]
    public async Task<IActionResult> Delete(int specialistId, DateOnly date)
    {
        // TODO: disable cascade delete consultations
        var c = await _context.ScheduleDays.Where(d => d.Date == date && d.SpecialistId == specialistId).ExecuteDeleteAsync();
        return c > 0 ? Ok() : NotFound();
    }

    [HttpDelete("{specialistId:int}/{from:regex(\\d\\d\\d\\d-\\d\\d-\\d\\d)}/{to:regex(\\d\\d\\d\\d-\\d\\d-\\d\\d)}"),
     Authorize(Roles.Employee)]
    public async Task<IActionResult> Delete(int specialistId, DateOnly from, DateOnly to)
    {
        int c = await _context.ScheduleDays
            .Where(d => d.Date >= from && d.Date <= to && d.SpecialistId == specialistId)
            .ExecuteDeleteAsync();
        return c > 0 ? Ok() : NotFound();
    }
}

public static class TimeOnlyExtensions
{
    public static TimeOnly TruncateToHours(this TimeOnly time) => new(time.Hour, 0);
    public static TimeOnly TruncateToMinutes(this TimeOnly time) => new(time.Hour, time.Minute);
    public static TimeOnly TruncateToSeconds(this TimeOnly time) => new(time.Hour, time.Minute, time.Second);
}