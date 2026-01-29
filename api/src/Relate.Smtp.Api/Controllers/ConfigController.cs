using Microsoft.AspNetCore.Mvc;

namespace Relate.Smtp.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public ConfigController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Get runtime configuration for web frontend
    /// </summary>
    [HttpGet("config.json")]
    [ResponseCache(Duration = 300)] // Cache for 5 minutes
    public IActionResult GetConfig()
    {
        var config = new
        {
            oidcAuthority = _configuration["Oidc:Authority"] ?? "",
            oidcClientId = _configuration["Oidc:ClientId"] ?? "",
            oidcRedirectUri = _configuration["Oidc:RedirectUri"] ?? "",
            oidcScope = _configuration["Oidc:Scope"] ?? "openid profile email"
        };

        return Ok(config);
    }
}
