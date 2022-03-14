using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Gateway.Lextatico.Api.Configurations
{
    public class SigningConfiguration
    {
        public SecurityKey Key { get; }
        public SigningCredentials SigningCredentials { get; }

        public SigningConfiguration(string secretKey)
        {
            var secretKeyBytes = Encoding.ASCII.GetBytes(secretKey);
            Key = new SymmetricSecurityKey(secretKeyBytes);


            SigningCredentials = new SigningCredentials(
                Key, SecurityAlgorithms.HmacSha256Signature);
        }
    }
}
