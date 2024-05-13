using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Psychologist.Server.Database;
using Psychologist.Server.Models;

namespace Psychologist.Server.Controllers;

[ApiController, Route("articles")]
public class ArticlesController : ControllerBase
{
    private readonly ILogger<ArticlesController> _logger;
    private readonly ApplicationDbContext _context;

    public ArticlesController(ILogger<ArticlesController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var articles = await _context.Articles.ToListAsync();
        return Ok(articles);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
        return article != null ? Ok(article) : NotFound();
    }

    [HttpPost, Authorize(Roles.Employee)]
    public async Task<IActionResult> Post([FromBody] ArticleDataModel model)
    {
        Article article = new()
        {
            Title = model.Title!,
            Text = model.Text!,
            Time = DateTime.UtcNow
        };
        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = article.Id }, article);
    }

    [HttpPut("{id:int}"), Authorize(Roles.Employee)]
    public async Task<IActionResult> Put(int id, [FromBody] ArticleDataModel model)
    {
        var article = await _context.Articles.FirstOrDefaultAsync(a => a.Id == id);
        if (article == null) return NotFound();

        article.Title = model.Title!;
        article.Text = model.Text!;

        await _context.SaveChangesAsync();
        return Ok(article);
    }
    
    [HttpDelete("{id:int}"), Authorize(Roles.Employee)]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await _context.Articles.Where(a => a.Id == id).ExecuteDeleteAsync();
        return c > 0 ? Ok() : NotFound();
    }
}