using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psychologist.Server.Database;
using Psychologist.Server.Models;
using Psychologist.Server.Utils;

namespace Psychologist.Server.Controllers;

[ApiController, Route("consultations")]
[Authorize(Roles.Employee)]
public class ConsultationsController : ControllerBase
{
    private readonly ILogger<ConsultationsController> _logger;
    private readonly ApplicationDbContext _context;

    public ConsultationsController(ILogger<ConsultationsController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet("{specialistId:int}")]
    [Produces<List<Consultation>>]
    public async Task<IActionResult> GetAll(int specialistId)
    {
        var consultations = await _context.Consultations.AsNoTracking()
            .Include(c => c.Visitor)
            .Where(c => c.SpecialistId == specialistId)
            .ToListAsync();
        return Ok(consultations);
    }

    [HttpGet("{id:int}")]
    [Produces<Consultation>]
    public async Task<IActionResult> Get(int id)
    {
        var consultation = await _context.Consultations.AsNoTracking()
            .Include(c => c.Visitor)
            .FirstOrDefaultAsync(c => c.Id == id);
        return consultation != null ? Ok(consultation) : NotFound();
    }

    [HttpGet("{specialistId:int}/{date:regex(\\d\\d\\d\\d-\\d\\d-\\d\\d)}/{time:regex(\\d\\d:\\d\\d)}")]
    [Produces<Consultation>]
    public async Task<IActionResult> Get(int specialistId, DateOnly date, TimeOnly time)
    {
        var consultation = await _context.Consultations.AsNoTracking()
            .Include(c => c.Visitor)
            .FirstOrDefaultAsync(c => c.SpecialistId == specialistId && c.ScheduleDate == date && c.Time == time);
        return consultation != null ? Ok(consultation) : NotFound();
    }

    private async Task<IActionResult> CreateConsultation(Consultation consultation)
    {
        _context.Consultations.Add(consultation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = consultation.Id }, consultation);
    }

    private async Task<IActionResult?> ValidateConsultationModel(ConsultationPostModel model)
    {
        var visitor = await _context.Visitors.FirstOrDefaultAsync(c => c.Id == model.VisitorId!.Value);
        if (visitor == null) return this.Problem400("Visitor not found");

        var day = await _context.ScheduleDays.FirstOrDefaultAsync(c => c.Date == model.ScheduleDate!.Value);
        if (day == null) return this.Problem400($"There is no schedule for the date {model.ScheduleDate!.Value:O}");

        // TODO: check work time range and break.

        var consultation = await _context.Consultations
            .FirstOrDefaultAsync(c => c.SpecialistId == model.SpecialistId &&
                                      c.ScheduleDate == model.ScheduleDate!.Value && c.Time == model.Time!.Value);
        if (consultation != null) return this.Problem400($"The selected date and time are already occupied.");

        return null;
    }

    /*[HttpGet("individual-consultation")]
    [Produces<List<IndividualConsultation>>]
    public async Task<IActionResult> GetIndividualConsultations()
    {
        var consultations = await _context.IndividualConsultations
            .Include(c => c.Visitor)
            .ToListAsync();
        return Ok(consultations);
    }

    [HttpGet("individual-work")]
    [Produces<List<IndividualWork>>]
    public async Task<IActionResult> GetIndividualWorks()
    {
        var consultations = await _context.IndividualWorks
            .Include(c => c.Visitor)
            .ToListAsync();
        return Ok(consultations);
    }

    [HttpGet("diagnostic-work")]
    [Produces<List<DiagnosticWork>>]
    public async Task<IActionResult> GetDiagnosticWorks()
    {
        var consultations = await _context.DiagnosticWorks
            .Include(c => c.Visitor)
            .ToListAsync();
        return Ok(consultations);
    }*/

    [HttpPost("individual-consultation")]
    [Produces<IndividualConsultation>]
    public async Task<IActionResult> Post([FromBody] IndividualConsultationPostModel model)
    {
        if (await ValidateConsultationModel(model) is { } result) return result;

        IndividualConsultation consultation = new();
        model.AssignTo(consultation);

        return await CreateConsultation(consultation);
    }

    [HttpPost("individual-work")]
    [Produces<IndividualWork>]
    public async Task<IActionResult> Post([FromBody] IndividualWorkPostModel model)
    {
        if (await ValidateConsultationModel(model) is { } result) return result;

        IndividualWork consultation = new();
        model.AssignTo(consultation);

        return await CreateConsultation(consultation);
    }

    [HttpPost("diagnostic-work")]
    [Produces<DiagnosticWork>]
    public async Task<IActionResult> Post([FromBody] DiagnosticWorkPostModel model)
    {
        if (await ValidateConsultationModel(model) is { } result) return result;

        DiagnosticWork consultation = new();
        model.AssignTo(consultation);

        return await CreateConsultation(consultation);
    }

    [HttpPut("individual-consultation/{id:int}")]
    [Produces<IndividualConsultation>]
    public async Task<IActionResult> Put(int id, [FromBody] IndividualConsultationPutModel model)
    {
        var consultation = await _context.Consultations.FirstOrDefaultAsync(c => c.Id == id) as IndividualConsultation;
        if (consultation == null) return NotFound();

        var visitor = await _context.Visitors.FirstOrDefaultAsync(c => c.Id == model.VisitorId!.Value);
        if (visitor == null) return this.Problem400("Visitor not found");

        model.AssignTo(consultation);

        await _context.SaveChangesAsync();
        return Ok(consultation);
    }

    [HttpPut("individual-work/{id:int}")]
    [Produces<IndividualWork>]
    public async Task<IActionResult> Put(int id, [FromBody] IndividualWorkPutModel model)
    {
        var consultation = await _context.Consultations.FirstOrDefaultAsync(c => c.Id == id) as IndividualWork;
        if (consultation == null) return NotFound();

        var visitor = await _context.Visitors.FirstOrDefaultAsync(c => c.Id == model.VisitorId!.Value);
        if (visitor == null) return this.Problem400("Visitor not found");

        model.AssignTo(consultation);

        await _context.SaveChangesAsync();
        return Ok(consultation);
    }

    [HttpPut("diagnostic-work/{id:int}")]
    [Produces<DiagnosticWork>]
    public async Task<IActionResult> Put(int id, [FromBody] DiagnosticWorkPutModel model)
    {
        var consultation = await _context.Consultations.FirstOrDefaultAsync(c => c.Id == id) as DiagnosticWork;
        if (consultation == null) return NotFound();

        var visitor = await _context.Visitors.FirstOrDefaultAsync(c => c.Id == model.VisitorId!.Value);
        if (visitor == null) return this.Problem400("Visitor not found");

        model.AssignTo(consultation);

        await _context.SaveChangesAsync();
        return Ok(consultation);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _context.Consultations.Where(a => a.Id == id).ExecuteDeleteAsync();
        return c > 0 ? Ok() : NotFound();
    }
}