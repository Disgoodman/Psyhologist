using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psychologist.Server.Database;
using Psychologist.Server.Models;

namespace Psychologist.Server.Controllers;

[ApiController, Route("visitors")]
public class VisitorsController : ControllerBase
{
    private readonly ILogger<VisitorsController> _logger;
    private readonly ApplicationDbContext _context;

    public VisitorsController(ILogger<VisitorsController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet, Authorize(Roles.Employee)]
    [Produces<List<Visitor>>]
    public async Task<IActionResult> GetAll([FromQuery(Name = "search")] string? searchString, [FromQuery(Name = "types")] HashSet<VisitorType>? types)
    {
        var query = _context.Visitors.AsQueryable();
        
        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(v => v.FirstName.Contains(searchString) || v.LastName.Contains(searchString));
        }

        if (types is { Count: > 0 and < 3 })
        {
            query = query.Where(v => types.Contains(v.Type));
        }

        var visitors = await query.ToArrayAsync();

        return Ok(visitors);
    }

    [HttpGet("{id:int}"), Authorize(Roles.Employee)]
    [Produces<Visitor>]
    public async Task<IActionResult> Get(int id)
    {
        var visitor = await _context.Visitors.FirstOrDefaultAsync(a => a.Id == id);
        return visitor != null ? Ok(visitor) : NotFound();
    }

    [HttpPost, Authorize(Roles.Employee)]
    [Produces<Visitor>]
    public async Task<IActionResult> Post([FromBody] VisitorDataModel model)
    {
        Visitor visitor = new()
        {
            FirstName = model.FirstName!,
            LastName = model.LastName!,
            Patronymic = model.Patronymic,
            Birthday = model.Birthday!.Value,
            Type = model.Type!.Value
        };

        _context.Visitors.Add(visitor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = visitor.Id }, visitor);
    }

    [HttpPut("{id:int}"), Authorize(Roles.Employee)]
    [Produces<Visitor>]
    public async Task<IActionResult> Put(int id, [FromBody] VisitorDataModel model)
    {
        var visitor = await _context.Visitors.FirstOrDefaultAsync(c => c.Id == id);
        if (visitor == null) return NotFound();

        visitor.FirstName = model.FirstName!;
        visitor.LastName = model.LastName!;
        visitor.Patronymic = model.Patronymic;
        visitor.Birthday = model.Birthday!.Value;
        visitor.Type = model.Type!.Value;

        await _context.SaveChangesAsync();
        return Ok(visitor);
    }

    [HttpDelete("{id:int}"), Authorize(Roles.Employee)]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _context.Visitors.Where(a => a.Id == id).ExecuteDeleteAsync();
        return c > 0 ? Ok() : NotFound();
    }
}