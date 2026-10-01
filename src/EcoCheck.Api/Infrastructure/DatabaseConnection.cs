using Microsoft.AspNetCore.WebUtilities;
using Npgsql;

namespace EcoCheck.Api.Infrastructure;

/// <summary>
/// Resolve a connection string do PostgreSQL a partir da configuração.
/// Aceita "ConnectionStrings:Default" (formato Npgsql) ou a variável DATABASE_URL
/// no formato URI (postgresql://user:pass@host:port/db), usada pelo Railway.
/// </summary>
public static class DatabaseConnection
{
    public static string Resolve(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        var databaseUrl = configuration["DATABASE_URL"];
        if (!string.IsNullOrWhiteSpace(databaseUrl))
        {
            return FromUrl(databaseUrl);
        }

        throw new InvalidOperationException(
            "Nenhuma conexão com o banco configurada. Defina 'ConnectionStrings__Default' ou 'DATABASE_URL'.");
    }

    public static string FromUrl(string databaseUrl)
    {
        if (!Uri.TryCreate(databaseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            throw new InvalidOperationException("DATABASE_URL deve estar no formato postgresql://usuario:senha@host:porta/banco.");
        }

        var userInfo = uri.UserInfo.Split(':', 2);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : null,
            Database = uri.AbsolutePath.TrimStart('/')
        };

        var query = QueryHelpers.ParseQuery(uri.Query);
        if (query.TryGetValue("sslmode", out var sslMode) &&
            Enum.TryParse<SslMode>(sslMode.ToString(), ignoreCase: true, out var parsedSslMode))
        {
            builder.SslMode = parsedSslMode;
        }

        return builder.ConnectionString;
    }
}
