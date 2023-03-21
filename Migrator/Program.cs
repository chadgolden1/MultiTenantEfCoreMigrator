using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Migrator;
using Serilog;
using Serilog.Context;
using ILogger = Serilog.ILogger;

ILogger Logger = Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {TenantName}{Message:lj}{NewLine}{Exception}")
    .CreateLogger();

List<MigratorTenantInfo> tenants = GetConfiguredTenants();

IEnumerable<Task> tasks = tenants.Select(t => MigrateTenantDatabase(t));
try
{
    Logger.Information("Starting parallel execution of pending migrations...");
    await Task.WhenAll(tasks);
}
catch
{
    Logger.Warning("Parallel execution of pending migrations is complete with error(s).");
    return (int)ExitCode.Error;
}

Logger.Information("Parallel execution of pending migrations is complete.");
return (int)ExitCode.Success;

async Task MigrateTenantDatabase(MigratorTenantInfo tenant)
{
    using var logContext = LogContext.PushProperty("TenantName", $"({tenant.Name}) ");
    DbContextOptions dbContextOptions = CreateDefaultDbContextOptions(tenant.ConnectionString);
    try
    {
        using var context = new EfCoreDbContext(dbContextOptions);
        await context.Database.MigrateAsync();
    }
    catch (Exception e)
    {
        Logger.Error(e, "Error occurred during migration");
        throw;
    }
}

DbContextOptions CreateDefaultDbContextOptions(string connectionString) =>
    new DbContextOptionsBuilder()
        .LogTo(action: Logger.Information, filter: MigrationInfoLogFilter(), options: DbContextLoggerOptions.None)
        .UseSqlServer(connectionString)
        .Options;


Func<EventId, LogLevel, bool> MigrationInfoLogFilter() => (eventId, level) =>
    level > LogLevel.Information ||
    (level == LogLevel.Information &&
    new[]
    {
        RelationalEventId.MigrationApplying,
        RelationalEventId.MigrationAttributeMissingWarning,
        RelationalEventId.MigrationGeneratingDownScript,
        RelationalEventId.MigrationGeneratingUpScript,
        RelationalEventId.MigrationReverting,
        RelationalEventId.MigrationsNotApplied,
        RelationalEventId.MigrationsNotFound,
        RelationalEventId.MigrateUsingConnection
    }.Contains(eventId));

List<MigratorTenantInfo> GetConfiguredTenants()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appSettings.json", optional: false);

    IConfiguration config = builder.Build();

    return config.GetSection(nameof(MigratorTenantInfo)).Get<List<MigratorTenantInfo>>();
}
