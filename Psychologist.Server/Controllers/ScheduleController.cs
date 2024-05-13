using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psychologist.Server.Database;
using Psychologist.Server.Models;

namespace Psychologist.Server.Controllers;

[ApiController, Route("schedule"), Authorize(Roles.Employee)]
public class ScheduleController : ControllerBase
{
    private readonly ILogger<ScheduleController> _logger;
    private readonly ApplicationDbContext _context;

    public ScheduleController(ILogger<ScheduleController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var schedule = await _context.ScheduleDays.ToListAsync();
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

    [HttpGet(@"{date:regex(\d\d\d\d-\d\d-\d\d)}")]
    public async Task<IActionResult> Get(DateOnly date)
    {
        var day = await _context.ScheduleDays
            .Include(d => d.Consultations)
            .ThenInclude(c => c.Visitor)
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

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ScheduleDayPostModel model)
    {
        var existedDay = await _context.ScheduleDays.FirstOrDefaultAsync(d => d.Date == model.Date!.Value);
        if (existedDay != null) return Conflict("Day already exists.");

        ScheduleDay day = new()
        {
            Date = model.Date!.Value,
            StartTime = model.StartTime!.Value.TruncateToHours(),
            EndTime = model.EndTime!.Value.TruncateToHours(),
            BreakTime = model.BreakTime!.Value.TruncateToHours()
        };
        _context.ScheduleDays.Add(day);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { date = day.Date.ToString("O") }, day);
    }

    // Not used, can be deleted
    [HttpPost("bulk")]
    public async Task<IActionResult> Post([FromBody] ScheduleDayPostModel[] model)
    {
        var existedDay = await _context.ScheduleDays.FirstOrDefaultAsync(d =>
            model.Select(m => m.Date).Contains(d.Date));
        if (existedDay != null) return Conflict($"Schedule for date {existedDay.Date:dd.MM.yyyy} already exists.");

        var days = model.Select(d => new ScheduleDay()
        {
            Date = d.Date!.Value,
            StartTime = d.StartTime!.Value.TruncateToHours(),
            EndTime = d.EndTime!.Value.TruncateToHours(),
            BreakTime = d.BreakTime!.Value.TruncateToHours()
        }).ToList();
        _context.ScheduleDays.AddRange(days);
        await _context.SaveChangesAsync();

        return Ok(days);
    }

    [HttpPost("range")]
    public async Task<IActionResult> Post([FromBody] ScheduleRangePostModel model)
    {
        var existedDay = await _context.ScheduleDays.FirstOrDefaultAsync(d =>
            d.Date >= model.StartDate && d.Date <= model.EndDate);
        if (existedDay != null) return Conflict($"Schedule for date {existedDay.Date:dd.MM.yyyy} already exists.");

        int GetDayOfWeekNumber(DayOfWeek day) => day == DayOfWeek.Sunday ? 6 : (int)day - 1;

        List<ScheduleDay> days = new();
        for (DateOnly date = model.StartDate!.Value; date < model.EndDate; date = date.AddDays(1))
        {
            var dayOfWeek = GetDayOfWeekNumber(date.DayOfWeek);
            if (model.Weekdays!.TryGetValue(dayOfWeek, out var schedule))
            {
                days.Add(new()
                {
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

    [HttpPut("{date:regex(\\d\\d\\d\\d-\\d\\d-\\d\\d)}")]
    [Produces<ScheduleDay>]
    public async Task<IActionResult> Put(DateOnly date, [FromBody] ScheduleDayPutModel model)
    {
        var day = await _context.ScheduleDays.FirstOrDefaultAsync(d => d.Date == date);
        if (day == null) return NotFound();

        day.StartTime = model.StartTime!.Value.TruncateToHours();
        day.EndTime = model.EndTime!.Value.TruncateToHours();
        day.BreakTime = model.BreakTime!.Value.TruncateToHours();

        await _context.SaveChangesAsync();
        return Ok(day);
    }

    [HttpDelete("{date:regex(\\d\\d\\d\\d-\\d\\d-\\d\\d)}")]
    public async Task<IActionResult> Delete(DateOnly date)
    {
        // TODO: disable cascade delete consultations
        var c = await _context.ScheduleDays.Where(d => d.Date == date).ExecuteDeleteAsync();
        return c > 0 ? Ok() : NotFound();
    }

    [HttpDelete("{from:regex(\\d\\d\\d\\d-\\d\\d-\\d\\d)}/{to:regex(\\d\\d\\d\\d-\\d\\d-\\d\\d)}")]
    public async Task<IActionResult> Delete(DateOnly from, DateOnly to)
    {
        int c = await _context.ScheduleDays.Where(d => d.Date >= from && d.Date <= to).ExecuteDeleteAsync();
        return c > 0 ? Ok() : NotFound();
    }
}

public static class TimeOnlyExtensions
{
    public static TimeOnly TruncateToHours(this TimeOnly time) => new(time.Hour, 0);
    public static TimeOnly TruncateToMinutes(this TimeOnly time) => new(time.Hour, time.Minute);
    public static TimeOnly TruncateToSeconds(this TimeOnly time) => new(time.Hour, time.Minute, time.Second);
}