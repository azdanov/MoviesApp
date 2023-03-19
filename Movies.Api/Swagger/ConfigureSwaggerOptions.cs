using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Movies.Api.Swagger;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    readonly IApiVersionDescriptionProvider _provider;
    readonly IHostEnvironment _hostEnvironment;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IHostEnvironment hostEnvironment)
    {
        _provider = provider;
        _hostEnvironment = hostEnvironment;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                new OpenApiInfo
                {
                    Title = _hostEnvironment.ApplicationName,
                    Version = description.ApiVersion.ToString(),
                }
            );
        }
    }
}