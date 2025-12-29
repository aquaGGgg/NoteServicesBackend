using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Notes.WebApi.Controllers;

[ApiController]
[Route("api/v1/version")]
[AllowAnonymous]
public sealed class VersionController : ControllerBase
{
    private readonly IConfiguration cfg;
    private readonly IHostEnvironment env;

    public VersionController(IConfiguration cfg, IHostEnvironment env)
    {
        this.cfg = cfg;
        this.env = env;
    }

    [HttpGet]
    public ActionResult<VersionResponse> Get()
    {
        // Эти значения можно задать в appsettings.json / env vars
        var apiVersion = cfg["App:ApiVersion"] ?? "1";
        var build = cfg["App:Build"] ?? "dev";
        var commit = cfg["App:Commit"] ?? "unknown";

        // Минимально допустимые версии клиентов
        var minWeb = cfg["Clients:Web:MinVersion"] ?? "0";
        var minAndroid = cfg["Clients:Android:MinVersion"] ?? "0";

        // Можно “жестко” рубить доступ, если надо
        var forceWeb = cfg.GetValue("Clients:Web:ForceUpdate", false);
        var forceAndroid = cfg.GetValue("Clients:Android:ForceUpdate", false);

        // Можно показывать сообщение в UI
        var message = cfg["App:Message"]; // null ок

        var response = new VersionResponse(
            ApiVersion: apiVersion,
            Environment: env.EnvironmentName,
            Build: build,
            Commit: commit,
            ServerTimeUtc: DateTimeOffset.UtcNow,
            Clients: new ClientsBlock(
                Web: new ClientPolicy(minWeb, forceWeb),
                Android: new ClientPolicy(minAndroid, forceAndroid)
            ),
            Message: message
        );

        return Ok(response);
    }

    public sealed record VersionResponse(
        string ApiVersion,
        string Environment,
        string Build,
        string Commit,
        DateTimeOffset ServerTimeUtc,
        ClientsBlock Clients,
        string? Message
    );

    public sealed record ClientsBlock(
        ClientPolicy Web,
        ClientPolicy Android
    );

    public sealed record ClientPolicy(
        string MinVersion,
        bool ForceUpdate
    );
}
