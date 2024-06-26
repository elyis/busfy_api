using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using busfy_api.src.Domain.Entities.Shared;
using Microsoft.IdentityModel.Tokens;
using webApiTemplate.src.App.IService;
using webApiTemplate.src.Domain.Entities.Config;
using webApiTemplate.src.Domain.Entities.Shared;

namespace webApiTemplate.src.App.Service
{
    public class JwtService : IJwtService
    {
        private readonly SigningCredentials _signingCredentials;

        public JwtService(JwtSettings jwtSettings)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key));
            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        }

        private string GenerateAccessToken(Dictionary<string, string> claims, TimeSpan timeSpan)
        {
            var tokenClaims = claims.Select(claim => new Claim(claim.Key, claim.Value));

            var token = new JwtSecurityToken(
                claims: tokenClaims,
                expires: DateTime.UtcNow.Add(timeSpan),
                signingCredentials: _signingCredentials
            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken() => Guid.NewGuid().ToString();

        public TokenPair GenerateDefaultTokenPair(TokenInfo tokenInfo)
        {
            var claims = new Dictionary<string, string>{
                { "UserId", tokenInfo.UserId.ToString() },
                { ClaimTypes.Role, tokenInfo.Role},
                { "SessionId", tokenInfo.SessionId.ToString()}
            };
            var timeSpan = new TimeSpan(2, 0, 0, 0);
            return GenerateTokenPair(claims, timeSpan);
        }

        private TokenPair GenerateTokenPair(Dictionary<string, string> claims, TimeSpan timeSpan) =>
            new TokenPair(
                    GenerateAccessToken(claims, timeSpan),
                    GenerateRefreshToken()
                );

        private List<Claim> GetClaims(string token) =>
            new JwtSecurityTokenHandler()
                .ReadJwtToken(token.Replace("Bearer ", ""))
                .Claims
                .ToList();

        public TokenInfo GetTokenPayload(string token)
        {
            var claims = GetClaims(token);
            var userIdStr = claims.FirstOrDefault(claim => claim.Type == "UserId")?.Value;
            return new TokenInfo
            {
                Role = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role)?.Value,
                UserId = Guid.Parse(claims.FirstOrDefault(claim => claim.Type == "UserId")?.Value),
                SessionId = Guid.Parse(claims.FirstOrDefault(claim => claim.Type == "SessionId")?.Value)
            };
        }
    }
}