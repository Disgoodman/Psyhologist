using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

namespace Psychologist.Server.Models;

[PgName("consultation_type")]
public enum ConsultationType
{
    [PgName("individual_consultation")] IndividualConsultation,
    [PgName("individual_work")] IndividualWork,
    [PgName("diagnostic_work")] DiagnosticWork,
}

[Table("consultations")]
[Index(nameof(ScheduleDate), nameof(Time))]
[JsonDerivedType(typeof(IndividualConsultation))]
[JsonDerivedType(typeof(IndividualWork))]
[JsonDerivedType(typeof(DiagnosticWork))]
public class Consultation
{
    [Column("id")] public int Id { get; set; }
    [Column("schedule_date")] public DateOnly ScheduleDate { get; set; }
    [Column("specialist_id")] public int SpecialistId { get; set; }
    [Column("visitor_id"), JsonIgnore] public int VisitorId { get; set; }
    [Column("created_by_visitor")] public bool CreatedByVisitor { get; set; } = false;
    [Column("time")] public TimeOnly Time { get; set; }
    [Column("topic"), MaxLength(200)] public string Topic { get; set; } = "";

    [Column("visitor_arrived"), DefaultValue(false)]
    public bool VisitorArrived { get; set; } = false;

    [Column("primary"), DefaultValue(true)]
    public bool Primary { get; set; } = true;

    [Column("type"), DefaultValue(false)] public ConsultationType Type { get; set; }

    [JsonIgnore] public ScheduleDay Day { get; set; } = null!;
    public Visitor Visitor { get; set; } = null!;
    [JsonIgnore] public Specialist Specialist { get; set; } = null!;
}

public class IndividualConsultation : Consultation
{
    [Column("request_code")] public string RequestCode { get; set; } = "";
    [Column("notes")] public string Notes { get; set; } = "";
}

public class IndividualWork : Consultation
{
    [Column("purpose")] public string Purpose { get; set; } = "";
}

public class DiagnosticWork : Consultation
{
    [Column("request_code")] public string RequestCode { get; set; } = "";
    [Column("revealed")] public string Revealed { get; set; } = "";
    [Column("prescribed")] public string Prescribed { get; set; } = "";
}

public class ConsultationPostModel
{
    public int? SpecialistId { get; set; }
    public DateOnly? ScheduleDate { get; set; }
    public int? VisitorId { get; set; }
    public TimeOnly? Time { get; set; }
    public string? Topic { get; set; } = "";
    public bool? VisitorArrived { get; set; } = false;
    public bool? Primary { get; set; }
    public ConsultationType? Type { get; set; }

    public void AssignTo(Consultation c)
    {
        c.SpecialistId = SpecialistId!.Value;
        c.ScheduleDate = ScheduleDate!.Value;
        c.VisitorId = VisitorId!.Value;
        c.Time = Time!.Value;
        c.Topic = Topic!;
        c.VisitorArrived = VisitorArrived!.Value;
        c.Primary = Primary!.Value;
        c.Type = Type!.Value;
    }

    public Consultation ToModel()
    {
        if (this is IndividualConsultationPostModel individualConsultationPostModel)
        {
            var individualConsultation = new IndividualConsultation();
            individualConsultationPostModel.AssignTo(individualConsultation);
            return individualConsultation;
        }

        if (this is IndividualWorkPostModel individualWorkPostModel)
        {
            var individualWork = new IndividualWork();
            individualWorkPostModel.AssignTo(individualWork);
            return individualWork;
        }

        if (this is DiagnosticWorkPostModel diagnosticWorkPostModel)
        {
            var diagnosticWork = new DiagnosticWork();
            diagnosticWorkPostModel.AssignTo(diagnosticWork);
            return diagnosticWork;
        }

        var consultation = new Consultation();
        this.AssignTo(consultation);
        return consultation;
    }
}

public class IndividualConsultationPostModel : ConsultationPostModel
{
    public IndividualConsultationPostModel() => Type = ConsultationType.IndividualConsultation;

    public string? RequestCode { get; set; }
    public string? Notes { get; set; } = "";

    public void AssignTo(IndividualConsultation c)
    {
        base.AssignTo(c);
        c.RequestCode = RequestCode!;
        c.Notes = Notes!;
    }
}

public class IndividualWorkPostModel : ConsultationPostModel
{
    public IndividualWorkPostModel() => Type = ConsultationType.IndividualWork;

    public string? Purpose { get; set; }

    public void AssignTo(IndividualWork c)
    {
        base.AssignTo(c);
        c.Purpose = Purpose!;
    }
}

public class DiagnosticWorkPostModel : ConsultationPostModel
{
    public DiagnosticWorkPostModel() => Type = ConsultationType.DiagnosticWork;

    public string? RequestCode { get; set; }
    public string? Revealed { get; set; }
    public string? Prescribed { get; set; }

