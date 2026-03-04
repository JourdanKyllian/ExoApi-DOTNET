using Archi.API.Data;
using Archi.API.Swagger;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;


// Logs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        theme: Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme.Colored,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log-api-s.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, svc, cfg) => cfg
        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
        .Enrich.FromLogContext()
        .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.SystemConsoleTheme.Colored)
        .WriteTo.File("logs/log-api-s.txt", rollingInterval: RollingInterval.Day));

    builder.Services.AddControllers();

    builder.Services.AddDbContext<ArchiDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("archilogdb")));

// Versioning
    builder.Services
        .AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
// Swagger
    builder.Services.AddSwaggerWithVersioning();  

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        app.UseSwaggerWithVersioning(provider);  
    }

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();
    app.MapControllers();

    Log.Information("Démarrage de l'API Archi v1.0");
    app.Run();
}
catch (Exception ex) { Log.Fatal(ex, "Échec au démarrage"); }
finally { Log.CloseAndFlush(); }