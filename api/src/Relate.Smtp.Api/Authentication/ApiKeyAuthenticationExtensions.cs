using Microsoft.AspNetCore.Authentication;

namespace Relate.Smtp.Api.Authentication;

public static class ApiKeyAuthenticationExtensions
{
    public const string ApiKeyScheme = "ApiKey";

    public static AuthenticationBuilder AddApiKeyAuthentication(this AuthenticationBuilder builder)
    {
        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            ApiKeyScheme,
            options => { });
    }
}
