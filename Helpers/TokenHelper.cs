using JwtAuth.Responses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace JwtAuth.Helpers
{
    public class TokenHelper
    {
        public const string Issuer = "https://localhost:44327/";
        public const string Audience = "https://localhost:44327/";
        public const string Secret = "OFRC1j9aaR2BvADxNWlG2pmuD392UfQBZZLM1fuzDEzDlEpSsn+btrpJKd3FfY855OMA9oK4Mc8y48eYUrVUSw==";

        public static string GenerateToken(LoginResponse _User)
        {
            var claimsIdentity = new List<Claim>
            {
               new Claim(ClaimTypes.Name, _User.UserId),
                new Claim(ClaimTypes.Email, _User.Email),
            };
            return GenerateAccessToken(claimsIdentity);
        }

        public static string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = Convert.FromBase64String(Secret);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var tokeOptions = new JwtSecurityToken(
               issuer: Issuer,
               audience: Audience,
               claims: claims,
               expires: DateTime.Now.AddMinutes(30),
               signingCredentials: signingCredentials
           );

            return new JwtSecurityTokenHandler().WriteToken(tokeOptions);
        }

        public static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }


        public static ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(Secret))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }

}
