namespace EcoCheck.Api.Services;

/// <summary>
/// Regiões aceitas na pergunta opcional de localização.
/// Países: códigos ISO 3166-1 alfa-2. Estados: apenas as UFs do Brasil (ISO 3166-2:BR, sem o prefixo "BR-").
/// Nenhuma informação mais precisa que o estado é aceita.
/// </summary>
public static class RegionCatalog
{
    public const string Brazil = "BR";

    public static readonly IReadOnlySet<string> CountryCodes = new HashSet<string>(StringComparer.Ordinal)
    {
        "AD", "AE", "AF", "AG", "AI", "AL", "AM", "AO", "AQ", "AR", "AS", "AT", "AU", "AW", "AX",
        "AZ", "BA", "BB", "BD", "BE", "BF", "BG", "BH", "BI", "BJ", "BL", "BM", "BN", "BO", "BQ",
        "BR", "BS", "BT", "BV", "BW", "BY", "BZ", "CA", "CC", "CD", "CF", "CG", "CH", "CI", "CK",
        "CL", "CM", "CN", "CO", "CR", "CU", "CV", "CW", "CX", "CY", "CZ", "DE", "DJ", "DK", "DM",
        "DO", "DZ", "EC", "EE", "EG", "EH", "ER", "ES", "ET", "FI", "FJ", "FK", "FM", "FO", "FR",
        "GA", "GB", "GD", "GE", "GF", "GG", "GH", "GI", "GL", "GM", "GN", "GP", "GQ", "GR", "GS",
        "GT", "GU", "GW", "GY", "HK", "HM", "HN", "HR", "HT", "HU", "ID", "IE", "IL", "IM", "IN",
        "IO", "IQ", "IR", "IS", "IT", "JE", "JM", "JO", "JP", "KE", "KG", "KH", "KI", "KM", "KN",
        "KP", "KR", "KW", "KY", "KZ", "LA", "LB", "LC", "LI", "LK", "LR", "LS", "LT", "LU", "LV",
        "LY", "MA", "MC", "MD", "ME", "MF", "MG", "MH", "MK", "ML", "MM", "MN", "MO", "MP", "MQ",
        "MR", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "MZ", "NA", "NC", "NE", "NF", "NG", "NI",
        "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PA", "PE", "PF", "PG", "PH", "PK", "PL", "PM",
        "PN", "PR", "PS", "PT", "PW", "PY", "QA", "RE", "RO", "RS", "RU", "RW", "SA", "SB", "SC",
        "SD", "SE", "SG", "SH", "SI", "SJ", "SK", "SL", "SM", "SN", "SO", "SR", "SS", "ST", "SV",
        "SX", "SY", "SZ", "TC", "TD", "TF", "TG", "TH", "TJ", "TK", "TL", "TM", "TN", "TO", "TR",
        "TT", "TV", "TW", "TZ", "UA", "UG", "UM", "US", "UY", "UZ", "VA", "VC", "VE", "VG", "VI",
        "VN", "VU", "WF", "WS", "XK", "YE", "YT", "ZA", "ZM", "ZW"
    };

    public static readonly IReadOnlySet<string> BrazilStateCodes = new HashSet<string>(StringComparer.Ordinal)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA",
        "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    public static string? Normalize(string? code) =>
        string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();

    /// <summary>
    /// Valida um par país/estado já normalizado e adiciona os erros encontrados.
    /// Usado tanto no envio das respostas quanto no filtro das estatísticas.
    /// </summary>
    public static void Validate(
        string? country,
        string? state,
        IDictionary<string, string[]> errors,
        string countryKey = "countryCode",
        string stateKey = "stateCode")
    {
        if (country is not null && !CountryCodes.Contains(country))
        {
            errors[countryKey] = ["País inválido."];
        }

        if (state is null)
        {
            return;
        }

        if (country != Brazil)
        {
            errors[stateKey] = ["O estado só pode ser informado para o Brasil."];
        }
        else if (!BrazilStateCodes.Contains(state))
        {
            errors[stateKey] = ["Estado inválido."];
        }
    }
}
