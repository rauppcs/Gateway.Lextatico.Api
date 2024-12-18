using Serilog;

namespace Gateway.Lextatico.Api.Extensions
{
    public static class SerilogExtensions
    {
        public static void UseLextaticoSerilog(this IHostBuilder builder, IWebHostEnvironment hostEnvironment,
            IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            builder.UseSerilog();
        }
    }
}