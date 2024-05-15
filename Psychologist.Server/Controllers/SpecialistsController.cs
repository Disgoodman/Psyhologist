using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psychologist.Server.Database;
using Psychologist.Server.Models;

namespace Psychologist.Server.Controllers;

[ApiController, Route("specialists")]
public class SpecialistsController : ControllerBase
{
    private readonly ILogger<SpecialistsController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public SpecialistsController(
        ILogger<SpecialistsController> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    [HttpGet, Authorize]
    [Produces<List<Specialist>>]
    public async Task<IActionResult> GetAll([FromQuery(Name = "search")] string? searchString)
    {
        var query = _context.Specialists.IgnoreAutoIncludes().AsNoTracking();

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(v => (v.FirstName + " " + v.LastName + " " + v.Patronymic).Contains(searchString));
        }

        bool isAdmin = User.IsInRole(Roles.Admin);
        if (isAdmin) query = query.Include(s => s.User);
        var specialists = await query.ToListAsync();

        return Ok(specialists);
    }

    [HttpGet("{id:int}"), Authorize]
    [Produces<Specialist>]
    public async Task<IActionResult> Get(int id)
    {
        bool isAdmin = User.IsInRole(Roles.Admin);
        var query = _context.Specialists.AsNoTracking().Where(s => s.Id == id);
        if (isAdmin) query = query.Include(s => s.User);
        var specialist = await query.FirstOrDefaultAsync();
        return specialist != null ? Ok(specialist) : NotFound();
    }

    private ObjectResult ProblemFromIdentityError(IEnumerable<IdentityError> errors) => Problem(
        statusCode: StatusCodes.Status400BadRequest,
        title: string.Join(", ", errors.Select(e => e.Description))
    );

    [HttpPost, Authorize(Roles.Admin)]
    [Produces<Specialist>]
    public async Task<IActionResult> Post([FromBody] SpecialistPostModel model)
    {
        Specialist specialist = new()
        {
            FirstName = model.FirstName!,
            LastName = model.LastName!,
            Patronymic = model.Patronymic,
            Type = model.Type!,
            PrimaryVisitPrice = model.PrimaryVisitPrice!.Value,
            SecondaryVisitPrice = model.SecondaryVisitPrice!.Value,
        };

        var user = new ApplicationUser { Email = model.Email, UserName = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded) return ProblemFromIdentityError(result.Errors);
                result = await _userManager.AddToRoleAsync(user, Roles.Employee);
        if (!result.Succeeded) return ProblemFromIdentityError(result.Errors);

        specialist.User = user;

        _context.Specialists.Add(specialist);
        await _context.SaveChangesAsync();

        // TODO: rollback create user if fail

        return CreatedAtAction(nameof(Get), new { id = specialist.Id }, specialist);
    }

    [HttpPut("{id:int}"), Authorize(Roles.Admin)]
    [Produces<Specialist>]
    public async Task<IActionResult> Put(int id, [FromBody] SpecialistDataModel model)
    {
        var specialist = await _context.Specialists.FirstOrDefaultAsync(c => c.Id == id);
        if (specialist == null) return NotFound();

        specialist.FirstName = model.FirstName!;
        specialist.LastName = model.LastName!;
        specialist.Patronymic = model.Patronymic;
        specialist.Type = model.Type!;
        specialist.PrimaryVisitPrice = model.PrimaryVisitPrice!.Value;
        specialist.SecondaryVisitPrice = model.SecondaryVisitPrice!.Value;

        // TODO: update password

        await _context.SaveChangesAsync();
        return Ok(specialist);
    }

    [HttpDelete("{id:int}"), Authorize(Roles.Employee)]
    public async Task<IActionResult> Delete(int id)
    {
        var specialist = await _context.Specialists.Include(s => s.User).FirstOrDefaultAsync(a => a.Id == id);
        if (specialist == null) return NotFound();

        _context.Specialists.Remove(specialist);
        await _userManager.DeleteAsync(specialist.User!);
        await _context.SaveChangesAsync();

        return Ok();
    }
}