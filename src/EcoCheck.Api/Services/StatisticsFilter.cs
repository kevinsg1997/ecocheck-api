namespace EcoCheck.Api.Services;

/// <summary>Filtro regional das estatísticas. Códigos já normalizados e validados.</summary>
public record StatisticsFilter(string? CountryCode = null, string? StateCode = null)
{
    public static readonly StatisticsFilter None = new();

    public bool IsActive => CountryCode is not null;

    public string CacheKey => $"statistics:{CountryCode ?? "*"}:{StateCode ?? "*"}";
}
