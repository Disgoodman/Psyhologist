using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Psychologist.Server.Configuration;
using Psychologist.Server.Database;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddProblemDetails();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date"
    });
    c.MapType<TimeOnly>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "time"
    });
    c.MapType<DateTime>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "date-time",
    });
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Roles.Admin, policy => policy.RequireRole(Roles.Admin));
    options.AddPolicy(Roles.Employee, policy => policy.RequireRole(Roles.Employee, Roles.Admin));
    options.AddPolicy(Roles.Visitor, policy => policy.RequireRole(Roles.Visitor, Roles.Employee, Roles.Admin));
});

builder.Services.AddIdentityApiEndpoints<ApplicationUser>(o =>
    {
        // o.SignIn.RequireConfirmedEmail = true;
    })
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


var connectionString = Environment.GetEnvironmentVariable("DB")
                       ?? builder.Configuration.GetConnectionString("PgsqlConnection")!;
builder.Services.AddNpgsqlDataSource(connectionString, ApplicationDbContext.ConfigureNpgsqlBuilder);

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseNpgsql());
//options => options.UseInMemoryDatabase("AppDb"));

builder.Services.AddScoped<DatabaseInitializer>();

builder.Services.Configure<AuthConfig>(builder.Configuration.GetSection(AuthConfig.SectionName));

// FluentValidation
//ValidatorOptions.Global.DisplayNameResolver = (type, member, expression) => CamelCaseNamingPolicy.ToCamelCase(member.Name);
//ValidatorOptions.Global.PropertyNameResolver = (type, member, expression) => CamelCaseNamingPolicy.ToCamelCase(member.Name);
ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;

builder.Services.AddFluentValidationAutoValidation(fv => fv.DisableDataAnnotationsValidation = true);
builder.Services.AddValidatorsFromAssemblyContaining<Program>(lifetime: ServiceLifetime.Singleton);

builder.Services.AddDateOnlyTimeOnlyStringConverters();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var efContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    //await efContext.Database.EnsureDeletedAsync();
    await efContext.Database.EnsureCreatedAsync();

    var databaseInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await databaseInitializer.Initialize();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.UsePathBase("/api");

app.UseRouting();

app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapIdentityApi<ApplicationUser>();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();