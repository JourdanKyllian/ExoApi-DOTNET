using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi; 
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Archi.API.Swagger;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        => _provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var desc in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(desc.GroupName, new OpenApiInfo
            {
                Title       = "Archi.API",
                Version     = desc.GroupName,
                Description = desc.IsDeprecated ? "Version dépréciée" : "API REST Architecture"
            });
        }
    }
}

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerWithVersioning(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();
        return services;
    }

    public static IApplicationBuilder UseSwaggerWithVersioning(
        this IApplicationBuilder app,
        IApiVersionDescriptionProvider provider)
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            foreach (var desc in provider.ApiVersionDescriptions)
                c.SwaggerEndpoint(
                    $"/swagger/{desc.GroupName}/swagger.json",
                    $"Archi.API {desc.GroupName.ToUpperInvariant()}");
            c.RoutePrefix = string.Empty;
        });
        return app;
    }
}