    public void AssignTo(DiagnosticWork c)
    {
        base.AssignTo(c);
        c.RequestCode = RequestCode!;
        c.Revealed = Revealed!;
        c.Prescribed = Prescribed!;
    }
}

public class ConsultationPostModelValidator<T> : AbstractValidator<T> where T : ConsultationPostModel
{
    public ConsultationPostModelValidator()
    {
        RuleFor(c => c.SpecialistId).NotNull();
        RuleFor(c => c.ScheduleDate).NotNull();
        RuleFor(c => c.Time).NotNull();
        RuleFor(c => c.VisitorId).NotNull();
        RuleFor(c => c.Topic).NotNull().NotEmpty();
        RuleFor(c => c.VisitorArrived).NotNull();
        RuleFor(c => c.Primary).NotNull();
        RuleFor(c => c.Type).NotNull().IsInEnum();
    }
}

public class ConsultationPostModelValidator : ConsultationPostModelValidator<ConsultationPostModel>;

public class IndividualConsultationPostModelValidator : ConsultationPostModelValidator<IndividualConsultationPostModel>
{
    public IndividualConsultationPostModelValidator()
    {
        RuleFor(c => c.RequestCode).NotNull().NotEmpty();
        RuleFor(c => c.Notes).NotNull();
    }
}

public class IndividualWorkPostModelValidator : ConsultationPostModelValidator<IndividualWorkPostModel>
{
    public IndividualWorkPostModelValidator()
    {
        RuleFor(c => c.Purpose).NotNull().NotEmpty();
    }
}

public class DiagnosticWorkPostModelValidator : ConsultationPostModelValidator<DiagnosticWorkPostModel>
{
    public DiagnosticWorkPostModelValidator()
    {
        RuleFor(c => c.RequestCode).NotNull().NotEmpty();
        RuleFor(c => c.Revealed).NotNull();
        RuleFor(c => c.Prescribed).NotNull();
    }
}

public class ConsultationPutModel
{
    public int? VisitorId { get; set; }
    public string? Topic { get; set; }
    public bool? VisitorArrived { get; set; } = false;
    public bool? Primary { get; set; }

    public void AssignTo(Consultation c)
    {
        c.VisitorId = VisitorId!.Value;
        c.Topic = Topic!;
        c.Primary = Primary!.Value;
        c.VisitorArrived = VisitorArrived!.Value;
    }
}

public class IndividualConsultationPutModel : ConsultationPutModel
{
    public string? RequestCode { get; set; }
    public string? Notes { get; set; }

    public void AssignTo(IndividualConsultation c)
    {
        base.AssignTo(c);
        c.RequestCode = RequestCode!;
        c.Notes = Notes!;
    }
}

public class IndividualWorkPutModel : ConsultationPutModel
{
    public string? Purpose { get; set; }

    public void AssignTo(IndividualWork c)
    {
        base.AssignTo(c);
        c.Purpose = Purpose!;
    }
}

public class DiagnosticWorkPutModel : ConsultationPutModel
{
    public string? RequestCode { get; set; }
    public string? Revealed { get; set; }
    public string? Prescribed { get; set; }

    public void AssignTo(DiagnosticWork c)
    {
        base.AssignTo(c);
        c.RequestCode = RequestCode!;
        c.Revealed = Revealed!;
        c.Prescribed = Prescribed!;
    }
}

public class ConsultationPutModelValidator<T> : AbstractValidator<T> where T : ConsultationPutModel
{
    public ConsultationPutModelValidator()
    {
        RuleFor(c => c.VisitorId).NotNull();
        RuleFor(c => c.Topic).NotNull();
        RuleFor(c => c.VisitorArrived).NotNull();
    }
}

public class ConsultationPutModelValidator : ConsultationPutModelValidator<ConsultationPutModel>;

public class IndividualConsultationPutModelValidator : ConsultationPutModelValidator<IndividualConsultationPutModel>
{
    public IndividualConsultationPutModelValidator()
    {
        RuleFor(c => c.RequestCode).NotNull().NotEmpty();
        RuleFor(c => c.Notes).NotNull();
    }
}

public class IndividualWorkPutModelValidator : ConsultationPutModelValidator<IndividualWorkPutModel>
{
    public IndividualWorkPutModelValidator()
    {
        RuleFor(c => c.Purpose).NotNull().NotEmpty();
    }
}

public class DiagnosticWorkPutModelValidator : ConsultationPutModelValidator<DiagnosticWorkPutModel>
{
    public DiagnosticWorkPutModelValidator()
    {
        RuleFor(c => c.Primary).NotNull();
        RuleFor(c => c.RequestCode).NotNull().NotEmpty();
        RuleFor(c => c.Revealed).NotNull();
        RuleFor(c => c.Prescribed).NotNull();
    }
}