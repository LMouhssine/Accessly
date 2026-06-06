using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Accessly.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core tools (migrations). It lets the tools create the
/// context without booting the API host. The connection string is only used by tools that
/// touch the database; <c>migrations add</c> does not connect.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Server=localhost,1433;Database=Accessly;User Id=sa;Password=Your_strong_Passw0rd!;TrustServerCertificate=True;Encrypt=False";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContextFactory).Assembly.FullName));

        return new AppDbContext(optionsBuilder.Options);
    }
}
