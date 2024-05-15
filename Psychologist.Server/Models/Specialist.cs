using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using FluentValidation;
using Psychologist.Server.Database;

namespace Psychologist.Server.Models;

[Table("specialists")]
public class Specialist
{
    [Column("id")] public int Id { get; set; }
    [Column("first_name"), MaxLength(30)] public string FirstName { get; set; } = "";
    [Column("last_name"), MaxLength(30)] public string LastName { get; set; } = "";
    [Column("patronymic"), MaxLength(30)] public string? Patronymic { get; set; }
    [Column("type"), MaxLength(50)] public string Type { get; set; } = "";
    [Column("primary_visit_price")] public decimal PrimaryVisitPrice { get; set; }
    [Column("secondary_visit_price")] public decimal SecondaryVisitPrice { get; set; }
    [Column("user_id"), JsonIgnore] public int UserId { get; set; }

    [JsonIgnore] public string FullName => FirstName + " " + LastName + (Patronymic is { } p ? " " + p : "");
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Email => User?.Email;

    [JsonIgnore] public ApplicationUser? User { get; set; } = null!;
    [JsonIgnore] public ICollection<Consultation> Consultations { get; } = null!;
}

public class SpecialistDataModel
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Patronymic { get; set; }
    public string? Type { get; set; }
    public decimal? PrimaryVisitPrice { get; set; }
    public decimal? SecondaryVisitPrice { get; set; }
}

public class SpecialistDataModelValidator : AbstractValidator<SpecialistDataModel>
{
    public SpecialistDataModelValidator()
    {
        RuleFor(user => user.FirstName).NotNull().NotEmpty().MaximumLength(30);
        RuleFor(user => user.LastName).NotNull().NotEmpty().MaximumLength(30);
        RuleFor(user => user.Patronymic).MaximumLength(30);
        RuleFor(user => user.Type).NotNull().NotEmpty().MaximumLength(50);
        RuleFor(user => user.PrimaryVisitPrice).NotNull().GreaterThanOrEqualTo(0);
        RuleFor(user => user.SecondaryVisitPrice).NotNull().GreaterThanOrEqualTo(0);
    }
}

public class SpecialistPostModel : SpecialistDataModel
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class SpecialistPostModelValidator : AbstractValidator<SpecialistPostModel>
{
    public SpecialistPostModelValidator()
    {
        Include(new SpecialistDataModelValidator());
        RuleFor(user => user.Email).NotNull().EmailAddress();
        RuleFor(user => user.Password).NotNull().Length(4, 30);
    }
}