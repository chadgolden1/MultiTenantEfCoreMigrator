# MultiTenantEF6Migrator
An example ```net5.0``` console application that executes pending Entity Framework Core (EF Core) migrations to configured tenant databases.

## Remarks
This is useful for efficiently executing migrations in multi-tenanted scenarios where each tenant has its own connection string/database as part of a shared DbContext.

It offers a way to execute migrations in code as opposed to using the PowerShell module or dotnet ef tool. This approach could be used within CI/CD pipelines.

Uses TPL to execute pending migrations in parallel - much faster than serial execution