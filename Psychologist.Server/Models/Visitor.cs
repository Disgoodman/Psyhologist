using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using FluentValidation;
using NpgsqlTypes;
using Psychologist.Server.Database;

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
    [Column("user_id"), JsonIgnore] public int? UserId { get; set; } = null;
    [Column("first_name"), MaxLength(30)] public string FirstName { get; set; } = "";
    [Column("last_name"), MaxLength(30)] public string LastName { get; set; } = "";
    [Column("patronymic"), MaxLength(30)] public string? Patronymic { get; set; }
    [Column("birthday")] public DateOnly Birthday { get; set; }
    [Column("type")] public VisitorType Type { get; set; } = VisitorType.Student;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Email => User?.Email;

    [JsonIgnore] public ApplicationUser? User { get; set; } = null!;

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

public class VisitorRegisterModel : VisitorDataModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class VisitorRegisterModelValidator : AbstractValidator<VisitorRegisterModel>
{
    public VisitorRegisterModelValidator()
    {
        Include(new VisitorPostModelValidator());
        RuleFor(user => user.Email).NotNull().NotEmpty().EmailAddress();
        RuleFor(user => user.Password).NotNull().NotEmpty().MinimumLength(6);
    }
}

public class AppointmentPostModel
{
    public int? SpecialistId { get; set; }
    public DateOnly? Date { get; set; }
    public TimeOnly? Time { get; set; }
    public bool? Primary { get; set; } = true;
    public ConsultationType? Type { get; set; }
}

public class AppointmentPostModelValidator : AbstractValidator<AppointmentPostModel>
{
    public AppointmentPostModelValidator()
    {
        RuleFor(c => c.SpecialistId).NotNull();
        RuleFor(c => c.Date).NotNull();
        RuleFor(c => c.Time).NotNull();
        RuleFor(c => c.Primary).NotNull();
        RuleFor(c => c.Type).NotNull().IsInEnum();
    }
}