using Microsoft.Extensions.Configuration;
using Portfolio.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portfolio.Services.Services
{
    public class UnsubscribeTokenService : IUnsubscribeTokenService
    {
        private readonly string _encryptionKey;

        public UnsubscribeTokenService(IConfiguration configuration)
        {
            _encryptionKey = configuration["UnsubscribeEncryptionKey"] ?? "YourDefaultKey123!";
        }

        public string GenerateToken(string email)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(email);
            return Convert.ToBase64String(plainTextBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        public string DecryptEmail(string token)
        {
            token = token.Replace("-", "+").Replace("_", "/");
            var padding = (4 - token.Length % 4) % 4;
            token += new string('=', padding);

            var base64Bytes = Convert.FromBase64String(token);
            return Encoding.UTF8.GetString(base64Bytes);
        }
    }
}
