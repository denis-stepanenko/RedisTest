using Caching;
using Community.Microsoft.Extensions.Caching.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString")
    ?? throw new ArgumentNullException("There is no connection string");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    // Prefix
    options.InstanceName = "local";
});

// Memory Cache
builder.Services.AddMemoryCache();

// Caching with PostgreSQL
// builder.Services.AddDistributedPostgreSqlCache(options =>
// {
//     options.ConnectionString = connectionString;
//     options.SchemaName = "public";
//     options.TableName = "TestCache";
// });

builder.Services.AddSerilog();

var app = builder.Build();

// dotnet run seed
if (args.Contains("seed"))
{
    using var scope = app.Services.CreateScope();

    var service = scope.ServiceProvider;
    var context = service.GetService<ApplicationDbContext>()
        ?? throw new ArgumentNullException("Couldn't resolve EF context during data seeding");

    context.Users.Add(new() { Name = "Dennis" });
    await context.SaveChangesAsync();

    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();