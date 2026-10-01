using System.Text.Json;
using System.Text.Json.Serialization;
using EcoCheck.Api.Data;
using EcoCheck.Api.Infrastructure;
using EcoCheck.Api.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// O Railway informa a porta pela variável PORT.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(DatabaseConnection.Resolve(builder.Configuration))
        .UseSnakeCaseNamingConvention());

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ScoringService>();
builder.Services.AddScoped<SurveyService>();
builder.Services.AddScoped<StatisticsService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums trafegam como texto: "water", "good_habits", "consumption_and_mobility".
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false));
        options.AllowInputFormatterExceptionMessages = false;
    })
    .ConfigureEcoCheckValidationResponses();

builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Title = context.ProblemDetails.Status switch
        {
            StatusCodes.Status404NotFound => "Recurso não encontrado.",
            StatusCodes.Status405MethodNotAllowed => "Método não permitido.",
            _ => context.ProblemDetails.Title
        };
    });
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEcoCheckCors(builder.Configuration);
builder.Services.AddEcoCheckRateLimiting(builder.Configuration);
builder.Services.AddEcoCheckForwardedHeaders();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.AddOpenApi();

if (!builder.Environment.IsDevelopment())
{
    // O proxy do Railway termina o TLS; a aplicação redireciona para a porta pública 443.
    builder.Services.AddHttpsRedirection(options => options.HttpsPort = 443);
    builder.Services.AddHsts(options => options.MaxAge = TimeSpan.FromDays(365));
}

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    app.Logger.LogInformation("Aplicando migrations pendentes...");
    db.Database.Migrate();
}

if (app.Configuration.GetValue<bool>("ReverseProxy:Enabled"))
{
    app.UseForwardedHeaders();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    // O health check do Railway é feito por HTTP interno e não deve ser redirecionado.
    app.UseWhen(
        context => !context.Request.Path.StartsWithSegments("/health"),
        branch => branch.UseHttpsRedirection());
}

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.XContentTypeOptions = "nosniff";
    headers.XFrameOptions = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseCors(ServiceCollectionExtensions.CorsPolicyName);
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();
