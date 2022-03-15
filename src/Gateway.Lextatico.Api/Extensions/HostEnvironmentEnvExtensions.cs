namespace Gateway.Lextatico.Api.Extensions
{
    public static class HostEnvironmentEnvExtensions
    {
        public static bool IsLocalDevelopment(this IHostEnvironment hostEnvironment)
        {
            if (hostEnvironment == null)
                throw new ArgumentNullException(nameof(hostEnvironment));

            return hostEnvironment.IsEnvironment("LocalDevelopment");
        }

        public static bool IsDocker()
        {
            var result = bool.TryParse(Environment.GetEnvironmentVariable("IS_DOCKER"), out var isDocker);

            return result && isDocker;
        }
    }
}
