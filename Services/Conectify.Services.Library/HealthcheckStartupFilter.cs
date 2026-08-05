using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Conectify.Services.Library;

/// <summary>
/// Publishes the healthcheck on /health in every service that uses the library, without the service
/// having to remember to do it - a check nobody can reach tells nobody anything, and half of the
/// services used to be missing the endpoint. It is put in front of the rest of the pipeline on
/// purpose, so that a probe cannot be redirected to https or turned away by authorization.
/// </summary>
internal sealed class HealthcheckStartupFilter : IStartupFilter
{
    public const string Path = "/health";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.UseHealthChecks(Path);
        next(app);
    };
}
