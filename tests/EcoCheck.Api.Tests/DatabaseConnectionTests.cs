using EcoCheck.Api.Infrastructure;
using Npgsql;

namespace EcoCheck.Api.Tests;

public class DatabaseConnectionTests
{
    [Fact]
    public void FromUrl_ConvertsRailwayStyleUrl()
    {
        var connectionString = DatabaseConnection.FromUrl("postgresql://user:p%40ss@db.example.com:6543/railway");

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        Assert.Equal("db.example.com", builder.Host);
        Assert.Equal(6543, builder.Port);
        Assert.Equal("user", builder.Username);
        Assert.Equal("p@ss", builder.Password);
        Assert.Equal("railway", builder.Database);
    }

    [Fact]
    public void FromUrl_WithoutPort_UsesDefault5432()
    {
        var builder = new NpgsqlConnectionStringBuilder(DatabaseConnection.FromUrl("postgres://u:p@localhost/db"));

        Assert.Equal(5432, builder.Port);
    }

    [Fact]
    public void FromUrl_ReadsSslMode()
    {
        var builder = new NpgsqlConnectionStringBuilder(
            DatabaseConnection.FromUrl("postgresql://u:p@localhost:5432/db?sslmode=require"));

        Assert.Equal(SslMode.Require, builder.SslMode);
    }

    [Fact]
    public void FromUrl_InvalidScheme_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => DatabaseConnection.FromUrl("mysql://u:p@localhost/db"));
    }
}
