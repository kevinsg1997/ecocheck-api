using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EcoCheck.Api.Infrastructure;

public static class ServiceCollectionExtensions
{
    public const string CorsPolicyName = "frontend";

    /// <summary>
    /// Origens lidas de "Cors:AllowedOrigins", como array (appsettings) ou texto separado
    /// por vírgulas (variável de ambiente Cors__AllowedOrigins).
    /// </summary>
    public static IServiceCollection AddEcoCheckCors(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("Cors:AllowedOrigins");
        var origins = section.GetChildren().Select(c => c.Value).ToList();
        if (origins.Count == 0 && !string.IsNullOrWhiteSpace(section.Value))
        {
            origins = [.. section.Value.Split(',')];
        }

        var allowedOrigins = origins
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(o => o!.Trim().TrimEnd('/'))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        services.AddCors(options => options.AddPolicy(CorsPolicyName, policy => policy
            .WithOrigins(allowedOrigins)
            .WithMethods("GET", "POST")
            .WithHeaders("Content-Type")
            .SetPreflightMaxAge(TimeSpan.FromHours(1))));

        return services;
    }

    /// <summary>
    /// Limites por IP mantidos apenas em memória. O IP não é armazenado nem registrado em log.
    /// </summary>
    public static IServiceCollection AddEcoCheckRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var limits = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>()
                     ?? new RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(RateLimitPolicies.Submit, context =>
                RateLimitPartition.GetFixedWindowLimiter(PartitionKey(context), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limits.SubmitPermitLimit,
                    Window = TimeSpan.FromMinutes(limits.SubmitWindowMinutes),
                    QueueLimit = 0
                }));

            options.AddPolicy(RateLimitPolicies.Read, context =>
                RateLimitPartition.GetFixedWindowLimiter(PartitionKey(context), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limits.ReadPermitLimit,
                    Window = TimeSpan.FromSeconds(limits.ReadWindowSeconds),
                    QueueLimit = 0
                }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                }

                var problemDetails = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetails.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = new ProblemDetails
                    {
                        Status = StatusCodes.Status429TooManyRequests,
                        Title = "Muitas requisições em pouco tempo. Aguarde alguns minutos e tente novamente."
                    }
                });
            };
        });

        return services;
    }

    /// <summary>
    /// No Railway a API fica atrás de um proxy que termina o HTTPS. Os headers X-Forwarded-*
    /// informam o IP real (usado só no rate limiting) e o esquema original (https).
    /// </summary>
    public static IServiceCollection AddEcoCheckForwardedHeaders(this IServiceCollection services)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            // Os IPs do proxy do Railway não são fixos. Com ForwardLimit = 1 (padrão), só o valor
            // adicionado pelo último proxy é considerado, então o cliente não consegue forjá-lo.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        return services;
    }

    /// <summary>
    /// Padroniza erros de validação e JSON malformado em português, sem detalhes internos.
    /// </summary>
    public static IMvcBuilder ConfigureEcoCheckValidationResponses(this IMvcBuilder builder)
    {
        builder.ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var modelState = context.ModelState;

                // Chaves "$..." vêm do desserializador JSON; a chave do parâmetro aparece quando o corpo está ausente ou inválido.
                var isMalformedBody = modelState.Keys.Any(k => k.StartsWith('$') || k == "request" || k == string.Empty);

                var problem = isMalformedBody
                    ? new ValidationProblemDetails(new Dictionary<string, string[]>
                    {
                        ["body"] = ["O corpo da requisição não é um JSON válido."]
                    })
                    : new ValidationProblemDetails(modelState);

                problem.Title = "Os dados enviados são inválidos.";
                problem.Status = StatusCodes.Status400BadRequest;

                return new BadRequestObjectResult(problem)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });

        return builder;
    }

    private static string PartitionKey(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
