using Gateway.Lextatico.Api.Configurations;
using Newtonsoft.Json;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureAppConfiguration((hostContext, builder) =>
{
    if (hostContext.HostingEnvironment.EnvironmentName == "LocalDevelopment")
        builder.AddUserSecrets<Program>();
});

builder.WebHost.ConfigureAppConfiguration(ic =>
{
    ic.AddJsonFile("configuration.json");
    ic.AddJsonFile($"configuration.{builder.Environment.EnvironmentName}.json", true);
});

builder.Services.AddLextaticoJwt(builder.Configuration);

builder.Services.AddLexitaticoCors();

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

var conf = new OcelotPipelineConfiguration()
{
    PreErrorResponderMiddleware = async (ctx, next) =>
    {
        var dateTimeNow = DateTime.Now;

        if (ctx.Request.Path.Equals(new PathString("/healthchecks-data-ui")))
        {
            await ctx.Response.WriteAsync(JsonConvert.SerializeObject(new
            {
                status = "Healthy",
                totalDuration = DateTime.Now - dateTimeNow,
                entries = new
                {
                    API = new
                    {
                        data = new { },
                        description = "Gateway up!",
                        duration = DateTime.Now - dateTimeNow,
                        status = "Healthy",
                        tags = Array.Empty<string>()
                    }
                }
            }));
        }
        else
        {
            await next.Invoke();
        }
    }
};

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.UseOcelot(conf).Wait();

app.Run();
