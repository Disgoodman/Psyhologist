using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using FluentValidation;
using NpgsqlTypes;

namespace Psychologist.Server.Models;

[PgName("visitor_type")]
public enum VisitorType
{
    [PgName("student")] Student,
    [PgName("parent")] Parent,
    [PgName("specialist")] Specialist,
    
}

[Table("visitors")]
public class Visitor
{
    [Column("id")] public int Id { get; set; }
    [Column("first_name"), MaxLength(30)] public string FirstName { get; set; } = "";
    [Column("last_name"), MaxLength(30)] public string LastName { get; set; } = "";
    [Column("patronymic"), MaxLength(30)] public string? Patronymic { get; set; }
    [Column("birthday")] public DateOnly Birthday { get; set; }
    [Column("phone")] public VisitorType Type { get; set; } = VisitorType.Student;

    [JsonIgnore] public ICollection<Consultation> Consultations { get; } = null!;
}

public class VisitorDataModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Patronymic { get; set; }
    public DateOnly? Birthday { get; set; }
    public VisitorType? Type { get; set; }
}

public class VisitorPostModelValidator : AbstractValidator<VisitorDataModel>
{
    public VisitorPostModelValidator()
    {
        RuleFor(user => user.FirstName).NotNull().NotEmpty().MaximumLength(30);
        RuleFor(user => user.LastName).NotNull().NotEmpty().MaximumLength(30);
        RuleFor(user => user.Patronymic).MaximumLength(30);
        RuleFor(user => user.Birthday).NotNull().LessThan(DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(user => user.Type).NotNull().IsInEnum();
    }
}