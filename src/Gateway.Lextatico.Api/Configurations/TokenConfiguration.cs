namespace Gateway.Lextatico.Api.Configurations
{
    public class TokenConfiguration
    {
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public int Seconds { get; set; }
        public int SecondsRefresh { get; set; }
    }
}
