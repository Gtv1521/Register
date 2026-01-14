using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using FrameworkDriver_Api.src.Models;
using FrameworkDriver_Api.src.Interfaces;
using FrameworkDriver_Api.src.Exceptions;
using System.Collections.Concurrent;

namespace FrameworkDriver_Api.src.Utils
{
    public class Token : IToken<UserModel>
    {
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, DateTime> _revokedTokens = new();
        public Token(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //  se crea token JWT 
        public async Task<string> GenerateToken(UserModel user, int timeInHours)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"] ?? _configuration["Jwt:Issuer"],
                Subject = new ClaimsIdentity(claims),
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddHours(timeInHours),
                SigningCredentials = creds
            };

            var handler = new JsonWebTokenHandler();
            return await Task.FromResult(handler.CreateToken(descriptor));
        }

        public async Task<string> GenerateRefreshToken(string Id)
        {
            if (Id == null) throw new FailedException("Fallo crear token refresh");
            return await Task.FromResult(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
        }

        public bool IsRevoked(string jti)
        {
            var now = DateTime.UtcNow;
            foreach (var token in _revokedTokens.Where(t => t.Value < now).ToList())
            {
                _revokedTokens.TryRemove(token.Key, out _);
            }

            return _revokedTokens.ContainsKey(jti);
        }

        public void Revoke(string jti, DateTime expiration)
        {
            _revokedTokens.TryAdd(jti, expiration);
        }
    }
}