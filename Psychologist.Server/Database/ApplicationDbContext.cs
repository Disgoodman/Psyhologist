using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Psychologist.Server.Models;
using UserKey = System.Int32;

namespace Psychologist.Server.Database;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, UserKey>
{
    public DbSet<Article> Articles { get; set; } = null!;
    public DbSet<Visitor> Visitors { get; set; } = null!;
    public DbSet<ScheduleDay> ScheduleDays { get; set; } = null!;
    public DbSet<Consultation> Consultations { get; set; } = null!;
    public DbSet<IndividualConsultation> IndividualConsultations { get; set; } = null!;
    public DbSet<IndividualWork> IndividualWorks { get; set; } = null!;
    public DbSet<DiagnosticWork> DiagnosticWorks { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Consultation>()
            .HasDiscriminator(c => c.Type)
            .HasValue<Consultation>((ConsultationType)(-1))
            .HasValue<IndividualConsultation>(ConsultationType.IndividualConsultation)
            .HasValue<IndividualWork>(ConsultationType.IndividualWork)
            .HasValue<DiagnosticWork>(ConsultationType.DiagnosticWork);

        builder.Entity<IndividualConsultation>();
        builder.Entity<IndividualWork>();
        builder.Entity<DiagnosticWork>();
    }

    /// <summary> Method that configures the <see cref="NpgsqlDataSourceBuilder"/>. </summary>
    public static void ConfigureNpgsqlBuilder(NpgsqlDataSourceBuilder builder)
    {
        //builder.MapEnum<>();
    }
}

// https://learn.microsoft.com/en-us/aspnet/identity/overview/extensibility/change-primary-key-for-users-in-aspnet-identity

public class ApplicationUser : IdentityUser<UserKey>
{
}

public class ApplicationRole : IdentityRole<UserKey>
{
    public ApplicationRole()
    {
    }

    public ApplicationRole(String roleName) : base(roleName)
    {
    }
}

public static class ClaimsPrincipalExtensions
{
    /*public static string? GetUserId(this ClaimsPrincipal principal) =>
        principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;*/
    public static UserKey? GetUserId(this ClaimsPrincipal principal) =>
        UserKey.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
}

public static class Roles
{
    public const string Admin = nameof(Admin);
    public const string Employee = nameof(Employee);

    public static readonly ImmutableArray<string> List = [Admin, Employee];
}