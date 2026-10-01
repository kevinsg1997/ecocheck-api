namespace EcoCheck.Api.Infrastructure;

public static class RateLimitPolicies
{
    public const string Submit = "submit";
    public const string Read = "read";
}

public class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public int SubmitPermitLimit { get; set; } = 5;
    public int SubmitWindowMinutes { get; set; } = 10;
    public int ReadPermitLimit { get; set; } = 60;
    public int ReadWindowSeconds { get; set; } = 60;
}
