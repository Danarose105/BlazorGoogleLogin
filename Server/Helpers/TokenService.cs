using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlazorGoogleLogin.Helpers
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        private readonly string _securityKey;
        private readonly string _validIssuer;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _securityKey = _configuration["JWTSettings:securityKey"];
            _validIssuer = _configuration["JWTSettings:validIssuer"];

        }

        public string GenerateToken(Claim[] claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_securityKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWTSettings:validIssuer"],
                audience: _configuration["JWTSettings:validIssuer"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_securityKey);
            try
            {
                var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _validIssuer,
                    ValidAudience = _validIssuer,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return claimsPrincipal;
            }
            catch
            {
                return null;
            }
        }

        public bool IsTokenExpired(string token)
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            return jwtToken.ValidTo < DateTime.UtcNow;
        }

        public string RefreshToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
            {
                return null;
            }

            var jwtToken = new JwtSecurityTokenHandler().ReadToken(token) as JwtSecurityToken;
            var expirationTime = jwtToken.ValidTo;
            var currentTime = DateTime.UtcNow;

            // Refresh the token if it's about to expire in the next 5 minutes
            if (expirationTime > currentTime && expirationTime <= currentTime.AddMinutes(5))
            {
                Console.WriteLine("refresh");
                var claims = principal.Claims.ToArray();
                return GenerateToken(claims);
            }

            return token; // Return the existing token if it's not close to expiration
        }


    }
}
