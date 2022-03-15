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

if (HostEnvironmentEnvExtensions.IsDocker())
    Thread.Sleep(30000);

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureAppConfiguration((hostContext, builder) =>
{
    if (hostContext.HostingEnvironment.IsLocalDevelopment())
        builder.AddUserSecrets<Program>();
});

builder.WebHost.ConfigureAppConfiguration(ic =>
{
    ic.AddJsonFile("configuration.json");
    ic.AddJsonFile($"configuration.{builder.Environment.EnvironmentName}.json", true);
});

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

app.UseEndpoints(cf =>
{
    cf.MapHealthChecks("/status",
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

    cf.MapHealthChecks("/healthchecks-data-ui", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
});

app.UseSwaggerForOcelotUI();

app.UseOcelot().Wait();

app.Run();
