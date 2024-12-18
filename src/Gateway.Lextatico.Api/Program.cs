using System.Net.Mime;
using Gateway.Lextatico.Api.Configurations;
using Gateway.Lextatico.Api.Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using HostEnvironmentEnvExtensions = Gateway.Lextatico.Api.Extensions.HostEnvironmentEnvExtensions;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsLocalDevelopment())
    builder.Configuration.AddUserSecrets<Program>();

builder.Configuration
    .AddJsonFile("configuration.json")
    .AddJsonFile($"configuration.{builder.Environment.EnvironmentName}.json", true);

builder.Host.UseLextaticoSerilog(builder.Environment, builder.Configuration);

builder.Services.AddLextaticoJwt(builder.Configuration);

builder.Services.AddLexitaticoCors();

builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddCheck("API", () => new HealthCheckResult(HealthStatus.Healthy, "Gatway is up!"));

builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();

    app.UseHsts();
}

app.UseRouting();

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.MapHealthChecks("/status",
    new HealthCheckOptions()
    {
        ResponseWriter = async (context, report) =>
        {
            var result = JsonConvert.SerializeObject(
                new
                {
                    statusApplication = report.Status.ToString(),
                    healthChecks = report.Entries.Select(e => new
                    {
                        check = e.Key,
                        ErrorMessage = e.Value.Exception?.Message,
                        status = Enum.GetName(typeof(HealthStatus), e.Value.Status)
                    })
                });
            context.Response.ContentType = MediaTypeNames.Application.Json;
            await context.Response.WriteAsync(result);
        }
    });

app.MapHealthChecks("/healthchecks-data-ui", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.UseEndpoints(e =>
{
    e.MapControllers();
});

app.UseSwaggerForOcelotUI();

app.UseOcelot();

app.Run();