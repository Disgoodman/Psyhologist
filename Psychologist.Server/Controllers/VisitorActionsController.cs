using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psychologist.Server.Database;
using Psychologist.Server.Models;
using Psychologist.Server.Utils;

namespace Psychologist.Server.Controllers;

[ApiController]
public class VisitorActionsController : ControllerBase
{
    private readonly ILogger<VisitorActionsController> _logger;
    private readonly ApplicationDbContext _context;

    public VisitorActionsController(ILogger<VisitorActionsController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public record struct ConsultationInterval(TimeOnly Start, TimeOnly End);

    public static List<ConsultationInterval> GetFreeIntervals(ScheduleDay day)
    {
        List<ConsultationInterval> consultationIntervals = new();
        for (var time = day.StartTime; time < day.EndTime; time = time.AddHours(1))
        {
            bool isBreak = time >= day.BreakTime && time < day.BreakTime.Value.AddHours(1);
            bool consultation = day.Consultations?.Any(c => c.Time == time) ?? false;
            if (isBreak || consultation) continue;
            consultationIntervals.Add(new(time, time.AddHours(1)));
        }

        return consultationIntervals;
    }

    [HttpGet(@"schedule/{specialistId:int}/{date:regex(\d\d\d\d-\d\d-\d\d)}/appointment"), Authorize(Roles.Visitor)]
    public async Task<IActionResult> Get(int specialistId, DateOnly date)
    {
        var day = await _context.ScheduleDays.AsNoTracking()
            .Include(d => d.Consultations)
            .ThenInclude(c => c.Visitor)
            .Where(d => d.SpecialistId == specialistId)
            .FirstOrDefaultAsync(d => d.Date == date);

        if (day == null) return NotFound();

        var freeIntervals = GetFreeIntervals(day);

        return Ok(new
        {
            day.Date, day.StartTime, day.EndTime, day.BreakTime,
            FreeIntervals = freeIntervals
        });
    }

    [HttpPost("appointment"), Authorize(Roles.Visitor)]
    [Produces<Consultation>]
    public async Task<IActionResult> Post([FromBody] AppointmentPostModel model)
    {
        int userId = User.GetUserId()!.Value;
        var visitor = await _context.Visitors.FirstOrDefaultAsync(v => v.UserId == userId);
        if (visitor == null) return this.Problem400("Auth fail. Visitor record not found");

        var day = await _context.ScheduleDays.FirstOrDefaultAsync(
            c => c.SpecialistId == model.SpecialistId && c.Date == model.Date!.Value);
        if (day == null) return this.Problem400($"There is no schedule for the date {model.Date!.Value:O}");

        // TODO: check work time range and break.

        var existedConsultation = await _context.Consultations.FirstOrDefaultAsync(
            c => c.SpecialistId == model.SpecialistId && c.ScheduleDate == model.Date!.Value && c.Time == model.Time!.Value);
        if (existedConsultation != null) return this.Problem400("The selected date and time are already occupied.");

        bool visitorIsBusy = await _context.Consultations.AnyAsync(
            c => c.VisitorId == visitor.Id && c.ScheduleDate == model.Date!.Value && c.Time == model.Time!.Value);
        if (visitorIsBusy) return this.Problem400("You already have a consultation at this time.");

        Consultation consultation = model.Type switch
        {
            ConsultationType.IndividualConsultation => new IndividualConsultation(),
            ConsultationType.IndividualWork => new IndividualWork(),
            ConsultationType.DiagnosticWork => new DiagnosticWork(),
            _ => throw new ArgumentOutOfRangeException() // unreachable code
        };
        consultation.Type = model.Type!.Value;
        consultation.SpecialistId = model.SpecialistId!.Value;
        consultation.Visitor = visitor;
        consultation.ScheduleDate = model.Date!.Value;
        consultation.Time = model.Time!.Value;
        consultation.Primary = model.Primary!.Value;

        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();

        return Ok(consultation);
    }

    [HttpGet("visitor-consultations"), Authorize(Roles = Roles.Visitor)]
    [Produces<List<VisitorConsultationModel>>]
    public async Task<IActionResult> GetVisitorConsultations()
    {
        int userId = User.GetUserId()!.Value;
        var visitor = await _context.Visitors.FirstAsync(v => v.UserId == userId);

        var consultations = await _context.Consultations.AsNoTracking()
            .Include(c => c.Specialist)
            .Where(c => c.VisitorId == visitor.Id)
            .OrderByDescending(c => c.ScheduleDate).ThenByDescending(c => c.Time)
            .ToListAsync();

        return Ok(consultations.Select(VisitorConsultationModel.Create));
    }

    [HttpPost("cancel-consultation/{consultationId:int}"), Authorize(Roles = Roles.Visitor)]
    public async Task<IActionResult> CancelConsultation(int consultationId)
    {
        int userId = User.GetUserId()!.Value;
        var visitor = await _context.Visitors.FirstAsync(v => v.UserId == userId);

        var consultation = await _context.Consultations.FirstOrDefaultAsync(c => c.Id == consultationId);
        if (consultation == null) return NotFound();

        if (consultation.VisitorId != visitor.Id)
            return this.Problem400("You can't cancel the consultation that was not created by you");

        if (consultation.ScheduleDate.ToDateTime(consultation.Time) < DateTime.Now)
            return this.Problem400("You can't cancel the consultation that has already passed");

        _context.Consultations.Remove(consultation);
        await _context.SaveChangesAsync();

        return Ok();
    }
}

[JsonDerivedType(typeof(VisitorIndividualConsultationModel))]
[JsonDerivedType(typeof(VisitorIndividualWorkModel))]
[JsonDerivedType(typeof(VisitorDiagnosticWorkModel))]
public record VisitorConsultationModel(
    int Id,
    DateOnly ScheduleDate,
    TimeOnly Time,
    Specialist Specialist,
    string Topic,
    bool Primary,
    ConsultationType Type)
{
    public static VisitorConsultationModel Create(Consultation c) => c switch
    {
        IndividualConsultation ic => new VisitorIndividualConsultationModel(
            ic.Id, ic.ScheduleDate, ic.Time, ic.Specialist, ic.Topic, ic.Primary, ic.RequestCode, ic.Notes),
        IndividualWork iw => new VisitorIndividualWorkModel(
            iw.Id, iw.ScheduleDate, iw.Time, iw.Specialist, iw.Topic, iw.Primary, iw.Purpose),
        DiagnosticWork dw => new VisitorDiagnosticWorkModel(
            dw.Id, dw.ScheduleDate, dw.Time, dw.Specialist, dw.Topic, dw.Primary,
            dw.RequestCode, dw.Revealed, dw.Prescribed),
        _ => new VisitorConsultationModel(c.Id, c.ScheduleDate, c.Time, c.Specialist, c.Topic, c.Primary, c.Type)
    };
}

public record VisitorIndividualConsultationModel(
    int Id,
    DateOnly ScheduleDate,
    TimeOnly Time,
    Specialist Specialist,
    string Topic,
    bool Primary,
    string RequestCode,
    string Notes)
    : VisitorConsultationModel(Id, ScheduleDate, Time, Specialist, Topic, Primary, ConsultationType.IndividualConsultation);

public record VisitorIndividualWorkModel(
    int Id,
    DateOnly ScheduleDate,
    TimeOnly Time,
    Specialist Specialist,
    string Topic,
    bool Primary,
    string Purpose)
    : VisitorConsultationModel(Id, ScheduleDate, Time, Specialist, Topic, Primary, ConsultationType.IndividualWork);

public record VisitorDiagnosticWorkModel(
    int Id,
    DateOnly ScheduleDate,
    TimeOnly Time,
    Specialist Specialist,
    string Topic,
    bool Primary,
    string RequestCode,
    string Revealed,
    string Prescribed)
    : VisitorConsultationModel(Id, ScheduleDate, Time, Specialist, Topic, Primary, ConsultationType.DiagnosticWork);