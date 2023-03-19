using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Movies.Api.Auth;

public class ApiKeyAuthFilter : IAuthorizationFilter
{
    private readonly IConfiguration _configuration;

    public ApiKeyAuthFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var potentialApiKey))
        {
            context.Result = new UnauthorizedObjectResult("Missing API key");
            return;
        }

        var apiKey = _configuration.GetValue<string>("ApiKey");

        if (apiKey != potentialApiKey)
        {
            context.Result = new UnauthorizedObjectResult("Invalid API key");
        }
    }
}