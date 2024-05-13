using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;

namespace Psychologist.Server.Models;

[Table("articles")]
public class Article
{
    [Column("id")] public int Id { get; set; }
    [Column("time")] public DateTime Time { get; set; }
    [Column("title")] public string Title { get; set; } = "";
    [Column("text")] public string Text { get; set; } = "";
}

public class ArticleDataModel
{
    public string? Title { get; set; }
    public string? Text { get; set; }
}

public class ArticlePostModelValidator : AbstractValidator<ArticleDataModel>
{
    public ArticlePostModelValidator()
    {
        RuleFor(user => user.Title).NotNull().NotEmpty();
        RuleFor(user => user.Text).NotNull();
    }
}