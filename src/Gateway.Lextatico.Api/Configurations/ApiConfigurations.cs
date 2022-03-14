using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Gateway.Lextatico.Api.Configurations
{
    public static class ApiConfigurations
    {
        public static IServiceCollection AddLextaticoJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var signingConfigurations = new SigningConfiguration(configuration["SecretKeyJwt"]);
            services.AddSingleton(signingConfigurations);

            var tokenConfiguration = new TokenConfiguration();
            configuration.Bind("TokenConfiguration", tokenConfiguration);

            services.AddSingleton(tokenConfiguration);

            services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(bearerOptions =>
            {
                var paramsValidation = bearerOptions.TokenValidationParameters;
                paramsValidation.IssuerSigningKey = signingConfigurations.Key;
                paramsValidation.ValidAudience = tokenConfiguration.Audience;
                paramsValidation.ValidIssuer = tokenConfiguration.Issuer;
                paramsValidation.ValidateIssuerSigningKey = true;
                paramsValidation.ValidateLifetime = true;
                paramsValidation.RequireExpirationTime = true;
                paramsValidation.ClockSkew = TimeSpan.FromSeconds(30);
            });

            services.AddAuthorizationCore();

            return services;
        }

        public static IServiceCollection AddLexitaticoCors(this IServiceCollection services)
        {
            services.AddCors(optionsCors =>
                {
                    optionsCors.AddDefaultPolicy(optionsPolicy =>
                    {
                        optionsPolicy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    });
                });

            return services;
        }
    }
}
