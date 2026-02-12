using Archi.API.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Asp.Versioning;  

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddDbContext<ArchiDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("archilogdb")));

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader(); 
});



builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.MapControllers();

app.Run();
